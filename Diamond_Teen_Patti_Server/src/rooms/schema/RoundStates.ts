import { Schema, ArraySchema, type } from "@colyseus/schema";
import { RoundPlayerStates } from "./RoundPlayerStates";
import { Card } from "../helpers/Card";

export class RoundStates extends Schema {

    @type("int8") status: ROUND_STATUS = ROUND_STATUS.none;

    @type([RoundPlayerStates]) players = new ArraySchema<RoundPlayerStates>;
    @type("int32") pot_amount: number = 0;
    @type("int32") turn_player_index: number = -1;
    @type("boolean") can_show: boolean = false;

    @type("string") joker_card_data: string = "";

    joker_card: Card = null;
    side_show_status: SIDE_SHOW_STATUS = new SIDE_SHOW_STATUS();

    Reset_Round() {
        this.status = ROUND_STATUS.Reset;
        this.players.clear();
        this.pot_amount = 0;
        this.turn_player_index = -1;
        this.can_show = false;
        this.joker_card_data = "";

        this.Sideshow_Clear();
    }

    Sideshow_Create(sender: string, receiver: string) {
        this.side_show_status.sender_sid = sender;
        this.side_show_status.receiver_sid = receiver;
        this.side_show_status.isProcessingSideshow = true;
    }
    Sideshow_Clear() {
        this.side_show_status = new SIDE_SHOW_STATUS();
    }

    Assign_Joker_Card(card: Card)
    {
        this.joker_card = card;
        this.joker_card_data = this.joker_card.c_type + "_" + this.joker_card.c_number;
    }
}

export enum ROUND_STATUS {
    none = 0,
    CollectingBoot = 1,
    DistributingCards = 2,
    TurnRunning = 3,
    WinnerDeclared = 4,
    Reset = 5
}
export enum WIN_REASON {
    OTHER_LEFT = 0,
    REQUESTED_SHOW = 1,
    PLAYER_PACKED = 2,
    POT_LIMIT_REACHED = 3,
}


export class SIDE_SHOW_STATUS {
    isProcessingSideshow: boolean = false;

    sender_sid: string = "";
    receiver_sid: string = "";
}