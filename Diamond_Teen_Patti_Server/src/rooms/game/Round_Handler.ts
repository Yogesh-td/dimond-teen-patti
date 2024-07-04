import { Client } from "colyseus";
import { TPRoom } from "../TPRoom";
import { PlayerStates } from "../schema/PlayerStates";
import { RoundPlayerStates } from "../schema/RoundPlayerStates";
import { ROUND_STATUS, RoundStates, WIN_REASON } from "../schema/RoundStates";
import { SOCKET_EVENTS } from "../../keys/EventAndKeys";
import { Card } from "../helpers/Card";

export class Round_Handler
{
    main_room: TPRoom = null;
    current_round: RoundStates = null;

    constructor(round_players: PlayerStates[], round: RoundStates, room: TPRoom, dealer_sessionid: string)
    {
        this.main_room = room;
        this.current_round = round;

        round_players.forEach((state, index) =>
        {
            this.current_round.players.push(new RoundPlayerStates().assign(
                {
                    sessionId: state.sessionid,
                    social_id: state.social_id,
                    is_bot: state.is_bot,
                    balance: state.balance,
                    isDealer: state.sessionid == dealer_sessionid
                }
            ));
        });

        this.Collect_Boot_Amount();
    }

    Handle_Room_Events(client: Client, event_type: string | number, data: any)
    {
        switch (event_type)
        {
            case SOCKET_EVENTS.PlaceBet:
                this.Player_Placed_Bet(client.sessionId, data)
                break;
            case SOCKET_EVENTS.RequestToSeeCards:
                this.Player_RequestTo_SeeCards(client.sessionId, client);
                break;
            case SOCKET_EVENTS.PlayerPack:
                this.Player_Pack(client.sessionId);
                break;
            case SOCKET_EVENTS.PlayerShow:
                this.Player_Show(client.sessionId);
                break;
            case SOCKET_EVENTS.Sideshow_Request:
                this.Player_RequestTo_Sideshow(client.sessionId);
                break;
            case SOCKET_EVENTS.Sideshow_Accept:
                this.Player_RequestTo_Sideshow_Accepted(client.sessionId);
                break;
            case SOCKET_EVENTS.Sideshow_Reject:
                this.Player_RequestTo_Sideshow_Reject(client.sessionId);
                break;
            case SOCKET_EVENTS.RequestTurnRemainingTime:
                this.Player_RequestTo_TurnRemainingTime(client);
                break;
            default:
                console.log("Undefined data recieved");
                break;
        }
    }
    Player_Left(client: Client): RoundPlayerStates
    {
        var left_player: RoundPlayerStates = this.current_round.players.find(x => x.sessionId == client.sessionId);
        if (!left_player.IsFinished())
            left_player.is_left_game = true;

        return left_player;
    }
    Player_Balance_Updated(session_id: string, added_amount: number) {
        var left_player: RoundPlayerStates = this.current_round.players.find(x => x.sessionId == session_id);
        if (left_player)
            left_player.balance += added_amount;
    }



    private Collect_Boot_Amount() {
        this.current_round.status = ROUND_STATUS.CollectingBoot;

        var boot_amount: number = this.main_room.table_info.boot_amount;
        this.current_round.players.forEach((player, index) =>
        {
            if (!player.is_left_game) {
                player.current_bet_amount = boot_amount;
                player.last_bet_amount = boot_amount;
                player.balance -= boot_amount;
            }

            this.current_round.pot_amount += boot_amount;
        });

        this.main_room.main_thread_time = this.main_room.clock.setTimeout(() => {
            this.Distribute_Cards();
        }, 1500);
    }
    private Distribute_Cards() {
        this.current_round.status = ROUND_STATUS.DistributingCards;

        this.main_room.main_thread_time = this.main_room.clock.setTimeout(() => {
            this.Assign_Cards_To_Players();
        }, this.current_round.players.length * 1000);

    }
    protected Assign_Cards_To_Players() {
        const dealerIndex: number = Array.from(this.current_round.players.values()).findIndex(x => x.isDealer);
        if (dealerIndex == -1)
            return;

        this.StartOrChange_Turn(true, this.main_room.table_info.boot_amount, dealerIndex + 1);
    }



