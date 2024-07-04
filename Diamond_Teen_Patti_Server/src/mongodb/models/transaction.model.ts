import { ObjectId } from 'mongodb';
import mongoose from 'mongoose';

const transactionSchema = new mongoose.Schema({
    user_social_id: String,
    amount: Number,
    transaction_reason: String, //GAMEPLAY, DEPOSIT, WITHDRAW
    transaction_type: String, //CR, DR
    status: String, // PENDING, COMPLETED
    message: String,
    created_at: Date,
    modified_at: Date
},
{ versionKey: false });

const Transaction = mongoose.model('transactions', transactionSchema);
export { Transaction };