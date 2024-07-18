using AutoMapper;
using DTOs.Enum;
using BusinessObject.Models;
using DAO;
using DTOs.Hostel;
using Repository.Interface;

namespace Repository.Implement
{
	public class HostelRepository : IHostelRepository
	{
		private readonly IMapper _mapper;

		public HostelRepository(IMapper mapper)
		{
			_mapper = mapper;
		}

		public async Task<int> CreateHostel(CreateHostelRequestDto createHostelRequestDto)
		{
            var hostelType = Enum.Parse<HostelTypeEnum>(createHostelRequestDto.HostelType);

            Hostel hostel = new Hostel
			{
				HostelName = createHostelRequestDto.HostelName,
				HostelAddress = createHostelRequestDto.HostelAddress,
				HostelDescription = createHostelRequestDto.HostelDescription,
				AccountID = createHostelRequestDto.AccountID,
				Status = (int)HostelEnum.Prepare,
				HostelType = HostelTypeExtensions.ToFriendlyString(hostelType),
				CreateDate = DateTime.Now,
			};

			await HostelDao.Instance.CreateAsync(hostel);

			return hostel.HostelID;
		}

		public async Task<IEnumerable<HostelResponseDto>> GetAllHostels()
		{
			var hostels = await HostelDao.Instance.GetAllHostelsAsync();
			hostels = hostels.Where(h => h.Status == (int)HostelEnum.Available);
			var hostelResponseDto = hostels.Select(x => new HostelResponseDto
			{
				HostelID = x.HostelID,
				HostelName = x.HostelName,
				HostelAddress = x.HostelAddress,
				HostelDescription = x.HostelDescription,
				AccountID = x.AccountID,
				OwnerName = x.OwnerAccount.Name,
				Status = x.Status,
				NumOfAvailableRoom = x.Rooms.Count(r => r.Status == (int)RoomEnum.Available || r.Status == (int)RoomEnum.Viewing),
				Images = x.Images.Select(i => i.ImageURL).ToList(),
				NumOfTotalRoom = x.Rooms.Count,
				CreateDate = x.CreateDate,
				Phone = x.OwnerAccount.Phone,
				HostelType = x.HostelType,
				LowestPrice = x.Rooms.Min(r => r.RoomFee),
				LowestArea = x.Rooms.Min(r => r.Area),
			});
			return hostelResponseDto;
            //return _mapper.Map<IEnumerable<HostelResponseDto>>(hostels);
        }

		public async Task<HostelResponseDto> GetHostelDetailById(int id)
		{
			var hostel = await HostelDao.Instance.GetHostelById(id);

			return _mapper.Map<HostelResponseDto>(hostel);
		}

        public async Task<InformationHouse> GetHostelInformation(int id)
        {
            return await HostelDao.Instance.GetHostelInformation(id);
        }

        public async Task<IEnumerable<HostelResponseDto>> GetOwnerHostels(int ownerId)
		{
			var hostels = await HostelDao.Instance.GetAllHostelsAsync();
			hostels = hostels.Where(h => h.AccountID == ownerId).OrderByDescending(h => h.HostelID);
			return _mapper.Map<IEnumerable<HostelResponseDto>>(hostels);
		}

		public async Task UpdateHostel(int hostelId, UpdateHostelRequestDto updateHostelRequestDto)
		{
			var currentHostel = await HostelDao.Instance.GetHostelById(hostelId);
            var hostelType = Enum.Parse<HostelTypeEnum>(updateHostelRequestDto.HostelType);

            currentHostel.HostelName = updateHostelRequestDto.HostelName;
			currentHostel.HostelDescription = updateHostelRequestDto.HostelDescription;
			currentHostel.HostelAddress = updateHostelRequestDto.HostelAddress;
			currentHostel.HostelType = HostelTypeExtensions.ToFriendlyString(hostelType);

			await HostelDao.Instance.UpdateAsync(currentHostel);
		}

		public async Task UpdateHostelImage(int hostelId, List<string> imageUrls)
		{
			var currentHostel = await HostelDao.Instance.GetHostelById(hostelId);

			if (currentHostel.Images == null)
			{
				currentHostel.Images = new List<HostelImage>();
			}

            foreach (var imageUrl in imageUrls)
            {
                var hostelImage = new HostelImage
                {
                    ImageURL = imageUrl,
                    HostelID = currentHostel.HostelID,
                };

                currentHostel.Images.Add(hostelImage);
            }

            await HostelDao.Instance.UpdateAsync(currentHostel);
		}

		public async Task UpdateHostelStatus(int hostelId, int status)
		{
			var currentHostel = await HostelDao.Instance.GetHostelById(hostelId);

			currentHostel.Status = status;

			await HostelDao.Instance.UpdateAsync(currentHostel);
		}

		public async Task<HostelDetailAdminView> GetHostelDetailAdminView(int id)
		{
			var hostel = await HostelDao.Instance.GetHostelById(id);

			return _mapper.Map<HostelDetailAdminView>(hostel);
		}

		public async Task<IEnumerable<HostelsAdminView>> GetHostelsAdminView()
		{
			var hostels = await HostelDao.Instance.GetAllHostelsAsync();

			return _mapper.Map<IEnumerable<HostelsAdminView>>(hostels);
		}
	}
}
