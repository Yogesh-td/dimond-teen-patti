import express from 'express';
import { User } from '../models/user.model';
import { AdminUser } from '../models/adminuser.model';
import { metadata } from '../metadata';
import { Setting_Route } from './settings.router';
import { TRANSACTION_REASON, TRANSACTION_STATUS, Transaction_Route } from './transaction.router';
import { User_Route } from './user.router';
import { AES_Crypto } from '../aes_crypto';
import { GameSettings } from '../models/gamesettings.model';
import { TableSettings } from '../models/tablesettings.model';
import { IapSettings } from '../models/iapsettings.model';
import { RoomsHandler } from '../../rooms/RoomsHandler';

class AdminUser_Route {
    async Get_User(user_id: string, access_token: string) {
        return await AdminUser.findOne({ user_id: user_id, access_token: access_token });
    }
    async Get_UserWithIdPassword(user_id: string, password: string) {
        return await AdminUser.findOne({ user_id: user_id, password: password });
    }
    async Get_UserWithId(user_id: string) {
        return await User.findOne({ user_id: user_id });
    }

    async Authenticated_User_With_AccessToken(access_token: any, ts: any, ts_auth: any, body: any): Promise<[any, string]> {
        if (!access_token || !ts || !ts_auth)
            return [null, "Invalid access_token or ts or ts_auth"];

        if ((body == null && !aes_crypto.Validate_Query(ts, ts_auth)) || (body != null && !aes_crypto.Validate(body, ts_auth)))
            return [null, "Unauthorized user!"];

        const user = await AdminUser.findOne({ access_token: access_token });
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
const aes_crypto = new AES_Crypto();
const adminuser_route = new AdminUser_Route();


//------------- CUSTOMER USER -----------------
router.post('/register_customer', async (req, res) => {
    try {
        const user_route = new User_Route();

        const { social_id, password } = req.body;
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const auth = await user_route.Authenticated_User_Without_AccessToken(ts, ts_auth, req.body);
        if (!auth[0])
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        if (!social_id)
            return res.status(400).json({ metadata: new metadata(false, "social_id is required!", "social_id is required!") });
        else if (!password)
            return res.status(400).json({ metadata: new metadata(false, "password is required!", "password is required!") });
        else if (password && password.length < 8)
            return res.status(400).json({ metadata: new metadata(false, "password characters must be 8 or greater!", "incorrect password format") });

        const fetched_user = await user_route.Get_UserWithId(social_id);

        if (fetched_user) {
            return res.status(400).json({ metadata: new metadata(false, "Social id already exist!", "Social id already exist!") });
        }
        else {
            var createdUser = await User.create({
                social_id: social_id,
                password: password,

                username: "GUEST_" + user_route.randomString(5),
                avatar_index: 0,
                refer_code: user_route.randomString(10).toLowerCase(),
                refered_by: "",

                wallet_balance: 0,
                total_winnings: 0,

                access_token: "",
                last_room_id: "",
                last_room_token: "",
                is_loggedin: false,
                is_active: true,

                created_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }),
                modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }),
            });

            const game_settings = await new Setting_Route().Get_Game_Settings();
            createdUser = await new Transaction_Route().Create_Transaction(
                createdUser.social_id,
                game_settings.user_register_coins,
                TRANSACTION_REASON.DEPOSIT,
                TRANSACTION_STATUS.COMPLETED,
                "Joining Bonus"
            );

            res.status(200).json(
                {
                    metadata: new metadata(true, "User Registered Successfully!", "User Registered Successfully!")
                });
        }
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "Something went wrong!", (e as Error).message)
        });
    }
});

router.get('/get_all_customers', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, null);

        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const user_route = new User_Route();
        const fetched_users = await user_route.Get_AllUsers();

        if (!fetched_users) {
            return res.status(400).json({ metadata: new metadata(false, "No customers found!", "No customers found!") });
        }
        else {
            res.status(200).json(
                {
                    metadata: new metadata(true, "Fetched Users Successfully!", "Fetched Users Successfully!"),
                    data: fetched_users
                });
        }
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "Something went wrong!", (e as Error).message)
        });
    }
});

router.post('/get_customer', async (req, res) => {
    try {
        const { social_id } = req.body;
        const { ts, ts_auth, access_token } = req.headers;

        if (!social_id)
            return res.status(400).json({ metadata: new metadata(false, "social_id is required!", "social_id is required!") });

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);

        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const user_route = new User_Route();
        const fetched_users = await user_route.Get_UserWithId(social_id);

        if (!fetched_users) {
            return res.status(400).json({ metadata: new metadata(false, "No customers found!", "No customers found!") });
        }
        else {
            res.status(200).json(
                {
                    metadata: new metadata(true, "Fetched Users Successfully!", "Fetched Users Successfully!"),
                    data: fetched_users
                });
        }
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "Something went wrong!", (e as Error).message)
        });
    }
});

