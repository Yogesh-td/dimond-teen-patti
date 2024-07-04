using System;
using System.Collections;
using TeenPatti.App;
using UnityEngine;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace TeenPatti.Notifications
{
    public class IOSNotification_Handler : INOTIFICATION_HANDLER
    {
        private bool IsPermissionGranted
        {
            get { return PlayerPrefs.GetInt("IOSNOTI", 0) == 0 ? false : true; }
            set { PlayerPrefs.SetInt("IOSNOTI", value ? 1 : 0); }
        }


        public bool Has_Permission()
        {
            return IsPermissionGranted;
        }
        public void Request_Permission()
        {
            AppManager.Instance.StartCoroutine(Request_Permission_Popup());
        }
        public void Register_Channel()
        {
        }
        public void Register_Notification(string title, string desc, DateTime firetime)
        {
#if UNITY_IOS
            var timeTrigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = firetime - DateTime.Now,
                Repeats = false
            };

            var notification = new iOSNotification()
            {
                Identifier = "hello_world_notification",
                Title = title,
                Body = desc,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
                CategoryIdentifier = "default_category",
                ThreadIdentifier = "thread1",
                Trigger = timeTrigger
            };

            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }


        private IEnumerator Request_Permission_Popup()
        {
            yield return new WaitForSeconds(0);
#if UNITY_IOS
            using (AuthorizationRequest request = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true))
            {
                while (!request.IsFinished)
                    yield return null;

                IsPermissionGranted = request.Granted;
            }
#endif
        }

        public void Cancel_Notifications()
        {
#if UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
        }
    }
}