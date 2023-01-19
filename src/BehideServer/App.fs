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
    | Msg.FailedToParse -> FailedToParseMsg
    | Msg.Ping -> Ping
    | Msg.RegisterPlayer (clientVersion, username) ->
        option {
            let playerExist = State.Players.tryGetFromIpPort ipPort |> Option.isSome

            match clientVersion <> Version.GetVersion() with
            | true -> return BadServerVersion
            | false when username.Length < 1 || playerExist -> return PlayerNotRegistered
            | false ->
                let player =
                    { Id = Id.CreateOf PlayerId
                      IpPort = ipPort
                      Username = username
                      CurrentRoomId = None }

                return! player
                        |> State.Players.tryAdd
                        |> Option.map (fun _ -> PlayerRegistered player.Id)
        }
        |> Option.defaultValue PlayerNotRegistered

    | Msg.CreateRoom (playerId, epicId) ->
        match State.Players.tryGet playerId with
        | None -> RoomNotCreated
        | Some creator ->
            let room =
                { Id = RoomId.Create()
                  EpicId = epicId
                  CurrentRound = 0
                  MaxPlayers = 4
                  Owner = creator.Id
                  Players = [| creator |] }

            room
            |> State.Rooms.tryAdd
            |> function
                | Some () -> RoomCreated room.Id
                | None -> RoomNotCreated
    | Msg.DeleteRoom roomId ->
        option {
            let! _player = State.Players.tryGetFromIpPort ipPort
            let! room = State.Rooms.tryGet roomId
            do! room.Id |> State.Rooms.tryRemove

            return RoomDeleted
        }
        |> Option.defaultValue RoomNotDeleted

    | Msg.JoinRoom roomId ->
        option {
            let! player = State.Players.tryGetFromIpPort ipPort
            let! room = State.Rooms.tryGet roomId

            // Check if player is already in room
            let playerAlreadyJoined = room.Players |> Array.tryFind (fun x -> x.Id = player.Id) |> Option.isSome
            match playerAlreadyJoined with
            | true -> do! None
            | false -> ()

            // Update room
            let newPlayerList = Array.append [| player |] room.Players
            let newRoom = { room with Players = newPlayerList }
            do! State.Rooms.tryUpdate newRoom

            // Update player
            let newPlayer = { player with CurrentRoomId = Some newRoom.Id }
            do! State.Players.tryUpdate newPlayer

            return RoomJoined room
        }
        |> Option.defaultValue RoomNotJoined
    | Msg.LeaveRoom ->
        option {
            let! player = State.Players.tryGetFromIpPort ipPort
            let! playerRoomId = player.CurrentRoomId
            let! room = State.Rooms.tryGet playerRoomId

            // Update room
            let newPlayerList = room.Players |> Array.filter (fun x -> x.Id <> player.Id)
            let newRoom = { room with Players = newPlayerList }
            do! State.Rooms.tryUpdate newRoom

            // Update player
            let newPlayer = { player with CurrentRoomId = None }
            do! State.Players.tryUpdate newPlayer

            return RoomLeaved newPlayer.Id
        }
        |> Option.defaultValue RoomNotLeaved

    |> tap (Log.debug "%A")
