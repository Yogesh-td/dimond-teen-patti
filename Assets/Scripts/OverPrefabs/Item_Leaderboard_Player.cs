using TeenPatti.App;
using TeenPatti.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Screens
{
    public class Item_Leaderboard_Player : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI txt_rank;
        [SerializeField] Image img_rank;

        [Space]
        [SerializeField] Image img_avatar;
        [SerializeField] TextMeshProUGUI txt_playername;
        [SerializeField] TextMeshProUGUI txt_earnings;

        [Space]
        [SerializeField] Sprite[] sprs_ranks;

        public void Initialize(LEADERBOARD_PLAYER data)
        {
            txt_rank.text = data.rank.ToString("n0");
            if (data.rank > sprs_ranks.Length)
                img_rank.gameObject.SetActive(false);
            else
            {
                img_rank.gameObject.SetActive(true);
                img_rank.sprite = sprs_ranks[data.rank - 1];
            }


            img_avatar.sprite = AppManager.Instance.AvatarSettings.Get_Avatar(data.avatar_index);
            txt_playername.text = data.username;
            txt_earnings.text = string.Format("₹ {0}", data.total_winnings.ToString("n0"));
        }
    }
}