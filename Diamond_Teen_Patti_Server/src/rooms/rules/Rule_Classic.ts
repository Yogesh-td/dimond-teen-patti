import { CARD_HAND_TYPE } from "../game/Round_Handler";
import { RoundPlayerStates } from "../schema/RoundPlayerStates";
import { ScoreGenerator } from "./ScoreGenerator";

export class Rule_Classic
{
    //constructor() {
    //    var players: RoundPlayerStates[] = [];

    //    var cards: Card[] = [];
    //    cards.push(new Card(0, 14));
    //    cards.push(new Card(1, 4));
    //    cards.push(new Card(2, 10));
    //    players.push(new RoundPlayerStates().assign(
    //        {
    //            cards: cards
    //        }));

    //    console.log(cards);

    //    cards = [];
    //    cards.push(new Card(0, 10));
    //    cards.push(new Card(1, 6));
    //    cards.push(new Card(2, 12));
    //    players.push(new RoundPlayerStates().assign(
    //        {
    //            cards: cards
    //        }));

    //    console.log(cards);

    //    const result = this.Generate_Player_Scoring(players);
    //    console.log(result[1]);
    //    console.log(result[2]);
    //}


    Generate_Player_Scoring(players: RoundPlayerStates[]): [RoundPlayerStates[], number, CARD_HAND_TYPE] {
        var winners: RoundPlayerStates[] = [];

        var current_hand_type: CARD_HAND_TYPE = CARD_HAND_TYPE.none;
        var current_highest_score: number = 0;

        players.forEach(player => {
            const temp_card_data = new ScoreGenerator().Evaluate_Cards(player.cards);

            var player_score: number = temp_card_data[0];
            var player_hand_type: CARD_HAND_TYPE = temp_card_data[1];

            if (player_score == current_highest_score)
                winners.push(player);
            else if (player_score > current_highest_score) {
                current_highest_score = player_score;
                current_hand_type = player_hand_type;

                winners = [];
                winners.push(player);
            }
        });

        return [winners, current_highest_score, current_hand_type];
    }
}