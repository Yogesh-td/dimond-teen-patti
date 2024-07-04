export class metadata {
    status: boolean;
    user_msg: string;
    dev_msg: string;

    constructor(_status: boolean, _user_msg: string, _dev_msg: string) {
        this.status = _status;
        this.user_msg = _user_msg;
        this.dev_msg = _dev_msg;
    }
}