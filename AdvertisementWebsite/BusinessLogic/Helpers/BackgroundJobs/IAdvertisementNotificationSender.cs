namespace BusinessLogic.Helpers.BackgroundJobs;

public interface IAdvertisementNotificationSender
{
    public Task SendNotifications(int newAdvertisementId);
}
