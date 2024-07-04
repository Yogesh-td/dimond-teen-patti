import { Schema, MapSchema, ArraySchema, type } from "@colyseus/schema";
import { PlayerStates } from "./PlayerStates";
import { RoundStates } from "./RoundStates";

export class TPRoomStates extends Schema
{
    @type("string") table_id: string = "";

    @type({ map: PlayerStates }) all_players = new MapSchema<PlayerStates>();
    @type(["string"]) all_seats = new ArraySchema<string>;

    @type("int32") turn_time: number = 0;
    @type(RoundStates) current_round = new RoundStates();


    Initialize(maxplayers: number) {
        for (var i = 0; i < maxplayers; i++) {
            this.all_seats.push("");
        }
    }
    Add_Player(sessionid: string, player: PlayerStates) {
        this.all_players.set(sessionid, player);
        for (var i = 0; i < this.all_seats.length; i++) {
            if (this.all_seats[i] == null || this.all_seats[i] == "") {
                this.all_seats[i] = sessionid;
                break;
            }
        }
    }
    Remove_Player(sessionid: string) {
        this.all_players.delete(sessionid);
        for (var i = 0; i < this.all_seats.length; i++) {
            if (this.all_seats[i] == sessionid) {
                this.all_seats[i] = "";
                break;
            }
        }
    }
    Contains_Player(access_token: string): boolean
    {
        const player_index = Array.from(this.all_players.values()).findIndex(x => x.access_token == access_token);
        return player_index == -1 ? false : true;
    }
}