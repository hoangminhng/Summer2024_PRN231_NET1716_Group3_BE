using DTOs.BillPayment;
using DTOs.Contract;
using DTOs.Room;
using DTOs.Service;

namespace Repository.Interface
{
    public interface IBillPaymentRepository
    {
        Task CreateFirstBill(
            RoomDetailResponseDto hiredRoomDto,
            GetContractDto currentContractDto,
            DateTime billingMonth);
        Task CreateBillPaymentMonthly(
            RoomDetailResponseDto hiredRoomDto,
            GetContractDto currentContract,
            RoomBillPaymentDto roomBillPaymentDto,
            DateTime billingMonth);
        Task<BillPaymentDto> GetCurrentMonthBillPayment(int contractId, int month, int year);
        Task<BillPaymentDto> GetLastMonthBillPayment(int contractId, int roomId);
        Task<BillPaymentDto> GetBillPaymentById(int id);
        Task<BillPaymentDto> GetBillPaymentByTnxRef(string tnxRef);
        Task<BillPaymentDto> CreateBillPayment(BillPaymentDto billPaymentDto);
        Task<BillPaymentDto> UpdateBillPayment(BillPaymentDto billPaymentDto);
        Task<IEnumerable<BillPaymentDto>> GetBillPaymentsByContractId(int contractId);
        Task<BillPaymentDto> GetBillPaymentDetail(int billPaymentId);
        Task<MonthlyBillPaymentResponseDto> GetLastMonthBillPaymentsByOwnerId(int ownerId);
        Task<IEnumerable<BillPaymentHistoryMember>> GetBillPaymentHistoryMembers(int accountId);
        Task<NumberService> GetOldNumberServiceElectricAndWater(int roomID);
        Task<IEnumerable<BillMonthlyPaymentMember>> GetMonthlyBillPaymentForMember(int accountId);
        Task<IEnumerable<BillPaymentDto>> GetBillPaymentsForOwner(int accountId);
    }
}
