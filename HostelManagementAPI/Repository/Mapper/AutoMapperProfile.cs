using AutoMapper;
using BusinessObject.Models;
using DTOs;
using DTOs.Account;
using DTOs.BillPayment;
using DTOs.Complain;
using DTOs.Contract;
using DTOs.Enum;
using DTOs.Hostel;
using DTOs.Membership;
using DTOs.MemberShipRegisterTransaction;
using DTOs.Room;
using DTOs.RoomAppointment;
using DTOs.RoomService;
using DTOs.TypeService;

namespace Repository.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        //CreateMap<SendMessageDto, Message>();
        CreateMap<Account, AccountDto>();
        //.ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountID));
        CreateMap<AccountDto, Account>();
        //.ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountID));
        CreateMap<Account, AccountViewDetail>().ReverseMap();
        CreateMap<AccountViewDetail, AccountDto>().ReverseMap();
        CreateMap<Account, AccountMemberShipInformationDtos>().ReverseMap();
        CreateMap<AccountDto, AccountLoginDto>().ReverseMap();
        CreateMap<Room, RoomOfHostelAdminView>().ReverseMap();
        CreateMap<MemberShip, GetMemberShipDto>().ReverseMap();
        CreateMap<MemberShip, CreateMemberShipDto>().ReverseMap();
        CreateMap<TypeService, CreateTypeServiceDto>().ReverseMap();
        CreateMap<TypeService, UpdateTypeServiceDto>().ReverseMap();
        CreateMap<TypeService, ViewAllTypeServiceDto>().ReverseMap();
        CreateMap<Account, ViewMemberShipDto>().ReverseMap();
        CreateMap<Room, RoomListResponseDto>()
            .ForMember(dest => dest.RoomThumbnail, opt => opt.MapFrom(src => src.RoomImages.FirstOrDefault().RoomUrl))
            .ForMember(dest => dest.OwnerID, opt => opt.MapFrom(src => src.Hostel.OwnerAccount.AccountID))
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Hostel.OwnerAccount.Name))
            .ForMember(dest => dest.HostelName, opt => opt.MapFrom(src => src.Hostel.HostelName))
            .ForMember(dest => dest.HostelAddress, opt => opt.MapFrom(src => src.Hostel.HostelAddress));

        CreateMap<MemberShipRegisterTransaction, ViewTransactionMembership>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.OwnerAccount.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.OwnerAccount.Email));
        CreateMap<Room, RoomDetailResponseDto>()
            .ForMember(dest => dest.RoomThumbnail, opt => opt.MapFrom(src => src.RoomImages.FirstOrDefault().RoomUrl))
            .ForMember(dest => dest.RoomImageUrls, opt => opt.Ignore())
            .ForMember(dest => dest.RoomServices, opt => opt.MapFrom(src => src.RoomServices.Select(rs => new RoomServiceResponseDto
            {
                RoomServiceId = rs.RoomServiceId,
                ServiceName = rs.TypeService.TypeName,
                ServicePrice = (double)rs.Price,
                Status = rs.Status,
                TypeServiceID = rs.TypeService.TypeServiceID,
                Unit = rs.TypeService.Unit,
            })));
        CreateMap<Hostel, HostelResponseDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.OwnerAccount != null ? src.OwnerAccount.Name : string.Empty))
            .ForMember(dest => dest.NumOfAvailableRoom, opt => opt.MapFrom(src => src.Rooms != null ? src.Rooms.Count(r => r.Status == (int)RoomEnum.Available) : 0))
            .ForMember(dest => dest.NumOfTotalRoom, opt => opt.MapFrom(src => src.Rooms != null ? src.Rooms.Count() : 0))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(img => img.ImageURL).ToList() : new List<string>()))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.OwnerAccount != null ? src.OwnerAccount.Phone : string.Empty));
        CreateMap<Hostel, HostelsAdminView>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.OwnerAccount != null ? src.OwnerAccount.Name : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.OwnerAccount != null ? src.OwnerAccount.Email : string.Empty));
        CreateMap<Hostel, HostelDetailAdminView>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.OwnerAccount != null ? src.OwnerAccount.Name : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.OwnerAccount != null ? src.OwnerAccount.Email : string.Empty))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.OwnerAccount != null ? src.OwnerAccount.Phone : string.Empty))
            .ForMember(dest => dest.NumberOfRoom, opt => opt.MapFrom(src => src.Rooms != null ? src.Rooms.Count() : 0));

        CreateMap<Complain, CreateComplainDto>().ReverseMap();
        CreateMap<ComplainDto, Complain>();
        CreateMap<Complain, ComplainDto>()
            .ForMember(dest => dest.AccountComplainName, opt => opt.MapFrom(src => src.ComplainAccount != null ? src.ComplainAccount.Name : null))
            .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.Room.Hostel.AccountID != null ? src.Room.Hostel.AccountID : null))
            .ForMember(dest => dest.HostelName, opt => opt.MapFrom(src => src.Room.Hostel.HostelName != null ? src.Room.Hostel.HostelName : null))
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Room.Hostel.OwnerAccount.Name != null ? src.Room.Hostel.OwnerAccount.Name : null))
            .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomName : null));

        CreateMap<MemberShipRegisterTransaction, ViewHistoryMemberShipDtos>()
           .ForMember(dest => dest.MembershipName, opt => opt.MapFrom(src => src.MemberShip != null ? src.MemberShip.MemberShipName : string.Empty))
           .ForMember(dest => dest.CapacityHostel, opt => opt.MapFrom(src => src.MemberShip != null ? src.MemberShip.CapacityHostel : 0))
           .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.MemberShip != null ? src.MemberShip.Month : 0));
        CreateMap<Contract, UpdateContractDto>();
        //.ForMember(dest => dest.OwnerAccountId, opt => opt.MapFrom(src => src.Acc));
        CreateMap<Account, CustomerViewAccount>().ReverseMap();
        //CreateMap<Contract, GetContractDto>()
        //    .ForMember(dest => dest.OwnerAccountId, opt => opt.MapFrom(src => src.OwnerAccountID))
        //    .ForMember(dest => dest.StudentAccountID, opt => opt.MapFrom(src => src.StudentAccountID))
        //    .ForMember(dest => dest.RoomID, opt => opt.MapFrom(src => src.RoomID))
        //    .ForMember(dest => dest.ContractMemberDetails, opt => opt.MapFrom(src => src.Members));
        CreateMap<Contract, GetContractDto>()
            .ForMember(dest => dest.OwnerAccountId, opt => opt.MapFrom(src => src.OwnerAccountID))
            .ForMember(dest => dest.OwnerAccountName, opt => opt.MapFrom(src => src.OwnerAccount.Name))
            .ForMember(dest => dest.OwnerPhone, opt => opt.MapFrom(src => src.OwnerAccount.Phone))
            .ForMember(dest => dest.OwnerCitizen, opt => opt.MapFrom(src => src.OwnerAccount.CitizenCard))
            .ForMember(dest => dest.StudentAccountID, opt => opt.MapFrom(src => src.StudentAccountID))
            .ForMember(dest => dest.StudentLeadAccountName, opt => opt.MapFrom(src => src.StudentLeadAccount.Name))
            .ForMember(dest => dest.StudentLeadPhone, opt => opt.MapFrom(src => src.StudentLeadAccount.Phone))
            .ForMember(dest => dest.StudentLeadCitizen, opt => opt.MapFrom(src => src.StudentLeadAccount.CitizenCard))
            .ForMember(dest => dest.RoomID, opt => opt.MapFrom(src => src.RoomID))
            .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room.RoomName))
            .ForMember(dest => dest.RoomDescription, opt => opt.MapFrom(src => src.Room.Description))
            .ForMember(dest => dest.HostelName, opt => opt.MapFrom(src => src.Room.Hostel.HostelName))
            .ForMember(dest => dest.HostelAddress, opt => opt.MapFrom(src => src.Room.Hostel.HostelAddress))
            .ForMember(dest => dest.ContractMemberDetails, opt => opt.MapFrom(src => src.Members))
            .ForMember(dest => dest.RoomServiceDetails, opt => opt.MapFrom(src => src.Room.RoomServices.Select(rs => new RoomServiceResponseForContractDto
            {
                RoomId = rs.RoomId,
                RoomServiceId = rs.RoomServiceId,
                TypeServiceName = rs.TypeService.TypeName,
                ServiceName = rs.TypeService.Unit,
                ServicePrice = rs.Price ?? 0,
                IsSelected = rs.IsSelected ?? false
            }).ToList()))
            .ForMember(dest => dest.InitWaterNumber, opt => opt.MapFrom(src => src.InitWaterNumber))
            .ForMember(dest => dest.InitElectricityNumber, opt => opt.MapFrom(src => src.InitElectricityNumber));
        CreateMap<GetContractDto, Contract>();

        CreateMap<RoomService, RoomServiceResponseForContractDto>()
            .ForMember(dest => dest.RoomServiceId, opt => opt.MapFrom(src => src.RoomServiceId))
            .ForMember(dest => dest.TypeServiceName, opt => opt.MapFrom(src => src.TypeService.TypeName))
            .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.TypeService.Unit))
            .ForMember(dest => dest.ServicePrice, opt => opt.MapFrom(src => src.Price));

        CreateMap<ContractMember, GetContractDetailsDto>()
            .ForMember(dest => dest.ContractMemberID, opt => opt.MapFrom(src => src.ContractMemberID))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.CitizenCard, opt => opt.MapFrom(src => src.CitizenCard));
        CreateMap<RoomAppointment, GetAppointmentDto>()
            .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room.RoomName))
            .ForMember(dest => dest.RoomFee, opt => opt.MapFrom(src => src.Room.RoomFee))
            .ForMember(dest => dest.ViewerName, opt => opt.MapFrom(src => src.Viewer.Name))
            .ForMember(dest => dest.ViewerPhone, opt => opt.MapFrom(src => src.Viewer.Phone))
            .ForMember(dest => dest.ViewerEmail, opt => opt.MapFrom(src => src.Viewer.Email))
            .ForMember(dest => dest.ViewerCitizenCard, opt => opt.MapFrom(src => src.Viewer.CitizenCard));
        //CreateMap<UpdateContractDto, GetContractDto>().ReverseMap();
        CreateMap<CreateRoomAppointmentDto, RoomAppointment>();
        //     .ForMember(dest => dest.ServiceID, opt => opt.MapFrom(src => src.Service.ServiceID))
        //     .ForMember(dest => dest.TypeServiceID, opt => opt.MapFrom(src => src.Service.TypeServiceID))
        //     .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.ServiceName))
        //     .ForMember(dest => dest.ServicePrice, opt => opt.MapFrom(src => src.Service.ServicePrice))
        //     .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        CreateMap<MemberShipRegisterTransaction, MemberShipRegisterTransactionDto>()
        .ForMember(dest => dest.MemberShipName, opt => opt.MapFrom(src => src.MemberShip.MemberShipName))
        .ForMember(dest => dest.CapacityHostel, opt => opt.MapFrom(src => src.MemberShip.CapacityHostel))
        .ForMember(dest => dest.MemberShipFee, opt => opt.MapFrom(src => src.MemberShip.MemberShipFee))
        .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.MemberShip.Month));




        CreateMap<MemberShipRegisterTransactionDto, MemberShipRegisterTransaction>();

        CreateMap<BillPayment, BillPaymentDto>()
            .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Contract.Room.RoomName))
            .ForMember(dest => dest.RenterName, opt => opt.MapFrom(src => src.Contract.StudentLeadAccount.Name))
            .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.Contract.RoomID));

        CreateMap<BillPaymentDto, BillPayment>();

        CreateMap<BillPaymentDetail, BillPaymentDetailResponseDto>()
            .ForMember(dest => dest.ServicePrice, opt => opt.MapFrom(src => src.RoomService != null ? src.RoomService.Price : (double?)null))
            .ForMember(dest => dest.ServiceType, opt => opt.MapFrom(src => src.RoomService != null && src.RoomService.TypeService != null ? src.RoomService.TypeService.TypeName : null))
            .ForMember(dest => dest.ServiceUnit, opt => opt.MapFrom(src => src.RoomService != null && src.RoomService.TypeService != null ? src.RoomService.TypeService.Unit : null));

        CreateMap<BillPaymentDetailResponseDto, BillPaymentDetail>();

        CreateMap<RoomService, RoomServiceView>()
        .ForMember(dest => dest.TypeServiceName, opt => opt.MapFrom(src => src.TypeService.TypeName))
        .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.TypeService.Unit))
        .ForMember(dest => dest.ServicePrice, opt => opt.MapFrom(src => src.Price));



        CreateMap<Room, RentingRoomResponseDto>()
            .ForMember(dest => dest.RoomThumbnail, opt => opt.MapFrom(src => src.RoomImages.FirstOrDefault() != null ? src.RoomImages.FirstOrDefault().RoomUrl : null))
            .ForMember(dest => dest.HostelName, opt => opt.MapFrom(src => src.Hostel.HostelName))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.RoomContract.FirstOrDefault() != null ? src.RoomContract.FirstOrDefault().StudentLeadAccount.Name : null))
            .ForMember(dest => dest.StudentAccountId, opt => opt.MapFrom(src => src.RoomContract.FirstOrDefault() != null ? src.RoomContract.FirstOrDefault().StudentLeadAccount.AccountID : (int?)null))
            .ForMember(dest => dest.ContractId, opt => opt.MapFrom(src => src.RoomContract.FirstOrDefault() != null ? src.RoomContract.FirstOrDefault().ContractID : (int?)null));

        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.NotificationID))
            .ForMember(dest => dest.AccountNoticeId, opt => opt.MapFrom(src => src.AccountNoticeId))
            .ForMember(dest => dest.ReceiveAccountId, opt => opt.MapFrom(src => src.ReceiveAccountId))
            .ForMember(dest => dest.NotificationText, opt => opt.MapFrom(src => src.NotificationText))
            .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
            .ForMember(dest => dest.NotificationType, opt => opt.MapFrom(src => src.NotificationType))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ForwardToPath, opt => opt.MapFrom(src => src.ForwardToPath))
            .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead))
            .ReverseMap();

        //CreateMap<Room, MemberRoomRentedResponse>()
        //     .ForMember(dest => dest.RoomThumbnail, opt => opt.MapFrom(src => src.RoomImages.FirstOrDefault() != null ? src.RoomImages.FirstOrDefault().RoomUrl : null))
        //    .ForMember(dest => dest.HostelName, opt => opt.MapFrom(src => src.Hostel.HostelName))
        //    .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.RoomContract.FirstOrDefault() != null ? src.RoomContract.FirstOrDefault().StudentLeadAccount.Name : null))
        //    .ForMember(dest => dest.StudentAccountId, opt => opt.MapFrom(src => src.RoomContract.FirstOrDefault() != null ? src.RoomContract.FirstOrDefault().StudentLeadAccount.AccountID : (int?)null))
        //    .ForMember(dest => dest.ContractId, opt => opt.MapFrom(src => src.RoomContract.FirstOrDefault() != null ? src.RoomContract.FirstOrDefault().ContractID : (int?)null));


    }
}