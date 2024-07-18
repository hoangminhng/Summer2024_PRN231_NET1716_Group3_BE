using DTOs.Enum;
using DTOs.Hostel;
using DTOs.Room;
using DTOs.RoomAppointment;
using DTOs.RoomService;
using Microsoft.AspNetCore.Http;
using Repository.Interface;
using Service.Exceptions;
using Service.Interface;
using Service.Mail;

namespace Service.Implement
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IHostelRepository _hostelRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMailService _mailService;
        private readonly INotificationService _notificationService;

        public RoomService(
            IRoomRepository roomRepository,
            IHostelRepository hostelRepository,
            IAccountRepository accountRepository,
            IContractRepository contractRepository,
            ICloudinaryService cloudinaryService,
            IMailService mailService,
            INotificationService notificationService)
        {
            _roomRepository = roomRepository;
            _hostelRepository = hostelRepository;
            _accountRepository = accountRepository;
            _contractRepository = contractRepository;
            _cloudinaryService = cloudinaryService;
            _mailService = mailService;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<RoomListResponseDto>> GetListRoomsByHostelId(int hostelId)
        {
            return await _roomRepository.GetListRoomsByHostelId(hostelId);
        }
        
        public async Task<IEnumerable<RoomListResponseDto>> GetListRoomsByHostelIdForContract(int hostelId)
        {
            return await _roomRepository.GetListRoomsByHostelIdForContract(hostelId);
        }

        public async Task<IEnumerable<RoomListResponseDto>> GetListRoomByHostelIdForMember(int hostelId)
        {
            var rooms = await _roomRepository.GetListRoomsByHostelId(hostelId);
            return rooms.Where(r => r.Status == (int)RoomEnum.Available || r.Status == (int)RoomEnum.Viewing);
        }

        public async Task ChangeRoomStatus(int roomId, int status)
        {
            var room = await _roomRepository.GetRoomDetailById(roomId);
            if (room == null)
            {
                throw new ServiceException("Room not found with this ID");
            }

            if (!Enum.IsDefined(typeof(RoomEnum), status))
            {
                throw new ServiceException("Invalid status value");
            }

            var currentContract = await _contractRepository.GetCurrentContractByRoom(roomId);
            if (currentContract != null && status != (int)RoomEnum.Hiring)
            {
                throw new ServiceException("This room is hiring, can not change status");
            }

            await _roomRepository.UpdateRoomStatus(roomId, status);
        }

        public async Task<RoomDetailResponseDto> GetRoomDetailByRoomId(int roomId)
        {
            var room = await _roomRepository.GetRoomDetailById(roomId);
            if (room == null)
            {
                throw new ServiceException("Room not found with this ID");
            }

            var roomDetail = await _roomRepository.GetRoomDetailById(roomId);
            if (roomDetail.Status == (int)RoomEnum.Hiring)
            {
                //var contracts = await _contractRepository.GetContractsAsync();
                //var currentContract = contracts.FirstOrDefault(c => c.RoomID == roomId && c.Status == 1);
                //if (currentContract != null)
                //{
                //	var renterAcocunt = await _accountRepository.GetAccountById((int)currentContract.StudentAccountID);
                //	roomDetail.RenterName = renterAcocunt.Name;
                //}
            }

            return roomDetail;
        }

        public async Task<CreateRoomResponseDto> CreateRoom(CreateRoomRequestDto createRoomRequestDto)
        {
            var hostel = await _hostelRepository.GetHostelDetailById(createRoomRequestDto.HostelID);
            if (hostel == null)
            {
                throw new ServiceException("Hostel not found with this ID");
            }

            var roomId = await _roomRepository.CreateRoom(createRoomRequestDto);

            return new CreateRoomResponseDto { RoomID = roomId };
        }

        public async Task UploadRoomImage(IFormFileCollection files, int roomId)
        {
            var room = await _roomRepository.GetRoomById(roomId);
            if (room != null)
            {
                try
                {
                    var images = new List<string>();
                    foreach (var file in files)
                    {
                        var result = await _cloudinaryService.AddPhotoAsync(file);
                        if (result.Error != null)
                        {
                            throw new ServiceException("Error uploading image to Cloudinary: " + result.Error.Message);
                        }

                        string imageUrl = result.SecureUrl.AbsoluteUri;
                        images.Add(imageUrl);
                    }

                    await _roomRepository.UploadRoomImage(roomId, images);
                }
                catch (Exception ex)
                {
                    throw new ServiceException("Upload room image fail with error", ex);
                }
            }
            else
            {
                throw new ServiceException("Room not found with this ID");
            }
        }

        public async Task UpdateRoom(int roomId, RoomRequestDto updateRoomRequestDto)
        {
            var room = await _roomRepository.GetRoomById(roomId);
            if (room == null)
            {
                throw new ServiceException("Room not found with this ID");
            }

            await _roomRepository.UpdateRoom(roomId, updateRoomRequestDto);
        }

        public async Task<IEnumerable<RoomOfHostelAdminView>> GetHostelDetailWithRoomAdminView(int hostelId)
        {
            return await _roomRepository.GetHostelDetailWithRoomAdminView(hostelId); ;
        }

        public async Task<List<string>> GetRoomImagesByHostelId(int hostelId)
        {
            return await _roomRepository.GetRoomImagesByHostelId(hostelId);
        }

        public async Task<IEnumerable<GetAppointmentDto>> GetRoomAppointmentsAsync()
        {
            return await _roomRepository.GetRoomAppointmentsAsync();
        }

        public async Task<GetAppointmentDto> GetAppointmentById(int id)
        {
            return await _roomRepository.GetAppointmentById(id);
        }

        public async Task CreateRoomAppointmentAsync(CreateAppointmentSendEmailDto createAppointmentSendEmailDto)
        {
            CreateRoomAppointmentDto createRoomAppointmentDto = new CreateRoomAppointmentDto
            {
                RoomId = createAppointmentSendEmailDto.RoomId,
                ViewerId = createAppointmentSendEmailDto.ViewerId,
                AppointmentTime = createAppointmentSendEmailDto.AppointmentTime
            };
            await _roomRepository.CreateRoomAppointmentAsync(createRoomAppointmentDto);
            var ownerInfo = await _roomRepository.GetOwnerInfoByRoomId(createAppointmentSendEmailDto.RoomId);
            _mailService.SendMail(SendRoomAppointment.SendViewingAppointmentNotification(
                ownerInfo.Email,
                ownerInfo.Name,
                createAppointmentSendEmailDto.RoomName,
                createAppointmentSendEmailDto.AppointmentTime.ToString()));

            var room = await _roomRepository.GetRoomById(createAppointmentSendEmailDto.RoomId);
            var inf = new InformationHouse
            {
                HostelName = room.HostelName,
                Address = room.HostelAddress,
                RoomName = room.RoomName
            };

            var owner = await _accountRepository.GetAccountById((int)room.OwnerID);

            _notificationService.SendOwnerWhenMemberMakeAppointment(owner.AccountId, owner?.FirebaseToken, owner.Name, inf);
        }

        public async Task CreateRoomHiringRequestAsync(HireRequestSendEmailDto createAppointmentSendEmailDto)
        {
            CreateRoomAppointmentDto createRoomAppointmentDto = new CreateRoomAppointmentDto
            {
                RoomId = createAppointmentSendEmailDto.RoomId,
                ViewerId = createAppointmentSendEmailDto.ViewerId,
                AppointmentTime = DateTime.Now
            };
            await _roomRepository.CreateRoomHiringRequestAsync(createRoomAppointmentDto);
            var ownerInfo = await _roomRepository.GetOwnerInfoByRoomId(createAppointmentSendEmailDto.RoomId);
            _mailService.SendMail(SendRoomAppointment.SendViewingHiringDirectlyNotification(
                ownerInfo.Email,
                ownerInfo.Name,
                createAppointmentSendEmailDto.RoomName,
                DateTime.Now.ToString()));

            var room = await _roomRepository.GetRoomById(createAppointmentSendEmailDto.RoomId);
            var inf = new InformationHouse
            {
                HostelName = room.HostelName,
                Address = room.HostelAddress,
                RoomName = room.RoomName
            };

            var owner = await _accountRepository.GetAccountById((int)room.OwnerID);

            _notificationService.SendOwnerWhenMemberMakeHiringRequest(owner.AccountId, owner?.FirebaseToken, owner.Name, inf);
        }

        public async Task UpdateRoomServicesIsSelectStatusAsync(int roomId, List<RoomServiceUpdateDto> roomServiceUpdates)
        {
            await _roomRepository.UpdateRoomServicesIsSelectStatusAsync(roomId, roomServiceUpdates);
        }

        public async Task<bool> UpdateRoomStatus(int roomId, int status)
        {
            var room = await _roomRepository.GetRoomById(roomId);
            if (room == null)
            {
                throw new ServiceException("Room not found with this ID");
            }

            if (!Enum.IsDefined(typeof(RoomEnum), status))
            {
                throw new ServiceException("Invalid status value");
            }

            await _roomRepository.UpdateRoomStatus(roomId, status);
            return true;
        }
        public async Task<GetAppointmentContract> GetApppointmentToCreateContract(int roomID)
        {
            return await _roomRepository.GetApppointmentToCreateContract(roomID);
        }

        public async Task<IEnumerable<RoomServiceView>> GetRoomServicesByRoom(int roomId)
        {
            return await _roomRepository.GetRoomServicesByRoom(roomId);
        }

        public async Task<IEnumerable<RentingRoomResponseDto>> GetHiringRoomsForOwner(int ownerId)
        {
            return await _roomRepository.GetHiringRoomsForOwner(ownerId);
        }

        public async Task<IEnumerable<GetAppointmentOwner>> GetRoomAppointmentListByOwner(int hostelID)
        {
            return await _roomRepository.GetRoomAppointmentListByOwner(hostelID);
        }

        public async Task CancelAppointmentRoom(int appointmentID)
        {
            await _roomRepository.CancelAppointmentRoom(appointmentID);
            var account = await _roomRepository.GetAppointmentById(appointmentID);

            var room = await _roomRepository.GetAppointmentById(appointmentID);
            //var hostel = _roomRepository.GetRoomById(room.RoomId);
            var currentHostel = await _roomRepository.GetRoomDetailById(room.RoomId);
            var hostel = await _hostelRepository.GetHostelDetailById((int)currentHostel.HostelID);
            var inf = new InformationHouse()
            {
                HostelName = hostel.HostelName,
                Address = hostel.HostelAddress,
                RoomName = room.RoomName,
            };
            _mailService.SendMail(SendMailUserHiring.SendEmailDeclineAppointment(account.ViewerEmail, account.ViewerName, inf));
        }

        public async Task<List<MemberRoomRentedResponse>> GetRentedRoomList(int accountId)
        {
            var contracts = await _contractRepository.GetContractByStudentId(accountId);

            contracts = contracts.Where(c => c.Status != (int)ContractStatusEnum.pending);

            var rooms = new List<MemberRoomRentedResponse>();

            foreach (var contract in contracts)
            {
                var room = await _roomRepository.GetRoomById((int)contract.RoomID);
                var roomDetail = await _roomRepository.GetRoomDetailById((int)contract.RoomID);
                var memberRoomRentedResponse = new MemberRoomRentedResponse
                {
                    RoomID = room.RoomID,
                    RoomName = room.RoomName,
                    HostelID = room.HostelID,
                    HostelName = room.HostelName,
                    OwnerId = room.OwnerID,
                    OwnerName = room.OwnerName,
                    Status = contract.Status,
                    RoomThumbnail = roomDetail.RoomThumbnail,
                    StudentAccountId = accountId,
                    ContractId = contract.ContractID,
                    DateStart = contract.DateStart,
                    DateEnd = contract.DateEnd,
                    RoomFee = contract.RoomFee,
                };

                rooms.Add(memberRoomRentedResponse);
            }

            return rooms;
        }

        public async Task UpdateRoomServicePrice(UpdateRoomServicesPriceRequest request)
        {
            await _roomRepository.UpdateRoomServicePrice(request);
        }

        public async Task<IEnumerable<GetAppointmentMember>> GetRoomAppointmentListByMember(int accountID)
        {
            return await _roomRepository.GetRoomAppointmentListByMember(accountID);
        }

        public async Task<bool> CancelAllAppointmentViewing(int roomId)
        {
               return await _roomRepository.CancelAllAppointmentViewing(roomId);
        }

        //     public Task AddRoomService(AddRoomServicesDto addRoomServicesDto)
        //     {
        //return _roomRepository.AddRoomServicesAsync(addRoomServicesDto); ;
        //     }

        //     public Task RemoveRoomServiceAsync(int roomId, int serviceId)
        //     {
        //         return _roomRepository.RemoveRoomServiceAsync(roomId, serviceId);
        //     }

        //     public Task<IEnumerable<RoomServiceResponseDto>> GetRoomServicesByRoomIdAsync(int roomId)
        //     {
        //return _roomRepository.GetRoomServicesByRoomIdAsync(roomId);
        //     }
    }
}
