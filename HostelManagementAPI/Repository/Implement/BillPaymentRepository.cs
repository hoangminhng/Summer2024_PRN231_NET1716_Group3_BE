using AutoMapper;
using BusinessObject.Models;
using DAO;
using DTOs.BillPayment;
using DTOs.Contract;
using DTOs.Enum;
using DTOs.Room;
using DTOs.Service;
using Repository.Interface;

namespace Repository.Implement
{
    public class BillPaymentRepository : IBillPaymentRepository
    {
        private readonly IMapper _mapper;

        public BillPaymentRepository(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task CreateFirstBill(
            RoomDetailResponseDto hiredRoomDto,
            GetContractDto currentContractDto,
            DateTime billingMonth)
        {
            double? totalAmount = 0;
            int daysInMonth = DateTime.DaysInMonth(billingMonth.Year, billingMonth.Month);
            DateTime contractStartDate = currentContractDto.DateStart.Value;
            int contractStartDay = contractStartDate.Day;

            int daysStayed = daysInMonth - contractStartDay + 1;
            double dailyRoomFee = (double)currentContractDto.RoomFee / daysInMonth;
            totalAmount += dailyRoomFee * daysStayed;

            var billPayment = new BillPayment
            {
                ContractId = currentContractDto.ContractID,
                BillAmount = totalAmount,
                Month = billingMonth.Month,
                Year = billingMonth.Year,
                CreatedDate = DateTime.Now,
                TotalAmount = totalAmount,
                BillPaymentStatus = (int)BillPaymentStatus.Pending,
                BillType = (int)BillType.MonthlyPayment,
                AccountPayId = currentContractDto.StudentAccountID,
                AccountReceiveId = currentContractDto.OwnerAccountId,
                Details = new List<BillPaymentDetail>()
            };

            var selectedServices = await RoomServiceDao.Instance.GetRoomServicesIsSelected((int)currentContractDto.RoomID);
            foreach (var service in selectedServices)
            {
                if (service.TypeService.Unit.Equals("m³") && service.TypeService.TypeName.Equals("Water"))
                {
                    var initWaterService = new BillPaymentDetail
                    {
                        RoomServiceID = service.RoomServiceId,
                        OldNumberService = currentContractDto.InitWaterNumber,
                        NewNumberService = currentContractDto.InitWaterNumber,
                        Quantity = 0,
                        ServiceTotalAmount = 0,
                    };

                    billPayment.Details.Add(initWaterService);
                    continue;
                }

                if (service.TypeService.Unit.Equals("kWh") && service.TypeService.TypeName.Equals("Electricity"))
                {
                    var initElectricityService = new BillPaymentDetail
                    {
                        RoomServiceID = service.RoomServiceId,
                        OldNumberService = currentContractDto.InitWaterNumber,
                        NewNumberService = currentContractDto.InitElectricityNumber,
                        Quantity = 0,
                        ServiceTotalAmount = 0,
                    };

                    billPayment.Details.Add(initElectricityService);
                    continue;
                }

                if (service.TypeService.Unit.Equals("Month"))
                {
                    double servicePrice = service.Price ?? 0;
                    double dailyServicePrice = servicePrice / daysInMonth;
                    double proratedServicePrice = dailyServicePrice * daysStayed;

                    var monthlyService = new BillPaymentDetail
                    {
                        RoomServiceID = service.RoomServiceId,
                        OldNumberService = 0,
                        NewNumberService = 0,
                        Quantity = 1,
                        ServiceTotalAmount = proratedServicePrice,
                    };

                    billPayment.Details.Add(monthlyService);
                    totalAmount += proratedServicePrice;
                    continue;
                }
            }

            double remainingDeposit = (double)(currentContractDto.DepositFee - totalAmount);
            if (remainingDeposit >= 0)
            {
                billPayment.TotalAmount = 0;
                billPayment.BillPaymentStatus = (int)BillPaymentStatus.Paid;
                billPayment.PaidDate = DateTime.Now;
            }
            else
            {
                billPayment.TotalAmount = -remainingDeposit;
            }

            var currentContract = await ContractDao.Instance.GetContractById(currentContractDto.ContractID);
            if (currentContract != null)
            {
                currentContract.DepositFee = Math.Max(remainingDeposit, 0);
                await ContractDao.Instance.UpdateAsync(currentContract);
            }

            await BillPaymentDao.Instance.CreateAsync(billPayment);
        }


        public async Task CreateBillPaymentMonthly(
            RoomDetailResponseDto hiredRoomDto,
            GetContractDto currentContractDto,
            RoomBillPaymentDto roomBillPaymentDto,
            DateTime billingMonth)
        {
            double? totalAmount = 0;

            totalAmount += (double)currentContractDto.RoomFee;

            if (currentContractDto.DepositFee >= 0)
            {
                double remainingDeposit = (double)(currentContractDto.DepositFee - totalAmount);

                var currentContract = await ContractDao.Instance.GetContractById(currentContractDto.ContractID);
                if (currentContract != null)
                {
                    currentContract.DepositFee = Math.Max(remainingDeposit, 0);
                    await ContractDao.Instance.UpdateAsync(currentContract);
                }

                totalAmount -= currentContractDto.DepositFee;
            }

            var billPaymentDetails = new List<BillPaymentDetail>();
            var selectedServices = await RoomServiceDao.Instance.GetRoomServicesIsSelected((int)currentContractDto.RoomID);

            foreach (var service in selectedServices)
            {
                //Serivce fix price per month
                if (service.TypeService.Unit.Equals("Month"))
                {
                    double servicePrice = service.Price ?? 0;

                    totalAmount += servicePrice;

                    var billPaymentDetail = new BillPaymentDetail
                    {
                        RoomServiceID = service.RoomServiceId,
                        OldNumberService = 0,
                        NewNumberService = 0,
                        Quantity = 1,
                        ServiceTotalAmount = servicePrice,
                    };

                    billPaymentDetails.Add(billPaymentDetail);
                }
                else
                {
                    //Get last bill payment
                    double oldNumberService = 0;
                    var lastBillPayment = await BillPaymentDao.Instance.GetLastBillPaymentDetail(service.RoomServiceId);
                    if (lastBillPayment != null)
                    {
                        oldNumberService = lastBillPayment.NewNumberService ?? 0;
                    }

                    //
                    var serviceReading = roomBillPaymentDto.ServiceReadings
                        .FirstOrDefault(sr => sr.RoomServiceId == service.RoomServiceId);
                    //if (serviceReading == null)
                    //{
                    //    throw new Exception($"No new number service value provided for RoomServiceId {service.RoomServiceId}");
                    //}

                    //Calculate usage service => total price
                    double newNumberService = serviceReading.NewNumberService;
                    double usage = newNumberService - oldNumberService;
                    double serviceTotalAmount = usage * (service.Price ?? 0);

                    var billPaymentDetail = new BillPaymentDetail
                    {
                        RoomServiceID = service.RoomServiceId,
                        OldNumberService = oldNumberService,
                        NewNumberService = newNumberService,
                        Quantity = (int)usage,
                        ServiceTotalAmount = serviceTotalAmount
                    };

                    billPaymentDetails.Add(billPaymentDetail);
                    totalAmount += serviceTotalAmount;
                }
            }

            var billPayment = new BillPayment
            {
                ContractId = currentContractDto.ContractID,
                BillAmount = (double)currentContractDto.RoomFee,
                Month = billingMonth.Month,
                Year = billingMonth.Year,
                CreatedDate = DateTime.Now,
                TotalAmount = totalAmount,
                BillPaymentStatus = (int)BillPaymentStatus.Pending,
                BillType = (int)BillType.MonthlyPayment,
                Details = billPaymentDetails,
                AccountPayId = currentContractDto.StudentAccountID,
                AccountReceiveId = currentContractDto.OwnerAccountId,
            };

            await BillPaymentDao.Instance.CreateAsync(billPayment);
        }

        public async Task<MonthlyBillPaymentResponseDto> GetLastMonthBillPaymentsByOwnerId(int ownerId)
        {
            var lastBillPayments = await BillPaymentDao.Instance.GetLastBillPaymentsByOwnerId(ownerId);

            var billPaymentDtos = _mapper.Map<IEnumerable<BillPaymentDto>>(lastBillPayments).ToList();

            var currentDate = DateTime.Now;
            //var currentDate = new DateTime(2024, 7, 1);
            var existingBills = new List<BillPaymentDto>();

            foreach (var billPaymentDto in billPaymentDtos)
            {
                var contract = await ContractDao.Instance.GetContractByContractIDAsync(billPaymentDto.ContractId.Value);
                var room = await RoomDao.Instance.GetRoomById(contract.RoomID.Value);
                billPaymentDto.RoomName = room.RoomName;
                billPaymentDto.RenterName = contract.StudentLeadAccount.Name;

                var billPaymentDetails = await BillPaymentDao.Instance.GetBillPaymentDetail(billPaymentDto.BillPaymentID.Value);
                billPaymentDto.BillPaymentDetails = _mapper.Map<List<BillPaymentDetailResponseDto>>(billPaymentDetails);

                var firstBillingMonth = new DateTime(contract.DateStart.Value.Year, contract.DateStart.Value.Month, 1);
                var contractStartDate = contract.DateStart.Value;
                var monthsSinceStart = ((currentDate.Year - contractStartDate.Year) * 12) + currentDate.Month - contractStartDate.Month;


                bool isFirstMonth = monthsSinceStart == 0;
                var billingMonth = isFirstMonth ? contractStartDate : firstBillingMonth.AddMonths(monthsSinceStart);

                if (isFirstMonth)
                {
                    billPaymentDto.StartDate = contract.DateStart.Value;
                    billPaymentDto.EndDate = new DateTime(contract.DateStart.Value.Year, contract.DateStart.Value.Month, DateTime.DaysInMonth(contract.DateStart.Value.Year, contract.DateStart.Value.Month));
                }
                else
                {
                    billPaymentDto.Month = billingMonth.Month;
                    billPaymentDto.StartDate = billingMonth;
                    billPaymentDto.EndDate = billingMonth.AddMonths(1).AddDays(-1);
                    billPaymentDto.IsFirstBill = false;
                }

                var existingBillPayment = await BillPaymentDao.Instance.GetCurrentBillPayment(contract.ContractID, currentDate.Month, currentDate.Year);
                if (existingBillPayment != null && existingBillPayment.ContractId == billPaymentDto.ContractId)
                {
                    existingBills.Add(billPaymentDto);
                }
            }

            var allContracts = await ContractDao.Instance.GetContractsByOwnerIDAsync(ownerId);
            var signedContracts = allContracts.Where(c => c.Status == (int)ContractStatusEnum.signed).ToList();

            var existingContractIds = billPaymentDtos.Select(bp => bp.ContractId).ToList();

            foreach (var contract in signedContracts)
            {
                if (!existingContractIds.Contains(contract.ContractID))
                {
                    var room = await RoomDao.Instance.GetRoomById(contract.RoomID.Value);
                    var renterName = contract.StudentLeadAccount.Name;
                    var selectedServices = await RoomServiceDao.Instance.GetRoomServicesIsSelected(room.RoomID);

                    var firstBillingMonth = new DateTime(contract.DateStart.Value.Year, contract.DateStart.Value.Month, 1);
                    var billingMonth = firstBillingMonth.AddMonths(((currentDate.Year - contract.DateStart.Value.Year) * 12) + currentDate.Month - contract.DateStart.Value.Month);
                    bool isFirstMonth = billingMonth.Month == contract.DateStart.Value.Month && billingMonth.Year == contract.DateStart.Value.Year;

                    var startDate = isFirstMonth ? contract.DateStart.Value : new DateTime(billingMonth.Year, billingMonth.Month, 1);
                    var endDate = isFirstMonth
                        ? new DateTime(contract.DateStart.Value.Year, contract.DateStart.Value.Month, DateTime.DaysInMonth(contract.DateStart.Value.Year, contract.DateStart.Value.Month))
                        : startDate.AddMonths(1).AddDays(-1);

                    int daysInMonth = DateTime.DaysInMonth(billingMonth.Year, billingMonth.Month);
                    DateTime contractStartDate = contract.DateStart.Value;
                    int contractStartDay = contractStartDate.Day;

                    int daysStayed = daysInMonth - contractStartDay + 1;

                    var defaultBillPaymentDto = new BillPaymentDto
                    {
                        RoomId = contract.RoomID.Value,
                        ContractId = contract.ContractID,
                        BillType = (int)BillType.MonthlyPayment,
                        RoomName = room.RoomName,
                        RenterName = renterName,
                        CreatedDate = DateTime.Now,
                        BillAmount = contract.RoomFee,
                        Month = DateTime.Now.Month,
                        Year = DateTime.Now.Year,
                        BillPaymentStatus = (int)BillPaymentStatus.Pending,
                        BillPaymentDetails = new List<BillPaymentDetailResponseDto>(),
                        StartDate = startDate,
                        EndDate = endDate,
                        IsFirstBill = true,
                    };

                    foreach (var service in selectedServices)
                    {
                        if (service.TypeService.Unit.Equals("m³") && service.TypeService.TypeName.Equals("Water"))
                        {
                            var initWaterService = new BillPaymentDetailResponseDto
                            {
                                RoomServiceID = service.RoomServiceId,
                                OldNumberService = contract.InitWaterNumber,
                                NewNumberService = contract.InitWaterNumber,
                                Quantity = 0,
                                ServiceTotalAmount = 0,
                                ServicePrice = service.Price,
                                ServiceType = service.TypeService.TypeName,
                                ServiceUnit = service.TypeService.Unit,
                            };

                            defaultBillPaymentDto.BillPaymentDetails.Add(initWaterService);
                            continue;
                        }

                        if (service.TypeService.Unit.Equals("kWh") && service.TypeService.TypeName.Equals("Electricity"))
                        {
                            var initElectricityService = new BillPaymentDetailResponseDto
                            {
                                RoomServiceID = service.RoomServiceId,
                                OldNumberService = contract.InitWaterNumber,
                                NewNumberService = contract.InitElectricityNumber,
                                Quantity = 0,
                                ServiceTotalAmount = 0,
                                ServicePrice = service.Price,
                                ServiceType = service.TypeService.TypeName,
                                ServiceUnit = service.TypeService.Unit,
                            };

                            defaultBillPaymentDto.BillPaymentDetails.Add(initElectricityService);
                            continue;
                        }

                        if (service.TypeService.Unit.Equals("Month"))
                        {
                            double servicePrice = service.Price ?? 0;
                            double dailyServicePrice = servicePrice / daysInMonth;
                            double proratedServicePrice = dailyServicePrice * daysStayed;

                            var monthlyService = new BillPaymentDetailResponseDto
                            {
                                RoomServiceID = service.RoomServiceId,
                                OldNumberService = 0,
                                NewNumberService = 0,
                                Quantity = 1,
                                ServiceTotalAmount = proratedServicePrice,
                                ServicePrice = service.Price,
                                ServiceType = service.TypeService.TypeName,
                                ServiceUnit = service.TypeService.Unit,
                            };

                            defaultBillPaymentDto.BillPaymentDetails.Add(monthlyService);
                            continue;
                        }
                    }

                    billPaymentDtos.Add(defaultBillPaymentDto);
                }
            }

            foreach (var bill in existingBills)
            {
                billPaymentDtos.Remove(bill);
            }

            return new MonthlyBillPaymentResponseDto
            {
                billPaymentDtos = billPaymentDtos
            };
        }

        public async Task<BillPaymentDto> GetCurrentMonthBillPayment(int contractId, int month, int year)
        {

            var currentBillPayment = await BillPaymentDao.Instance.GetCurrentBillPayment(contractId, month, year);
            if (currentBillPayment != null)
            {
                return _mapper.Map<BillPaymentDto>(currentBillPayment);
            }

            return null;
        }

        public async Task<BillPaymentDto> GetLastMonthBillPayment(int contractId, int roomId)
        {
            var lastBillPayment = await BillPaymentDao.Instance.GetLastBillPayment(contractId);
            var lastBillPaymentDto = _mapper.Map<BillPaymentDto>(lastBillPayment);
            if (lastBillPaymentDto == null)
            {
                lastBillPaymentDto = new BillPaymentDto();
                lastBillPaymentDto.ContractId = contractId;
                lastBillPaymentDto.BillType = (int)BillType.MonthlyPayment;
                lastBillPaymentDto.BillPaymentID = 0;
            }

            var selectedServices = await RoomServiceDao.Instance.GetRoomServicesIsSelected(roomId);
            var billPaymentDetails = new List<BillPaymentDetail>();

            foreach (var service in selectedServices)
            {
                var lastBillPaymentDetail = await BillPaymentDao.Instance.GetLastBillPaymentDetail(service.RoomServiceId);
                if (lastBillPaymentDetail != null)
                {
                    billPaymentDetails.Add(lastBillPaymentDetail);
                }
                else
                {
                    var billPaymentDetail = new BillPaymentDetail
                    {
                        RoomServiceID = service.RoomServiceId,
                        RoomService = service,
                    };

                    billPaymentDetails.Add(billPaymentDetail);
                }
            }

            var billPaymentDetailDtos = _mapper.Map<IEnumerable<BillPaymentDetailResponseDto>>(billPaymentDetails);
            lastBillPaymentDto.BillPaymentDetails = (List<BillPaymentDetailResponseDto>)billPaymentDetailDtos;

            return lastBillPaymentDto;
        }

        public async Task<BillPaymentDto> GetBillPaymentById(int id)
        {
            var billPayment = await BillPaymentDao.Instance.GetBillPayment(id);
            return _mapper.Map<BillPaymentDto>(billPayment);
        }

        public async Task<BillPaymentDto> GetBillPaymentByTnxRef(string tnxRef)
        {
            var billPayment = await BillPaymentDao.Instance.GetBillPaymentByTnxRef(tnxRef);
            return _mapper.Map<BillPaymentDto>(billPayment);
        }

        public async Task<BillPaymentDto> CreateBillPayment(BillPaymentDto billPaymentDto)
        {
            var billPayment = _mapper.Map<BillPayment>(billPaymentDto);

            await BillPaymentDao.Instance.CreateAsync(billPayment);

            billPaymentDto.BillPaymentID = billPayment.BillPaymentID;

            return billPaymentDto;
        }

        public async Task<BillPaymentDto> UpdateBillPayment(BillPaymentDto billPaymentDto)
        {
            var billPayment = _mapper.Map<BillPayment>(billPaymentDto);

            await BillPaymentDao.Instance.UpdateAsync(billPayment);

            return billPaymentDto;
        }

        public async Task<IEnumerable<BillPaymentDto>> GetBillPaymentsByContractId(int contractId)
        {
            var billPayments = await BillPaymentDao.Instance.GetBillPaymentByContractId(contractId);

            return _mapper.Map<IEnumerable<BillPaymentDto>>(billPayments);
        }

        public async Task<BillPaymentDto> GetBillPaymentDetail(int billPaymentId)
        {
            var billPayment = await BillPaymentDao.Instance.GetBillPaymentByBillPaymentId(billPaymentId);
            var billPaymentDto = _mapper.Map<BillPaymentDto>(billPayment);

            var billPaymentDetail = await BillPaymentDao.Instance.GetBillPaymentDetail(billPaymentId);
            var billPaymentDetailDtos = _mapper.Map<IEnumerable<BillPaymentDetailResponseDto>>(billPaymentDetail);

            billPaymentDto.BillPaymentDetails = (List<BillPaymentDetailResponseDto>)billPaymentDetailDtos;

            return billPaymentDto;
        }

        public async Task<IEnumerable<BillPaymentHistoryMember>> GetBillPaymentHistoryMembers(int accountId)
        {
            var billPayment = await BillPaymentDao.Instance.GetBillPaymentHistoryMember(accountId);
            var result = billPayment.Select(x => new BillPaymentHistoryMember
            {
                BillPaymentId = x.BillPaymentID,
                BillAmount = x.BillAmount,
                TotalAmount = x.TotalAmount,
                CreatedDate = x.CreatedDate,
                BillType = x.BillType,
                PaidDate = x.PaidDate
            });

            result = result.OrderBy(x => x.PaidDate);
            return result.ToList();
        }

        public async Task<NumberService> GetOldNumberServiceElectricAndWater(int roomID)
        {
            var contractNewest = ContractDao.Instance.GetContractsAsync().Result.OrderByDescending(x => x.CreatedDate).FirstOrDefault(x => x.RoomID == roomID);
            if (contractNewest == null)
            {
                return new NumberService
                {
                    ElectricNumber = 0,
                    WaterNumber = 0
                };
            }
            var typeElectric = RoomServiceDao.Instance.GetRoomServicesByRoom(roomID).Result.FirstOrDefault(x => x.TypeServiceId == 1 && x.Status == 0).RoomServiceId;
            var typeWater = RoomServiceDao.Instance.GetRoomServicesByRoom(roomID).Result.FirstOrDefault(x => x.TypeServiceId == 2 && x.Status == 0).RoomServiceId;
            var billNewsest = BillPaymentDao.Instance.GetAllAsync().Result.OrderByDescending(x => x.CreatedDate).FirstOrDefault(x => x.ContractId == contractNewest.ContractID);
            if (billNewsest == null)
            {
                return new NumberService
                {
                    ElectricNumber = 0,
                    WaterNumber = 0
                };
            }
            var electricNumber = BillPaymentDetailDao.Instance.GetAllAsync().Result.FirstOrDefault(x => x.BillPaymentID == billNewsest.BillPaymentID && x.RoomServiceID == typeElectric).NewNumberService;
            var waterNumber = BillPaymentDetailDao.Instance.GetAllAsync().Result.FirstOrDefault(x => x.BillPaymentID == billNewsest.BillPaymentID && x.RoomServiceID == typeWater).NewNumberService;
            return new NumberService
            {
                ElectricNumber = (double)electricNumber,
                WaterNumber = (double)waterNumber
            };
        }

        public async Task<IEnumerable<BillMonthlyPaymentMember>> GetMonthlyBillPaymentForMember(int accountId)
        {
            var billPayment = await BillPaymentDao.Instance.GetBillMonthlyPaymentForMember(accountId);
            var result = billPayment.Select(x => new BillMonthlyPaymentMember
            {
                BillPaymentID = x.BillPaymentID,
                ContractId = x.ContractId,
                BillAmount = x.BillAmount,
                TotalAmount = x.TotalAmount,
                CreatedDate = x.CreatedDate,
                Month = x.Month,
                Year = x.Year,
                BillPaymentStatus = x.BillPaymentStatus,
                RoomID = x.Contract.RoomID,
                RoomName = x.Contract.Room.RoomName
            });
            return result.ToList();
        }

        public async Task<IEnumerable<BillPaymentDto>> GetBillPaymentsForOwner(int accountId)
        {
            var billPayment = await BillPaymentDao.Instance.GetBillPaymentHistoryOnwer(accountId);

            return _mapper.Map<IEnumerable<BillPaymentDto>>(billPayment);

        }
    }
}
