module BehideServer.App

open BehideServer.Types
open BehideServer.Helpers
open BehideServer.Log
open SuperSimpleTcp
open FsToolkit.ErrorHandling

let sendResponse (tcp: SimpleTcpServer) ipPort (response: byte []) =
    tcp.SendAsync(ipPort, response)
    |> Async.AwaitTask
    |> Async.Start

let proceedMsg ipPort msg =
    match msg with
    | Msg.Ping -> Ping
    | Msg.RegisterPlayer (clientVersion, username) ->
        match clientVersion <> Version.GetVersion() with
        | true -> BadServerVersion
        | false when username.Length < 1 -> PlayerNotRegistered
        | false ->
            let player =
                { Id = Id.CreateOf PlayerId
                  IpPort = ipPort
                  Username = username }

            player
            |> State.Msg.RegisterPlayer
            |> State.updateState
            |> function
                | true -> PlayerRegistered player.Id
                | false -> PlayerNotRegistered

    | Msg.CreateRoom (playerId, epicId) ->
        match State.Players.tryGet playerId with
        | None -> RoomNotCreated
        | Some creator ->
            let room =
                { Id = RoomId.Create()
                  EpicId = epicId
                  Owner = creator.Id
                  Players = [| creator |] }

            room
            |> State.Msg.CreateRoom
            |> State.updateState
            |> function
                | true -> RoomCreated room.Id
                | false -> RoomNotCreated
    | Msg.DeleteRoom roomId ->
        option {
            let! _playerId = State.Players.tryGetFromIpPort ipPort
            let! room = State.Rooms.tryGet roomId
            do! room.Id |> State.Rooms.tryRemove

            return RoomDeleted
        }
        |> Option.defaultValue RoomNotDeleted

    | Msg.GetRoom roomId ->
        option {
            let! _playerId = State.Players.tryGetFromIpPort ipPort
            let! room = State.Rooms.tryGet roomId

            return room.EpicId
        }
        |> Option.map RoomFound
        |> Option.defaultValue RoomNotFound

    |> tap (Log.debug "%A")
