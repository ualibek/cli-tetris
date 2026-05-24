module Tetris.Pieces

open Tetris.Types

// ── Shape definitions ───────────────────────────────────────────────────────
// Each shape is expressed as offsets from the piece's origin (pivot).
// All four rotations are listed explicitly so you can tweak SRS data later
// without touching any other module.
//
// Layout convention: (col-offset, row-offset)  row grows downward.

/// Returns the four cell offsets for a given piece kind and rotation.
let shapeOffsets (kind: PieceKind) (rot: Rotation) : (int * int) list =
    match kind, rot with
    // ── I ──────────────────────────────────────────────────────────────────
    | I, R0 -> [(-1,0);(0,0);(1,0);(2,0)]
    | I, R1 -> [(1,-1);(1,0);(1,1);(1,2)]
    | I, R2 -> [(-1,1);(0,1);(1,1);(2,1)]
    | I, R3 -> [(0,-1);(0,0);(0,1);(0,2)]
    // ── O ──────────────────────────────────────────────────────────────────
    | O, _  -> [(0,0);(1,0);(0,1);(1,1)]   // symmetric – rotation is identity
    // ── T ──────────────────────────────────────────────────────────────────
    | T, R0 -> [(0,0);(-1,1);(0,1);(1,1)]
    | T, R1 -> [(0,-1);(0,0);(1,0);(0,1)]
    | T, R2 -> [(-1,0);(0,0);(1,0);(0,1)]
    | T, R3 -> [(0,-1);(-1,0);(0,0);(0,1)]
    // ── S ──────────────────────────────────────────────────────────────────
    | S, R0 -> [(0,0);(1,0);(-1,1);(0,1)]
    | S, R1 -> [(0,-1);(0,0);(1,0);(1,1)]
    | S, R2 -> [(0,0);(1,0);(-1,1);(0,1)]  // tiles to R0 (2-state)
    | S, R3 -> [(0,-1);(0,0);(1,0);(1,1)]  // tiles to R1 (2-state)
    // ── Z ──────────────────────────────────────────────────────────────────
    | Z, R0 -> [(-1,0);(0,0);(0,1);(1,1)]
    | Z, R1 -> [(1,-1);(0,0);(1,0);(0,1)]
    | Z, R2 -> [(-1,0);(0,0);(0,1);(1,1)]  // 2-state
    | Z, R3 -> [(1,-1);(0,0);(1,0);(0,1)]  // 2-state
    // ── J ──────────────────────────────────────────────────────────────────
    | J, R0 -> [(-1,0);(-1,1);(0,1);(1,1)]
    | J, R1 -> [(0,-1);(1,-1);(0,0);(0,1)]
    | J, R2 -> [(-1,0);(0,0);(1,0);(1,1)]
    | J, R3 -> [(0,-1);(0,0);(-1,1);(0,1)]
    // ── L ──────────────────────────────────────────────────────────────────
    | L, R0 -> [(1,0);(-1,1);(0,1);(1,1)]
    | L, R1 -> [(0,-1);(0,0);(0,1);(1,1)]
    | L, R2 -> [(-1,0);(0,0);(1,0);(-1,1)]
    | L, R3 -> [(-1,-1);(0,-1);(0,0);(0,1)]

// ── Rotation helpers ────────────────────────────────────────────────────────

let rotateCW (r: Rotation) : Rotation =
    match r with R0 -> R1 | R1 -> R2 | R2 -> R3 | R3 -> R0

let rotateCCW (r: Rotation) : Rotation =
    match r with R0 -> R3 | R1 -> R0 | R2 -> R1 | R3 -> R2

// ── Cell computation ─────────────────────────────────────────────────────────

/// Convert shape offsets + origin into absolute board cells.
let toCells (origin: Cell) (offsets: (int * int) list) : Cell list =
    offsets |> List.map (fun (dc, dr) -> { Col = origin.Col + dc; Row = origin.Row + dr })

/// Build an ActivePiece from kind, rotation, and origin.
let makePiece (kind: PieceKind) (rot: Rotation) (origin: Cell) : ActivePiece =
    let cells = shapeOffsets kind rot |> toCells origin
    { Kind = kind; Rotation = rot; Origin = origin; Cells = cells }

// ── Spawn ────────────────────────────────────────────────────────────────────

/// Canonical spawn origin for each piece (top-centre of a 10-wide board).
/// Adjust row so the piece appears just at the top edge (R7).
let spawnOrigin (kind: PieceKind) : Cell =
    match kind with
    | I -> { Col = 4; Row = 0 }
    | O -> { Col = 4; Row = 0 }
    | _  -> { Col = 4; Row = 1 }

/// Spawn a new active piece at the top of the board.
let spawnPiece (kind: PieceKind) : ActivePiece =
    makePiece kind R0 (spawnOrigin kind)

// ── Random piece selection ────────────────────────────────────────────────────

let private allKinds = [| I; O; T; S; Z; J; L |]
let private rng = System.Random()

/// Pick a random piece kind (R8).
let randomKind () : PieceKind =
    allKinds.[rng.Next(allKinds.Length)]
