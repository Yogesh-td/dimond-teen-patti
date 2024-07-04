using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TeenPatti.ColyseusStates;

namespace TeenPatti.Gameplay
{
    public class RoundManager
    {
        public static event Action<int> Event_Turn_Changed;
        public static event Action<int> Event_PotAmount_Changed;
        public static event Action Event_Winner_Declared;
        public static event Action Event_Reset_Player_For_Round;

        RoundStates round_state;
        List<PlayerBehaviour> round_player_behaviours = new List<PlayerBehaviour>();

        public sbyte round_status => round_state.status;
        public bool local_in_round => round_player_behaviours.Exists(x => x.IsLocal);

        public RoundManager(RoundStates state, TableDealerManager tableDealerManager)
        {
            round_state = state;

            if (round_state.status >= ROUND_STATUS.DistributingCards)
            {
                tableDealerManager.Handle_Status_Visibility(false);
                tableDealerManager.Handle_Status_PotAmount(true);

                Assign_RoundPlayer_Indexes();
                foreach (var item in round_player_behaviours)
                    item.Force_SyncStates(round_state.turn_player_index);

                if (!string.IsNullOrEmpty(round_state.joker_card_data))
                    tableDealerManager.Receive_Joker_Card(round_state.joker_card_data);

                TPRoomManager.Instance.Send_Local_RequestTurnRemainingTime();
            }
            else
            {
                tableDealerManager.Handle_Status_Visibility(true);
                tableDealerManager.Handle_Status_PotAmount(false);
            }

            round_state.OnStatusChange(On_Status_Changed);
            round_state.OnTurn_player_indexChange(On_TurnPlayerIndex_Changed);
            round_state.OnPot_amountChange(On_PotAmount_Changed);
        }


        public void On_Round_Sideshow_Processing()
        {
            Event_Turn_Changed?.Invoke(-1);
        }
        public CARD_DATA[] Get_Joker_Card()
        {
            if (TPRoomManager.Instance.GameType == App.GAME_TYPE.AK47)
            {
                List<CARD_DATA> joker_cards = new List<CARD_DATA>();
                joker_cards.Add(new CARD_DATA() { c_type = 0, c_number = 14 });
                joker_cards.Add(new CARD_DATA() { c_type = 0, c_number = 13 });
                joker_cards.Add(new CARD_DATA() { c_type = 0, c_number = 4 });
                joker_cards.Add(new CARD_DATA() { c_type = 0, c_number = 7 });

                return joker_cards.ToArray();
            }
            else if (TPRoomManager.Instance.GameType == App.GAME_TYPE.HUKAM)
            {
                if (round_state == null || string.IsNullOrEmpty(round_state.joker_card_data))
                    return null;
                else
                {
                    string[] card_data = round_state.joker_card_data.Split("_");

                    List<CARD_DATA> joker_cards = new List<CARD_DATA>();
                    joker_cards.Add(new CARD_DATA() { c_type = int.Parse(card_data[0]), c_number = int.Parse(card_data[1]) });

                    return joker_cards.ToArray();
                }
            }
            else
                return null;
        }



        private void On_Status_Changed(sbyte currentValue, sbyte previousValue)
        {
            switch ((int)currentValue) 
            {
                case ROUND_STATUS.CollectingBoot:
                    Assign_RoundPlayer_Indexes();
                    Collect_Boot();
                    break;
                case ROUND_STATUS.DistributingCards:
                    Distribute_Cards();
                    break;
                case ROUND_STATUS.WinnerDeclared:
                    Event_Winner_Declared?.Invoke();
                    break;
                case ROUND_STATUS.Reset:
                    round_player_behaviours.Clear();
                    Event_Reset_Player_For_Round?.Invoke();
                    break;
            }
        }
        private void On_TurnPlayerIndex_Changed(int currentValue, int previousValue)
        {
            if (round_state.status != ROUND_STATUS.TurnRunning)
                return;

            Event_Turn_Changed?.Invoke(currentValue);
        }
        private void On_PotAmount_Changed(int currentValue, int previousValue)
        {
            Event_PotAmount_Changed?.Invoke(currentValue);
        }




        private void Assign_RoundPlayer_Indexes()
        {
            int current_index = 0;

            PlayerBehaviour[] alltableBehaviours = GameManager.Instance.All_Table_Players;
            round_state.players.ForEach(player => 
            {
                PlayerBehaviour playerbehaviour = Array.Find(alltableBehaviours, x => x.SocialId == player.social_id);
                playerbehaviour.Update_IndexInRound(current_index, player);

                round_player_behaviours.Add(playerbehaviour);
                current_index++;
            });
        }
        private void Collect_Boot()
        {
            GameManager.Instance.Collect_BootAmount(round_player_behaviours, round_state.pot_amount / round_player_behaviours.Count).Forget();
        }
        private void Distribute_Cards()
        {
            GameManager.Instance.Distribute_Cards(round_player_behaviours, round_state.joker_card_data).Forget();
        }
    }
}