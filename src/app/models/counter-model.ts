import { IMessageModel } from "./imessage-model";

export class CounterModel implements IMessageModel {
    Action: string = "UpdateCounter";
    Name: string = "";
    Value: number = 0;
}