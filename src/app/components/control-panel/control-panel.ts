import { Component, computed, ElementRef, Input, model, signal, ViewChild } from '@angular/core';
import { WebSocketService } from '../../services/web-socket-service';
import { ScoreData } from '../../models/scoredata.model';
import { TeamScoreModel } from '../../models/teamscore-model';
import { CounterModel } from '../../models/counter-model';
import { LeaderBoardModel, LeaderboardUpdate } from '../../models/leaderboard-model';
import { LeaderboardSortPipe } from "../../pipes/leaderboardsort-pipe";
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TeamNameModel } from '../../models/teamname-model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-control-panel',
  imports: [FormsModule, RouterLink],
  templateUrl: './control-panel.html',
  styleUrl: './control-panel.scss'
})
export class ControlPanel {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  private scoreDataSignal = signal<ScoreData>({
    TeamAScore: 0,
    TeamBScore: 0,
    Counter: 0,
    Leaderboard: [],
    TeamAName: 'Takım A',
    TeamAColor: '#2196F3',
    TeamBName: 'Takım B',
    TeamBColor: '#FF5722',
    CounterName: 'Sayaç'});
  public scoreData = computed(() => this.scoreDataSignal());
  counterName = model("Sayaç");
  teamAName = model("Takım A");
  teamBName = model("Takım B");
  entryName = model("");
  entryScore = model(0);
  entryColor = model("#5271E0");
  status = model("")
  activeTab: string = "team-scores";
  constructor(private ws: WebSocketService) {
    ws.connect(environment.webSocketUrl + '/controlpanel')
    this.ws.messageSubject.subscribe((data) => {
      this.scoreDataSignal.set({...data})
      this.counterName.set(data.CounterName)
      this.teamAName.set(data.TeamAName)
      this.teamBName.set(data.TeamBName)
    }, (e) => {
      console.log("Error: ", e)
    })
    ws.connectionStatusSubject.subscribe(e => {
      this.status.set(e)
    })
  }

  selectTab(tab:string)
  {
    this.activeTab = tab;
  }

  updateScore(team: string, score:number){
    const newScore = team === 'A' ? this.scoreDataSignal().TeamAScore + score : this.scoreDataSignal().TeamBScore + score;
    const x = new TeamScoreModel();
    x.Team = team;
    x.Score = newScore;
    this.ws.send(x);
  }
  counterNameUpdate()
  {
    const x = new CounterModel;
    x.Name = this.counterName();
    x.Value = this.scoreDataSignal().Counter;
    this.ws.send(x)
  }
  updateCounter(val:number){
    let newValue = this.scoreDataSignal().Counter + val;
    if (newValue < 0) newValue = 0;
    const x = new CounterModel;
    x.Name = this.scoreDataSignal().CounterName;
    x.Value = newValue;
    this.ws.send(x);
  }

  addLeaderboardEntry(){
    const x = new LeaderBoardModel;
    const z = new LeaderboardUpdate;
    x.Name = this.entryName();
    x.Color = this.entryColor();
    x.Score = this.entryScore();
    z.Entry = x;
    if(x.Name != "" && x.Name != null && x.Score >= 0)
    {
      this.ws.send(z)
      this.entryName.set("");
      this.entryScore.set(0)
      this.entryColor.set("#5271E0")
    }else{
      console.error("Hata: girilen değerler hatalı!")
    }
  }
  resetAll(){
    this.removeEnties()
    const counter = new CounterModel
    this.ws.send(counter)
    const tsa = new TeamScoreModel
    tsa.Team = 'A';
    this.ws.send(tsa)
    const tsb = new TeamScoreModel
    tsb.Team = 'B';
    this.ws.send(tsb)
  }
  selectColor(color:string)
  {
    this.entryColor.set(color)
  }
  changeEntryScore(val:number, i:number){
    const x = this.scoreDataSignal().Leaderboard[i]
    if(x != null)
    {
      x.Score += val
      const z = new LeaderboardUpdate;
      z.ActionType = "LeaderboardUpdate";
      z.Entry = x;
      this.ws.send(z)
    }
  }
  deleteEntry(i:number) {
    const x = this.scoreDataSignal().Leaderboard[i]
    if(x != null)
    {
      const z = new LeaderboardUpdate;
      z.ActionType = "LeaderboardRemove";
      z.Entry = x;
      this.ws.send(z)
    }
  }
  updateEntryName(i:number, name: string){
    const x = this.scoreDataSignal().Leaderboard[i]
    if(x != null)
    {
      x.Name = name;
      const z = new LeaderboardUpdate;
      z.ActionType = "LeaderboardUpdate";
      z.Entry = x;
      this.ws.send(z)
    }
  }
  updateEntryScore(i:number, score:number){
    const x = this.scoreDataSignal().Leaderboard[i]
    if(x != null)
    {
      x.Score = score;
      const z = new LeaderboardUpdate;
      z.ActionType = "LeaderboardUpdate";
      z.Entry = x;
      this.ws.send(z)
    }
  }
  updateEntryColor(i:number, color: string){
    const x = this.scoreDataSignal().Leaderboard[i]
    if(x != null)
    {
      x.Color = color;
      const z = new LeaderboardUpdate;
      z.ActionType = "LeaderboardUpdate";
      z.Entry = x;
      this.ws.send(z)
    }
  }
  removeEnties()
  {
    const all = this.scoreDataSignal().Leaderboard
    all.forEach((e,i) => {
      this.deleteEntry(i)
    })
  }
  saveFile()
  {
    const data = this.scoreDataSignal().Leaderboard
    const jsonData = JSON.stringify(data, null, 4);

    // 2. Blob oluştur
    const blob = new Blob([jsonData], { type: 'application/json' });

    // 3. URL oluştur
    const url = URL.createObjectURL(blob);

    // 4. Geçici <a> elementi ile indirme başlat
    const a = document.createElement('a');
    a.href = url;
    a.download = 'data.json'; // kaydedilecek dosya adı
    a.click();

    // 5. URL'yi temizle
    URL.revokeObjectURL(url);
  }
  selectFile()
  {
    this.fileInput.nativeElement.click();
  }
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    const reader = new FileReader();

    reader.onload = () => {
      try {
        const a: LeaderBoardModel[] = JSON.parse(reader.result as string);
        this.removeEnties()
        a.forEach(e => {
          const x = new LeaderBoardModel;
          const z = new LeaderboardUpdate;
          x.Name = e.Name;
          x.Color = e.Color;
          x.Score = e.Score;
          z.Entry = x;
          this.ws.send(z)
        })
      } catch (error) {
        alert('Geçersiz JSON dosyası!');
      }
    };

    reader.readAsText(file);
  }
  updateTeamName(team:string)
  {
    const x = new TeamNameModel;
    x.Team = team;
    x.Name = team == 'A' ? this.teamAName() : this.teamBName()
    this.ws.send(x)
  }
}
