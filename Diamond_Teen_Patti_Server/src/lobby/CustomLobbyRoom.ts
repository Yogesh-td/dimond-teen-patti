import { LobbyRoom } from "@colyseus/core";
import { Client } from "colyseus";
import { PLAYER_INFO_KEYS, SOCKET_EVENTS } from "../keys/EventAndKeys";
import { User_Route } from "../mongodb/routes/user.router";
import { logger } from "../mongodb/logger";

export class CustomLobbyRoom extends LobbyRoom
{
    user_settings = new User_Route();

    async onCreate(options: any)
    {
        await super.onCreate(options);

        this.maxClients = 9999;
        this.roomId = "Lobby";
        this.autoDispose = true;

        new logger("Lobby created!");
    }
    onDispose()
    {
        new logger("Lobby disposed");
    }
    onJoin(client: Client, options: any) {
        super.onJoin(client, options);
        client.userData = { access_token: options[PLAYER_INFO_KEYS.access_token] };

        this.user_settings.Update_LggedIn_Status(client.userData.access_token, true);
    }
    onLeave(client: Client) {
        this.user_settings.Update_LggedIn_Status(client.userData.access_token, false);
    }

    Player_Balance_Updated(access_token: string, new_balance: number): boolean
    {
        var client_index = -1;
        for (var i = 0; i < this.clients.length; i++) {
            if (this.clients[i].userData.access_token == access_token) {
                client_index = i;
                break;
            }
        }

        if (client_index == -1)
        {
            new logger("Updating player balance from LOBBY failed! Player not found!");
            return false;
        }
        else
        {
            new logger("Updating player balance from LOBBY success! PlayerAccessToken: " + this.clients[client_index].userData.access_token);
            this.clients[client_index].send(SOCKET_EVENTS.PlayerBalanceUpdated, new_balance);
            return true;
        }
    }
}