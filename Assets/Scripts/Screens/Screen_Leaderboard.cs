using TeenPatti.App;
using TeenPatti.Audios;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Screen_Leaderboard : Base_Screen
    {
        [Space]
        [SerializeField] Item_Leaderboard_Player player_prefab_prefab;
        [SerializeField] Transform player_parent;

        [Space]
        [SerializeField] Item_Leaderboard_Player local_player;
        [SerializeField] Button btn_close;

        private void Start()
        {
            btn_close.onClick.AddListener(OnClick_Close);
        }

        public override void Show()
        {
            SceneChangeManager.Instance.Show_Loading();

            ApiManager.Instance.Api_GetLeaderboard(
            (player_data) =>
            {
                SceneChangeManager.Instance.Hide_Loading();
                local_player.Initialize(player_data.local_player);

                foreach (var player in player_data.other_players)
                {
                    Item_Leaderboard_Player item = Instantiate(player_prefab_prefab, player_parent);
                    item.Initialize(player);
                }

                base.Show();
                player_parent.localPosition = new Vector3(player_parent.localPosition.x, 0);
            },
            error =>
            {
                SceneChangeManager.Instance.Hide_Loading();
                ScreenManager.Instance.Show_Warning(error, "Got it!");
                OnClick_Close();
            });
        }
        public override void Hide()
        {
            for (int i = 0; i < player_parent.childCount; i++)
                Destroy(player_parent.GetChild(i).gameObject);

            base.Hide();
        }

        private void OnClick_Close()
        {
            AudioManager.Instance.Play_Sound(SOUNDS.BUTTON_CLOSE);
            ScreenManager.Instance.HideScreen(this.screen_type);
        }
    }
}