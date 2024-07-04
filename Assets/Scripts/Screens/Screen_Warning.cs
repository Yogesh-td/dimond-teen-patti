using System;
using TeenPatti.Audios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_Warning : Base_Screen
    {
        [Header("Components")]
        [SerializeField] TextMeshProUGUI txt_content;
        [SerializeField] Button btn_okay;
        [SerializeField] Button btn_close;

        [Space]
        [SerializeField] RectTransform[] layouts_to_refresh;

        Action button_callback;

        private void Start()
        {
            btn_okay.onClick.AddListener(OnClick_Okay);
            btn_close.onClick.AddListener(OnClick_Close);
        }

        public void Show(string _content_txt, string _button_txt, Action _button_callback = null, bool _show_close_btn = true)
        {
            txt_content.text = _content_txt;
            btn_okay.GetComponentInChildren<TextMeshProUGUI>().text = _button_txt;
            button_callback = _button_callback;
            btn_close.gameObject.SetActive(_show_close_btn);

            base.Show();
            foreach (var item in layouts_to_refresh)
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        }
        public override void Hide()
        {
            button_callback = null;
            base.Hide();
        }



        private void OnClick_Okay()
        {
            button_callback?.Invoke();
            OnClick_Close();
        }
        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}