router.post('/update_customer', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const { social_id, password, username, total_winnings, is_active } = req.body;
        if (!social_id)
            return res.status(400).json({ metadata: new metadata(false, "social_id is required!", "social_id is required!") });
        if (password && password.length < 8)
            return res.status(400).json({ metadata: new metadata(false, "password characters must be 8 or greater!", "incorrect password format") });

        var customer = await new User_Route().Get_UserWithId(social_id);
        if (customer == null)
            return res.status(400).json({ metadata: new metadata(false, "no customer found!", "no customer found!") });

        if (password)
            customer.password = password;
        if (username)
            customer.username = username;
        if (total_winnings)
            customer.total_winnings = total_winnings;
        if (is_active != null)
            customer.is_active = is_active;

        const updated_cusotmer = await User.findOneAndUpdate(
            { social_id: customer.social_id },
            {
                $set: {
                    password: customer.password,
                    username: customer.username,
                    wallet_balance: customer.wallet_balance,
                    total_winnings: customer.total_winnings,
                    is_active: customer.is_active,
                }
            },
            { new: true }
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "Customer Updated Successfully!", "Customer Updated Successfully!"),
                data: updated_cusotmer
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});
//---------------------------------------------


//------------- TRANSACTION TABLE -------------
router.get('/get_transaction_admincommission', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, null);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const transactions = await new Transaction_Route().Get_Transaction_AdminCommission();
        res.status(200).json(
            {
                metadata: new metadata(true, "Fetched all admin commissions", "Fetched all admin commissions"),
                data: transactions
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.post('/get_customer_transactions', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const { social_id } = req.body;
        if (!social_id)
            return res.status(400).json({ metadata: new metadata(false, "social_id is required!", "social_id is required!") });

        const transactions = await new Transaction_Route().Get_Transactions(social_id);
        res.status(200).json(
            {
                metadata: new metadata(true, "Fetched all customer transactions", "Fetched all customer transactions"),
                data: transactions
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.post('/customer_deposit', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const { social_id, amount } = req.body;
        if (!social_id)
            return res.status(400).json({ metadata: new metadata(false, "social_id is required!", "social_id is required!") });
        if (!amount)
            return res.status(400).json({ metadata: new metadata(false, "amount is required!", "amount is required!") });

        var customer = await new User_Route().Get_UserWithId(social_id);
        if (customer == null)
            return res.status(400).json({ metadata: new metadata(false, "no customer found!", "no customer found!") });

        customer = await new Transaction_Route().Create_Transaction(
            customer.social_id,
            amount > 0 ? amount : -amount,
            TRANSACTION_REASON.DEPOSIT,
            TRANSACTION_STATUS.COMPLETED,
            "Deposit by admin"
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "Deposit for customer successfully!", "Deposit for customer successfully!"),
                data: customer
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.get('/get_all_withdraw_transactions', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, null);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const transactions = await new Transaction_Route().Get_Transaction_Withdrawls();
        res.status(200).json(
            {
                metadata: new metadata(true, "Fetched all withdrawl transactions", "Fetched all withdrawl transactions"),
                data: transactions
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.post('/customer_withdraw', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const { social_id, amount } = req.body;
        if (!social_id)
            return res.status(400).json({ metadata: new metadata(false, "social_id is required!", "social_id is required!") });
        if (!amount)
            return res.status(400).json({ metadata: new metadata(false, "amount is required!", "amount is required!") });

        var customer = await new User_Route().Get_UserWithId(social_id);
        if (customer == null)
            return res.status(400).json({ metadata: new metadata(false, "no customer found!", "no customer found!") });

        if (customer.wallet_balance < amount)
            return res.status(400).json({ metadata: new metadata(false, "customer balance is not sufficient", "customer balance is not sufficient") });
        else {
            customer = await new Transaction_Route().Create_Transaction(
                customer.social_id,
                amount > 0 ? -amount : amount,
                TRANSACTION_REASON.WITHDRAW,
                TRANSACTION_STATUS.COMPLETED,
                "Withdraw by admin"
            );
        }

        res.status(200).json(
            {
                metadata: new metadata(true, "Withdrawn for customer successfully!", "Withdrawn for customer successfully!"),
                data: customer
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.post('/complete_withdraw_request', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const { trx_id } = req.body;
        if (!trx_id)
            return res.status(400).json({ metadata: new metadata(false, "trx_id is required!", "trx_id is required!") });

        var transaction = await new Transaction_Route().Get_Transaction(trx_id);
        if (transaction == null)
            return res.status(400).json({ metadata: new metadata(false, "no transaction found with this id!", "no transaction found with this id!") });

        const _updated_trx = await new Transaction_Route().Update_Transaction(
            trx_id,
            TRANSACTION_STATUS.COMPLETED
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "Deposit for customer successfully!", "Deposit for customer successfully!"),
                data: _updated_trx
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});
//---------------------------------------------



//------------- ADMIN USER --------------------
router.post('/register_admin', async (req, res) => {
    try {
        const { user_id, password } = req.body;
        const { ts, ts_auth } = req.headers;

        const auth = await adminuser_route.Authenticated_User_Without_AccessToken(ts, ts_auth, req.body);
        if (!auth[0])
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        if (!user_id)
            return res.status(400).json({ metadata: new metadata(false, "user_id is required!", "user_id is required!") });
        else if (!password)
            return res.status(400).json({ metadata: new metadata(false, "password is required!", "password is required!") });
        else if (password && password.length < 8)
            return res.status(400).json({ metadata: new metadata(false, "password characters must be 8 or greater!", "incorrect password format") });

        const fetched_user = await adminuser_route.Get_UserWithId(user_id);

        if (fetched_user) {
            return res.status(400).json({ metadata: new metadata(false, "User id already exist!", "User id already exist!") });
        }
        else {
            var createdUser = await AdminUser.create({
                user_id: user_id,
                password: password,

                access_token: "",

                created_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }),
                modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }),
            });

            res.status(200).json(
                {
                    metadata: new metadata(true, "AdminUser Registered Successfully!", "AdminUser Registered Successfully!")
                });
        }
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "Something went wrong!", (e as Error).message)
        });
    }
});

router.post('/login_admin', async (req, res) => {
    try {
        const { user_id, password } = req.body;
        const { ts, ts_auth } = req.headers;

        const auth = await adminuser_route.Authenticated_User_Without_AccessToken(ts, ts_auth, req.body);
        if (!auth[0])
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        if (!user_id)
            return res.status(400).json({ metadata: new metadata(false, "user_id is required!", "user_id is required!") });
        else if (!password)
            return res.status(400).json({ metadata: new metadata(false, "password is required!", "password is required!") });

        const fetched_user = await AdminUser.findOne({ user_id: user_id, password: password });

        if (fetched_user) {
            res.status(200).json(
                {
                    metadata: new metadata(true, "AdminUser Logged In!", "AdminUser Logged In!"),
                    data: AdminUser.toClientObject(fetched_user)
                });
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
//---------------------------------------------



//--------------- GAME SETTINGS ---------------
router.post('/update_gamesettings', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });


        const { user_register_coins, referer_coins, refered_coins, support_url, support_email, whatsapp_number, game_variations, notification_msgs, app_version, has_bot, admin_commission } = req.body;
        var game_settings = await new Setting_Route().Get_Game_Settings();

        if (user_register_coins != null)
            game_settings.user_register_coins = user_register_coins;
        if (referer_coins != null)
            game_settings.referer_coins = referer_coins;
        if (refered_coins != null)
            game_settings.refered_coins = refered_coins;
        if (support_url != null)
            game_settings.support_url = support_url;
        if (support_email != null)
            game_settings.support_email = support_email;
        if (whatsapp_number != null)
            game_settings.whatsapp_number = whatsapp_number;
        if (game_variations != null)
            game_settings.game_variations = game_variations;
        if (notification_msgs != null)
            game_settings.notification_msgs = notification_msgs;
        if (app_version != null)
            game_settings.app_version = app_version;
        if (has_bot != null)
            game_settings.has_bot = has_bot;
        if (admin_commission != null)
            game_settings.admin_commission = admin_commission;

        const updated_game_settings = await GameSettings.findOneAndUpdate(
            { },
            {
                $set: {
                    user_register_coins: game_settings.user_register_coins,
                    referer_coins: game_settings.referer_coins,
                    refered_coins: game_settings.refered_coins,
                    support_url: game_settings.support_url,
                    support_email: game_settings.support_email,
                    whatsapp_number: game_settings.whatsapp_number,
                    game_variations: game_settings.game_variations,
                    notification_msgs: game_settings.notification_msgs,
                    app_version: game_settings.app_version,
                    has_bot: game_settings.has_bot,
                    admin_commission: game_settings.admin_commission
                }
            },
            { new: true }
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "Game_Settings Updated Successfully!", "Game_Settings Updated Successfully!"),
                data: updated_game_settings
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});
//---------------------------------------------



//------------ GAME_TABLES SETTINGS -----------
router.post('/add_gametable', async (req, res) => {
    try {
        const { boot_amount, max_blind, max_bet, pot_limit, is_delux } = req.body;
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        if (!boot_amount)
            return res.status(400).json({ metadata: new metadata(false, "boot_amount is required!", "boot_amount is required!") });
        else if (!max_blind)
            return res.status(400).json({ metadata: new metadata(false, "max_blind is required!", "max_blind is required!") });
        else if (!max_bet)
            return res.status(400).json({ metadata: new metadata(false, "max_bet is required!", "max_bet is required!") });
        else if (!pot_limit)
            return res.status(400).json({ metadata: new metadata(false, "pot_limit is required!", "pot_limit is required!") });
        else if (is_delux == null)
            return res.status(400).json({ metadata: new metadata(false, "is_delux is required!", "is_delux is required!") });


        var createdTable = await TableSettings.create({
            boot_amount: boot_amount,
            max_blind: max_blind,
            max_bet: max_bet,
            pot_limit: pot_limit,
            is_delux: is_delux
        });

        res.status(200).json(
            {
                metadata: new metadata(true, "Table Added Successfully!", "Table Added Successfully!"),
                data: createdTable
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "Something went wrong!", (e as Error).message)
        });
    }
});

router.post('/remove_gametable', async (req, res) => {
    try {
        const { table_id } = req.body;
        const { ts, ts_auth, access_token } = req.headers;

        if (!table_id)
            return res.status(400).json({ metadata: new metadata(false, "table_id is required!", "table_id is required!") });
        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        var deletedTable = await TableSettings.findOneAndDelete({ _id: table_id });
        if (!deletedTable)
            return res.status(400).json({ metadata: new metadata(false, "Something went wrong while deleting table", "Something went wrong while deleting table") });

        res.status(200).json(
            {
                metadata: new metadata(true, "Table Deleted Successfully!", "Table Deleted Successfully!")
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "Something went wrong!", (e as Error).message)
        });
    }
});

router.post('/update_gametable', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });


        const { table_id, boot_amount, max_blind, max_bet, pot_limit, is_delux } = req.body;
        if (!table_id)
            return res.status(400).json({ metadata: new metadata(false, "table_id is required!", "table_id is required!") });

        var table = await new Setting_Route().Get_Table(table_id);
        if (!table)
            return res.status(400).json({ metadata: new metadata(false, "table_id not found!", "table_id not found!") });

        if (boot_amount)
            table.boot_amount = boot_amount;
        if (max_blind)
            table.max_blind = max_blind;
        if (max_bet)
            table.max_bet = max_bet;
        if (pot_limit)
            table.pot_limit = pot_limit;
        if (is_delux != null)
            table.is_delux = is_delux;

        const updated_table = await TableSettings.findOneAndUpdate(
            { _id: table._id },
            {
                $set: {
                    boot_amount: table.boot_amount,
                    max_blind: table.max_blind,
                    max_bet: table.max_bet,
                    pot_limit: table.pot_limit,
                    is_delux: table.is_delux
                }
            },
            { new: true }
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "Table Updated Successfully!", "Table Updated Successfully!"),
                data: updated_table
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});
//---------------------------------------------



