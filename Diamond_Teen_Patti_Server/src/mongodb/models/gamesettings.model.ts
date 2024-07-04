import mongoose from 'mongoose';

const gamesettingsSchema = new mongoose.Schema({
    user_register_coins: Number,
    referer_coins: Number,
    refered_coins: Number,
    support_url: String,
    support_email: String,
    whatsapp_number: String,
    game_variations: [{
        mode: String,
        is_active: Boolean
    }],
    notification_msgs: Array,
    app_version: String,
    has_bot: Boolean,
    admin_commission: Number
},
{ versionKey: false });

const GameSettings = mongoose.model('game_settings', gamesettingsSchema);
export { GameSettings };