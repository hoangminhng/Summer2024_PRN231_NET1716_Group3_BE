using DTOs;
using DTOs.BillPayment;
using DTOs.Service;

namespace Service.Interface
{
    public interface IBillPaymentService
    {
        Task CreateBillPaymentMonthly(CreateBillPaymentRequestDto createBillPaymentRequestDto);
        Task<BillPaymentDto> CreateDepositPayment(DepositRoomInputDto depositRoomInputDto, int accountId);
        Task<BillPaymentDto> ConfirmBillingTransaction(VnPayReturnUrlDto vnPayReturnUrlDto);
        Task<BillPaymentDto> GetLastMonthBillPayment(int contractId);
        Task<IEnumerable<BillPaymentDto>> GetBillPaymentsByContractId(int contractId);
        Task<BillPaymentDto> GetBillPaymentDetail(int billPaymentId);
        Task<BillPaymentDto> PrepareBillingForMonthlyPayment(int billpaymentId, int accountId);
        Task<MonthlyBillPaymentResponseDto> GetLastMonthBillPaymentsByOwnerId(int ownerId);
        Task<IEnumerable<BillPaymentHistoryMember>> GetPaymentHistoryByMemberAccount(int memberId);
        Task<IEnumerable<BillPaymentDto>> GetPaymentHistoryByOwnerAccount(int ownerId);
        Task<NumberService> GetOldNumberServiceElectricAndWater(int roomID);
        Task<IEnumerable<BillMonthlyPaymentMember>> GetMonthlyBillPaymentForMember(int memberId);

    }
}
