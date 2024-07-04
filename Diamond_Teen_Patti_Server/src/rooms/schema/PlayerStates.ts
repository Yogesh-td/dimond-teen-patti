import { Schema, type } from "@colyseus/schema";
import { Client, Deferred } from "colyseus";

export class PlayerStates extends Schema {

    sessionid: string = "";
    access_token: string = "";
    is_bot: boolean = false;

    @type("string") social_id: string = "";
    @type("string") username: string = "";
    @type("int32") avatar_index: number = 0;

    balance: number = 0;
    session_profit: number = 0;

    is_ready_toplay: boolean = false;
    is_disconnected: boolean = false;
    requested_leave_type: number = 0;

    reconnection_caller: Deferred<Client>;
}