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
    { Rooms = new ConcurrentDictionary<RoomId, Room>()
      Players = new ConcurrentDictionary<PlayerId, Player>() }

let updateState (event: Msg) =
    let log = Common.logger

    match event with
    | Msg.RegisterPlayer player -> state.Players.TryAdd(player.Id, player)
    | Msg.UnregisterPlayer playerId -> state.Players.TryRemove(playerId, unbox ())
    | Msg.CreateRoom room -> state.Rooms.TryAdd(room.Id, room)
    | Msg.RemoveRoom roomId -> state.Rooms.TryRemove(roomId, unbox ())