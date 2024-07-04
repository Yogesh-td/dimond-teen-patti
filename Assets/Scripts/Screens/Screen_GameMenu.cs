using TeenPatti.App;
using TeenPatti.Audios;
using TeenPatti.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_GameMenu : Base_Screen
    {
        [Space]
        [SerializeField] Button btn_howtoplay;
        [SerializeField] Button btn_switchtable;
        [SerializeField] Button btn_exittable;
        [SerializeField] Button btn_close;

        private void Start()
        {
            btn_howtoplay.onClick.AddListener(OnClick_HowToPlay);
            btn_switchtable.onClick.AddListener(OnClick_SwitchTable);
            btn_exittable.onClick.AddListener(OnClick_ExitTable);
            btn_close.onClick.AddListener(OnClick_Close);
        }

        public override void Show()
        {
            if (!string.IsNullOrEmpty(ColyseusRoomManager.Instance.Get_Metadata(TPRoomManager.Instance.RoomId).metadata.private_key))
                btn_switchtable.gameObject.SetActive(false);

            base.Show();
        }



        private void OnClick_HowToPlay()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            ScreenManager.Instance.SwitchScreen(SCREEN_TYPE.RULES, this.screen_type);
        }
        private void OnClick_SwitchTable()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            GameManager.Instance.Request_Switch_Table();
            OnClick_Close();
        }
        private void OnClick_ExitTable()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_TAP);
            GameManager.Instance.Request_Leave_Table();
            OnClick_Close();
        }
        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}