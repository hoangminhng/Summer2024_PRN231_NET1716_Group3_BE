using DTOs;
using DTOs.Enum;
using DTOs.Hostel;
using Microsoft.IdentityModel.Tokens;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IFirebaseMessagingService _messagingService;



        public NotificationService(INotificationRepository notificationRepository, IFirebaseMessagingService messagingService)
        {
            _notificationRepository = notificationRepository;
            _messagingService = messagingService;
        }

        public async Task<IEnumerable<NotificationDto>> GetAllNotifications()
        {
            return await _notificationRepository.GetAllNotifications();
        }
        public async Task CreateNotification(NotificationDto notificationDto)
        {
            await _notificationRepository.CreateNotification(notificationDto);
        }

        public async Task<List<NotificationDto>> GetNotificationsBaseOnReceiveId(int accountId)
        {
            var list = await _notificationRepository.GetAllNotifications();

            return list.Where(n => n.ReceiveAccountId == accountId).ToList();
        }

        public async Task SendMemberWhoGetNewContract(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse)
        {
            string title = "You has a contract that need to sign";
            string body = $"You have a new contract of room {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address}. Please move to the contract page for further detail";

            int type = (int)NotificationTypeEnum.owner_create_contract;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }
                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }


            }
            catch
            {

            }

        }

        public async Task SendMembersWhoGetDeclineContract(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse)
        {
            string title = "Your room appointment has been decline";
            string body = $"The room {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address} already get a new renter. Please pick another room.";

            int type = (int)NotificationTypeEnum.owner_create_contract_but_this_is_for_decline_member;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }

                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }
            }
            catch
            {

            }



        }

        public async Task SendOwnerWhenMemberDepositContract(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse)
        {
            string title = "Your contract has been signed";
            string body = $"Your contract of room {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address} has been sign, please make the first monthly bill for this room.";

            int type = (int)NotificationTypeEnum.member_sign_contract;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }

                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }
            }
            catch
            {

            }
        }

        public async Task SendOwnerWhenMemberPayMonthlyFee(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse)
        {
            string title = $"You have received a new monthly payment";
            string body = $"You monthly payment of room {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address} has been paid.";

            int type = (int)NotificationTypeEnum.member_pay_bill;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }

                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }
            }
            catch
            {

            }
        }

        public async Task SendMemberWhenOwnerCreateNewMonthlyPayment(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse)
        {
            string title = $"Your room has a new bill";
            string body = $"Your room {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address} has a new bill, check it out!";

            int type = (int)NotificationTypeEnum.owner_create_bill;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }

                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }
            }
            catch
            {

            }
        }

        public async Task SendOwnerWhenMemberComplain(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse, string complain)
        {
            string title = $"You have a new complain";
            string body = $"Your room {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address} has a new complain. Please check it out!";

            int type = (int)NotificationTypeEnum.member_send_complain;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }

                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }
            }
            catch
            {

            }
        }

        public async Task SendMemberWhenOwnerReplyComplain(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse, string complainResponse)
        {
            string title = $"Your complain has been response";
            string body = $"Your complain of room {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address} has been answer. Check it out!";

            int type = (int)NotificationTypeEnum.owner_reply_complain;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }

                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }
            }
            catch
            {

            }
        }

        public async Task SendOwnerWhenMemberMakeAppointment(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse)
        {
            string title = $"Your have a new appointment";
            string body = $"Your room {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address} has a new appointment. Check it out!";

            int type = (int)NotificationTypeEnum.member_make_appointment;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }

                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }
            }
            catch
            {

            }
        }

        public async Task SendOwnerWhenMemberMakeHiringRequest(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse)
        {
            string title = $"Your have a new hiring request";
            string body = $"Your room {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address} has a new hiring request. Check it out!";

            int type = (int)NotificationTypeEnum.member_make_hiring_request;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }

                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }
            }
            catch
            {

            }
        }

        public async Task SendOwnerWhenMemberRentRoom(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse)
        {
            string title = $"Your room has been rent";
            string body = $"Your room {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address} has been rented. Please make a contract for the room!";

            int type = (int)NotificationTypeEnum.member_make_appointment;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }

                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }
            }
            catch
            {

            }
        }

        public async Task MarkNotificationAsRead(int id)
        {
            await _notificationRepository.UpdateNotificationStatus(id);
        }

        public async Task SendMemberWhenContractExpired(int accountReceivedId, string? firebaseToken, string name, InformationHouse informationHouse)
        {
            string title = $"Your contract has been expired";
            string body = $"Your contract for {informationHouse.RoomName} in {informationHouse.HostelName} at {informationHouse.Address} has been expired. " +
                $"In case this is a mistake, please contact us for more information. Thank you!";

            int type = (int)NotificationTypeEnum.member_contract_expired;
            var noti = new NotificationDto
            {
                ReceiveAccountId = accountReceivedId,
                Title = title,
                NotificationText = body,
                CreateDate = DateTime.Now,
                NotificationType = type,
                IsRead = false
            };

            noti.ForwardToPath = NotificationDto.GetForwardPath(type);

            try
            {
                noti = await _notificationRepository.CreateNotification(noti);
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "type", type.ToString() },
                    { "accountId", accountReceivedId.ToString() },
                    { "forwardToPath", noti.ForwardToPath },
                    {"notificationId", noti.NotificationId?.ToString() }

                };

                if (!firebaseToken.IsNullOrEmpty())
                {
                    _messagingService.SendPushNotification(firebaseToken, title, body, data);
                }
            }
            catch
            {

            }
        }

        //public async Task SendOwnerWhenMemberDeclineContract()
        //{

        //}
    }
}
