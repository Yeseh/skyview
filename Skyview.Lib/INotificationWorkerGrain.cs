namespace Skyview.Lib;

public interface INotificationWorkerGrain : IGrainWithIntegerKey
{
    Task Enable();
    Task Disable();
    Task<bool> IsEnabled();
}