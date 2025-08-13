import { IMessageModel } from "./imessage-model";

export class PingModel implements IMessageModel
{
    Action: string = "Ping";
    Message: string = new Date().getTime().toString();
}