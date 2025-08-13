import { IMessageModel } from "./imessage-model";

export class TeamNameModel implements IMessageModel
{
    Action: string = "UpdateTeamName";
    Team: string = "A";
    Name: string = "Takım A"; 
}