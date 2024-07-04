import { Client, Delayed, Room, ServerError, updateLobby } from "colyseus";
import { TPRoomStates } from "./schema/TPRoomStates";
import { Setting_Route } from "../mongodb/routes/settings.router";
import { METADATA_KEYS, PLAYER_INFO_KEYS, SOCKET_EVENTS } from "../keys/EventAndKeys";
import { PlayerStates } from "./schema/PlayerStates";
import { User_Route } from "../mongodb/routes/user.router";
import { ROUND_STATUS } from "./schema/RoundStates";
import { Round_Handler } from "./game/Round_Handler";
import { Transaction_Route } from "../mongodb/routes/transaction.router";
import { Round_Classic } from "./game/Round_Classic";
import { Round_Hukam } from "./game/Round_Hukam";
import { Round_AK47 } from "./game/Round_AK47";
import { Round_Joker } from "./game/Round_Joker";
import { Round_Muflis } from "./game/Round_Muflis";
import { Round_Royal } from "./game/Round_Royal";
import { Round_Potblind } from "./game/Round_Potblind";
import { AES_Crypto } from "../mongodb/aes_crypto";
import { BotClient } from "./bot/BotClient";
import { Schema, ArraySchema } from '@colyseus/schema';
import { logger } from "../mongodb/logger";

export class TPRoom extends Room<TPRoomStates>
{
    room_key: string = "";

    app_settings = new Setting_Route();
    user_settings = new User_Route();
    transaction_settings = new Transaction_Route();
    aes_encryption = new AES_Crypto();

    table_info: Table_Info;
    turn_time: number = 30;
    sideshow_time: number = 10;
    bot_add_time: number = 3;

    lastRound_dealer_sessionid: string;
    round_handler: Round_Handler;

    bot_join_time: Delayed;
    main_thread_time: Delayed;


    onCreate(options: any)
    {
        this.autoDispose = false;
        this.maxClients = 5;

        if (options[METADATA_KEYS.private_key] != "")
            this.room_key = this.aes_encryption.Decrypt(options[METADATA_KEYS.private_key]);

        this.setMetadata(
            {
                [METADATA_KEYS.current_users]: 0,
                [METADATA_KEYS.max_users]: this.maxClients,
                [METADATA_KEYS.private_key]: this.room_key
            }).then(() => updateLobby(this));

        this.setState(new TPRoomStates().assign(
        {
                turn_time: this.turn_time
        }));
        this.state.Initialize(this.maxClients);

        this.RegisterMessageHandlers();
        new logger("TPRoom created! RoomId: " + this.roomId + " Variation: " + this.roomName);
    }

    async onAuth(client: Client, options: any)
    {
        const fetched_setting = await this.app_settings.Get_Game_Settings();
        const fetched_table = await this.app_settings.Get_Table(options[PLAYER_INFO_KEYS.table_id]);
        var fetched_user = await this.user_settings.Get_User(options[PLAYER_INFO_KEYS.social_id], options[PLAYER_INFO_KEYS.access_token]);

        var isBot: boolean = false;
        if (!fetched_user) {
            const bot_data = require("./bot/BotData.json").bots;
            for (var i = 0; i < bot_data.length; i++) {
                if (bot_data[i].access_token == options[PLAYER_INFO_KEYS.access_token]) {
                    isBot = true;
                    break;
                }
            }
        }

        if (isBot) {
            client.userData = { balance: 9999999999999, is_bot : true };
        }
        else {
            var received_room_key = options[METADATA_KEYS.private_key];
            if (received_room_key != "") {
                try {
                    received_room_key = this.aes_encryption.Decrypt(received_room_key);
                } catch (e) {
                    throw new ServerError(401, "401:" + (e as Error).message);
                }
            }

            if (!fetched_table)
                throw new ServerError(401, "401:Requested table not found!");
            else if (!fetched_user)
                throw new ServerError(401, "401:Invalid or unauthorized user!");
            else if (this.state.table_id != "" && this.state.table_id != fetched_table._id.toString())
                throw new ServerError(401, "401:You cannot enter in this room, This is not the table you want to join!");
            else if (fetched_user.wallet_balance < fetched_table.boot_amount)
                throw new ServerError(401, "401:Invalid balance for this table");
            else if (fetched_setting.game_variations.findIndex(x => x.mode == this.roomName && x.is_active) == -1)
                throw new ServerError(401, "401:" + this.metadata[PLAYER_INFO_KEYS.game_type] + ", this game variation is not found or is disabled by admin!");
            else if (this.state.Contains_Player(fetched_user.access_token))
                throw new ServerError(401, "402:You cannot play on this table as your instance is already present in this table! Please try again!");
            else if (received_room_key != this.room_key)
                throw new ServerError(401, "402:Invalid room key");

            client.userData = { balance: fetched_user.wallet_balance, is_bot: false };
        }

        this.state.table_id = fetched_table._id.toString();
        this.table_info = Object.assign(new Table_Info(),
            {
                boot_amount: fetched_table.boot_amount,
                max_blind: fetched_table.max_blind,
                max_bet: fetched_table.max_bet,
                pot_limit: fetched_table.pot_limit
            });

        this.setMetadata(
            {
                [METADATA_KEYS.table_id]: this.state.table_id,
                [METADATA_KEYS.current_users]: this.metadata[METADATA_KEYS.current_users] + 1
            }).then(() => updateLobby(this));

        return true;
    }