    private StartOrChange_Turn(last_bet_isBlind: boolean, last_bet_amount: number, nextTurnIndex: number)
    {
        var nextTurnData = this.Get_Player(nextTurnIndex);

        var nextTurnPlayer = nextTurnData[0];
        if (nextTurnPlayer.IsFinished()) {
            var nextTurnIndex_Next = nextTurnIndex + 1;
            if (nextTurnIndex_Next >= this.current_round.players.length)
                nextTurnIndex_Next = 0;

            this.StartOrChange_Turn(last_bet_isBlind, last_bet_amount, nextTurnIndex_Next);
        }
        else
        {
            if (last_bet_isBlind)
                this.Update_Player_CurrentBetAmount(nextTurnPlayer, nextTurnPlayer.isblind ? last_bet_amount : last_bet_amount * 2);
            else
                this.Update_Player_CurrentBetAmount(nextTurnPlayer, nextTurnPlayer.isblind ? last_bet_amount / 2 : last_bet_amount);


            if (this.Remaining_Playing_Players().length > 2) {
                var previousPlayer = this.Get_Previous_Player(nextTurnData[1]);
                if (previousPlayer != null && !previousPlayer.isblind && !nextTurnPlayer.isblind)
                    nextTurnPlayer.cansideshow = true;
                else
                    nextTurnPlayer.cansideshow = false;
            }
            else {
                this.current_round.can_show = true;
                nextTurnPlayer.cansideshow = false;
            }

            this.current_round.turn_player_index = nextTurnData[1];
            this.current_round.status = ROUND_STATUS.TurnRunning;
            this.main_room.main_thread_time?.clear();

            if (this.current_round.can_show && nextTurnPlayer.is_left_game) {
                var winners: RoundPlayerStates[] = [];
                winners.push(this.Get_Previous_Player(this.current_round.turn_player_index));

                this.Declare_Result(winners, WIN_REASON.OTHER_LEFT, CARD_HAND_TYPE.none);
            }
            else
            {
                this.main_room.main_thread_time = this.main_room.clock.setTimeout(() => this.Turn_Player_TimerEnds(), this.main_room.turn_time * 1000);

                nextTurnPlayer.total_turns += 1;
                if (nextTurnPlayer.isblind && nextTurnPlayer.total_turns > this.main_room.table_info.max_blind)
                    this.Player_RequestTo_SeeCards(nextTurnPlayer.sessionId, this.main_room.clients.find(x => x.sessionId == nextTurnPlayer.sessionId));
                if (nextTurnPlayer.balance < nextTurnPlayer.current_bet_amount)
                    this.Player_Pack(nextTurnPlayer.sessionId);
            }
        }
    }
    private Turn_Player_TimerEnds()
    {
        var currentTurnPlayer = this.current_round.players[this.current_round.turn_player_index];
        this.Player_Pack(currentTurnPlayer.sessionId);
    }




    private Update_Player_CurrentBetAmount(player: RoundPlayerStates, updateAmount: number)
    {
        if (updateAmount > this.main_room.table_info.max_bet || player.isbetlimitreached)
            return;

        player.current_bet_amount = updateAmount;
        if (player.current_bet_amount >= this.main_room.table_info.max_bet)
            player.isbetlimitreached = true;
    }
    private Remaining_Playing_Players(): RoundPlayerStates[]
    {
        return Array.from(this.current_round.players.values()).filter(x => !x.IsFinished());
    }
    private Update_Player_Bet(betPlayer: RoundPlayerStates)
    {
        betPlayer.last_bet_amount = betPlayer.current_bet_amount;
        betPlayer.balance -= betPlayer.last_bet_amount;

        this.current_round.pot_amount += betPlayer.last_bet_amount;
        this.main_room.broadcast(SOCKET_EVENTS.PlaceBet,
            {
                sessionId: betPlayer.sessionId,
                betAmount: betPlayer.last_bet_amount
            }
        );
    }
    private Get_Previous_Player(playerIndex: number): RoundPlayerStates
    {
        var previousPlayerIndex: number = playerIndex - 1;
        while (true) {
            if (previousPlayerIndex == playerIndex)
                break;

            if (previousPlayerIndex < 0)
                previousPlayerIndex = this.current_round.players.length - 1;

            var previousPlayer: RoundPlayerStates = this.current_round.players[previousPlayerIndex];
            if (previousPlayer.IsFinished())
                previousPlayerIndex--;
            else
                break;
        }

        if (previousPlayerIndex == playerIndex)
            return null;
        else
            return this.current_round.players[previousPlayerIndex];
    }
    private Get_Player(turnIndex: number): [RoundPlayerStates, number]
    {
        if (turnIndex >= this.current_round.players.length)
            turnIndex = 0;

        var nextTurnPlayer = this.current_round.players[turnIndex];
        return [nextTurnPlayer, turnIndex];
    }




