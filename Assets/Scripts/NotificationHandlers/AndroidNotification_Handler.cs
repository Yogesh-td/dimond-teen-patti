using System;

#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
using UnityEngine.Playables;
#endif

namespace TeenPatti.Notifications
{
    public class AndroidNotification_Handler : INOTIFICATION_HANDLER
    {
        const string notification_permission = "android.permission.POST_NOTIFICATIONS";
        const string notification_channel_id = "default_channel";

        public bool Has_Permission()
        {
#if UNITY_ANDROID
            return Permission.HasUserAuthorizedPermission(notification_permission);
#else
            return false;
#endif
        }
        public void Request_Permission()
        {
            if (Has_Permission())
                return;

#if UNITY_ANDROID
            Permission.RequestUserPermission(notification_permission);
#endif
        }
        public void Register_Channel()
        {
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel()
            {
                Id = notification_channel_id,
                Name = "Default Channel",
                Importance = Importance.Default,
                Description = "Generic Notification"
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
        }
        public void Register_Notification(string title, string desc, DateTime firetime)
        {
#if UNITY_ANDROID
            var notification = new AndroidNotification()
            {
                Title = title,
                Text = desc,
                FireTime = firetime
            };

            AndroidNotificationCenter.SendNotification(notification, notification_channel_id);
#endif
        }

        public void Cancel_Notifications()
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllScheduledNotifications();
#endif
        }
    }
}