    onJoin(client: Client, options: any)
    {
        new logger("Player joined room! RoomId: " + this.roomId + " SocialId: " + options[PLAYER_INFO_KEYS.social_id]);
        this.state.Add_Player(client.sessionId, new PlayerStates().assign(
            {
                sessionid: client.sessionId,
                access_token: options[PLAYER_INFO_KEYS.access_token],
                social_id: options[PLAYER_INFO_KEYS.social_id],
                is_bot: client.userData.is_bot,

                username: options[PLAYER_INFO_KEYS.username],
                avatar_index: options[PLAYER_INFO_KEYS.avatar_index],
                balance: client.userData.balance,

                is_ready_toplay: false
            }
        ));

        if (!client.userData.is_bot)
            this.user_settings.Update_Room_States(options[PLAYER_INFO_KEYS.access_token], this.roomId, client._reconnectionToken);

        if (this.room_key != "")
            return;

        if (this.clients.length < 2)
            this.Bot_SetTimer();
        else
            this.Bot_ClearTimer();
    }

    async onLeave(client: Client, consented: boolean)
    {
        var player_in_room = this.state.all_players.get(client.sessionId);
        if (player_in_room == null)
            return;

        if (player_in_room.is_disconnected)
            return;

        player_in_room.is_disconnected = true;
        if (!consented)
        {
            try {
                new logger("Player left room! RoomId: " + this.roomId + " SocialId: " + player_in_room.social_id);

                player_in_room.reconnection_caller = this.allowReconnection(client, "manual");
                if (this.state.all_players.size <= 1)
                    player_in_room.reconnection_caller.reject();

                const updated_client = await player_in_room.reconnection_caller;

                new logger("Player rejoined room! RoomId: " + this.roomId + " SocialId: " + player_in_room.social_id);

                player_in_room.is_disconnected = false;
                player_in_room.reconnection_caller = null;

                this.user_settings.Update_Room_States(player_in_room.access_token, this.roomId, updated_client._reconnectionToken);
            }
            catch (e) {
                new logger("Player data removed from room! RoomId: " + this.roomId + " SocialId: " + player_in_room.social_id);

                await this.user_settings.Update_Room_States(this.state.all_players.get(client.sessionId).access_token, "", "");
                await this.Update_Player_Balance(player_in_room, true);
                await this.Remove_Player(player_in_room.sessionid);
            }
        }
    }

    async onDispose()
    {
        new logger("Room disposing! RoomId: " + this.roomId);

        var players = Array.from(this.state.all_players.values());
        for (var i = 0; i < players.length; i++)
        {
            if (!players[i].is_bot) {
                await this.user_settings.Update_Room_States(players[i].access_token, "", "");
                await this.Update_Player_Balance(players[i], true);
            }
        }

        new logger("Room disposed! RoomId: " + this.roomId);
    }


