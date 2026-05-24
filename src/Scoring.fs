module Tetris.Scoring

// ── Scoring table (R19) ──────────────────────────────────────────────────────
//
//  Lines cleared simultaneously │ Points
//  ─────────────────────────────┼────────
//  1 (Single)                   │   100
//  2 (Double)                   │   300
//  3 (Triple)                   │   500
//  4 (Tetris)                   │   800

/// Return the points awarded for clearing `n` lines simultaneously.
/// Returns 0 for 0 lines (no clear) or any value outside 1-4.
let pointsFor (linesCleared: int) : int =
    match linesCleared with
    | 1 -> 100
    | 2 -> 300
    | 3 -> 500
    | 4 -> 800
    | _ -> 0    // 0 lines cleared or out-of-range: no points

/// Apply a line-clear result to the current score and return the new score (R20).
let updateScore (current: int) (linesCleared: int) : int =
    current + pointsFor linesCleared
