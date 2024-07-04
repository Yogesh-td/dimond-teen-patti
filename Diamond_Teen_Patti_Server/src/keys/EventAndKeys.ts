export enum PLAYER_INFO_KEYS
{
    table_id = "table_id",
    social_id = "social_id",
    access_token = "access_token",
    username = "username",
    avatar_index = "avatar_index",
    game_type = "game_type"
}
export enum METADATA_KEYS {
    table_id = "table_id",
    current_users = "current_users",
    max_users = "max_users",
    private_key = "private_key"
}



export enum SOCKET_EVENTS
{
    PlayerReady = "a",
    PlaceBet = "c",
    RequestToSeeCards = "d",
    PlayerPack = "e",
    PlayerShow = "f",
    Sideshow_Request = "g",
    Sideshow_Accept = "h",
    Sideshow_Reject = "i",
    Sideshow_Finished = "j",
    DeclareWinner = "k",
    RequestToLeaveTable = "l",
    RequestTurnRemainingTime = "m",
    PlayerChat = "n",
    PlayerGift = "o",
    PlayerBalanceUpdated = "p",
    PlayerKicked = 4000,
    PlayerSwitchedTable = 4001,
}