    private RegisterMessageHandlers()
    {
        this.onMessage(SOCKET_EVENTS.PlayerReady, (client) =>
        {
            this.state.all_players.get(client.sessionId).is_ready_toplay = true;
            this.Check_ReadyPlayers_And_StartRound();
        });
        this.onMessage(SOCKET_EVENTS.RequestToLeaveTable, (client, backToMenu) => {
            if ((this.room_key != "" && backToMenu) || this.room_key == "")
                this.KickPlayer(client, backToMenu);
        });
        this.onMessage(SOCKET_EVENTS.PlayerChat, (client, msg) => {
            this.broadcast(SOCKET_EVENTS.PlayerChat,
                {
                    sessionId: client.sessionId,
                    message: msg
                });
        });
        this.onMessage(SOCKET_EVENTS.PlayerGift, (client, msg) => {
            this.broadcast(SOCKET_EVENTS.PlayerGift,
                {
                    sessionId: client.sessionId,
                    message: msg.receiver_session_id + ":" + msg.gif_index
                });
        });
        this.onMessage(SOCKET_EVENTS.PlayerPack, (client) =>
        {
            const delay: number = this.state.all_players.get(client.sessionId).is_bot ? Math.floor(Math.random() * 8) + 3 : 0;
            this.clock.setTimeout(() => { this.round_handler?.Handle_Room_Events(client, SOCKET_EVENTS.PlayerPack, null); }, delay * 1000);
        });
        this.onMessage(SOCKET_EVENTS.PlayerShow, (client) => {
            const delay: number = this.state.all_players.get(client.sessionId).is_bot ? Math.floor(Math.random() * 8) + 3 : 0;
            this.clock.setTimeout(() => { this.round_handler?.Handle_Room_Events(client, SOCKET_EVENTS.PlayerShow, null); }, delay * 1000);
        });
        this.onMessage(SOCKET_EVENTS.PlaceBet, (client, isDoubled) => {
            const delay: number = this.state.all_players.get(client.sessionId).is_bot ? Math.floor(Math.random() * 8) + 3 : 0;
            this.clock.setTimeout(() => { this.round_handler?.Handle_Room_Events(client, SOCKET_EVENTS.PlaceBet, isDoubled); }, delay * 1000);
        });
        this.onMessage("*", (client, event_type, message) => {
            this.round_handler?.Handle_Room_Events(client, event_type, message);
        });
    }



    //private methods for room
    private IsPlayer_InRound(client_session_id: string): boolean
    {
        if (this.round_handler == null)
            return false;

        return this.state.current_round.players.findIndex(x => x.sessionId == client_session_id) >= 0;
    }

    private Check_ReadyPlayers_And_StartRound(checkRoundCondition: boolean = true)
    {
        if (checkRoundCondition && this.state.current_round.status != ROUND_STATUS.none)
            return;

        var playerArray = Array.from(this.state.all_players.values()).filter(x => x.is_ready_toplay && x.balance >= this.table_info.boot_amount);
        if (playerArray.length < 2) {
            this.state.current_round.status = ROUND_STATUS.none;
            return;
        }

        var dealerIndex = playerArray.findIndex(x => x.sessionid == this.lastRound_dealer_sessionid);
        if (dealerIndex == -1)
            dealerIndex = 0;
        else {
            dealerIndex++;
            if (dealerIndex >= playerArray.length)
                dealerIndex = 0;
        }

        if (this.roomName == "CLASSIC")
            this.round_handler = new Round_Classic(playerArray, this.state.current_round, this, playerArray[dealerIndex].sessionid);
        else if (this.roomName == "AK47")
            this.round_handler = new Round_AK47(playerArray, this.state.current_round, this, playerArray[dealerIndex].sessionid);
        else if (this.roomName == "HUKAM")
            this.round_handler = new Round_Hukam(playerArray, this.state.current_round, this, playerArray[dealerIndex].sessionid);
        else if (this.roomName == "JOKER")
            this.round_handler = new Round_Joker(playerArray, this.state.current_round, this, playerArray[dealerIndex].sessionid);
        else if (this.roomName == "MUFLIS")
            this.round_handler = new Round_Muflis(playerArray, this.state.current_round, this, playerArray[dealerIndex].sessionid);
        else if (this.roomName == "ROYAL")
            this.round_handler = new Round_Royal(playerArray, this.state.current_round, this, playerArray[dealerIndex].sessionid);
        else if (this.roomName == "POTBLIND")
            this.round_handler = new Round_Potblind(playerArray, this.state.current_round, this, playerArray[dealerIndex].sessionid);

        new logger("Game started! RoomId: " + this.roomId + "RoomName: " + this.roomName);
    }

