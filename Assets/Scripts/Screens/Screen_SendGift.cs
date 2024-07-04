using System.Collections.Generic;
using TeenPatti.App;
using TeenPatti.App.Settings;
using TeenPatti.Audios;
using TeenPatti.Gameplay;
using TeenPatti.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_SendGift : Base_Screen
    {
        [Space]
        [SerializeField] Button gif_prefab;
        [SerializeField] Transform gif_parent;

        [Space]
        [SerializeField] Button btn_close;

        string receiver_session_id;
        List<GifPlayer> all_btn_gifs = new List<GifPlayer>();

        private void Start()
        {
            btn_close.onClick.AddListener(OnClick_Close);

            GIF_IMAGE[] all_gifs = AppManager.Instance.Chat_Settings.Gift_Gifs;
            for (int i = 0; i < all_gifs.Length; i++)
            {
                Button btn_gif = Instantiate(gif_prefab, gif_parent);
                btn_gif.gameObject.SetActive(true);
                btn_gif.name = i.ToString();
                btn_gif.onClick.AddListener(() => { Send_Gif(btn_gif.name); });

                GifPlayer btn_gif_player = btn_gif.GetComponentInChildren<GifPlayer>();
                btn_gif_player.Apply_Gif(all_gifs[i]);

                all_btn_gifs.Add(btn_gif_player);
            }

            gif_parent.localPosition = new Vector3(gif_parent.localPosition.x, 0);
        }

        public override void Show()
        {
            base.Show();
            foreach (var item in all_btn_gifs)
                item.Play();

            gif_parent.localPosition = new Vector3(gif_parent.localPosition.x, 0);
        }

        public void Update_Receiver(PlayerBehaviour behaviour)
        {
            receiver_session_id = behaviour == null ? "" : behaviour.SessionId;
        }
        private void Send_Gif(string index)
        {
            TPRoomManager.Instance.Send_Local_Gift(int.Parse(index), receiver_session_id);
            OnClick_Close();
        }


        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}