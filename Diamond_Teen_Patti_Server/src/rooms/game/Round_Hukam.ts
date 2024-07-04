import { Card } from "../helpers/Card";
import { Deck } from "../helpers/Deck";
import { Rule_Hukam } from "../rules/Rule_Hukam";
import { RoundPlayerStates } from "../schema/RoundPlayerStates";
import { CARD_HAND_TYPE, Round_Handler } from "./Round_Handler";

export class Round_Hukam extends Round_Handler
{
    override Assign_Cards_To_Players() {
        const shuffledCards: Card[] = new Deck(true).cards;

        var currentCardIndex: number = 0;
        this.current_round.players.forEach((player) => {
            for (var i = 0; i < 3; i++) {
                player.cards.push(shuffledCards[currentCardIndex]);
                currentCardIndex++;
            }
        });

        this.current_round.Assign_Joker_Card(shuffledCards[currentCardIndex]);
        super.Assign_Cards_To_Players();
    }

    override Calculate_Winner(players: RoundPlayerStates[]): [RoundPlayerStates[], number, CARD_HAND_TYPE] {
        return new Rule_Hukam().Generate_Player_Scoring(players, this.current_round.joker_card);
    }
}