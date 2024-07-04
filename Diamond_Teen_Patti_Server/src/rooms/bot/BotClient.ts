import { Client, Room } from "colyseus.js";
import { TPRoom } from "../TPRoom";
import { METADATA_KEYS, PLAYER_INFO_KEYS, SOCKET_EVENTS } from "../../keys/EventAndKeys";
import { ROUND_STATUS } from "../schema/RoundStates";

class BotClient {

    private room: Room<TPRoom>;
    private social_id: string;
    private index_in_round: number;

    constructor(room_id: string, table_id: string, private_key: string)
    {
        this.social_id = Math.floor(Math.random() * 9999999999).toString();

        const bots_data = require("./BotData.json").bots;
        const bot_current = bots_data[Math.floor(Math.random() * bots_data.length)];

        const options = {
            [PLAYER_INFO_KEYS.table_id]: table_id,
            [PLAYER_INFO_KEYS.access_token]: bot_current.access_token.toString(),
            [PLAYER_INFO_KEYS.social_id]: this.social_id,
            [PLAYER_INFO_KEYS.username]: bot_current.username,
            [PLAYER_INFO_KEYS.avatar_index]: Math.floor(Math.random() * 21),
            [METADATA_KEYS.private_key]: private_key,
        };

        this.Join_Room(room_id, options);
    }


    private async Join_Room(room_id: string, options: any) {
        const client = new Client("ws://localhost:" + process.env.PORT);
        this.room = await client.joinById<TPRoom>(room_id, options);

        this.Register_Callbacks();
    }
    private Register_Callbacks() {
        this.room.onMessage("*", () => { });
        this.room.onStateChange.once(state =>
        {
            this.room.state.current_round.listen("status", (value, previous) => this.On_Status_Changed(value, previous));
            this.room.state.current_round.listen("turn_player_index", (value, previous) => this.On_TurnIndex_Changed(value, previous));
        });

        this.Send_LocalReady();
    }


    private On_Status_Changed(value: number, previous: number) {
        if (value == ROUND_STATUS.CollectingBoot)
            this.index_in_round = Array.from(this.room.state.current_round.players).findIndex(x => x.social_id == this.social_id);
    }
    private On_TurnIndex_Changed(value: number, previous: number) {
        if (value != this.index_in_round)
            return;

        var inputs = [];
        inputs.push(SOCKET_EVENTS.PlayerPack);
        inputs.push(SOCKET_EVENTS.PlaceBet);

        if (this.room.state.current_round.can_show)
            inputs.push(SOCKET_EVENTS.PlayerShow);

        const input_selected = Math.floor(Math.random() * inputs.length);
        this.Send_Local_SendTurn(inputs[input_selected]);
    }


    private Send_LocalReady() {
        this.room?.send(SOCKET_EVENTS.PlayerReady);
    }
    private Send_Local_SendTurn(input: any) {
        if (input == SOCKET_EVENTS.PlaceBet) {
            if (this.room.state.current_round.players[this.index_in_round].isbetlimitreached)
                this.room?.send(input, false);
            else
                this.room?.send(input, true);
        }
        else
            this.room?.send(input);
    }
}

export { BotClient }