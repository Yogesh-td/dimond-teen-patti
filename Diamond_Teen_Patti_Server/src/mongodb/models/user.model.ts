import mongoose from 'mongoose';

const userSchema = new mongoose.Schema({
    social_id: String,
    password: String,

    username: String,
    avatar_index: Number,
    refer_code: String,
    refered_by: String,

    wallet_balance: Number,
    total_winnings: Number,

    access_token: String,
    last_room_id: String,
    last_room_token: String,
    is_loggedin: Boolean,
    is_active: Boolean,

    created_at: Date,
    modified_at: Date,
},
{ versionKey: false });

userSchema.statics.toClientObject = function (user)
{
    const userObject = user?.toObject();
    const clientObject = {
        social_id: userObject.social_id,
        password: userObject.password,

        username: userObject.username,
        avatar_index: userObject.avatar_index,
        refer_code: userObject.refer_code,
        refered_by: userObject.refered_by,

        wallet_balance: userObject.wallet_balance,
        total_winnings: userObject.total_winnings,

        last_room_id: userObject.last_room_id,
        last_room_token: userObject.last_room_token,

        is_active: userObject.is_active
    };

    return clientObject;
};
userSchema.statics.toLeaderboardObject = function (user, rank, is_local) {
    const userObject = user?.toObject();
    const clientObject = {
        rank: rank,
        username: userObject.username,
        avatar_index: userObject.avatar_index,
        total_winnings: userObject.total_winnings,
        is_local: is_local
    };

    return clientObject;
};

var User = mongoose.model('users', userSchema);
export { User };