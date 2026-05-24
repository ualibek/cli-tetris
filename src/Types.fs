module Tetris.Types

// ── Coordinates ────────────────────────────────────────────────────────────
/// Column (x) and row (y) on the board. (0,0) = top-left cell.
type Cell = { Col: int; Row: int }

// ── Piece identity ─────────────────────────────────────────────────────────
type PieceKind = I | O | T | S | Z | J | L

/// One of the four standard rotations (0 = spawn, 1 = 90°CW, 2 = 180°, 3 = 270°CW).
type Rotation = R0 | R1 | R2 | R3

// ── Active piece ───────────────────────────────────────────────────────────
/// The piece currently falling. `origin` is the pivot; `cells` are
/// board-space coordinates computed from the shape definition.
type ActivePiece = {
    Kind    : PieceKind
    Rotation: Rotation
    Origin  : Cell          // pivot / reference point in board space
    Cells   : Cell list     // absolute board coordinates (derived)
}

// ── Board ──────────────────────────────────────────────────────────────────
/// Width = 10, Height = 20 per the requirements.
[<Literal>]
let BoardWidth  = 10
[<Literal>]
let BoardHeight = 20

/// The locked (settled) cells on the board.
/// `true` means the cell is occupied.
type Board = bool[,]   // [row, col]  –– row 0 is the top

// ── Game state ─────────────────────────────────────────────────────────────
type GameStatus =
    | Playing
    | GameOver

type GameState = {
    Board      : Board
    Active     : ActivePiece
    Next       : PieceKind
    Score      : int
    Status     : GameStatus
}

// ── Player actions ─────────────────────────────────────────────────────────
type Action =
    | MoveLeft      // R9
    | MoveRight     // R10
    | RotateCW      // R11
    | HardDrop      // R13
    | Quit
    | Restart
    | SoftDrop
