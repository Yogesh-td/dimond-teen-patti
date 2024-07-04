import mongoose from 'mongoose';

const iapsettingsSchema = new mongoose.Schema({
    product_id: String,
    coins: Number,
    price: Number
},
{ versionKey: false });

const IapSettings = mongoose.model('iap_settings', iapsettingsSchema);
export { IapSettings };