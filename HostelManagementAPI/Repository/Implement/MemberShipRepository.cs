﻿using AutoMapper;
using BusinessObject.Models;
using DAO;
using DTOs.Membership;
using Repository.Interface;

namespace Repository.Implement
{
    public class MemberShipRepository : IMemberShipRepository
    {
        private readonly IMapper _mapper;

        public MemberShipRepository(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task<bool> CreateMemberShip(CreateMemberShipDto createMemberShipDto)
        {
            var memberShip = _mapper.Map<MemberShip>(createMemberShipDto);
            memberShip.Status = 0;
            return await MemberShipDao.Instance.CreateAsync(memberShip);
        }
        public async Task<MemberShip> GetMembershipById(int memberShipID)
        {
            return await MemberShipDao.Instance.GetMemberShipById(memberShipID);
        }

        public async Task<IEnumerable<GetMemberShipDto>> GetMembershipsActive()
        {
            var memberShips = await MemberShipDao.Instance.GetAllAsync();
            var activeMemberShips = memberShips.Where(x => x.Status == 0);

            var result = activeMemberShips.Select(x => new GetMemberShipDto
            {
                MemberShipID = x.MemberShipID,
                MemberShipName = x.MemberShipName,
                CapacityHostel = x.CapacityHostel,
                Month = x.Month,
                MemberShipFee = x.MemberShipFee,
                Status = x.Status
            }); ;

            result = result.OrderBy(x => x.MemberShipFee);
            return result.ToList();
        }

        public async Task<IEnumerable<GetMemberShipDto>> GetMembershipExpire()
        {
            var memberShips = await MemberShipDao.Instance.GetAllAsync();
            var activeMemberShips = memberShips.Where(x => x.Status == 1);

            var result = activeMemberShips.Select(x => new GetMemberShipDto
            {
                MemberShipID = x.MemberShipID,
                MemberShipName = x.MemberShipName,
                CapacityHostel = x.CapacityHostel,
                Month = x.Month,
                MemberShipFee = x.MemberShipFee,
                Status = x.Status
            }); ;

            result = result.OrderBy(x => x.MemberShipFee);
            return result.ToList();
        }

        public async Task<IEnumerable<GetMemberShipDto>> GetAllMemberships()
        {
            var memberShips = await MemberShipDao.Instance.GetAllAsync();

            var result = memberShips.Select(x => new GetMemberShipDto
            {
                MemberShipID = x.MemberShipID,
                MemberShipName = x.MemberShipName,
                CapacityHostel = x.CapacityHostel,
                Month = x.Month,
                MemberShipFee = x.MemberShipFee,
                Status = x.Status
            }); ;

            result = result.OrderBy(x => x.MemberShipFee);
            return result.ToList();
        }

        public async Task<bool> UpdateMembershipStatus(MemberShip memberShip)
        {
            if (memberShip == null)
            {
                return false;
            }
            else
            {
                try
                {
                    await MemberShipDao.Instance.UpdateAsync(memberShip);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                    throw;
                }
            }
        }

        public async Task UpdateMemberShip(MemberShip memberShip)
        {
            await MemberShipDao.Instance.UpdateAsync(memberShip);
        }

        public async Task<bool> CheckMembershipNameExist(string memberShipName)
        {
            var result = MemberShipDao.Instance.GetAllAsync().Result.Where(x => x.MemberShipName.ToLower() == memberShipName.ToLower()).FirstOrDefault();
            if (result != null)
            {
                return true;
            }
            return false;
        }

        public async Task<GetMemberShipDto> GetDetailMemberShip(int packageID)
        {
            var membership = await MemberShipDao.Instance.GetMemberShipById(packageID);
            return _mapper.Map<GetMemberShipDto>(membership);
        }
    }
}
