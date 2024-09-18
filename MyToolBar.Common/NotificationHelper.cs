using MyToolBar.Plugin;

namespace MyToolBar.Common;

/*
 仅提供接口和调用方法，通知实体的校验和处理逻辑由具体实现方负责
 */

public enum NotificationType { Msg, Warning}
public enum NotificationTimeSpan { Short, Long }
public record NotificationSource(Type Plugin,int id,string desc);
public record Notification(string Msg,NotificationType Type,NotificationSource Source,NotificationTimeSpan TimeSpan);

public interface INotificationReceiver
{
    void OnNotificationReceived(Notification notification);
}

public static class NotificationManager
{
    private static WeakReference<INotificationReceiver>? _receiver;

    public static bool RegisterReceiver(INotificationReceiver receiver)
    {
        if (_receiver != null && _receiver.TryGetTarget(out _))
        {
            return false;
        }
        _receiver = new(receiver);
        return true;
    }

    public static bool Send(Notification notification)
    {
        if (_receiver != null&&_receiver.TryGetTarget(out var receiver))
        {
            receiver.OnNotificationReceived(notification);
            return true;
        }
        return false;
    }
}
