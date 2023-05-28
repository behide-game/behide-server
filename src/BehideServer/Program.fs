module BehideServer.Program

open BehideServer
open BehideServer.Types
open BehideServer.Helpers
open BehideServer.Log

open System
open SuperSimpleTcp

let listenPort = 28000

[<EntryPoint>]
let main _ =
    // Start server
    let tcp = new SimpleTcpServer(listenPort |> Common.getLocalEP |> string)
    tcp.Start()

    Log.info "Server started at %A" (listenPort |> Common.getLocalEP)

    // Setup events
    tcp.Events.ClientConnected.Add(fun x -> Log.debug "Client connected: %A" x.IpPort)
    tcp.Events.ClientDisconnected.Add(fun x -> Log.debug "%A disconnected" x.IpPort)

    tcp.Events.DataReceived.Add (fun x ->
        x.Data
        |> Msg.TryParse
        |> Option.map App.proceedMsg
        |> Option.defaultValue FailedToParseMsg
        |> ResponseBuilder.ToResponse
        |> Response.ToBytes
        |> App.sendResponse tcp x.IpPort
    )

#if DEBUG
    // Setup the debug log system
    while true do
        Console.ReadLine() |> ignore

        if not State.state.Rooms.IsEmpty then
            Log.debug "[STATE] -> Rooms: %i" State.state.Rooms.Count

            State.state.Rooms
            |> Seq.iteri (fun index room -> Log.debug "\t[STATE] -> [ROOM] %i: %A" index room)
        else
            Log.debug "[STATE] -> State is empty"
#else
    while true do
        Console.ReadLine() |> ignore
#endif

    0
