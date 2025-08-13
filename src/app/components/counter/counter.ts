import { Component, computed, signal } from '@angular/core';
import { WebSocketService } from '../../services/web-socket-service';
import { ScoreData } from '../../models/scoredata.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-counter',
  imports: [],
  templateUrl: './counter.html',
  styleUrl: './counter.scss'
})
export class Counter {
  private scoreDataSignal = signal<ScoreData>({
    TeamAScore: 0,
    TeamBScore: 0,
    Counter: 0,
    Leaderboard: [],
    TeamAName: 'Takım A',
    TeamAColor: '#2196F3',
    TeamBName: 'Takım B',
    TeamBColor: '#FF5722',
    CounterName: 'Sayaç'
  });
  public scoreData = computed(() => this.scoreDataSignal());
  constructor(private ws: WebSocketService) {
    this.ws.connect(environment.webSocketUrl + '/counter')
    this.ws.messageSubject.subscribe((data) => {
      this.scoreDataSignal.set({...data})
    }, (e) => {
      console.log("Error: ", e)
    })
  }
}
