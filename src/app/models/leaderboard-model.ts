import { IMessageModel } from "./imessage-model";

export class LeaderBoardModel {
    Id?: string;
    Name: string = "";
    Color: string = "";
    Score: number = 0;
}

export class LeaderboardUpdate implements IMessageModel
{
    Action: string = "UpdateLeaderboard";
    ActionType: string = "LeaderboardAdd"; // LeaderboardAdd | LeaderboardUpdate | LeaderboardRemove
    Entry: LeaderBoardModel = new LeaderBoardModel;
}