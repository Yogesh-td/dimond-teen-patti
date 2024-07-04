using Global.Helpers;
using TeenPatti.Audios;
using TeenPatti.ColyseusStates;
using TeenPatti.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Gameplay
{
    public class Player_Local : PlayerBehaviour
    {
        [Header("----Override Fields----")]
        [SerializeField] Button cards_btn_see;
        [SerializeField] GameObject obj_pack_tag;


        private void Start()
        {
            base.isLocal = true;
            cards_btn_see.onClick.AddListener(OnClick_SeeCards);
        }


        private void OnClick_SeeCards()
        {
            Handle_CardSee_Button(false);
            TPRoomManager.Instance.Send_Local_RequestToSeeCards();
        }


        private void Handle_CardSee_Button(bool value)
        {
            if (value && TPRoomManager.Instance.GameType == App.GAME_TYPE.POTBLIND)
                return;

            cards_btn_see?.gameObject.SetActive(value);
        }
        private void Handle_PackTag(bool value)
        {
            obj_pack_tag.SetActive(value);
        }


        public override void FillPlayer(string _session_id, PlayerStates _states)
        {
            base.FillPlayer(_session_id, _states);
            ScreenManager.Instance.ShowScreen(SCREEN_TYPE.GAMEPLAY_LOCAL);
        }
        public override void RemovePlayer()
        {
            ScreenManager.Instance.HideScreen(SCREEN_TYPE.GAMEPLAY_LOCAL);
            base.RemovePlayer();
        }
        public override void Update_IndexInRound(int _playerIndexInRound, RoundPlayerStates _playerroundstates)
        {
            base.Update_IndexInRound(_playerIndexInRound, _playerroundstates);

            Screen_Gameplay_Local screen_gameplaylocal = (Screen_Gameplay_Local)ScreenManager.Instance.Get_Screen(SCREEN_TYPE.GAMEPLAY_LOCAL);
            screen_gameplaylocal.Initialize_LocalPlayer(_playerroundstates);
        }
        public override void Force_SyncStates(int turn_player_index)
        {
            foreach (var item in cards_img)
            {
                item.gameObject.SetActive(true);
                item.transform.parent.gameObject.SetActive(true);
            }

            base.Force_SyncStates(turn_player_index);

            if (!PlayerRoundStates.isblind)
                OnClick_SeeCards();
        }
        public override void Round_Ends()
        {
            base.Round_Ends();
            Handle_CardSee_Button(false);
        }


        public override void ReceiveFromDealer_Card(int cardIndex, Sprite cardImage, Item_AnimCard prefab, Vector3 dealer_card_position)
        {
            Animation parent_anim = cards_img[cardIndex].transform.parent.GetComponent<Animation>();

            parent_anim.gameObject.SetActive(true);
            parent_anim.clip = parent_anim.GetClip("card" + (cardIndex + 1));
            parent_anim.Play();

            cards_img[cardIndex].sprite = cardImage;
            cards_img[cardIndex].gameObject.SetActive(true);
        }
        protected override void Perform_CardSee_Animation(Image card_img, Sprite card_spr, bool isJoker)
        {
            base.Perform_CardSee_Animation(card_img, card_spr, isJoker);

            Animation parent_anim = card_img.transform.parent.GetComponent<Animation>();
            parent_anim.clip = parent_anim.GetClip("see_card_" + (parent_anim.gameObject.name.Contains("2") ? "centre" : "side"));
            parent_anim.Play();

            Timer.Schedule(this, parent_anim.clip.length / 2f, () =>
            {
                AudioManager.Instance.Play_Sound(SOUNDS.CARD_FLIP);

                card_img.sprite = card_spr;
                card_img.transform.GetChild(0).gameObject.SetActive(isJoker);
            });
        }


        protected override void RoundManager_On_Turn_Changed(int round_index)
        {
            if (PlayerRoundStates == null)
                return;

            base.RoundManager_On_Turn_Changed(round_index);
            Handle_CardSee_Button(PlayerRoundStates.isblind);

            Screen_Gameplay_Local screen_gameplaylocal = (Screen_Gameplay_Local)ScreenManager.Instance.Get_Screen(SCREEN_TYPE.GAMEPLAY_LOCAL);
            if (turn)
                screen_gameplaylocal.Show_Turn_Panel();
            else
                screen_gameplaylocal.Hide_Turn_Panel();
        }
        protected override void RoundManager_On_Reset_Round()
        {
            base.RoundManager_On_Reset_Round();
            Handle_CardSee_Button(false);
            Handle_PackTag(false);
        }


        protected override void PlayerRoundStates_On_IsBlind_Changed(bool currentValue, bool previousValue)
        {
            if (PlayerRoundStates.ispack)
                return;

            base.PlayerRoundStates_On_IsBlind_Changed(currentValue, previousValue);
            if (!currentValue)
                Handle_CardSee_Button(false);
        }
        protected override void PlayerRoundStates_On_IsPack_Changed(bool currentValue, bool previousValue)
        {
            if (!currentValue)
                return;

            base.PlayerRoundStates_On_IsPack_Changed(currentValue, previousValue);

            Handle_CardSee_Button(false);
            Handle_PackTag(true);

            Screen_Gameplay_Local screen_gameplaylocal = (Screen_Gameplay_Local)ScreenManager.Instance.Get_Screen(SCREEN_TYPE.GAMEPLAY_LOCAL);
            screen_gameplaylocal.Hide_Turn_Panel();
        }
    }
}