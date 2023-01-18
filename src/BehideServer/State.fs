module BehideServer.State

open BehideServer.Types
open System.Collections.Concurrent

type State =
    { Rooms: ConcurrentDictionary<RoomId, Room>
      Players: ConcurrentDictionary<PlayerId, Player> }

let state =
    { Players = new ConcurrentDictionary<PlayerId, Player>()
      Rooms = new ConcurrentDictionary<RoomId, Room>() }


module private Option =
    let fromBool b = b |> function | true -> Some () | false -> None

module Players =
    let tryGet playerId =
        state.Players.TryGetValue playerId
        |> function
            | true, x -> Some x
            | false, _ -> None

    let tryGetFromIpPort ipPort =
        state.Players.Values
        |> Seq.tryFind (fun player -> player.IpPort = ipPort)

    let tryRemove (playerId: PlayerId) =
        playerId
        |> state.Players.TryRemove
        |> function
            | true, _ -> Some ()
            | false, _ -> None

    let tryUpdate (newPlayer: Player) =
        newPlayer.Id
        |> tryGet
        |> Option.bind (fun previousPlayer ->
            (newPlayer.Id, newPlayer, previousPlayer)
            |> state.Players.TryUpdate
            |> Option.fromBool
        )

    let tryAdd (player: Player) =
        state.Players.TryAdd(player.Id, player)
        |> Option.fromBool

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

    let tryUpdate (newRoom: Room) =
        newRoom.Id
        |> tryGet
        |> Option.bind (fun previousRoom ->
            (newRoom.Id, newRoom, previousRoom)
            |> state.Rooms.TryUpdate
            |> Option.fromBool
        )

    let tryAdd (room: Room) =
        state.Rooms.TryAdd(room.Id, room)
        |> Option.fromBool