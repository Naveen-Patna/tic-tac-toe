import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppService, GameMode, GameState } from './app.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  private service = inject(AppService);

  game: GameState | null = null;
  mode: GameMode = 'TwoPlayer';
  error = '';

  constructor() {
    this.startGame();
  }

  startGame() {
    this.error = '';

    this.service.createGame(this.mode).subscribe({
      next: g => this.game = g,
      error: e => this.error = e?.error?.message ?? 'Operation failed.'
    });
  }

  changeMode(mode: GameMode) {
    this.mode = mode;
    this.startGame();
  }

  clickCell(row: number, column: number) {
    if (!this.game ||
        this.game.status !== 'InProgress' ||
        this.game.board[row][column]) {
      return;
    }

    this.service.move(
      this.game.id,
      this.game.currentPlayer,
      row,
      column
    ).subscribe({
      next: g => this.game = g,
      error: e => this.error = e?.error?.message ?? 'Move failed.'
    });
  }

  undo() {
    if (!this.game?.canUndo) {
      return;
    }

    this.service.undo(this.game.id).subscribe({
      next: g => this.game = g,
      error: e => this.error = e?.error?.message ?? 'Undo failed.'
    });
  }

  resetGame() {
    if (!this.game) {
      return;
    }

    this.service.resetGame(this.game.id).subscribe({
      next: g => this.game = g,
      error: e => this.error = e?.error?.message ?? 'Reset failed.'
    });
  }

  resetScoreboard() {
    this.service.resetScoreboard().subscribe({
      next: () => this.refreshGame(),
      error: e => this.error = e?.error?.message ?? 'Reset failed.'
    });
  }

  refreshGame() {
    if (!this.game) {
      return;
    }

    this.service.getGame(this.game.id).subscribe({
      next: game => this.game = game,
      error: e => this.error = e?.error?.message ?? 'Refresh failed.'
    });
  }

  winning(row: number, column: number) {
    return this.game?.winningCells.includes(row * 3 + column) ?? false;
  }

  message() {
    if (!this.game) {
      return '';
    }

    if (this.game.status === 'Won') {
      return `Player ${this.game.winner} wins!`;
    }

    if (this.game.status === 'Draw') {
      return 'Draw game!';
    }

    return `Player ${this.game.currentPlayer}'s turn`;
  }
}