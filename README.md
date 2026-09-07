# Tic Tac Toe Assessment

A simple Tic Tac Toe application built with Angular and .NET Web API.

## Tech Stack

- Angular
- TypeScript
- .NET Web API
- C#
- xUnit

## Features

- Two-player mode
- Player vs Computer mode
- 3x3 Tic Tac Toe board
- Win and draw detection
- Winning cell highlighting
- Move history
- Undo
- Scoreboard
- Reset Game
- Reset Scoreboard
- REST API
- In-memory game and scoreboard storage

## Architecture

```text
Angular
   ↓
REST API
   ↓
Controller
   ↓
GameService
   ↓
Repository
   ↓
In-Memory Storage
```

The backend is the source of truth for the game state and scoreboard.

## Backend

### Run

```bash
cd backend/TicTacToe.Api
dotnet restore
dotnet run
```

The API runs on:

```text
http://localhost:5000
```

Swagger:

```text
http://localhost:5000/swagger
```

## Frontend

### Run

Requires Node.js and npm.

```bash
cd frontend
npm install
npm start
```

Open:

```text
http://localhost:4200
```

## API

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/games` | Create a game |
| GET | `/api/games/{id}` | Get game state |
| POST | `/api/games/{id}/moves` | Make a move |
| POST | `/api/games/{id}/undo` | Undo |
| POST | `/api/games/{id}/reset` | Reset game |
| GET | `/api/scoreboard` | Get scoreboard |
| POST | `/api/scoreboard/reset` | Reset scoreboard |

## Computer Mode

The computer plays as O.

Move selection priority:

1. Win if possible
2. Block X if required
3. Take the center
4. Take a corner
5. Take any available cell

## Undo

- Two-player mode: removes the last move.
- Computer mode: removes the human move and the computer's response.
- Undo is disabled after a game is completed.

## Tests

Run backend tests:

```bash
cd backend/TicTacToe.Tests
dotnet test
```

Tests cover:

- Valid and invalid moves
- Turn switching
- Row, column and diagonal wins
- Draw
- Undo
- Reset
- Scoreboard
- Computer moves
- Moves after game completion

## Assumptions

- Game state is stored in memory.
- Restarting the backend clears games and scoreboard.
- Authentication and persistent database storage are outside the scope of this assessment.
