export class logger {
    constructor(data: any)
    {
        console.log(`[${new Date().toLocaleString('en', { timeZone: 'Asia/Kolkata' }).toString()}] ${data}`);
    }
}