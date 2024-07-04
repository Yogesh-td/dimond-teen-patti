import { Card } from "../helpers/Card";
import { Deck } from "../helpers/Deck";
import { Rule_Joker } from "../rules/Rule_Joker";
import { RoundPlayerStates } from "../schema/RoundPlayerStates";
import { CARD_HAND_TYPE, Round_Handler } from "./Round_Handler";

export class Round_Joker extends Round_Handler {
    override Assign_Cards_To_Players() {
        const shuffledCards: Card[] = new Deck(true).cards;

        var currentCardIndex: number = 0;
        this.current_round.players.forEach((player) =>
        {
            player.cards.push(new Card(shuffledCards[currentCardIndex].c_type, 15));
            currentCardIndex++;

            for (var i = 0; i < 2; i++) {
                player.cards.push(shuffledCards[currentCardIndex]);
                currentCardIndex++;
            }
        });

        super.Assign_Cards_To_Players();
    }

    override Calculate_Winner(players: RoundPlayerStates[]): [RoundPlayerStates[], number, CARD_HAND_TYPE] {
        return new Rule_Joker().Generate_Player_Scoring(players);
    }
}