    private Player_Placed_Bet(playerSessionId: string, isBetDoubled: boolean)
    {
        if (this.current_round.status != ROUND_STATUS.TurnRunning)
            return;

        var requestedPlayerIndex: number = this.current_round.players.findIndex(x => x.sessionId == playerSessionId);
        if (requestedPlayerIndex < 0 || requestedPlayerIndex != this.current_round.turn_player_index)
            return;

        var betPlayer: RoundPlayerStates = this.current_round.players[requestedPlayerIndex];
        this.main_room.main_thread_time.pause();

        if (!betPlayer.isbetlimitreached && isBetDoubled && betPlayer.balance >= (betPlayer.current_bet_amount * 2))
            this.Update_Player_CurrentBetAmount(betPlayer, betPlayer.current_bet_amount * 2);

        this.Update_Player_Bet(betPlayer);

        if (this.current_round.pot_amount >= this.main_room.table_info.pot_limit)
            this.Declare_Result(this.Remaining_Playing_Players(), WIN_REASON.POT_LIMIT_REACHED, CARD_HAND_TYPE.none);
        else
            this.StartOrChange_Turn(betPlayer.isblind, betPlayer.last_bet_amount, this.current_round.turn_player_index + 1);
    }
    protected Player_RequestTo_SeeCards(playerSessionId: string, playerSocket: Client)
    {
        if (this.current_round.status != ROUND_STATUS.TurnRunning)
            return;

        var requestedPlayerIndex: number = this.current_round.players.findIndex(x => x.sessionId == playerSessionId);
        if (requestedPlayerIndex < 0)
            return;

        var requestedPlayer: RoundPlayerStates = this.current_round.players[requestedPlayerIndex];
        if (requestedPlayer == null || requestedPlayer.IsFinished())
            return;

        if (!requestedPlayer.isblind) {
            playerSocket.send(SOCKET_EVENTS.RequestToSeeCards, new PLAYER_CARDS(playerSessionId, requestedPlayer.cards));
            return;
        }

        requestedPlayer.isblind = false;
        if (this.current_round.players[this.current_round.turn_player_index] == requestedPlayer)
            this.Update_Player_CurrentBetAmount(requestedPlayer, requestedPlayer.current_bet_amount * 2);

        playerSocket.send(SOCKET_EVENTS.RequestToSeeCards, new PLAYER_CARDS(playerSessionId, requestedPlayer.cards));

        if (this.Remaining_Playing_Players().length > 2) {
            var previousPlayer = this.Get_Previous_Player(requestedPlayerIndex);
            if (previousPlayer != null && !previousPlayer.isblind && !requestedPlayer.isblind)
                requestedPlayer.cansideshow = true;
            else
                requestedPlayer.cansideshow = false;
        }
    }
    private Player_Pack(playerSessionId: string)
    {
        if (this.current_round.status != ROUND_STATUS.TurnRunning)
            return;

        var requestedPlayerIndex: number = this.current_round.players.findIndex(x => x.sessionId == playerSessionId);
        if (requestedPlayerIndex < 0 || requestedPlayerIndex != this.current_round.turn_player_index)
            return;

        var betPlayer: RoundPlayerStates = this.current_round.players[requestedPlayerIndex];
        if (betPlayer == null)
            return;

        this.main_room.main_thread_time?.pause();
        if (this.current_round.can_show && this.Get_Player(this.current_round.turn_player_index + 1)[0].is_left_game) {
            var winners: RoundPlayerStates[] = [];
            winners.push(betPlayer);

            this.Declare_Result(winners, WIN_REASON.OTHER_LEFT, CARD_HAND_TYPE.none);
        }
        else
        {
            betPlayer.ispack = true;

            const remainingPlayers: RoundPlayerStates[] = this.Remaining_Playing_Players();
            if (remainingPlayers.length <= 1)
                this.Declare_Result(remainingPlayers, WIN_REASON.PLAYER_PACKED, CARD_HAND_TYPE.none);
            else
                this.StartOrChange_Turn(betPlayer.isblind, betPlayer.current_bet_amount, this.current_round.turn_player_index + 1);
        }
    }
    private Player_Show(playerSessionId: string)
    {
        if (this.current_round.status != ROUND_STATUS.TurnRunning || !this.current_round.can_show)
            return;

        var requestedPlayerIndex: number = this.current_round.players.findIndex(x => x.sessionId == playerSessionId);
        if (requestedPlayerIndex < 0 || requestedPlayerIndex != this.current_round.turn_player_index)
            return;

        var showPlayer: RoundPlayerStates = this.current_round.players[requestedPlayerIndex];
        if (showPlayer == null)
            return;

        this.main_room.main_thread_time.pause();
        if (this.current_round.can_show && this.Get_Player(this.current_round.turn_player_index + 1)[0].is_left_game) {
            var winners_updated: RoundPlayerStates[] = [];
            winners_updated.push(showPlayer);

            this.Declare_Result(winners_updated, WIN_REASON.OTHER_LEFT, CARD_HAND_TYPE.none);
        }
        else {
            this.Update_Player_Bet(showPlayer);
            this.Declare_Result(this.Remaining_Playing_Players(), WIN_REASON.REQUESTED_SHOW, CARD_HAND_TYPE.none);
        }
    }
    protected Player_RequestTo_Sideshow(playerSessionId: string)
    {
        if (this.current_round.status != ROUND_STATUS.TurnRunning || this.current_round.can_show)
            return;

        var requestedPlayerIndex: number = this.current_round.players.findIndex(x => x.sessionId == playerSessionId);
        if (requestedPlayerIndex < 0 || requestedPlayerIndex != this.current_round.turn_player_index)
            return;

        var requestedPlayer: RoundPlayerStates = this.current_round.players[requestedPlayerIndex];
        if (requestedPlayer.isblind || requestedPlayer.IsFinished())
            return;

        var previousPlayer = this.Get_Previous_Player(requestedPlayerIndex);

        if (previousPlayer == null)
            this.StartOrChange_Turn(requestedPlayer.isblind, requestedPlayer.last_bet_amount, this.current_round.turn_player_index + 1);
        else {
            this.main_room.main_thread_time?.pause();
            this.current_round.Sideshow_Create(playerSessionId, previousPlayer.sessionId);

            this.Update_Player_Bet(requestedPlayer);

            this.main_room.main_thread_time = this.main_room.clock.setTimeout(() => {
                this.Player_RequestTo_Sideshow_Reject(previousPlayer.sessionId);
            }, this.main_room.sideshow_time * 1000);

            this.main_room.broadcast(SOCKET_EVENTS.Sideshow_Request,
                {
                    sender_sid: playerSessionId,
                    receiver_sid: previousPlayer.sessionId,
                    remaining_time: (this.main_room.sideshow_time * 1000) - this.main_room.main_thread_time.elapsedTime
                });
        }
    }
    private Player_RequestTo_Sideshow_Reject(playerSessionId: string)
    {
        if (this.current_round.status != ROUND_STATUS.TurnRunning || this.current_round.can_show || !this.current_round.side_show_status.isProcessingSideshow)
            return;

        if (this.current_round.side_show_status.receiver_sid != playerSessionId)
            return;

        this.main_room.main_thread_time.clear();
        this.current_round.Sideshow_Clear();
        this.main_room.broadcast(SOCKET_EVENTS.Sideshow_Finished);

        var turnPlayer: RoundPlayerStates = this.current_round.players[this.current_round.turn_player_index];
        this.StartOrChange_Turn(turnPlayer.isblind, turnPlayer.last_bet_amount, this.current_round.turn_player_index + 1);
    }
    private Player_RequestTo_Sideshow_Accepted(playerSessionId: string)
    {
        if (this.current_round.status != ROUND_STATUS.TurnRunning || this.current_round.can_show || !this.current_round.side_show_status.isProcessingSideshow)
            return;

        if (this.current_round.side_show_status.receiver_sid != playerSessionId)
            return;

        this.main_room.main_thread_time.clear();

        var sender_player: RoundPlayerStates = this.current_round.players.find(x => x.sessionId == this.current_round.side_show_status.sender_sid);
        var receiver_player: RoundPlayerStates = this.current_round.players.find(x => x.sessionId == this.current_round.side_show_status.receiver_sid);

        this.current_round.Sideshow_Clear();
        this.main_room.broadcast(SOCKET_EVENTS.Sideshow_Finished);

        var players_with_cards: PLAYER_CARDS[] = [];
        players_with_cards.push(new PLAYER_CARDS(sender_player.sessionId, sender_player.cards));
        players_with_cards.push(new PLAYER_CARDS(receiver_player.sessionId, receiver_player.cards));

        this.main_room.clients.getById(sender_player.sessionId).send(SOCKET_EVENTS.Sideshow_Accept, players_with_cards);
        this.main_room.clients.getById(receiver_player.sessionId).send(SOCKET_EVENTS.Sideshow_Accept, players_with_cards);

        this.main_room.main_thread_time = this.main_room.clock.setTimeout(() => {
            var players: RoundPlayerStates[] = [];
            players.push(sender_player);
            players.push(receiver_player);

            const winners = this.Calculate_Winner(players);
            if (winners[0].length == players.length || winners[0][0] == sender_player)
                receiver_player.ispack = true;
            else
                sender_player.ispack = true;

            this.StartOrChange_Turn(sender_player.isblind, sender_player.last_bet_amount, this.current_round.turn_player_index + 1);

        }, 2000);
    }
    private Player_RequestTo_TurnRemainingTime(playerSocket: Client) {
        if (this.current_round.status != ROUND_STATUS.TurnRunning || this.current_round.side_show_status.isProcessingSideshow)
            return;

        playerSocket.send(SOCKET_EVENTS.RequestTurnRemainingTime, (this.main_room.main_thread_time != null && this.main_room.main_thread_time.active) ? this.main_room.main_thread_time.elapsedTime : 0);
    }



