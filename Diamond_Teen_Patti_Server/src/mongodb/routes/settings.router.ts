import express from 'express';
import { metadata } from '../metadata';
import { GameSettings } from '../models/gamesettings.model';
import { TableSettings } from '../models/tablesettings.model';
import { User_Route } from './user.router';
import { IapSettings } from '../models/iapsettings.model';

class Setting_Route {

    async Get_Game_Settings() {
        return await GameSettings.findOne({}, { _id: false });
    }
    async Get_Tables() {
        return await TableSettings.find({});
    }
    async Get_Table(obj_id: string) {
        return await TableSettings.findOne({ _id: obj_id });
    }
    async Get_Iaps() {
        return await IapSettings.find({});
    }
    async Get_Iap(_id: string) {
        return await IapSettings.findOne({ _id: _id });
    }
}


const router = express.Router();
const data_obj = new Setting_Route();

router.get('/getGameSettings', async (req, res) => {
    try {
        const { ts, ts_auth } = req.headers;

        const auth = await new User_Route().Authenticated_User_Without_AccessToken(ts, ts_auth, null);
        if (!auth[0])
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const game_settings = await data_obj.Get_Game_Settings();
        res.status(200).json({
                metadata: new metadata(true, "successful", "successful"),
                data: game_settings
        });
    } catch (e) {
        res.status(400).json({
                metadata: new metadata(false, "failed", (e as Error).message)
            });
    }
});

router.get('/getTables', async (req, res) => {
    try {
        const { ts, ts_auth } = req.headers;

        const auth = await new User_Route().Authenticated_User_Without_AccessToken(ts, ts_auth, null);
        if (!auth[0])
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const table_settings = await data_obj.Get_Tables();
        res.status(200).json({
                metadata: new metadata(true, "successful", "successful"),
                data: table_settings
            });
    } catch (e) {
        res.status(400).json({
                metadata: new metadata(false, "failed", (e as Error).message)
            });
    }
});

router.get('/getIaps', async (req, res) => {
    try {
        const { ts, ts_auth } = req.headers;

        const auth = await new User_Route().Authenticated_User_Without_AccessToken(ts, ts_auth, null);
        if (!auth[0])
            return res.status(400).json({ metadata: new metadata(false, auth[1], auth[1]) });

        const iap_settings = await data_obj.Get_Iaps();
        res.status(200).json({
            metadata: new metadata(true, "successful", "successful"),
            data: iap_settings
        });
    } catch (e) {
        res.status(400).json({
            metadata: new metadata(false, "failed", (e as Error).message)
        });
    }
});

export { router, Setting_Route };