    private async Compare_RealPlayers_And_BotPlayers() {
        const real_player_count = Array.from(this.state.all_players.values()).filter(x => !x.is_bot).length;
        const bot_player_count = Array.from(this.state.all_players.values()).filter(x => x.is_bot).length;

        if (real_player_count == 0)
            this.disconnect();
        else {
            if (real_player_count == 1) {
                if (bot_player_count == 0)
                    this.Bot_SetTimer();
                else
                    this.Check_ReadyPlayers_And_StartRound(false);
            }
            else {
                await this.Bot_RemoveAll();
                this.Check_ReadyPlayers_And_StartRound(false);
            }
        }
    }

    private async KickPlayer(client: Client, backToMenu: boolean)
    {
        var player_in_room = this.state.all_players.get(client.sessionId);

        if (this.IsPlayer_InRound(client.sessionId))
        {
            if (this.state.current_round.status == ROUND_STATUS.WinnerDeclared) 
                this.state.all_players.get(player_in_room.sessionid).requested_leave_type = backToMenu ? 1 : 2;
            else {
                this.main_thread_time?.pause();

                var player_in_round = this.round_handler.Player_Left(client);

                player_in_room.session_profit += player_in_round.balance - player_in_room.balance;
                player_in_room.balance = player_in_round.balance;

                new logger("Player kicked! InRound BackToMenu: " + backToMenu + "RoomId: " + this.roomId + " SocialId: " + player_in_room.social_id);
                await this.user_settings.Update_Room_States(player_in_room.access_token, "", "");
                await this.Update_Player_Balance(player_in_room, true);

                player_in_room.is_disconnected = true;
                client.leave(backToMenu ? SOCKET_EVENTS.PlayerKicked : SOCKET_EVENTS.PlayerSwitchedTable);

                this.main_thread_time?.resume();
            }
        }
        else
        {
            this.main_thread_time?.pause();

            new logger("Player kicked! OutRound BackToMenu: " + backToMenu + "RoomId: " + this.roomId + " SocialId: " + player_in_room.social_id);

            await this.user_settings.Update_Room_States(player_in_room.access_token, "", "");
            await this.Update_Player_Balance(player_in_room, true);
            this.Remove_Player(player_in_room.sessionid);

            client.leave(backToMenu ? SOCKET_EVENTS.PlayerKicked : SOCKET_EVENTS.PlayerSwitchedTable);

            this.main_thread_time?.resume();
        }
    }

    private async Update_Player_Balance(player: PlayerStates, update_transaction: boolean) {
        if (player.is_bot)
            return;

        new logger("Updating player balance! RoomId: " + this.roomId + " SocialId: " + player.social_id + " Balance: " + player.balance);
        await this.user_settings.Update_Balance(player.access_token, player.balance, (update_transaction && player.session_profit > 0) ? player.session_profit : 0);
        if (update_transaction && player.session_profit != 0)
        {
            if (player.session_profit > 0) {
                const game_settings = await this.app_settings.Get_Game_Settings();
                var admin_commision = Math.round((player.session_profit / 100) * game_settings.admin_commission);

                await this.user_settings.Update_Balance(player.access_token, player.balance - admin_commision, 0);
                await this.transaction_settings.Create_Transaction_PlayerSession(player.social_id, player.access_token, player.session_profit - admin_commision, "Game session played!", admin_commision);
            }
            else
                await this.transaction_settings.Create_Transaction_PlayerSession(player.social_id, player.access_token, player.session_profit, "Game session played!", 0);
        }
    }

    private Remove_Player(session_id: string)
    {
        this.state.Remove_Player(session_id);
        this.setMetadata(
            {
                [METADATA_KEYS.current_users]: this.metadata[METADATA_KEYS.current_users] - 1
            }
        ).then(() => updateLobby(this));

        if (Array.from(this.state.all_players.values()).filter(x => !x.is_bot).length <= 0)
            this.disconnect();
    }
    //-----------------------------



