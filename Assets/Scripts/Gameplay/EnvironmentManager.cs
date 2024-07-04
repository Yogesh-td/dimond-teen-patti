using TeenPatti.App;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Gameplay
{
    public class EnvironmentManager : MonoBehaviour
    {
        [System.Serializable]
        public struct ENVIRONMENTS
        {
            public GAME_TYPE game_type;
            public Sprite[] backgrounds;
            public Sprite[] tables;
            public Sprite[] dealers;
        }

        [Header("Components")]
        [SerializeField] Image img_background;
        [SerializeField] SpriteRenderer spr_table;
        [SerializeField] SpriteRenderer spr_dealer;

        [Header("Environment data")]
        [SerializeField] ENVIRONMENTS[] all_maps_variation;
        [SerializeField] ENVIRONMENTS[] all_maps_diamond;

        public void Initialize()
        {
            ITABLESETTINGS selected_table = AppManager.Instance.Get_Table(TPRoomManager.Instance.State.table_id);
            ENVIRONMENTS env = System.Array.Find(selected_table.is_delux ? all_maps_diamond : all_maps_variation, x => x.game_type == TPRoomManager.Instance.GameType);

            img_background.sprite = env.backgrounds[Random.Range(0, env.backgrounds.Length)];
            spr_table.sprite = env.tables[Random.Range(0, env.tables.Length)];
            spr_dealer.sprite = env.dealers[Random.Range(0, env.dealers.Length)];
        }
    }
}