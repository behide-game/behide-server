module BehideServer.State

open BehideServer.Types
open System.Collections.Concurrent

type State =
    { Rooms: ConcurrentDictionary<RoomId, Room>
      Players: ConcurrentDictionary<PlayerId, Player> }

[<RequireQualifiedAccess>]
type Msg =
    | RegisterPlayer of Player
    | UnregisterPlayer of PlayerId
    | CreateRoom of Room
    | RemoveRoom of RoomId

let state =
    { Players = new ConcurrentDictionary<PlayerId, Player>()
      Rooms = new ConcurrentDictionary<RoomId, Room>() }

let updateState (event: Msg) =
    match event with
    | Msg.RegisterPlayer player -> state.Players.TryAdd(player.Id, player)
    | Msg.UnregisterPlayer playerId -> state.Players.TryRemove(playerId, unbox ())
    | Msg.CreateRoom room -> state.Rooms.TryAdd(room.Id, room)
    | Msg.RemoveRoom roomId -> state.Rooms.TryRemove(roomId, unbox ())


module Players =
    let tryGet playerId =
        state.Players.TryGetValue playerId
        |> function
            | true, x -> Some x
            | false, _ -> None

    let tryGetFromIpPort ipPort =
        state.Players.Values
        |> Seq.tryFind (fun player -> player.IpPort = ipPort)

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