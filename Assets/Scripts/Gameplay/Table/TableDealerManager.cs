using Global.Helpers;
using TeenPatti.App;
using TeenPatti.App.Settings;
using TeenPatti.Gameplay.Effects;
using TeenPatti.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Gameplay
{
    public class TableDealerManager : MonoBehaviour
    {
        [Header("Out Table Components")]
        [SerializeField] Transform trs_deckPoint;

        [Header("On Table Components")]
        [SerializeField] TextMeshProUGUI txt_potamount;
        [SerializeField] TextMeshProUGUI txt_status;
        [SerializeField] Image img_joker;

        [Header("Electric Effect")]
        [SerializeField] Sideshow_Effect[] sideshow_effects;

        int animated_pot_amount;

        public Vector2 PotAmount_WorldPosition
        {
            get => txt_potamount.transform.position;
        }
        public Vector3 Deck_WorldPosition
        {
            get => trs_deckPoint.position;
        }


        private void Start()
        {
            RoundManager.Event_PotAmount_Changed += RoundManager_Event_PotAmount_Changed;
            RoundManager.Event_Reset_Player_For_Round += RoundManager_Event_Reset_Player_For_Round;
        }

        private void OnDestroy()
        {
            RoundManager.Event_PotAmount_Changed -= RoundManager_Event_PotAmount_Changed;
            RoundManager.Event_Reset_Player_For_Round -= RoundManager_Event_Reset_Player_For_Round;
        }


        private void RoundManager_Event_PotAmount_Changed(int updatedValue)
        {
            iTween.ValueTo(this.gameObject, iTween.Hash
                (
                    "from", animated_pot_amount,
                    "to", updatedValue,
                    "time", CoreSettings.Instance.Card_ThrowTime,
                    "easetype", iTween.EaseType.easeOutQuad,
                    "onupdate", nameof(Update_Animated_PotAmount)
                ));
        }
        private void Update_Animated_PotAmount(int value)
        {
            animated_pot_amount = value;
            txt_potamount.text = string.Format("pot amount: ₹ {0}", animated_pot_amount.To_KiloFormat());
        }
        private void RoundManager_Event_Reset_Player_For_Round()
        {
            img_joker.gameObject.SetActive(false);
            Update_Status("Waiting for server to start game!");
        }



        public void Update_Status(string msg)
        {
            txt_status.text = msg;
        }
        public void Handle_Status_Visibility(bool value)
        {
            txt_status.transform.parent.gameObject.SetActive(value);
        }


        public void Handle_Status_PotAmount(bool value)
        {
            txt_potamount.transform.parent.gameObject.SetActive(value);
        }


        public void Receive_Joker_Card(string cardData, Item_AnimCard prefab)
        {
            Item_AnimCard obj = Instantiate(prefab, Deck_WorldPosition, prefab.transform.rotation);
            obj.Initialize(img_joker.transform.position, () =>
            {
                img_joker.sprite = AppManager.Instance.DeckSettings.Card_Back_Sprite;
                img_joker.gameObject.SetActive(true);

                img_joker.transform.localScale = new Vector3(-1, 1, 1);
                iTween.ScaleTo(img_joker.gameObject, iTween.Hash
                    (
                        "x", 1,
                        "time", CoreSettings.Instance.Card_ThrowTime / 2f,
                        "delay", CoreSettings.Instance.Card_ThrowTime / 2f,
                        "easetype", iTween.EaseType.easeInOutQuad
                    ));

                Timer.Schedule(this, CoreSettings.Instance.Card_ThrowTime * 0.75f, () =>
                {
                    string[] card_processed_data = cardData.Split("_");
                    img_joker.sprite = AppManager.Instance.DeckSettings.Get_Card_Sprite(int.Parse(card_processed_data[0]), int.Parse(card_processed_data[1]));
                });
            });
        }
        public void Receive_Joker_Card(string cardData)
        {
            string[] card_processed_data = cardData.Split("_");
            img_joker.sprite = AppManager.Instance.DeckSettings.Get_Card_Sprite(int.Parse(card_processed_data[0]), int.Parse(card_processed_data[1]));

            img_joker.gameObject.SetActive(true);
        }





        public void Sideshow_Effect_Show(Vector3 start_pos, Vector3 end_pos)
        {
            foreach (var item in sideshow_effects)
                item.Init_Electric(start_pos, end_pos);
        }
        public void Sideshow_Effect_Hide()
        {
            foreach (var item in sideshow_effects)
                item.Cancel_Electic();
        }
    }
}