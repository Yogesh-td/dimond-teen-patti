using System;
using TeenPatti.App.Settings;
using TeenPatti.Audios;
using TeenPatti.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_SideshowReceived : Base_Screen
    {
        [Header("Components")]
        [SerializeField] TextMeshProUGUI txt_status;
        [SerializeField] TextMeshProUGUI txt_timer;

        [Space]
        [SerializeField] Button btn_accept;
        [SerializeField] Button btn_reject;

        float current_time;
        bool is_timer_started;
        TimeSpan remaining_timespan;

        private void Start()
        {
            btn_accept.onClick.AddListener(OnClick_Accept);
            btn_reject.onClick.AddListener(OnClick_Reject);
        }

        private void Update()
        {
            if (!is_timer_started)
                return;

            current_time -= Time.unscaledDeltaTime;
            if(current_time < 0)
            {
                is_timer_started = false;
                ScreenManager.Instance.HideScreen(this.screen_type);
            }

            remaining_timespan = TimeSpan.FromSeconds(current_time);
            txt_timer.text = string.Format("Remaining time for your decision:\n\n<color=yellow><size=40>{0:00}:{1:00} </size>sec</color>", remaining_timespan.Minutes, remaining_timespan.Seconds);
        }

        public override void Show()
        {
            base.Show();
            is_timer_started = true;

#if UNITY_ANDROID || UNITY_IOS
            if (CoreSettings.Instance.Vibration)
                Handheld.Vibrate();
#endif
        }


        public void Update_Details(string sender_username, float seconds)
        {
            current_time = seconds;
            txt_status.text = string.Format("<color=yellow><b><uppercase>{0}</uppercase></b></color> challenged you to sideshow your cards.\n\nWould you like to sideshow?", sender_username);
        }


        private void OnClick_Accept()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            TPRoomManager.Instance.Send_Sideshow_Responce(true);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
        private void OnClick_Reject()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            TPRoomManager.Instance.Send_Sideshow_Responce(false);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}