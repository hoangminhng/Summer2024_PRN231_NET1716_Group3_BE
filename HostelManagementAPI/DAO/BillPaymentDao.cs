using BusinessObject.Models;
using DTOs.Enum;
using Microsoft.EntityFrameworkCore;

namespace DAO
{
    public class BillPaymentDao : BaseDAO<BillPayment>
    {
        private static BillPaymentDao instance = null;
        private static readonly object instacelock = new object();

        public BillPaymentDao()
        {
        }

        public static BillPaymentDao Instance
        {
            get
            {
                lock (instacelock)
                {
                    if (instance == null)
                    {
                        instance = new BillPaymentDao();
                    }
                    return instance;
                }

            }
        }

        public async Task<BillPayment> GetCurrentBillPayment(int contractId, int month, int year)
        {
            using (var context = new DataContext())
            {
                return await context.BillPayment
                    .FirstOrDefaultAsync(b => b.ContractId == contractId && b.Month == month && b.Year == year);
            }
        }

        public async Task<BillPaymentDetail> GetLastBillPaymentDetail(int roomServiceId)
        {
            using (var context = new DataContext())
            {
                return await context.BillPaymentDetail
                    .Where(d => d.RoomServiceID == roomServiceId)
                    .Include(d => d.RoomService)
                    .Include(d => d.RoomService.TypeService)
                    .OrderByDescending(d => d.BillPayment.CreatedDate)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<BillPayment> GetLastBillPayment(int contractId)
        {
            using (var context = new DataContext())
            {
                return await context.BillPayment
                    .Where(b => b.ContractId == contractId && b.BillType == (int)BillType.MonthlyPayment)
                     .Include(bp => bp.Contract)
                        .ThenInclude(c => c.Room)
                    .Include(bp => bp.Contract)
                        .ThenInclude(c => c.StudentLeadAccount)
                    .OrderByDescending(d => d.CreatedDate)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<BillPayment> GetBillPayment(int id)
        {
            DataContext context = new DataContext();

            return await context.BillPayment.FindAsync(id);
        }

        public async Task<BillPayment> GetBillPaymentByTnxRef(string tnxRef)
        {
            DataContext context = new DataContext();

            return await context.BillPayment
                .Include(x => x.Contract)
                .ThenInclude(y => y.Room)
                .FirstOrDefaultAsync(b => b.TnxRef == tnxRef);
        }

        public async Task<IEnumerable<BillPayment>> GetBillPaymentByContractId(int contractId)
        {
            using (var context = new DataContext())
            {
                return await context.BillPayment
                    .Where(b => b.ContractId == contractId)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<BillPaymentDetail>> GetBillPaymentDetail(int billPaymentId)
        {
            using (var context = new DataContext())
            {
                return await context.BillPaymentDetail
                 .Where(d => d.BillPaymentID == billPaymentId)
                 .Include(d => d.RoomService)
                 .Include(d => d.RoomService.TypeService)
                 .ToListAsync();
            }
        }

        public async Task<BillPayment> GetBillPaymentByBillPaymentId(int billPaymentId)
        {
            using (var context = new DataContext())
            {
                return await context.BillPayment
                    .Include(bp => bp.Contract)
                        .ThenInclude(c => c.Room)
                    .Include(bp => bp.Contract)
                        .ThenInclude(c => c.StudentLeadAccount)
                    .Include(bp => bp.Details)
                        .ThenInclude(d => d.RoomService)
                        .ThenInclude(rs => rs.TypeService)
                    .FirstOrDefaultAsync(bp => bp.BillPaymentID == billPaymentId);
            }
        }

        public async Task<IEnumerable<BillPayment>> GetLastBillPaymentsByOwnerId(int ownerId)
        {
            using (var context = new DataContext())
            {
                var rooms = await context.Room
                    .Include(r => r.RoomContract)
                    .Where(r => r.Hostel.AccountID == ownerId)
                    .ToListAsync();

                var lastBillPayments = new List<BillPayment>();

                foreach (var room in rooms)
                {
                    foreach (var contract in room.RoomContract)
                    {
                        var lastBillPayment = await GetLastBillPayment(contract.ContractID);
                        if (lastBillPayment != null)
                        {
                            lastBillPayments.Add(lastBillPayment);
                        }
                    }
                }

                return lastBillPayments;
            }
        }

        public async Task<IEnumerable<BillPayment>> GetBillPaymentHistoryMember(int accountId)
        {
            using (var context = new DataContext())
            {
                var billPayment = await context.BillPayment
                    .Where(b => b.AccountPayId == accountId && b.BillPaymentStatus == (int)BillPaymentStatus.Paid)
                    .ToListAsync();
                return billPayment;
            }
        }

        public async Task<IEnumerable<BillPayment>> GetBillPaymentHistoryOnwer(int accountId)
        {
            using (var context = new DataContext())
            {
                var billPayment = await context.BillPayment
                    .Include(b => b.Contract)
                    .ThenInclude(c => c.Room)
                    .Include(bp => bp.Contract)
                    .ThenInclude(c => c.StudentLeadAccount)
                    .Where(b => b.AccountReceiveId == accountId)
                    .ToListAsync();
                return billPayment;
            }
        }

        public async Task<IEnumerable<BillPayment>> GetBillMonthlyPaymentForMember(int accountId)
        {
            using (var context = new DataContext())
            {
                var billPayment = await context.BillPayment
                    .Include(b => b.Contract)
                    .ThenInclude(c => c.Room)
                    .Where(b => b.AccountPayId == accountId && b.BillType == (int)BillType.MonthlyPayment && b.BillPaymentStatus == (int)BillPaymentStatus.Pending)
                    .ToListAsync();
                return billPayment;
            }
        }
    }
}
