import express from 'express';
import { ObjectId } from 'mongodb';
import { User } from '../models/user.model';
import { Transaction } from '../models/transaction.model';
import { metadata } from '../metadata';
import { User_Route } from './user.router';
import { RoomsHandler } from '../../rooms/RoomsHandler';
import { Setting_Route } from './settings.router';
import { logger } from '../logger';

enum TRANSACTION_REASON {
    GAMEPLAY = "GAMEPLAY",
    DEPOSIT = "DEPOSIT",
    WITHDRAW = "WITHDRAW",
    REFERRAL = "REFERRAL",
    COIN_PURCHASE = "COIN_PURCHASE",
    ADMIN_COMMISION = "ADMIN_COMMISSION"
}
enum TRANSACTION_STATUS {
    PENDING = "PENDING",
    COMPLETED = "COMPLETED"
}
enum TRANSACTION_TYPE {
    CREDIT = "CR",
    DEBIT = "DR"
}
class Transaction_Route
{
    async Create_Transaction(_user_social_id: String, _amount: number, _transaction_reason: string, _status: string, _message: string)
    {
        if (_amount == 0)
            return;

        Transaction.create({
            user_social_id: _user_social_id,
            amount: _amount > 0 ? _amount : _amount * -1,
            transaction_reason: _transaction_reason,
            transaction_type: _amount > 0 ? TRANSACTION_TYPE.CREDIT : TRANSACTION_TYPE.DEBIT,
            status: _status,
            message: _message,
            created_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }),
            modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' })
        });

        const updated_user = await User.findOneAndUpdate(
            { social_id: _user_social_id },
            { $inc: { wallet_balance: _amount }, $set: { modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }) } },
            { returnOriginal: false }
        );

        console.log("added amount: " + _amount + " newBalance: " + updated_user.wallet_balance);

        await new RoomsHandler().Player_Balance_Updated(updated_user.access_token, updated_user.wallet_balance, updated_user.last_room_id);
        return updated_user;
    }
    async Create_Transaction_PlayerSession(_user_social_id: string, _user_access_token: string, _amount: number, _msg: string, _adminCommision: number)
    {
        const fetched_user = await User.findOne({ social_id: _user_social_id, access_token: _user_access_token });
        if (!fetched_user)
            return;

        await Transaction.create({
            user_social_id: _user_social_id,
            amount: _amount > 0 ? _amount : _amount * -1,
            transaction_reason: TRANSACTION_REASON.GAMEPLAY,
            transaction_type: _amount > 0 ? TRANSACTION_TYPE.CREDIT : TRANSACTION_TYPE.DEBIT,
            status: TRANSACTION_STATUS.COMPLETED,
            message: _msg,
            created_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }),
            modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' })
        });

        if (_adminCommision <= 0)
            return;

        await Transaction.create({
            user_social_id: _user_social_id,
            amount: _adminCommision,
            transaction_reason: TRANSACTION_REASON.ADMIN_COMMISION,
            transaction_type: TRANSACTION_TYPE.DEBIT,
            status: TRANSACTION_STATUS.COMPLETED,
            message: "Admin commission",
            created_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }),
            modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' })
        });
    }
    async Update_Transaction(_trx_id: string, _status: TRANSACTION_STATUS) {
        const updated_transaction = await Transaction.findOneAndUpdate(
            { _id: _trx_id },
            { $set: { status: _status, modified_at: new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }) } },
            { returnOriginal: false }
        );
        return updated_transaction;
    }
    async Get_Transactions(user_social_id: string)
    {
        return await Transaction.find(
            { user_social_id: user_social_id }
        );
    }
    async Get_Transaction(transaction_oid: string) {
        return await Transaction.find(
            { _id: transaction_oid }
        );
    }
    async Get_Transaction_AdminCommission() {
        return await Transaction.find(
            { transaction_reason: TRANSACTION_REASON.ADMIN_COMMISION }
        );
    }
    async Get_Transaction_Withdrawls() {
        return await Transaction.find(
            { transaction_reason: TRANSACTION_REASON.WITHDRAW }
        );
    }
}

const router = express.Router();
const transaction_route = new Transaction_Route();

router.get('/getHistory', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        const auth = await new User_Route().Authenticated_User_With_AccessToken(access_token, ts, ts_auth, null);
        if (auth[0] == null)
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const fetched_transactions = await transaction_route.Get_Transactions(auth[0].social_id);
        res.status(200).json(
            {
                metadata: new metadata(true, "Transactions fetched successfully!", "Transactions fetched successfully!"),
                data: fetched_transactions
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

router.post('/withdraw', async (req, res) => {
    try {
        const { ts, ts_auth, access_token } = req.headers;

        const auth = await new User_Route().Authenticated_User_With_AccessToken(access_token, ts, ts_auth, req.body);
        if (auth[0] == null)
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const { amount } = req.body;
        if (!amount)
            return res.status(400).json({ metadata: new metadata(false, "amount is required!", "amount is required!") });

        if (auth[0].wallet_balance < amount)
            return res.status(400).json({ metadata: new metadata(false, "customer balance is not sufficient", "customer balance is not sufficient") });

        var updated_customer = await new Transaction_Route().Create_Transaction(
            auth[0].social_id,
            amount > 0 ? -amount : amount,
            TRANSACTION_REASON.WITHDRAW,
            TRANSACTION_STATUS.PENDING,
            "Withdraw by user"
        );

        res.status(200).json(
            {
                metadata: new metadata(true, "Withdrawl request sent successfully!", "Withdrawl request sent successfully!"),
                data: updated_customer
            });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

export { router, Transaction_Route, TRANSACTION_REASON, TRANSACTION_STATUS, TRANSACTION_TYPE };