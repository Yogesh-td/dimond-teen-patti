import { CARD_HAND_TYPE } from "../game/Round_Handler";
import { Card } from "../helpers/Card";
import { Deck } from "../helpers/Deck";
import { RoundPlayerStates } from "../schema/RoundPlayerStates";
import { ScoreGenerator } from "./ScoreGenerator";

export class Rule_AK47 {
    //constructor() {
    //    var cards: Card[] = [];
    //    cards.push(new Card(0, 3));
    //    cards.push(new Card(1, 10));
    //    cards.push(new Card(2, 10));

    //    console.log(cards);

    //    this.Convert_Jokers(cards, new Card(2, 10));

    //    console.log(cards);
    //}


    Generate_Player_Scoring(players: RoundPlayerStates[]): [RoundPlayerStates[], number, CARD_HAND_TYPE] {
        var winners: RoundPlayerStates[] = [];

        var current_hand_type: CARD_HAND_TYPE = CARD_HAND_TYPE.none;
        var current_highest_score: number = 0;

        players.forEach(player => {
            const convertedCards = this.Convert_Jokers(Array.from(player.cards));
            const temp_card_data = new ScoreGenerator().Evaluate_Cards(convertedCards);

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
    private Convert_Jokers(cards: Card[]): Card[]
    {
        const total_jokers_in_cards = cards.filter(x => (x.c_number == 14 || x.c_number == 13 || x.c_number == 4 || x.c_number == 7)).length;

        if (total_jokers_in_cards == 3) {
            for (var i = 0; i < cards.length; i++) {
                cards[i].c_number = 14;
            }
        }
        else if (total_jokers_in_cards == 2) {
            const non_joker_card: Card = cards.find(x => (x.c_number != 14 && x.c_number != 13 && x.c_number != 4 && x.c_number != 7));
            for (var i = 0; i < cards.length; i++) {
                if (cards[i] != non_joker_card) {
                    cards[i].c_number = non_joker_card.c_number;
                }
            }
        }
        else if (total_jokers_in_cards == 1) {
            const joker_player_card_index = cards.findIndex(x => (x.c_number == 14 || x.c_number == 13 || x.c_number == 4 || x.c_number == 7));
            const newDeck: Card[] = new Deck(false).cards;

            var original_joker_card: Card = cards[joker_player_card_index];
            var to_update_card: Card = null;
            var current_score: number = 0;

            for (var i = 0; i < newDeck.length; i++) {
                cards[joker_player_card_index] = newDeck[i];
                const score = new ScoreGenerator().Evaluate_Cards(cards)[0];

                if (score > current_score || (score == current_score && newDeck[i].c_type == original_joker_card.c_type)) {
                    to_update_card = newDeck[i];
                    current_score = score;
                }
            }

            if (to_update_card != null)
                cards[joker_player_card_index] = to_update_card;
        }

        return cards;
    }
}