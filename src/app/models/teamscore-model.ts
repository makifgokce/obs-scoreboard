import { IMessageModel } from "./imessage-model";

export class TeamScoreModel implements IMessageModel{
    Action: string = "UpdateScore";
    Team: string = "X";
    Score: number = 0;
}