//------------ IAP SETTINGS -------------------
router.post('/update_iap', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });


        const { iap_id, coins, price } = req.body;

        var iap = await new Setting_Route().Get_Iap(iap_id);
        if (!iap)
            return res.status(400).json({ metadata: new metadata(false, "iap not found!", "iap not found!") });

        if (coins)
            iap.coins = coins;
        if (price)
            iap.price = price;

        const updated_iap = await IapSettings.findOneAndUpdate(
            { _id: iap._id },
            {
                $set: {
                    coins: iap.coins,
                    price: iap.price
                }
            },
            { new: true }
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "IAP Updated Successfully!", "IAP Updated Successfully!"),
                data: updated_iap
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});
//---------------------------------------------


//---------------- GAMEPLAY -------------------
router.get('/get_all_rooms', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, null);

        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        var result = await new RoomsHandler().Get_All_Rooms();
        if (result[0]) {
            res.status(200).json(
                {
                    metadata: new metadata(true, result[1], result[1]),
                    data: result[2]
                });
        }
        else {
            return res.status(400).json({ metadata: new metadata(false, result[1], result[1]) });
        }
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "Something went wrong!", (e as Error).message)
        });
    }
});

router.post('/get_room_details', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        if (!access_token)
            return res.status(400).json({ metadata: new metadata(false, "access_token is required!", "access_token is required!") });

        const admin_user = await adminuser_route.Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);

        if (!admin_user[0])
            return res.status(400).json({ metadata: new metadata(false, admin_user[1], admin_user[1]) });

        const { room_id } = req.body;
        if (!room_id)
            return res.status(400).json({ metadata: new metadata(false, "room_id is required!", "room_id is required") });

        var result = await new RoomsHandler().Get_Room_Details(room_id);
        if (result[0]) {
            res.status(200).json(
                {
                    metadata: new metadata(true, result[1], result[1]),
                    data: result[2]
                });
        }
        else {
            return res.status(400).json({ metadata: new metadata(false, result[1], result[1]) });
        }
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "Something went wrong!", (e as Error).message)
        });
    }
});
//---------------------------------------------


export { router };