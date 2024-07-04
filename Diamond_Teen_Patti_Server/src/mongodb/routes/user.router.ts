import express from 'express';
import { metadata } from '../metadata';
import { User } from '../models/user.model';
import { Setting_Route } from './settings.router';
import { TRANSACTION_REASON, TRANSACTION_STATUS, Transaction_Route } from './transaction.router';
import { AES_Crypto } from '../aes_crypto';
import { logger } from '../logger';

class User_Route
{
    async Get_AllUsers() {
        return await User.find({});
    }
    async Get_User(social_id: string, access_token: string)
    {
        return await User.findOne({ social_id: social_id, access_token: access_token });
    }
    async Get_UserWithIdPassword(social_id: string, password: string) {
        return await User.findOne({ social_id: social_id, password: password });
    }
    async Get_UserWithId(social_id: string) {
        return await User.findOne({ social_id: social_id });
    }

    async Authenticated_User_With_AccessToken(access_token: any, ts: any, ts_auth: any, body: any): Promise<[any, string]> {
        if (!access_token || !ts || !ts_auth)
            return [null, "Invalid access_token or ts or ts_auth"];

        if ((body == null && !aes_crypto.Validate_Query(ts, ts_auth)) || (body != null && !aes_crypto.Validate(body, ts_auth)))
            return [null, "Unauthorized user!"];

        const user = await User.findOne({ access_token: access_token });
        if (user)
            return [user, "Validated"];
        else
            return [null, "User not found!"];
    }
    async Authenticated_User_Without_AccessToken(ts: any, ts_auth: any, body: any): Promise<[boolean, string]> {
        if (!ts || !ts_auth)
            return [false, "Invalid access_token or ts or ts_auth"];
        else if ((body == null && !aes_crypto.Validate_Query(ts, ts_auth)) || (body != null && !aes_crypto.Validate(body, ts_auth)))
            return [false, "Unauthorized user!"];
        else
            return [true, ""];
    }

    async Update_Balance(access_token: string, new_balance: number, session_winnings: number)
    {
        return await User.findOneAndUpdate(
            { access_token: access_token },
            {
                $set: { wallet_balance: new_balance },
                $inc: { total_winnings: session_winnings }
            }
        );
    }
    async Update_Room_States(access_token: string, room_id: string, room_rejointoken: string)
    {
        return await User.findOneAndUpdate(
            { access_token: access_token },
            { $set: { last_room_id: room_id, last_room_token: room_rejointoken } }
        );
    }
    async Update_LggedIn_Status(access_token: string, loggedin: boolean)
    {
        var user = await User.findOneAndUpdate(
            { access_token: access_token },
            { $set: { is_loggedin: loggedin } },
            { new: true }
        );

        if (user) {
            new logger(`User: ${user.social_id}, LoggedInStatus: ${user.is_loggedin}`);
        }
    }

    randomString(len: number): string {
        const charSet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        var randomString = '';
        for (var i = 0; i < len; i++) {
            var randomPoz = Math.floor(Math.random() * charSet.length);
            randomString += charSet.substring(randomPoz, randomPoz + 1);
        }
        return randomString;
    }
}


const router = express.Router();
const user_route = new User_Route();
const aes_crypto = new AES_Crypto();


router.post('/login', async (req, res) => {
    try {
        const { social_id, password } = req.body;
        const { ts, ts_auth } = req.headers;

        const auth = await user_route.Authenticated_User_Without_AccessToken(ts, ts_auth, req.body);
        if (!auth[0])
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        if (!social_id)
            return res.status(400).json({ metadata: new metadata(false, "social_id is required!", "social_id is required!") });
        else if (!password)
            return res.status(400).json({ metadata: new metadata(false, "password is required!", "password is required!") });

        const fetched_user = await User.findOneAndUpdate(
            { social_id: social_id, password: password },
            [
                {
                    $set: {
                        access_token: {
                            $cond: {
                                if: { $eq: ["$last_room_id", ""] },
                                then: user_route.randomString(26),
                                else: "$access_token"
                            }
                        },
                        modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' })
                    }
                }
            ],
            { new: true});

        if (fetched_user) {
            if (fetched_user.is_active) {
                res.status(200).json(
                    {
                        metadata: new metadata(true, "User Logged In!", "User Logged In!"),
                        data: fetched_user.access_token
                    });
            }
            else {
                return res.status(400).json({ metadata: new metadata(false, "User ID blocked by admin", "User ID blocked by admin") });
            }
        }
        else {
            return res.status(400).json({ metadata: new metadata(false, "Id or Password is incorrect", "incorrect credentials") });
        }
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "Something went wrong!", (e as Error).message)
        });
    }
});

