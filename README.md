# CLI Tetris – F# / .NET 10

## Project layout

```
Tetris/
├── Tetris.fsproj
└── src/
    ├── Types.fs     – All domain types (Cell, PieceKind, Rotation, GameState, …)
    ├── Pieces.fs    – Shape data, rotation helpers, spawn logic, random picker
    ├── Board.fs     – Collision detection, movement, locking, line-clearing
    ├── Scoring.fs   – Points table and score accumulation
    ├── Input.fs     – Non-blocking keyboard polling + blocking game-over read
    ├── Render.fs    – Terminal drawing (board, NEXT panel, score, controls)
    ├── Game.fs      – State machine, applyAction, gravity tick, main loop
    └── Program.fs   – Entry point
```

## How to build & run

```bash
cd Tetris
dotnet run
```
