﻿using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace DAO
{
    public class HostelDao : BaseDAO<Hostel>
    {
        private static HostelDao instance = null;
        private static readonly object instacelock = new object();

        private HostelDao()
        {
        }

        public static HostelDao Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HostelDao();
                }
                return instance;
            }
        }

        public async Task<Hostel> GetHostelById(int id)
        {
            using (var context = new DataContext())
            {
                return await context.Hostel
                .Include(x => x.OwnerAccount)
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HostelID == id);
            }
        }

        public async Task<IEnumerable<Hostel>> GetAllHostelsAsync()
        {
            var context = new DataContext();
            return await context.Hostel
                .Include(h => h.OwnerAccount)
                .Include(h => h.Rooms)
                .ToListAsync();
        }

        public async Task<IEnumerable<Hostel>> GetAllHostelsTotalActiveAsync()
        {
            var context = new DataContext();
            return await context.Hostel.Where(x => x.Status == 0)
                .ToListAsync();
        }

        //public async Task<IEnumerable<HostelService>> GetHostelServicesByHostelId(int hostelId)
        //{
        //    using (var context = new DataContext())
        //    {
        //        return await context.HostelService
        //            .Include(hs => hs.Service)
        //            .Where(hs => hs.HostelId == hostelId)
        //            .ToListAsync();
        //    }
        //}

        //public async Task AddHostelServices(IEnumerable<HostelService> hostelServices)
        //{
        //    using (var context = new DataContext())
        //    {
        //        foreach (var hostelService in hostelServices)
        //        {
        //            var existingHostelService = await context.HostelService
        //                .FirstOrDefaultAsync(hs => hs.HostelId == hostelService.HostelId && hs.ServiceId == hostelService.ServiceId);

        //            if (existingHostelService == null)
        //            {
        //                await context.HostelService.AddAsync(hostelService);
        //            }
        //            else
        //            {
        //                existingHostelService.Status = hostelService.Status;
        //                context.HostelService.Update(existingHostelService);
        //            }
        //        }

        //        await context.SaveChangesAsync();
        //    }
        //}
    }
}
