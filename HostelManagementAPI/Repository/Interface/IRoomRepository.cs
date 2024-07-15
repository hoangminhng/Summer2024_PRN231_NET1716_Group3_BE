using DTOs.Room;
using DTOs.RoomAppointment;
using DTOs.RoomService;

namespace Repository.Interface
{
    public interface IRoomRepository
    {
        Task<int> CreateRoom(CreateRoomRequestDto createRoomRequestDto);
        Task<RoomListResponseDto> GetRoomById(int roomId);
        Task UpdateRoom(int roomId, RoomRequestDto updateRoomRequestDto);
        Task<IEnumerable<RoomListResponseDto>> GetListRoomsByHostelId(int hostelId);
        Task<IEnumerable<RoomListResponseDto>> GetListRoomsByHostelIdForContract(int hostelId);
        Task<RoomDetailResponseDto> GetRoomDetailById(int roomId);
        Task UpdateRoomStatus(int roomId, int status);
        Task UploadRoomImage(int roomId, List<string> imageUrls);
        Task<IEnumerable<RoomOfHostelAdminView>> GetHostelDetailWithRoomAdminView(int hostelId);
        Task<List<string>> GetRoomImagesByHostelId(int hostelId);
        Task<IEnumerable<GetAppointmentDto>> GetRoomAppointmentsAsync();
        Task<GetAppointmentDto> GetAppointmentById(int id);
        Task CreateRoomAppointmentAsync(CreateRoomAppointmentDto createRoomAppointmentDto);
        Task CreateRoomHiringRequestAsync(CreateRoomAppointmentDto createRoomAppointmentDto);
        Task UpdateRoomServicesIsSelectStatusAsync(int roomId, List<RoomServiceUpdateDto> roomServiceUpdates);
        Task<IEnumerable<RoomServiceResponseDto>> GetRoomServicesIsSelected(int roomId);
        Task<OwnerInfoDto> GetOwnerInfoByRoomId(int roomId);
        Task<GetAppointmentContract> GetApppointmentToCreateContract(int roomID);
        Task<IEnumerable<RoomServiceView>> GetRoomServicesByRoom(int roomId);
        Task<List<int>> UpdateAppointmentRoom(int? roomID, int accountID);
        Task<IEnumerable<RentingRoomResponseDto>> GetHiringRoomsForOwner(int ownerId);
        Task<IEnumerable<GetAppointmentOwner>> GetRoomAppointmentListByOwner(int accountID);
        Task<IEnumerable<GetAppointmentMember>> GetRoomAppointmentListByMember(int accountID);
        Task CancelAppointmentRoom(int appointmentID);
        Task UpdateRoomServicePrice(UpdateRoomServicesPriceRequest request);
        Task<bool> CancelAllAppointmentViewing(int roomId);
        //Task AddRoomServicesAsync(AddRoomServicesDto roomServicesDto);
        //   Task RemoveRoomServiceAsync(int roomId, int serviceId);
        //      Task<IEnumerable<RoomServiceResponseDto>> GetRoomServicesByRoomIdAsync(int roomId);
    }
}
