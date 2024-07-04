using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using TeenPatti.App;
using TeenPatti.Audios;
using TeenPatti.ColyseusStates;
using TeenPatti.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Gameplay
{
    public class PlayerBehaviour : MonoBehaviour
    {
        [Header("Player UI")]
        [SerializeField] Image spr_avatar;
        [SerializeField] TextMeshProUGUI txt_username;

        [Header("Player win animation")]
        [SerializeField] Animation anim_win;

        [Header("Amount UI")]
        [SerializeField] GameObject amount_obj_parent;
        [SerializeField] TextMeshProUGUI amount_txt;

        [Header("Cards UI")]
        [SerializeField] protected Image[] cards_img;
        [SerializeField] GameObject obj_isdealer;

        [Header("Timer UI")]
        [SerializeField] GameObject timer_obj;
        [SerializeField] Image timer_img_filler;

        [Header("Chat message")]
        [SerializeField] TextMeshProUGUI chat_txt;
        [SerializeField] GameObject chat_obj;

        [Header("Gift Gifs")]
        [SerializeField] protected Button chat_gift;
        [SerializeField] GifPlayer chat_gif_player;

        [Header("Current Values")]
        [SerializeField] protected bool turn;
        [SerializeField] float turn_remainingTime;
        [SerializeField] float turn_maxTime;

        string sessionId;
        PlayerStates playerStates;

        int playerIndexInRound;
        RoundPlayerStates playerRoundStates;

        protected bool isLocal;
        bool isRecivedCards;


        public RoundPlayerStates PlayerRoundStates => playerRoundStates;
        public string SessionId => sessionId;
        public string SocialId => playerStates.social_id;
        public string Username => playerStates.username;
        public bool IsLocal => isLocal;
        public bool IsTurn => turn;
        public Vector3 Throw_Position => amount_obj_parent.transform.position;




        private void Awake()
        {
            RemovePlayer();
        }


        private void OnDestroy()
        {
            Deregister_Round_Callbacks();
        }
        private void Update()
        {
            if (!turn)
                return;

            turn_remainingTime -= Time.unscaledDeltaTime;
            if (turn_remainingTime < 0)
                turn_remainingTime = 0;

            timer_img_filler.fillAmount = turn_remainingTime / turn_maxTime;
        }


        #region Seat Reservation Methods
        public virtual void FillPlayer(string _session_id, PlayerStates _states)
        {
            sessionId = _session_id;
            playerStates = _states;

            txt_username.text = playerStates.username;
            spr_avatar.sprite = AppManager.Instance.AvatarSettings.Get_Avatar(playerStates.avatar_index);
            chat_gift?.gameObject.SetActive(true);

            Register_Round_Callbacks();
        }

        public virtual void RemovePlayer()
        {
            sessionId = "";
            playerStates = null;
            playerIndexInRound = -1;
            playerRoundStates = null;

            txt_username.text = "----";
            spr_avatar.sprite = AppManager.Instance.AvatarSettings.Get_Null_Avatar();
            chat_gift?.gameObject.SetActive(false);

            RoundManager_On_Reset_Round();
            Deregister_Round_Callbacks();
        }
        public virtual void Update_IndexInRound(int _playerIndexInRound, RoundPlayerStates _playerroundstates)
        {
            playerIndexInRound = _playerIndexInRound;
            playerRoundStates = _playerroundstates;
            obj_isdealer.SetActive(playerRoundStates.isDealer);

            Register_PlayerRoundChanges_Callbacks();
        }
        public virtual void Force_SyncStates(int turn_player_index)
        {
            RoundManager_On_Turn_Changed(turn_player_index);
        }
        public void Force_UpdateTurnTime(int elapsed_time)
        {
            turn_remainingTime = turn_maxTime - (elapsed_time / 1000f);
        }
        #endregion


        #region To perform animation methods
        public void SendToDealer_BootAmount(int boot_amount, Item_CoinAmount prefab, Vector3 dealer_pot_position)
        {
            Item_CoinAmount obj = Instantiate(prefab, amount_obj_parent.transform.position, Quaternion.identity);
            obj.Initialize(boot_amount.To_KiloFormat(), dealer_pot_position);
        }
        public virtual void ReceiveFromDealer_Card(int cardIndex, Sprite cardImage, Item_AnimCard prefab, Vector3 dealer_card_position)
        {
        }
        public void ReceiveFromDealer_Coins(int _amount, Item_CoinAmount prefab, Vector3 dealer_pot_position, Action callback = null)
        {
            Item_CoinAmount obj = Instantiate(prefab, dealer_pot_position, Quaternion.identity);
            obj.Initialize(string.Format("pot amount: ₹ {0}", _amount.To_KiloFormat()), amount_obj_parent.transform.position, callback);

            anim_win.gameObject.SetActive(true);
            AudioManager.Instance.Play_Sound(SOUNDS.WIN);
        }
        public async UniTaskVoid Received_Cards(CARD_DATA[] cards, CARD_DATA[] joker_cards)
        {
            if (isRecivedCards)
                return;

            int cardDelayMilisec = 200;
            isRecivedCards = true;
            for (int i = 0; i < cards.Length; i++)
            {
                bool isJoker = (joker_cards == null || joker_cards.Length == 0) ? false : Array.FindIndex(joker_cards, x => x.c_number == cards[i].c_number) != -1;
                Perform_CardSee_Animation(cards_img[i], cards[i].Get_Sprite(), isJoker);
                await UniTask.Delay(cardDelayMilisec);
            }
        }
        public virtual void Round_Ends()
        {
            turn = false;
        }
        protected virtual void Perform_CardSee_Animation(Image card_img, Sprite card_spr, bool isJoker)
        {
        }
        #endregion


        #region Round Register Callbacks
        private void Register_Round_Callbacks()
        {
            RoundManager.Event_Turn_Changed += RoundManager_On_Turn_Changed;
            RoundManager.Event_Winner_Declared += Round_Ends;
            RoundManager.Event_Reset_Player_For_Round += RoundManager_On_Reset_Round;
        }
        private void Deregister_Round_Callbacks()
        {
            RoundManager.Event_Turn_Changed -= RoundManager_On_Turn_Changed;
            RoundManager.Event_Winner_Declared -= Round_Ends;
            RoundManager.Event_Reset_Player_For_Round -= RoundManager_On_Reset_Round;
        }

        private void Register_PlayerRoundChanges_Callbacks()
        {
            PlayerRoundStates.OnLast_bet_amountChange(PlayerRoundStates_On_LastBetAmount_Changed);
            PlayerRoundStates.OnIsblindChange(PlayerRoundStates_On_IsBlind_Changed);
            PlayerRoundStates.OnIspackChange(PlayerRoundStates_On_IsPack_Changed);
            PlayerRoundStates.OnIsDealerChange(PlayerRoundStates_On_IsDealer_Changed);
        }



        protected virtual void RoundManager_On_Turn_Changed(int round_index)
        {
            turn_maxTime = TPRoomManager.Instance.State.turn_time;
            turn_remainingTime = turn_maxTime;

            turn = round_index == playerIndexInRound;
            timer_obj.SetActive(turn);

            PlayerRoundStates_On_IsBlind_Changed(playerRoundStates.isblind, !playerRoundStates.isblind);

            if (turn)
                AudioManager.Instance.Play_Sound(SOUNDS.TURN);
        }
        protected virtual void RoundManager_On_Reset_Round()
        {
            turn = false;
            isRecivedCards = false;
            timer_obj.gameObject.SetActive(false);
            amount_obj_parent.gameObject.SetActive(false);
            obj_isdealer.SetActive(false);
            anim_win.gameObject.SetActive(false);

            foreach (var item in cards_img)
            {
                item.gameObject.SetActive(false);
                item.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        private void PlayerRoundStates_On_LastBetAmount_Changed(int currentValue, int previousValue)
        {
            amount_txt.text = currentValue.To_KiloFormat();
            amount_obj_parent.gameObject.SetActive(true);
        }
        protected virtual void PlayerRoundStates_On_IsBlind_Changed(bool currentValue, bool previousValue)
        {
        }
        protected virtual void PlayerRoundStates_On_IsPack_Changed(bool currentValue, bool previousValue)
        {
        }
        private void PlayerRoundStates_On_IsDealer_Changed(bool currentValue, bool previousValue)
        {
            obj_isdealer.SetActive(currentValue);
        }
        #endregion


        #region Chat and Gifts
        public void Received_Chat(string msg)
        {
            AudioManager.Instance.Play_Sound(SOUNDS.RECEIVED_CHAT);

            StopCoroutine(nameof(Show_Chat));
            chat_txt.text = msg;
            StartCoroutine(nameof(Show_Chat));
        }
        IEnumerator Show_Chat()
        {
            chat_obj.SetActive(true);
            yield return new WaitForSeconds(3f);
            chat_obj.SetActive(false);
        }
        public void Received_Gift(int gif_index)
        {
            AudioManager.Instance.Play_Sound(SOUNDS.RECEIVED_CHAT);
            chat_gif_player.transform.parent.gameObject.SetActive(true);
            chat_gif_player.Apply_Gif(AppManager.Instance.Chat_Settings.Gift_Gifs[gif_index], () =>
            {
                chat_gif_player.transform.parent.gameObject.SetActive(false);
            });
        }
        #endregion
    }
}