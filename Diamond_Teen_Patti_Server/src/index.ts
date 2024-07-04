import { Server } from "colyseus";
import { createServer } from "http";
import express from "express";
import cors from 'cors';
import mongoose from 'mongoose';
import dotenv from 'dotenv';

import { monitor } from "@colyseus/monitor";
import { WebSocketTransport } from "@colyseus/ws-transport";

import { CustomLobbyRoom } from "./lobby/CustomLobbyRoom";

import { router as userRouter } from './mongodb/routes/user.router';
import { router as settingsRouter } from './mongodb/routes/settings.router';
import { router as transactionsRouter } from './mongodb/routes/transaction.router';
import { router as adminRoutes } from './mongodb/routes/admin.router';
import { TPRoom } from "./rooms/TPRoom";
import basicAuth from "express-basic-auth";
import { logger } from "./mongodb/logger";

dotenv.config();
const port = Number(process.env.PORT);

async function Initialize()
{
    try
    {
        function log_api_request(req: any, res: any, next: any) {
            const fullUrl = `${req.protocol}://${req.get('host')}${req.originalUrl}`;
            new logger(`API Called: ${req.method} ${fullUrl}`)
            next();
        }

        await mongoose.connect(process.env.DB_CONNECTION_STR + '/diamond_teen_patti');

        const app = express();
        app.use(cors());
        app.use(express.json());
        app.use(express.urlencoded({ extended: false }));

        app.use(log_api_request);
        app.use("/api/users", userRouter);
        app.use("/api/settings", settingsRouter);
        app.use("/api/transactions", transactionsRouter);
        app.use("/api/admin", adminRoutes);

        //const basicAuthMiddleware = basicAuth({
        //    users: {
        //        "teenpattikings": "IA&qwO!b6ki!y4^",
        //    },
        //    challenge: true
        //});
        //app.use("/colyseus", monitor());

        const gameServer = new Server({
            transport: new WebSocketTransport({
                server: createServer(app),
            })
        });

        gameServer.define('Lobby', CustomLobbyRoom);
        gameServer.define('CLASSIC', TPRoom, { game_type: "CLASSIC" });
        gameServer.define('AK47', TPRoom, { game_type: "AK47" });
        gameServer.define('HUKAM', TPRoom, { game_type: "HUKAM" });
        gameServer.define('JOKER', TPRoom, { game_type: "JOKER" });
        gameServer.define('MUFLIS', TPRoom, { game_type: "MUFLIS" });
        gameServer.define('ROYAL', TPRoom, { game_type: "ROYAL" });
        gameServer.define('POTBLIND', TPRoom, { game_type: "POTBLIND" });

        gameServer.onShutdown(() =>
        {
            console.log("Master process is shutdown!");
        });

        gameServer.listen(port);
        console.log("Server started at:" + new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }) + " and listening to port: " + port);

    } catch (e)
    {
        console.log('Error Initializing server: ' + e);
    }
}

Initialize();