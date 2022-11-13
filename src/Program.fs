module BehideServer.App

open BehideServer.Types
open System
open System.Net
open SuperSimpleTcp

let listenPort = 28000

[<EntryPoint>]
let main _ =
    let log = Common.getLogger ()
    let localIP = Common.getLocalIP ()
    let localEP = IPEndPoint(localIP, listenPort)

    let tcp = new SimpleTcpServer(localEP |> string)
    tcp.Start()

    log.Information $"Server started at {localEP}"

    tcp.Events.ClientConnected.Add(fun x ->
        log.Debug $"Client connected: {x.Reason}"
    )
    tcp.Events.ClientDisconnected.Add(fun x ->
        log.Debug $"Client disconnected: {x.Reason}"
    )
    tcp.Events.DataReceived.Add(fun x ->
        log.Debug $"Data received: {x.Data}"

        let msg = Msg.tryParse x.Data
        log.Debug $"Msg: {msg}"
    )


    let mutable keyPressed = Console.ReadKey true
    while keyPressed.Key <> ConsoleKey.C
        && keyPressed.Modifiers <> ConsoleModifiers.Control do
        keyPressed <- Console.ReadKey true

    0