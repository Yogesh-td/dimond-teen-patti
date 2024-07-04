using TeenPatti.Audios;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_Rules : Base_Screen
    {
        [Space]
        [SerializeField] Button btn_close;

        private void Start()
        {
            btn_close.onClick.AddListener(OnClick_Close);
        }

        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}