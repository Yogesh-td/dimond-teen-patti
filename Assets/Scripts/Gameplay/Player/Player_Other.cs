using Global.Helpers;
using TeenPatti.App.Settings;
using TeenPatti.Screens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Gameplay
{
    public class Player_Other : PlayerBehaviour
    {
        [Header("----Override Fields----")]
        [SerializeField] TextMeshProUGUI cards_txt_status;

        private void Start()
        {
            base.chat_gift.onClick.AddListener(OnClick_Gift);
        }


        private void OnClick_Gift()
        {
            ((Screen_SendGift)ScreenManager.Instance.Get_Screen(SCREEN_TYPE.GIFTS)).Update_Receiver(this);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.GIFTS);
        }



        public override void Force_SyncStates(int turn_player_index)
        {
            foreach (var item in cards_img)
                item.gameObject.SetActive(true);

            base.Force_SyncStates(turn_player_index);
        }

        protected override void RoundManager_On_Turn_Changed(int round_index)
        {
            if (PlayerRoundStates == null)
                return;

            base.RoundManager_On_Turn_Changed(round_index);
            if (cards_txt_status != null)
                cards_txt_status.transform.parent.gameObject.SetActive(true);
        }
        protected override void RoundManager_On_Reset_Round()
        {
            base.RoundManager_On_Reset_Round();
            if (cards_txt_status != null)
                cards_txt_status.transform.parent.gameObject.SetActive(false);
        }


        public override void ReceiveFromDealer_Card(int cardIndex, Sprite cardImage, Item_AnimCard prefab, Vector3 dealer_card_position)
        {
            base.ReceiveFromDealer_Card(cardIndex, cardImage, prefab, dealer_card_position);

            Item_AnimCard obj = Instantiate(prefab, dealer_card_position, prefab.transform.rotation);
            obj.Initialize(cards_img[cardIndex].transform.position, () =>
            {
                cards_img[cardIndex].sprite = cardImage;
                cards_img[cardIndex].gameObject.SetActive(true);
            });
        }
        protected override void Perform_CardSee_Animation(Image card_img, Sprite card_spr, bool isJoker)
        {
            base.Perform_CardSee_Animation(card_img, card_spr, isJoker);

            card_img.transform.localScale = new Vector3(-1, 1, 1);
            iTween.ScaleTo(card_img.gameObject, iTween.Hash
                (
                    "x", 1,
                    "time", CoreSettings.Instance.Card_ThrowTime,
                    "easetype", iTween.EaseType.easeInOutQuint
                ));
            Timer.Schedule(this, CoreSettings.Instance.Card_ThrowTime / 2f, () =>
            {
                card_img.sprite = card_spr;
                card_img.transform.GetChild(0).gameObject.SetActive(isJoker);
            });
        }


        protected override void PlayerRoundStates_On_IsBlind_Changed(bool currentValue, bool previousValue)
        {
            if (PlayerRoundStates.ispack)
                return;

            base.PlayerRoundStates_On_IsBlind_Changed(currentValue, previousValue);
            if (cards_txt_status != null)
                cards_txt_status.text = currentValue ? "Blind" : "Seen";
        }
        protected override void PlayerRoundStates_On_IsPack_Changed(bool currentValue, bool previousValue)
        {
            if (!currentValue)
                return;

            base.PlayerRoundStates_On_IsPack_Changed(currentValue, previousValue);
            if (cards_txt_status != null)
                cards_txt_status.text = "Pack";
        }
    }
}