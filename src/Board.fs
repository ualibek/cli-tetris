module Tetris.Board

open Tetris.Types
open Tetris.Pieces

// ── Construction ─────────────────────────────────────────────────────────────

/// Return a fresh empty board (all false).
let empty () : Board =
    Array2D.create BoardHeight BoardWidth false

// ── Bounds & collision ────────────────────────────────────────────────────────

/// True when a single cell is within the playfield walls (not the floor check).
let private inBounds (c: Cell) : bool =
    c.Col >= 0 && c.Col < BoardWidth &&
    c.Row >= 0 && c.Row < BoardHeight

/// True when a single cell does NOT collide with walls, floor, or locked cells.
let private cellFree (board: Board) (c: Cell) : bool =
    inBounds c && not board.[c.Row, c.Col]

/// True when ALL cells of the piece are free (no wall/floor/locked overlap).
/// This is the central validity predicate used by move and rotate.
let isValid (board: Board) (cells: Cell list) : bool =
    cells |> List.forall (cellFree board)

// ── Movement helpers ──────────────────────────────────────────────────────────

/// Translate a piece by (dc, dr). Returns the moved piece if valid, else None.
let tryMove (board: Board) (piece: ActivePiece) (dc: int) (dr: int) : ActivePiece option =
    let newOrigin = { Col = piece.Origin.Col + dc; Row = piece.Origin.Row + dr }
    let newCells  = shapeOffsets piece.Kind piece.Rotation |> toCells newOrigin
    if isValid board newCells then
        Some { piece with Origin = newOrigin; Cells = newCells }
    else
        None

/// Rotate the piece 90° CW (R11). Returns rotated piece if valid, else None.
let tryRotateCW (board: Board) (piece: ActivePiece) : ActivePiece option =
    let newRot   = rotateCW piece.Rotation
    let newCells = shapeOffsets piece.Kind newRot |> toCells piece.Origin
    if isValid board newCells then
        Some { piece with Rotation = newRot; Cells = newCells }
    else
        None

/// Move left (R9). Returns moved piece or original on collision.
let moveLeft (board: Board) (piece: ActivePiece) : ActivePiece =
    tryMove board piece -1 0 |> Option.defaultValue piece

/// Move right (R10). Returns moved piece or original on collision.
let moveRight (board: Board) (piece: ActivePiece) : ActivePiece =
    tryMove board piece 1 0 |> Option.defaultValue piece

/// Rotate CW (R11). Returns rotated piece or original on collision.
let rotateCW' (board: Board) (piece: ActivePiece) : ActivePiece =
    tryRotateCW board piece |> Option.defaultValue piece

/// Attempt a one-row gravity drop. Returns (piece, didLand).
/// `didLand = true` means the piece could not move down and should be locked.
let gravityStep (board: Board) (piece: ActivePiece) : ActivePiece * bool =
    match tryMove board piece 0 1 with
    | Some moved -> moved, false
    | None       -> piece, true

/// Hard-drop the piece to the lowest valid position (R13).
let hardDrop (board: Board) (piece: ActivePiece) : ActivePiece =
    let rec drop p =
        match tryMove board p 0 1 with
        | Some moved -> drop moved
        | None       -> p
    drop piece

// ── Locking ───────────────────────────────────────────────────────────────────

/// Lock the active piece into the board in-place (R14).
/// Mutates the board array (efficient; board is already mutable by design).
let lockPiece (board: Board) (piece: ActivePiece) : unit =
    piece.Cells |> List.iter (fun c ->
        if inBounds c then
            board.[c.Row, c.Col] <- true)

// ── Line clearing ──────────────────────────────────────────────────────────────

/// True when every cell in the given row is occupied.
let private rowFull (board: Board) (row: int) : bool =
    [ 0 .. BoardWidth - 1 ] |> List.forall (fun col -> board.[row, col])

/// Clear all full rows and shift remaining rows down (R15, R16).
/// Returns the number of rows cleared.
let clearLines (board: Board) : int =
    // Collect non-full rows from bottom to top, then rebuild.
    let surviving =
        [ 0 .. BoardHeight - 1 ]
        |> List.filter (fun r -> not (rowFull board r))
    let cleared = BoardHeight - surviving.Length

    // Write surviving rows into the bottom of the board.
    let mutable dest = BoardHeight - 1
    for src in List.rev surviving do
        for col in 0 .. BoardWidth - 1 do
            board.[dest, col] <- board.[src, col]
        dest <- dest - 1

    // Fill newly vacated top rows with false.
    for r in 0 .. cleared - 1 do
        for col in 0 .. BoardWidth - 1 do
            board.[r, col] <- false

    cleared

// ── Spawn-overlap check (game over) ───────────────────────────────────────────

/// True when a freshly spawned piece overlaps locked cells (R22 – game over).
let spawnBlocked (board: Board) (piece: ActivePiece) : bool =
    piece.Cells |> List.exists (fun c -> inBounds c && board.[c.Row, c.Col])
