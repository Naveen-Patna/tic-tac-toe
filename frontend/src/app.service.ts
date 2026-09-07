import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export type Player = 'X' | 'O';
export type GameMode = 'TwoPlayer' | 'Computer';

export interface Move {
  number: number; player: Player; row: number; column: number;
}

export interface GameState {
  id: string;
  board: (Player|null)[][];
  currentPlayer: Player;
  mode: GameMode;
  status: 'InProgress'|'Won'|'Draw';
  winner: Player|null;
  winningCells: number[];
  moveHistory: Move[];
  scoreboard: {xWins:number; oWins:number; draws:number};
  canUndo: boolean;
}

@Injectable({providedIn:'root'})
export class AppService {
  private http = inject(HttpClient);

  getGame(id:string) { return this.http.get<GameState>(`/api/games/${id}`); }

  createGame(mode: GameMode) {
    return this.http.post<GameState>('/api/games', {mode});
  }

  move(id:string, player:Player, row:number, column:number) {
    return this.http.post<GameState>(`/api/games/${id}/moves`, {player,row,column});
  }

  undo(id:string) { return this.http.post<GameState>(`/api/games/${id}/undo`, {}); }

  resetGame(id:string) { return this.http.post<GameState>(`/api/games/${id}/reset`, {}); }

  resetScoreboard() { return this.http.post<void>('/api/scoreboard/reset', {}); }
}
