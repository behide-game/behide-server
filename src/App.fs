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
        let localVersion = Version.GetVersion()

        match clientVersion <> localVersion with
        | true -> BadServerVersion
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
        let creatorOpt =
            State.state.Players.Values
            |> Seq.tryFind (fun player -> player.Id = playerId)

        match creatorOpt with
        | Some creator ->
            let room =
                { Id = RoomId.Create()
                  EpicId = epicId
                  Creator = creator.Id
                  Players = [| creator |] }

            room
            |> State.Msg.CreateRoom
            |> State.updateState
            |> function
                | true -> RoomCreated room.Id
                | false -> RoomNotCreated
        | None -> RoomNotCreated
    | Msg.DeleteRoom roomId ->
        State.state.Rooms.Values
        |> Seq.tryFind (fun room -> room.Id = roomId)
        |> function
            | Some _ ->
                roomId
                |> State.state.Rooms.TryRemove
                |> function
                    | true, _ -> RoomDeleted
                    | false, _ -> RoomNotDeleted
            | None -> RoomNotDeleted
    |> tap (Log.debug "%A")