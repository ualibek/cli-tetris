module Tetris.Game

open Tetris.Types
open Tetris.Pieces
open Tetris.Scoring

// ── Default settings ──────────────────────────────────────────────────────────

/// Gravity interval in milliseconds (R12 – tune experimentally).
[<Literal>]
let DefaultDropInterval = 500

// ── Initial state factory ─────────────────────────────────────────────────────

/// Build a brand-new game state with an empty board and two random pieces.
let initialState () : GameState =
    let board  = Board.empty ()
    let active = spawnPiece (randomKind ())
    let next   = randomKind ()
    { Board        = board
      Active       = active
      Next         = next
      Score        = 0
      Status       = Playing}

let lockAndTransition (state: GameState) (dropped: ActivePiece) : GameState = 
    Board.lockPiece state.Board dropped
    let cleared = Board.clearLines state.Board
    let score = updateScore state.Score cleared
    let nextActive = spawnPiece state.Next
    let nextNext = randomKind ()

    let nextGameState = {state with Active=nextActive
                                    Next = nextNext
                                    Score = score}
    if Board.spawnBlocked state.Board nextActive then 
        {nextGameState with Status=GameOver}
    else
        nextGameState


// ── Action dispatch ───────────────────────────────────────────────────────────
//
// `applyAction` translates one player action into a new GameState.
// You implement the bodies marked TODO; the signatures and surrounding
// plumbing are already in place.

/// Apply a single player action to the current state, returning the next state.
/// This is a pure function – it does NOT mutate the board; call Board.lockPiece
/// inside the lock branch and then copy the board reference forward.
let applyAction (state: GameState) (action: Action) : GameState =
    match state.Status with
    | GameOver ->
        // Only Restart / Quit are meaningful after game over.
        match action with
        | Restart -> initialState ()
        | Quit    -> state   // caller checks this and exits the loop
        | _       -> state

    | Playing ->
        match action with
        | MoveLeft ->
            // TODO: call Board.moveLeft and return updated state
            let moved = Board.moveLeft state.Board state.Active
            { state with Active = moved }

        | MoveRight ->
            // TODO: call Board.moveRight and return updated state
            let moved = Board.moveRight state.Board state.Active
            { state with Active = moved }

        | RotateCW ->
            // TODO: call Board.rotateCW' and return updated state
            let rotated = Board.rotateCW' state.Board state.Active
            { state with Active = rotated }

        | SoftDrop ->
            // TODO: attempt one-row drop; lock if landed
            let (dropped, didland) = Board.gravityStep state.Board state.Active
            if didland then
                // ── LOCK BRANCH ── implement: lock, clear lines, update score,
                //                  spawn next piece, check game-over
                lockAndTransition state dropped
            else
                { state with Active = dropped }

        | HardDrop ->
            let dropped = Board.hardDrop state.Board state.Active
            lockAndTransition state dropped

        | Restart -> initialState ()
        | Quit    -> state

// ── Gravity tick ──────────────────────────────────────────────────────────────

/// Called by the game loop every `DropInterval` ms regardless of player input.
/// Equivalent to an automatic SoftDrop (R12).
let gravityTick (state: GameState) : GameState =
    if state.Status = GameOver then state
    else applyAction state SoftDrop

// ── Main loop ─────────────────────────────────────────────────────────────────
//
// The loop runs at ~60 Hz polling for input; gravity fires on a separate
// elapsed-time counter so it is decoupled from the render rate.

/// Entry-point for the game.  Call this from Program.fs.
let run () : unit =
    System.Console.CursorVisible <- false
    System.Console.Title <- "CLI Tetris"
    System.Console.Clear()

    let mutable state      = initialState ()
    let mutable lastDrop   = System.Diagnostics.Stopwatch.GetTimestamp()
    let mutable running    = true

    while running do
        // ── Input ─────────────────────────────────────────────────────────
        match Input.pollAction () with
        | Some Quit ->
            running <- false
        | Some action ->
            state <- applyAction state action
        | None -> ()

        // ── Gravity ───────────────────────────────────────────────────────
        let now     = System.Diagnostics.Stopwatch.GetTimestamp()
        let elapsed = (now - lastDrop) * 1000L / System.Diagnostics.Stopwatch.Frequency
        if elapsed >= int64 DefaultDropInterval && state.Status = Playing then
            state    <- gravityTick state
            lastDrop <- now

        // ── Render ────────────────────────────────────────────────────────
        Render.render state

        // ── Frame cap: ~60 fps ────────────────────────────────────────────
        System.Threading.Thread.Sleep(16)
