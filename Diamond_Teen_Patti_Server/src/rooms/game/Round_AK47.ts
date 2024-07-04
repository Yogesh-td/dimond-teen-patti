import { Card } from "../helpers/Card";
import { Deck } from "../helpers/Deck";
import { Rule_AK47 } from "../rules/Rule_AK47";
import { RoundPlayerStates } from "../schema/RoundPlayerStates";
import { CARD_HAND_TYPE, Round_Handler } from "./Round_Handler";

export class Round_AK47 extends Round_Handler {
    override Assign_Cards_To_Players() {
        const shuffledCards: Card[] = new Deck(true).cards;

        var currentCardIndex: number = 0;
        this.current_round.players.forEach((player) => {
            for (var i = 0; i < 3; i++) {
                player.cards.push(shuffledCards[currentCardIndex]);
                currentCardIndex++;
            }
        });

        super.Assign_Cards_To_Players();
    }

    override Calculate_Winner(players: RoundPlayerStates[]): [RoundPlayerStates[], number, CARD_HAND_TYPE] {
        return new Rule_AK47().Generate_Player_Scoring(players);
    }
}