import { matchMaker } from "colyseus";

class RoomsHandler
{
    async Player_Balance_Updated(access_token: string, new_balance: number, room_id: string)
    {
        var args: any[] = [];
        args.push(access_token);
        args.push(new_balance);

        if (room_id == "") {
            var rooms = await matchMaker.query();
            rooms = rooms.filter(x => x.name == "Lobby");

            for (var i = 0; i < rooms.length; i++) {
                const clientFound: boolean = await matchMaker.remoteRoomCall(rooms[i].roomId, "Player_Balance_Updated", args);
                if (clientFound)
                    break;
            }
        }
        else {
            const room = await matchMaker.getRoomById(room_id);
            if (room != null) {
                await matchMaker.remoteRoomCall(room_id, "Player_Balance_Updated", args);
            }
        }
    }

    async Get_All_Rooms(): Promise<[boolean, string, any]>
    {
        try
        {
            const rooms = await matchMaker.query({});
            if (rooms == null)
                return [false, "No rooms found!", null];
            else
                return [true, "All rooms fetched", rooms];

        } catch (e)
        {
            return [false, (e as Error).message, null];
        }
    }

    async Get_Room_Details(room_id: string): Promise<[boolean, string, any]> {
        try {
            const room = await matchMaker.getRoomById(room_id);
            if (room == null)
                return [false, "No room found with this id", null];
            else {
                var result = await matchMaker.remoteRoomCall(room_id, "Get_Room_Details");
                return [true, "Details fetched!", result];
            }

        } catch (e) {
            return [false, (e as Error).message, null];
        }
    }
}

export { RoomsHandler }