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

Make sure you have .NET 10 SDK installed. Then you may simply run

```bash
cd Tetris
dotnet run
```

## LLM Usage

I used Claude (Anthropic) as a coding assistant throughout this project to help scaffold the initial codebase and debug rendering issues.

**What I used it for:**
I asked it to generate the initial multi-file F# project structure from my requirements document — types, board logic, piece definitions, scoring, input handling, rendering, and the game loop skeleton. It produced a working scaffold with the core APIs defined, leaving the main game logic (locking, line clearing, spawning) for me to implement. I also used it extensively to debug a persistent terminal rendering problem.

**What I had to reprompt or manually fix:**
The `.fsproj` file it generated used forward slashes in `<Compile Include="...">` paths, which broke the build on Windows immediately. The game-over overlay was initially baked into the frame with a hardcoded `frameHeight`, which caused an `ArgumentOutOfRangeException` when the console buffer was smaller than expected — this took several rounds of back-and-forth to properly resolve.

**What the LLM was not able to do correctly:**
The main failure was the terminal rendering on Windows. The LLM suggested several approaches in sequence — `Console.Clear()`, cursor-home with `SetCursorPosition`, ANSI escape codes, buffer size expansion — and each one either introduced a new bug or failed on Windows Terminal specifically. It did not know upfront that `Console.Clear()` behaves differently in Windows Terminal versus a standalone console host. I had to describe the visual symptoms repeatedly and eventually get the correct escape sequence combination (`\x1b[H\x1b[3J\x1b[2J`). 
