import mongoose from 'mongoose';

const tablesettingsSchema = new mongoose.Schema({
    boot_amount: Number,
    max_blind: Number,
    max_bet: Number,
    pot_limit: Number,
    is_delux: Boolean
},
{ versionKey: false });

const TableSettings = mongoose.model('game_tables', tablesettingsSchema);
export { TableSettings };