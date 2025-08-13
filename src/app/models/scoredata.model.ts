import { LeaderBoardModel } from "./leaderboard-model";

export class ScoreData{
    TeamAName:string = "Takım A";
    TeamAScore: number = 0;
    TeamAColor: string = "#2196F3";
    TeamBName: string = "Takım B";
    TeamBScore: number = 0;
    TeamBColor: string = "#FF5722";
    CounterName: string = "Sayaç";
    Counter: number = 0;
    Leaderboard: Array<LeaderBoardModel> = new Array<LeaderBoardModel>();
}