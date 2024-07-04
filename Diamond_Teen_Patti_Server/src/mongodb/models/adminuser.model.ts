import mongoose from 'mongoose';

const adminuserSchema = new mongoose.Schema({
    user_id: String,
    password: String,

    access_token: String,

    created_at: Date,
    modified_at: Date,
},
{ versionKey: false });

adminuserSchema.statics.toClientObject = function (adminuser)
{
    const adminuserObject = adminuser?.toObject();
    const clientObject = {
        user_id: adminuserObject.user_id,
        access_token: adminuserObject.access_token,

        created_at: adminuserObject.created_at,
        modified_at: adminuserObject.modified_at
    };

    return clientObject;
};

var AdminUser = mongoose.model('admin_users', adminuserSchema);
export { AdminUser };