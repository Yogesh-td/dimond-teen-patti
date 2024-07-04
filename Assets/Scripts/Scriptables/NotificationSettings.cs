using System;
using System.Collections.Generic;
using TeenPatti.App;
using TeenPatti.App.Settings;
using UnityEngine;

namespace TeenPatti.Notifications
{
    [CreateAssetMenu(fileName = "NotificationSettings", menuName = "Scriptables/NotificationSettings", order = 1)]
    public class NotificationSettings : ScriptableObject
    {
        INOTIFICATION_HANDLER handler;
        List<NotificationMsg> all_msgs;

        public void Initialize(List<NotificationMsg> msgs)
        {
            all_msgs = msgs;

#if UNITY_ANDROID
            handler = new AndroidNotification_Handler();
#elif UNITY_IOS
            handler = new IOSNotification_Handler();
#endif

            if (handler == null)
                return;

            handler.Request_Permission();
            Schedule_Notification();
        }


        public bool Has_Permission()
        {
            return handler == null ? false : handler.Has_Permission();
        }
        public void Request_Permission()
        {
            if (handler == null)
                return;

            handler.Request_Permission();
            Schedule_Notification();
        }
        public void Schedule_Notification()
        {
            if (!CoreSettings.Instance.Notification)
                return;

            NotificationMsg picked_msg = all_msgs[UnityEngine.Random.Range(0, all_msgs.Count)];
            handler.Register_Notification(picked_msg.title, picked_msg.desc, DateTime.Now.AddHours(3));
        }
        public void Cancel_Notifications()
        {
            handler.Cancel_Notifications();
        }
    }
}