    //public methods for round end
    async Round_Winners_Declared(dealer_sessionid: string)
    {
        new logger("Round finished! Winner declared! RoomId: " + this.roomId);

        this.lastRound_dealer_sessionid = dealer_sessionid;
        this.round_handler = null;

        for (var i = 0; i < this.state.current_round.players.length; i++)
        {
            var player_in_round = this.state.current_round.players[i];
            var player_in_room: PlayerStates = this.state.all_players.get(player_in_round.sessionId);


            if (!player_in_room.is_bot) {
                player_in_room.session_profit += player_in_round.balance - player_in_room.balance;
                player_in_room.balance = player_in_round.balance;
            }

            if (player_in_room.requested_leave_type != 0) {
                new logger("Update balance for players left! RoomId: " + this.roomId + " SocialId: " + player_in_room.social_id);
                await this.user_settings.Update_Room_States(player_in_room.access_token, "", "");
                await this.Update_Player_Balance(player_in_room, true);
                this.Remove_Player(player_in_room.sessionid);

                const player_con_client = this.clients.find(x => x.sessionId == player_in_room.sessionid);
                player_con_client?.leave(player_in_room.requested_leave_type == 1 ? SOCKET_EVENTS.PlayerKicked : SOCKET_EVENTS.PlayerSwitchedTable);
            }
            else if (player_in_room.is_disconnected) {
                if (player_in_room.reconnection_caller != null)
                    player_in_room.reconnection_caller.reject();
                else
                    this.Remove_Player(player_in_room.sessionid);
            }
            else{
                new logger("Update balance for finished round! RoomId: " + this.roomId + " SocialId: " + player_in_room.social_id);
                await this.Update_Player_Balance(player_in_room, false);
            }
        }

        this.main_thread_time = this.clock.setTimeout(() => {
            this.state.current_round.Reset_Round();
            this.main_thread_time = this.clock.setTimeout(() =>
            {
                this.state.current_round.status = ROUND_STATUS.none;
                if (this.room_key == "")
                    this.Compare_RealPlayers_And_BotPlayers();
                else
                    this.Check_ReadyPlayers_And_StartRound(false);
            }, 4000);
        },
        3000);
    }
    //-----------------------------



    //private methods for bot
    private async Bot_SetTimer() {
        const fetched_setting = await this.app_settings.Get_Game_Settings();
        if (!fetched_setting.has_bot)
            return;

        this.bot_join_time = this.clock.setTimeout(() => this.Bot_Add(), this.bot_add_time * 1000);
    }

    private Bot_ClearTimer() {
        this.bot_join_time?.clear();
    }

    private Bot_Add() {
        const bot = new BotClient(this.roomId, this.state.table_id, this.room_key);
        new logger("Added bot to room! RoomId: " + this.roomId + "Bot: " + bot);
    }

    private async Bot_RemoveAll() {
        const player = Array.from(this.state.all_players.values());

        for (var i = 0; i < player.length; i++) {
            if (player[i].is_bot) {
                await this.clients.find(x => x.sessionId == player[i].sessionid)?.leave();

                new logger("Removed bot from room! RoomId: " + this.roomId + "Bot: " + player[i]);
                this.Remove_Player(player[i].sessionid);
            }
        }
    }
    private Serialize_State(state: any): any {
        if (state instanceof Schema) {
            const serialized: any = {};

            for (const key in state) {
                if (Object.prototype.hasOwnProperty.call(state, key)) {
                    const value = state[key];
                    serialized[key] = this.Serialize_State(value);
                }
            }

            return serialized;
        } else if (Array.isArray(state) || state instanceof ArraySchema) {
            return state.map(item => this.Serialize_State(item));
        } else {
            return state;  // Primitive values
        }
    }
    //-----------------------------



    //public methods called from outside
    Player_Balance_Updated(access_token: string, new_balance: number): boolean {
        var client_exist = false;
        var players = Array.from(this.state.all_players.values());

        for (var i = 0; i < players.length; i++) {
            if (players[i].access_token == access_token) {
                client_exist = true;

                if (this.IsPlayer_InRound(players[i].sessionid)) {
                    const amount_added = new_balance - players[i].balance;
                    this.round_handler.Player_Balance_Updated(players[i].sessionid, amount_added);
                }

                new logger("Updating player balance from room! RoomId: " + this.roomId + " SocialId: " + players[i].social_id + " Balance: " + new_balance);
                players[i].balance = new_balance;
                break;
            }
        }
        if (!client_exist)
            new logger("Updating player balance from room! Player not found! RoomId: " + this.roomId);

        return client_exist;
    }

    Get_Room_Details(): any
    {
        var data = {
            room_metadata: this.metadata,
            room_table: {
                variation: this.roomName,
                boot_amount: this.table_info.boot_amount,
                max_blind: this.table_info.max_blind,
                max_bet: this.table_info.max_bet,
                pot_limit: this.table_info.pot_limit
            },
            room_details: this.Serialize_State(this.state)
        };
        return data;
    }
    //-----------------------------
}

class Table_Info
{
    boot_amount: number;
    max_blind: number;
    max_bet: number;
    pot_limit: number;
}