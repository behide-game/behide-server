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

let proceedMsg msg =
    match msg with
    | Msg.FailedToParse -> FailedToParseMsg
    | Msg.Ping -> Ping
    | Msg.CheckServerVersion clientVersion ->
        match clientVersion = Version.GetVersion() with
        | true -> CorrectServerVersion
        | false -> BadServerVersion

    | Msg.CreateRoom epicId ->
        let room =
            { Id = RoomId.Create()
              EpicId = epicId }

        room
        |> State.Rooms.tryAdd
        |> function
            | Some () -> RoomCreated room.Id
            | None -> RoomNotCreated

    | Msg.GetRoom roomId ->
        roomId
        |> State.Rooms.tryGet
        |> Option.map (fun room -> room.EpicId)
        |> Option.map RoomGet
        |> Option.defaultValue RoomNotGet

    | Msg.DeleteRoom roomId ->
        option {
            let! room = State.Rooms.tryGet roomId
            do! room.Id |> State.Rooms.tryRemove

            return RoomDeleted
        }
        |> Option.defaultValue RoomNotDeleted

    |> tap (Log.debug "%A")
