module BehideServer.App

open BehideServer.Types
open System
open System.Net
open Serilog
open SuperSimpleTcp

let listenPort = 28000

let sendResponse (tcp: SimpleTcpServer) ipPort response =
    tcp.SendAsync(ipPort, response |> Response.ToBytes)
    |> Async.AwaitTask
    |> Async.Start

let proceedMsg (log: ILogger) msgOpt =
    match msgOpt with
    | None ->
        log.Error "Failed to parse message"
        Response.PlayerNotRegistered
    | Some msg ->
        match msg with
        | Msg.Ping -> Response.Ping
        | Msg.RegisterPlayer (clientVersion, username) ->
            let localVersion = Version.GetVersion()

            match clientVersion <> localVersion with
            | true -> Response.BadServerVersion
            | false ->
                let player =
                    { Id = Id.CreateOf PlayerId
                      Username = username }

                player
                |> State.Msg.RegisterPlayer
                |> State.updateState
                |> function
                    | true -> Response.PlayerRegistered player.Id
                    | false -> Response.PlayerNotRegistered
        | Msg.RegisterRoom (playerId, epicId) ->
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
                    | true -> Response.RoomRegistered room.Id
                    | false -> Response.RoomNotRegistered
            | None -> Response.RoomNotRegistered

[<EntryPoint>]
let main _ =
    let log = Common.logger
    let localIP = Common.getLocalIP ()
    let localEP = IPEndPoint(localIP, listenPort)

    let tcp = new SimpleTcpServer(localEP |> string)
    tcp.Start()

    log.Information $"Server started at {localEP}"

    tcp.Events.ClientConnected.Add(fun x -> log.Debug $"Client connected: {x.IpPort}")
    tcp.Events.ClientDisconnected.Add(fun x -> log.Debug $"Client disconnected: {x.Reason}")

    tcp.Events.DataReceived.Add(fun x ->
        log.Debug $"Received msg: {x.Data |> Msg.TryParse}"

        x.Data
        |> Msg.TryParse
        |> proceedMsg log
        |> sendResponse tcp x.IpPort
    )

    while true do
        Console.ReadKey true |> ignore

        State.state.Players |> Seq.iter (fun x -> printfn "%A" x.Value)
        State.state.Rooms |> Seq.iter (fun x -> printfn "%A" x.Value)

    0