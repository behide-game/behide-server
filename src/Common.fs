module BehideServer.Common

open System.Net

#if DEBUG
let isRelease = false
#else
let isRelease = true
#endif

let getLocalIP () =
    Dns.GetHostName()
    |> Dns.GetHostEntry
    |> fun x -> x.AddressList
    |> Array.find (fun x -> x.AddressFamily = Sockets.AddressFamily.InterNetwork)

let getLocalEP listenPort = IPEndPoint(getLocalIP(), listenPort)