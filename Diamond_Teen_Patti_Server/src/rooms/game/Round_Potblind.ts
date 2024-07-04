import { Client } from "colyseus";
import { Card } from "../helpers/Card";
import { Deck } from "../helpers/Deck";
import { Rule_Classic } from "../rules/Rule_Classic";
import { RoundPlayerStates } from "../schema/RoundPlayerStates";
import { CARD_HAND_TYPE, Round_Handler } from "./Round_Handler";

export class Round_Potblind extends Round_Handler
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

        super.Assign_Cards_To_Players();
    }

    override Player_RequestTo_SeeCards(playerSessionId: string, playerSocket: Client){
    }
    override Player_RequestTo_Sideshow(playerSessionId: string) {
    }

    override Calculate_Winner(players: RoundPlayerStates[]): [RoundPlayerStates[], number, CARD_HAND_TYPE] {
        return new Rule_Classic().Generate_Player_Scoring(players);
    }
}