import { Schema, type } from "@colyseus/schema";
import { Card } from "../helpers/Card";

export class RoundPlayerStates extends Schema {
    sessionId: string = "";
    is_bot: boolean = false;
    @type("string") social_id: string = "";

    @type("int32") balance: number = 0;
    @type("int32") current_bet_amount: number;
    @type("int32") last_bet_amount: number;

    @type("boolean") isDealer: boolean = false;
    @type("boolean") isblind: boolean = true;
    @type("boolean") isbetlimitreached: boolean = false;
    @type("boolean") ispack: boolean = false;
    @type("boolean") cansideshow: boolean = false;

    cards: Card[] = [];
    total_turns: number = 0;
    is_left_game: Boolean = false;


    IsFinished(): boolean {
        return this.ispack;
    }
}