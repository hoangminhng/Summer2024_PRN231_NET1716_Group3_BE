using DTOs;

namespace Repository.Interface
{
    public interface INotificationRepository
    {
        Task<IEnumerable<NotificationDto>> GetAllNotifications();
        Task<NotificationDto> CreateNotification(NotificationDto notificationDto);
        Task UpdateNotificationStatus(int id);

    }
}