router.get('/getProfile', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;
        var auth = await user_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, null);

        if (auth[0] == null)
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        res.status(200).json(
            {
                metadata: new metadata(true, "User Logged In!", "User Logged In!"),
                data: User.toClientObject(auth[0])
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.post('/updateProfile', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;
        var auth = await user_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);

        if (auth[0] == null)
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const { username, avatar_index } = req.body;
        var fetched_user = auth[0];

        if (username)
        {
            if (username.length <= 0 || username.length > 15)
                return res.status(400).json({ metadata: new metadata(false, "username must be > 0 and < 16", "username must be > 0 and < 16") });

            fetched_user.username = username;
        }
        if (avatar_index || avatar_index == 0)
            fetched_user.avatar_index = avatar_index;

        const updated_user = await User.findOneAndUpdate(
            { social_id: fetched_user.social_id },
            { $set: { username: fetched_user.username, avatar_index: fetched_user.avatar_index, modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }) } },
            { new: true }
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "User Updated Successfully!", "User Updated Successfully!"),
                data: User.toClientObject(updated_user)
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.post('/updatePassword', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;
        var auth = await user_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);

        if (auth[0] == null)
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const { current_password, new_password } = req.body;
        var fetched_user = auth[0];

        if (current_password == null)
            return res.status(400).json({ metadata: new metadata(false, "current_passwword is required!", "current_passwword is required!") });
        else if (new_password == null)
            return res.status(400).json({ metadata: new metadata(false, "new_password is required!", "new_password is required!") });
        else if (new_password && new_password.length < 8)
            return res.status(400).json({ metadata: new metadata(false, "password characters must be 8 or greater!", "incorrect password format") });
        else if (current_password != fetched_user.password)
            return res.status(400).json({ metadata: new metadata(false, "current_password is incorrect!", "current_password is incorrect!") });

        const updated_user = await User.findOneAndUpdate(
            { social_id: fetched_user.social_id },
            { $set: { password: new_password, modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }) } },
            { new: true }
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "Password Updated Successfully!", "Password Updated Successfully!")
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.post('/redeemReferal', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;
        var auth = await user_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);

        if (auth[0] == null)
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const { refer_code } = req.body;

        if (!refer_code)
            return res.status(400).json({ metadata: new metadata(false, "refer_code is required!", "refer_code is required!") });

        var current_user = auth[0];
        var referer_user = await User.findOne({ refer_code: refer_code.toLowerCase() });

        if (!referer_user) {
            res.status(400).json({
                metadata: new metadata(false, "Invalid referral code! ", "Invalid referral code!")
            });
        }
        else
        {
            if (current_user.refered_by)
                return res.status(400).json({ metadata: new metadata(false, "Referral code already used!", "Referral code already used!") });
            if (refer_code == current_user.refer_code)
                return res.status(400).json({ metadata: new metadata(false, "You cannot refer yourself!", "You cannot refer yourself!") });

            current_user = await User.findOneAndUpdate(
                { social_id: current_user.social_id },
                { $set: { refered_by: referer_user.refer_code, modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }) } },
                { new: true }
            );

            const game_settings = await new Setting_Route().Get_Game_Settings();

            const trx_route = new Transaction_Route();
            referer_user = await trx_route.Create_Transaction(
                referer_user.social_id,
                game_settings.referer_coins,
                TRANSACTION_REASON.REFERRAL,
                TRANSACTION_STATUS.COMPLETED,
                "Referral Bonus from " + current_user.username
            );
            current_user = await trx_route.Create_Transaction(
                current_user.social_id,
                game_settings.refered_coins,
                TRANSACTION_REASON.REFERRAL,
                TRANSACTION_STATUS.COMPLETED,
                "Referral Bonus!"
            );

            res.status(200).json(
                {
                    metadata: new metadata(true, "Refer code applied!", "Refer code applied!"),
                    data: User.toClientObject(current_user)
                });
        }
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.post('/validatePurchase', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;
        var auth = await user_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);

        if (auth[0] == null)
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const { _id, package_name } = req.body;

        if (!_id)
            return res.status(400).json({ metadata: new metadata(false, "_id is required!", "_id is required!") });

        const iap_item = await new Setting_Route().Get_Iap(_id);
        if (iap_item == null)
            return res.status(400).json({ metadata: new metadata(false, "invalid product id!", "invalid product id!") });

        var current_user = auth[0];
        current_user = await new Transaction_Route().Create_Transaction(
            current_user.social_id,
            iap_item.coins,
            TRANSACTION_REASON.COIN_PURCHASE,
            TRANSACTION_STATUS.COMPLETED,
            "Product purchased!"
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "Purchase successfull", "Receipt is valid"),
                data: User.toClientObject(current_user)
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.get('/leaderboard', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;
        var auth = await user_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, null);

        if (auth[0] == null)
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const current_user = auth[0];
        const sorted_players = await User.find({ is_active: true }).sort({ total_winnings: -1 });

        var to_send_player_local = User.toLeaderboardObject(current_user, sorted_players.findIndex(x => x.social_id == current_user.social_id) + 1, true);
        var to_send_players = [];

        for (var i = 0; i < 10; i++)
        {
            if (i >= sorted_players.length)
                break;

            to_send_players.push(User.toLeaderboardObject(sorted_players[i], i + 1, sorted_players[i].social_id == current_user.social_id));
        }

        res.status(200).json(
            {
                metadata: new metadata(true, "Leaderboard fetched!", "Leaderboard fetched!"),
                data: {
                    local_player: to_send_player_local,
                    other_players: to_send_players
                }
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.post('/withdrawal', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;
        var auth = await user_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);

        if (auth[0] == null)
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const { amount } = req.body;
        var fetched_user = auth[0];

        if (!amount)
            return res.status(400).json({ metadata: new metadata(false, "amount is required!", "amount is required!") });

        if (fetched_user.wallet_balance < amount)
            return res.status(400).json({ metadata: new metadata(false, "balance is not sufficient", "balance is not sufficient") });

        if (amount < 100)
            return res.status(400).json({ metadata: new metadata(false, "minimun withdrawal amount is Rs.100", "minimun withdrawal amount is Rs.100") });

        const updateduser = await new Transaction_Route().Create_Transaction(
            fetched_user.social_id,
            amount > 0 ? -amount : amount,
            TRANSACTION_REASON.WITHDRAW,
            TRANSACTION_STATUS.PENDING,
            "Withdraw by user"
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "Purchase successfull", "Receipt is valid"),
                data: User.toClientObject(updateduser)
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});


export { router, User_Route };