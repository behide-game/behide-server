module BehideServer.State

open BehideServer.Types
open System.Collections.Concurrent

type State = { Rooms: ConcurrentDictionary<RoomId, Room> }

let state = { Rooms = new ConcurrentDictionary<RoomId, Room>() }


module private Option =
    let fromBool b = b |> function | true -> Some () | false -> None

module Rooms =
    let tryGet (roomId: RoomId) =
        roomId
        |> state.Rooms.TryGetValue
        |> function
            | true, x -> Some x
            | false, _ -> None

    let tryRemove (roomId: RoomId) =
        roomId
        |> state.Rooms.TryRemove
        |> function
            | true, _ -> Some ()
            | false, _ -> None

    let tryAdd (room: Room) =
        state.Rooms.TryAdd(room.Id, room)
        |> Option.fromBool