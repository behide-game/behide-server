module BehideServer.Program

open BehideServer
open BehideServer.Types
open BehideServer.Helpers
open BehideServer.Log

open System
open System.Net
open SuperSimpleTcp

let listenPort = 28000

let onDisconnect (x: ConnectionEventArgs) =
    // Unregister the player
    State.state.Players
    |> Seq.tryFind (fun kv -> kv.Value.IpPort = x.IpPort)
    |> Option.map (fun player -> player.Key)
    |> Option.map State.state.Players.TryRemove
    |> function
        | Some (true, player) -> Log.debug "Removed %s from registered players" player.Username
        | Some (false, _) -> Log.debug "Failed to remove player who has disconnected"
        | None -> Log.debug "Player who has disconnected was not registered: %A" x.IpPort

[<EntryPoint>]
let main _ =
    let localIP = Common.getLocalIP ()
    let localEP = IPEndPoint(localIP, listenPort)

    // Start server
    let tcp = new SimpleTcpServer(localEP |> string)
    tcp.Start()

    Log.info "Server started at %A" localEP

    // Setup events
    tcp.Events.ClientConnected.Add(fun x -> Log.debug "Client connected: %A" x.IpPort)
    tcp.Events.ClientDisconnected.Add onDisconnect

    tcp.Events.DataReceived.Add(fun x ->
        x.Data
        |> Msg.TryParse
        |> Option.map (App.proceedMsg x.IpPort)
        |> Option.defaultValue FailedToParseMsg
        |> ResponseBuilder.ToResponse
        |> Response.ToBytes
        |> App.sendResponse tcp x.IpPort
    )

#if DEBUG
    // Setup the debug log system
    while true do
        Console.ReadKey true |> ignore

        if not State.state.Players.IsEmpty || not State.state.Rooms.IsEmpty then
            Log.debug "[STATE] -> Players: %i; Rooms: %i" State.state.Players.Count State.state.Rooms.Count

            Log.debug "[STATE] -> Players: %i" State.state.Players.Count
            State.state.Players
            |> Seq.iteri (fun index element -> Log.debug "\t[STATE] -> [PLAYER] %i: %A" index element)

            Log.debug "[STATE] -> Rooms: %i" State.state.Rooms.Count
            State.state.Rooms
            |> Seq.iteri (fun index element -> Log.debug "\t[STATE] -> [ROOM] %i: %A" index element)
        else
            Log.debug "[STATE] -> State is empty"
#else
    while true do Console.ReadKey true |> ignore
#endif

    0