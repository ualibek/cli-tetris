module Tetris.Render

open Tetris.Types
open Tetris.Pieces

// ── Constants ────────────────────────────────────────────────────────────────

[<Literal>]
let private EmptyCell  = ". "
[<Literal>]
let private FilledCell = "[]"
[<Literal>]
let private LeftWall   = "<!"
[<Literal>]
let private RightWall  = "!>"

// ── Helpers ──────────────────────────────────────────────────────────────────

let private activeCellSet (piece: ActivePiece) : Set<int * int> =
    piece.Cells |> List.map (fun c -> c.Row, c.Col) |> Set.ofList


// ── Row / panel builders ──────────────────────────────────────────────────────

let private renderRow
    (board      : Board)
    (activeCells: Set<int * int>)
    (row        : int) : string =
    let cells =
        [ 0 .. BoardWidth - 1 ]
        |> List.map (fun col ->
            if   Set.contains (row, col) activeCells then FilledCell
            elif board.[row, col]                     then FilledCell
            else EmptyCell)
        |> String.concat ""
    sprintf "%s%s%s" LeftWall cells RightWall

let private renderFloor () : string =
    sprintf "%s%s%s" LeftWall (String.replicate BoardWidth "= ") RightWall

let private renderNextPanel (kind: PieceKind) : string list =
    let pivot = { Col = 1; Row = 1 }
    let cells =
        shapeOffsets kind R0
        |> toCells pivot
        |> List.map (fun c -> c.Row, c.Col)
        |> Set.ofList
    let rows =
        [ 0 .. 3 ] |> List.map (fun r ->
            [ 0 .. 3 ]
            |> List.map (fun c -> if Set.contains (r, c) cells then FilledCell else EmptyCell)
            |> String.concat "")
    [ "NEXT:" ] @ rows   // 5 lines

let private controlsPanel () : string list =
    [ "CONTROLS:"
      "<-  ->  move"
      "^   rotate CW"
      "Spc hard drop"
      "Q   quit" ]

// ── Frame assembly ────────────────────────────────────────────────────────────

/// Build the complete frame as a string list.  Always produces exactly
/// frameHeight lines so the cursor never drifts between ticks.
let private frameHeight = BoardHeight + 1 + 6   // 21 board/floor + 6 overlay slots = 27

let buildFrame (state: GameState) : string list =
    let active = activeCellSet state.Active

    // Left column: 20 board rows + floor = 21 lines
    let boardLines =
        [ for r in 0 .. BoardHeight - 1 -> renderRow state.Board active r ]
        @ [ renderFloor () ]

    // Right column: NEXT(5) + blank + SCORE(1) + blank + CONTROLS(5) = 13 lines
    let rightPanel =
        renderNextPanel state.Next
        @ [ "" ]
        @ [ sprintf "SCORE: %d" state.Score ]
        @ [ "" ]
        @ controlsPanel ()

    let totalRows  = max boardLines.Length rightPanel.Length
    let boardWidth = BoardWidth * 2 + 4
    let leftCol  = boardLines @ List.replicate (totalRows - boardLines.Length) (String.replicate boardWidth " ")
    let rightCol = rightPanel @ List.replicate (totalRows - rightPanel.Length) ""

    let mainLines =
        List.map2 (fun l r -> sprintf "  %s    %s" l r) leftCol rightCol

    // Game-over overlay: always 6 lines (blank + 5 box lines).
    // When Playing these are blank so frameHeight stays constant.
    let overlayLines =
        if state.Status = GameOver then
            [ ""
              "  +==========================+"
              "  |        GAME  OVER        |"
              sprintf "  |   Final score: %-10d|" state.Score
              "  |  [R] Restart  [Q] Quit   |"
              "  +==========================+" ]
        else
            List.replicate 6 ""

    // Guarantee exactly frameHeight lines.
    let combined = mainLines @ overlayLines
    let pad      = List.replicate (max 0 (frameHeight - combined.Length)) ""
    combined @ pad |> List.truncate frameHeight

// ── Output ────────────────────────────────────────────────────────────────────

/// Render a complete frame in-place: jump to (0,0) and overwrite every line.
let render (state: GameState) : unit =
    try
        let lines      = buildFrame state
        let clearWidth = max 1 (System.Console.WindowWidth - 1)
        let frame      = lines |> List.map (fun l -> l.PadRight(clearWidth)) |> String.concat "\n"
        System.Console.Write("\x1b[H\x1b[3J\x1b[2J")
        System.Console.Write(frame)
    with
    | :? System.ArgumentOutOfRangeException -> ()