    private Declare_Result(winners: RoundPlayerStates[], reason: WIN_REASON, hand_type: CARD_HAND_TYPE) {
        this.current_round.status = ROUND_STATUS.WinnerDeclared;

        switch (reason) {
            case WIN_REASON.OTHER_LEFT:
                this.Declare_Winner(winners, reason, hand_type, false);
                break;
            case WIN_REASON.PLAYER_PACKED:
                this.Declare_Winner(winners, reason, hand_type, false);
                break;
            case WIN_REASON.POT_LIMIT_REACHED:
                const winners_data_1 = this.Calculate_Winner(this.Remaining_Playing_Players());
                this.Declare_Winner(winners_data_1[0], reason, winners_data_1[2], true);
                break;
            case WIN_REASON.REQUESTED_SHOW:
                const winners_data_2 = this.Calculate_Winner(this.Remaining_Playing_Players());
                this.Declare_Winner(winners_data_2[0], reason, winners_data_2[2], true);
                break;
            default:
        }
    }
    protected Calculate_Winner(players: RoundPlayerStates[]): [RoundPlayerStates[], number, CARD_HAND_TYPE]
    {
        return null;
    }
    private Declare_Winner(winnerPlayers: RoundPlayerStates[], reason: WIN_REASON, handType: CARD_HAND_TYPE, should_show_cards: boolean) {
        this.main_room.main_thread_time?.clear();

        winnerPlayers.forEach((player) => {
            player.balance += this.current_round.pot_amount / winnerPlayers.length;
        });

        var players_with_cards: PLAYER_CARDS[] = [];
        this.current_round.players.forEach((player) => {
            if (should_show_cards && !player.IsFinished())
                players_with_cards.push(new PLAYER_CARDS(player.sessionId, player.cards));
        });

        this.main_room.main_thread_time = this.main_room.clock.setTimeout(() => {

            var winners_sids: string[] = [];
            winnerPlayers.forEach((player) => {
                winners_sids.push(player.sessionId);
            });

            this.main_room.broadcast(SOCKET_EVENTS.DeclareWinner,
                {
                    winners: winners_sids,
                    winners_reason: reason,
                    winners_handtype: handType,

                    pot_amount: this.current_round.pot_amount,
                    players_cards: players_with_cards
                }
            );

            this.main_room.Round_Winners_Declared(Array.from(this.current_round.players.values()).find(x => x.isDealer).sessionId);
        },
        2000);
    }
}

export class PLAYER_CARDS
{
    sessionId: string = "";
    cards: Card[] = [];

    constructor(_sessionId: string, _cards: Card[]) {
        this.sessionId = _sessionId;
        this.cards = _cards;
    }
}

export enum CARD_HAND_TYPE
{
    none = "none",
    TRAIL = "TRAIL",
    PURE_SEQUENCE = "PURE-SEQUENCE",
    SEQUENCE = "SEQUENCE",
    FLUSH = "FLUSH",
    PAIR = "PAIR",
    HIGHCARD = "HIGH"
}