module Tetris.Input

open Tetris.Types

// ── Raw key reading ───────────────────────────────────────────────────────────
//
// The game loop calls `pollAction` once per tick.
// Console.KeyAvailable prevents blocking so the gravity timer can fire
// even when the player is idle.

/// Map a ConsoleKey to a game Action, returning None for unrecognised keys.
let private keyToAction (key: System.ConsoleKey) : Action option =
    match key with
    | System.ConsoleKey.LeftArrow  -> Some MoveLeft
    | System.ConsoleKey.RightArrow -> Some MoveRight
    | System.ConsoleKey.UpArrow    -> Some RotateCW
    | System.ConsoleKey.Spacebar   -> Some HardDrop
    | System.ConsoleKey.Q          -> Some Quit
    | System.ConsoleKey.R          -> Some Restart
    | _                            -> None

/// Non-blocking key poll. Returns the mapped Action if a key is waiting,
/// otherwise None. The game loop calls this each tick.
let pollAction () : Action option =
    if System.Console.KeyAvailable then
        let info = System.Console.ReadKey(intercept = true)
        keyToAction info.Key
    else
        None

/// Blocking read – used on the Game Over screen (R24) where we wait
/// for the player to choose Restart or Quit.
let waitForRestartOrQuit () : Action =
    let rec loop () =
        let info = System.Console.ReadKey(intercept = true)
        match keyToAction info.Key with
        | Some Restart -> Restart
        | Some Quit    -> Quit
        | _            -> loop ()
    loop ()
