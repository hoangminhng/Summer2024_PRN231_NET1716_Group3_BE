using DTOs.Enum;
using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class NotificationDto
    {
        [Key]
        public int? NotificationId { get; set; }
        public int? AccountNoticeId { get; set; }
        public int? ReceiveAccountId { get; set; }
        public string? NotificationText { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? NotificationType { get; set; }
        public string? Title { get; set; }
        public string? ForwardToPath { get; set; }
        public bool? IsRead { get; set; }


        public static string GetForwardPath(int notificationType)
        {
            switch (notificationType)
            {
                case (int)NotificationTypeEnum.owner_create_contract:
                    return "/contracts";

                case (int)NotificationTypeEnum.owner_create_bill:
                    return "/payments";

                case (int)NotificationTypeEnum.member_sign_contract:
                    return "/owner/contracts";

                case (int)NotificationTypeEnum.member_pay_bill:
                    return "/owner/payments";

                case (int)NotificationTypeEnum.member_make_appointment:
                    return "/owner/appointments";

                case (int)NotificationTypeEnum.member_rent_room:
                    return "/owner/contracts";

                case (int)NotificationTypeEnum.member_send_complain:
                    return "/owner/complains";

                case (int)NotificationTypeEnum.owner_reply_complain:
                    return "/complains";

                case (int)NotificationTypeEnum.owner_package_expire:
                    return "/owner/packages";

                case (int)NotificationTypeEnum.contract_finish:
                    return "/contracts";

                case (int)NotificationTypeEnum.owner_create_contract_but_this_is_for_decline_member:
                    return "/member/contracts";

                case (int)NotificationTypeEnum.member_decline_contract:
                    return null;

                case (int)NotificationTypeEnum.member_make_hiring_request:
                    return "/owner/appointments";

                default:
                    return null;
            }
        }
    }
}
