const crypto = require('crypto');
const ALGORITHM = 'aes-256-cbc';
const INPT_ENCODING = 'base64';
const OTPT_ENCODING = 'utf-8';

class AES_Crypto
{
    Validate(data: string, hashedData: string): boolean 
    {
        return this.Decrypt(hashedData) == JSON.stringify(data);
    }
    Validate_Query(ts: string, hashed_ts: string)
    {
        return this.Decrypt(hashed_ts) == ts;
    }
    Encrypt(data: string): string {
        const iv = this.generate_iv();
        const cipher = crypto.createCipheriv(ALGORITHM, process.env.CYPER_KEY, iv);
        const encryptedData = cipher.update(data, OTPT_ENCODING, INPT_ENCODING) + cipher.final(INPT_ENCODING) + iv;
        return encryptedData;
    }
    Decrypt(data: string): string {
        const iv = data.slice(-16);
        const decipher = crypto.createDecipheriv(ALGORITHM, process.env.CYPER_KEY, iv);
        let originalData = decipher.update(data.slice(0, data.length - 16), INPT_ENCODING, OTPT_ENCODING) + decipher.final(OTPT_ENCODING);

        return originalData;
    }
    private generate_iv() {
        let result = '';
        const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        const charactersLength = characters.length;
        let counter = 0;
        while (counter < 16) {
            result += characters.charAt(Math.floor(Math.random() * charactersLength));
            counter += 1;
        }
        return result;
    }

}


export { AES_Crypto }