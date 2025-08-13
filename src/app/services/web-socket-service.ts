import { Injectable } from '@angular/core';
import { Subject, takeUntil, timer } from 'rxjs';
import { ScoreData } from '../models/scoredata.model';
import { IMessageModel } from '../models/imessage-model';
import { PingModel } from '../models/ping-model';

@Injectable({
  providedIn: 'root'
})
export class WebSocketService {
  private socket: WebSocket | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 1000;
  private reconnectInterval = 10000;
  private connectionTimeout: any = null;
  private heartbeatInterval: any = null;
  public messageSubject = new Subject<ScoreData>();
  public connectionStatusSubject = new Subject<'connecting' | 'connected' | 'disconnected'>();
  constructor() {
    this.connectionStatusSubject.next('disconnected');
  }

  connect(url:string){
    if (this.socket?.readyState === WebSocket.OPEN) {
      console.log('WebSocket zaten bağlı');
      return;
    }

    this.cleanup();
    this.connectionStatusSubject.next('connecting');

    try {
      this.socket = new WebSocket(url);
      
      this.socket.onopen = () => {
        console.log('WebSocket bağlantısı kuruldu');
        this.reconnectAttempts = 0;
        this.connectionStatusSubject.next('connected');
        this.startHeartbeat();
      };

      this.socket.onmessage = (event) => {
        try {
          const data: ScoreData = JSON.parse(event.data);
          this.messageSubject.next(data);
        } catch (error) {
          console.error('Mesaj işlenirken hata:', error);
        this.connectionStatusSubject.next('disconnected');
        }
      };

      this.socket.onerror = (error) => {
        console.error('WebSocket hatası:', error);
        this.connectionStatusSubject.next('disconnected');
      };

      this.socket.onclose = (event) => {
        console.log(`WebSocket bağlantısı kapatıldı. Kod: ${event.code}, Sebep: ${event.reason}`);
        this.stopHeartbeat();
        this.connectionStatusSubject.next('disconnected');
        this.tryReconnect(url);
      };

      // Bağlantı zaman aşımı
      this.connectionTimeout = setTimeout(() => {
        if (this.socket?.readyState !== WebSocket.OPEN) {
          console.error('WebSocket bağlantı zaman aşımına uğradı');
          this.connectionStatusSubject.next('disconnected');
          this.tryReconnect(url);
        }
      }, 5000);

    } catch (error) {
      console.error('WebSocket oluşturulurken hata:', error);
      this.connectionStatusSubject.next('disconnected');
      this.tryReconnect(url);
    }
  }
private tryReconnect(url: string): void {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      console.log(`${this.reconnectInterval / 1000} saniye sonra yeniden bağlanmaya çalışılıyor... (${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
      
      timer(this.reconnectInterval)
        .pipe(takeUntil(this.connectionStatusSubject))
        .subscribe(() => this.connect(url));
    } else {
      console.error('Maksimum yeniden bağlanma denemesi aşıldı');
    }
  }

  private startHeartbeat(): void {
    this.heartbeatInterval = setInterval(() => {
      if (this.socket?.readyState === WebSocket.OPEN) {
        this.send(new PingModel);
      }
    }, 30000);
  }

  private stopHeartbeat(): void {
    if (this.heartbeatInterval) {
      clearInterval(this.heartbeatInterval);
      this.heartbeatInterval = null;
    }
  }

  send(message: IMessageModel): void {
    if (this.socket?.readyState === WebSocket.OPEN) {
      try {
        const jsonMessage = JSON.stringify(message);
        this.socket.send(jsonMessage);
      } catch (error) {
        console.error('Mesaj gönderilirken hata:', error);
      }
    } else {
      console.warn('WebSocket bağlantısı açık değil');
    }
  }

  private cleanup(): void {
    this.stopHeartbeat();
    
    if (this.connectionTimeout) {
      clearTimeout(this.connectionTimeout);
      this.connectionTimeout = null;
    }

    if (this.socket) {
      this.socket.onopen = null;
      this.socket.onmessage = null;
      this.socket.onerror = null;
      this.socket.onclose = null;
      
      if (this.socket.readyState === WebSocket.OPEN) {
        this.socket.close();
      }
      
      this.socket = null;
    }
  }

  disconnect(): void {
    this.cleanup();
  }
  
}
