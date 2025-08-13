import { Pipe, PipeTransform } from '@angular/core';
import { LeaderBoardModel } from '../models/leaderboard-model';

@Pipe({
  name: 'leaderboardSort',
  standalone: true
})
export class LeaderboardSortPipe implements PipeTransform {
  transform(entries: LeaderBoardModel[] | null | undefined): (LeaderBoardModel & { rank: number })[] {
    if (!entries || !Array.isArray(entries)) return [];
    
    return [...entries]
      .sort((a, b) => b.Score - a.Score)
      .map((entry, index) => ({
        ...entry,
        rank: index + 1
      }));
  }
}