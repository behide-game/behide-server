module BehideServer.App

open BehideServer.Types
open BehideServer.Helpers
open BehideServer.Log
open SuperSimpleTcp

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
        let playerIdOpt = State.Players.tryGetFromIpPort ipPort
        let roomOpt = State.Rooms.tryGet roomId

        match playerIdOpt.IsSome && roomOpt.IsSome with
        | true ->
            roomId
            |> State.state.Rooms.TryRemove
            |> function
                | true, _ -> RoomDeleted
                | false, _ -> RoomNotDeleted
        | _ -> RoomNotDeleted

    |> tap (Log.debug "%A")
