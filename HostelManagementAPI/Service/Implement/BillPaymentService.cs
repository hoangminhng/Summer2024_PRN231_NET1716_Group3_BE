using DTOs;
using DTOs.BillPayment;
using DTOs.Enum;
using DTOs.Hostel;
using DTOs.Service;
using Repository.Interface;
using Service.Exceptions;
using Service.Interface;

namespace Service.Implement
{
    public class BillPaymentService : IBillPaymentService
    {
        private readonly IBillPaymentRepository _billPaymentRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly INotificationService _notificationService;
        private readonly IAccountRepository _accountRepository;

        public BillPaymentService(
            IBillPaymentRepository billPaymentRepository,
            IContractRepository contractRepository,
            IRoomRepository roomRepository,
            INotificationService notificationService,
            IAccountRepository accountRepository)
        {
            _billPaymentRepository = billPaymentRepository;
            _contractRepository = contractRepository;
            _roomRepository = roomRepository;
            _notificationService = notificationService;
            _accountRepository = accountRepository;
        }

        public async Task<IEnumerable<BillPaymentDto>> GetBillPaymentsByContractId(int contractId)
        {
            var currentContract = await _contractRepository.GetContractById(contractId);
            if (currentContract == null)
            {
                throw new ServiceException("Contract not found with this ID");
            }

            return await _billPaymentRepository.GetBillPaymentsByContractId(contractId);
        }

        public async Task<BillPaymentDto> GetLastMonthBillPayment(int contractId)
        {
            var currentContract = await _contractRepository.GetContractById(contractId);
            if (currentContract == null)
            {
                throw new ServiceException("Contract not found with this ID");
            }

            var roomId = currentContract.RoomID;

            var lastMonthBillPayment = await _billPaymentRepository.GetLastMonthBillPayment(contractId, (int)roomId);

            return lastMonthBillPayment;
        }

        public async Task CreateBillPaymentMonthly(CreateBillPaymentRequestDto createBillPaymentRequestDto)
        {
            foreach (var roomBillPayment in createBillPaymentRequestDto.RoomBillPayments)
            {
                var contractId = roomBillPayment.ContractId;

                var currentContract = await _contractRepository.GetContractDetailsByContractId(contractId);
                if (currentContract == null)
                {
                    throw new ServiceException($"Contract not found for ID: {contractId}");
                }

                var currentDate = DateTime.Now;
                //var currentDate = new DateTime(2024, 7, 1);

                var firstBillingMonth = new DateTime(currentContract.DateStart.Value.Year, currentContract.DateStart.Value.Month, 1);
                var contractStartDate = currentContract.DateStart.Value;
                var monthsSinceStart = ((currentDate.Year - contractStartDate.Year) * 12) + currentDate.Month - contractStartDate.Month;


                bool isFirstMonth = monthsSinceStart == 0;
                var billingMonth = isFirstMonth ? contractStartDate : firstBillingMonth.AddMonths(monthsSinceStart);

                var existingBillPayment = await _billPaymentRepository.GetCurrentMonthBillPayment(contractId, currentDate.Month, currentDate.Year);
                if (existingBillPayment != null)
                {
                    continue;
                }

                var hiredRoom = await _roomRepository.GetRoomDetailById((int)currentContract.RoomID);
                if (hiredRoom != null)
                {
                    if (isFirstMonth)
                    {
                        await _billPaymentRepository.CreateFirstBill(hiredRoom, currentContract, billingMonth);
                    }
                    else
                    {
                        await _billPaymentRepository.CreateBillPaymentMonthly(hiredRoom, currentContract, roomBillPayment, billingMonth);
                    }
                }


            }
        }

        public async Task<BillPaymentDto> CreateDepositPayment(DepositRoomInputDto depositRoomInputDto, int accountId)
        {
            var contract = await _contractRepository.GetContractById(depositRoomInputDto.ContractId);

            if (contract == null)
            {
                throw new ServiceException("Contract does not existed");
            }

            var billpayment = new BillPaymentDto
            {
                ContractId = contract.ContractID,
                BillAmount = contract.DepositFee,
                TotalAmount = contract.DepositFee,
                AccountPayId = contract.StudentAccountID,
                AccountReceiveId = contract.OwnerAccountId,
                BillType = (int)BillType.Deposit,
                BillPaymentStatus = (int)BillPaymentStatus.Pending,
                CreatedDate = DateTime.Now,
                TnxRef = DateTime.Now.Ticks.ToString(),
            };

            billpayment = await _billPaymentRepository.CreateBillPayment(billpayment);

            return billpayment;
        }

        public async Task<BillPaymentDto> GetBillPaymentDetail(int billPaymentId)
        {
            var billPayment = await _billPaymentRepository.GetBillPaymentById(billPaymentId);
            if (billPayment == null)
            {
                throw new ServiceException("Bill payment not found with this ID");
            }

            return await _billPaymentRepository.GetBillPaymentDetail(billPaymentId);
        }

        public async Task<BillPaymentDto> PrepareBillingForMonthlyPayment(int billpaymentId, int accountId)
        {
            var billpayment = await _billPaymentRepository.GetBillPaymentById(billpaymentId);

            if (billpayment == null)
            {
                throw new ServiceException("Bill payment not found with this ID");
            }

            var contract = await _contractRepository.GetContractById((int)billpayment.ContractId);
            if (contract == null || contract?.StudentAccountID != accountId)
            {
                throw new ServiceException("User are not allow to using this function");
            }

            if (billpayment.BillPaymentStatus != (int)BillPaymentStatus.Pending || billpayment.BillType != (int)BillType.MonthlyPayment)
            {
                throw new ServiceException("Bill is not suiable for this transaction");
            }

            billpayment.TnxRef = DateTime.Now.Ticks.ToString();
            await _billPaymentRepository.UpdateBillPayment(billpayment);

            return billpayment;
        }

        public async Task<BillPaymentDto> ConfirmBillingTransaction(VnPayReturnUrlDto vnPayReturnUrlDto)
        {
            var billPayment = await _billPaymentRepository.GetBillPaymentByTnxRef(vnPayReturnUrlDto.TnxRef);

            if (billPayment == null)
            {
                throw new ServiceException("No bill match");
            }

            if (vnPayReturnUrlDto == null)
            {
                throw new ServiceException("No transaction match");
            }

            if (billPayment.BillPaymentStatus == (int)BillPaymentStatus.Paid)
            {
                throw new ServiceException("Billpayment has been paid");
            }

            billPayment.BillPaymentStatus = (int)BillPaymentStatus.Paid;
            billPayment.PaidDate = DateTime.Now;

            await _billPaymentRepository.UpdateBillPayment(billPayment);

            var ownerAccount = await _accountRepository.GetAccountById((int)billPayment.AccountReceiveId);
            var room = await _roomRepository.GetRoomById((int)billPayment.RoomId);

            var inf = new InformationHouse
            {
                HostelName = room.HostelName,
                Address = room.HostelAddress,
                RoomName = room.RoomName
            };

            if (billPayment.BillType == (int)BillType.Deposit)
            {
                _notificationService.SendOwnerWhenMemberDepositContract(ownerAccount.AccountId, ownerAccount.FirebaseToken, ownerAccount.Name, inf);
            }
            else
            {
                _notificationService.SendOwnerWhenMemberPayMonthlyFee(ownerAccount.AccountId, ownerAccount.FirebaseToken, ownerAccount.Name, inf);
            }

            return billPayment;
        }

        public async Task<MonthlyBillPaymentResponseDto> GetLastMonthBillPaymentsByOwnerId(int ownerId)
        {
            return await _billPaymentRepository.GetLastMonthBillPaymentsByOwnerId(ownerId);
        }

        public Task<IEnumerable<BillPaymentHistoryMember>> GetPaymentHistoryByMemberAccount(int memberId)
        {
            return _billPaymentRepository.GetBillPaymentHistoryMembers(memberId);
        }

        public async Task<NumberService> GetOldNumberServiceElectricAndWater(int roomID)
        {
            return await _billPaymentRepository.GetOldNumberServiceElectricAndWater(roomID);
        }

        public async Task<IEnumerable<BillMonthlyPaymentMember>> GetMonthlyBillPaymentForMember(int memberId)
        {
            return await _billPaymentRepository.GetMonthlyBillPaymentForMember(memberId);
        }

        public async Task<IEnumerable<BillPaymentDto>> GetPaymentHistoryByOwnerAccount(int ownerId)
        {
            return await _billPaymentRepository.GetBillPaymentsForOwner(ownerId);
        }
    }
}
