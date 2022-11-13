namespace BehideServer.Types

open System

[<RequireQualifiedAccess>]
type private MsgHeader =
    | Ping = 1uy
    | RegisterGame = 2uy

[<RequireQualifiedAccess>]
type Msg =
    | Ping of int
    | RegisterGame of string

    static member toBytes msg =
        let content =
            match msg with
            | Ping x -> x |> BitConverter.GetBytes
            | RegisterGame x -> x |> Text.Encoding.ASCII.GetBytes

        let header =
            match msg with
            | Ping _ -> MsgHeader.Ping
            | RegisterGame _ -> MsgHeader.RegisterGame
            |> byte

        Array.append [| header |] content

    member this.toBytes() = this |> Msg.toBytes

    static member tryParse(bytes: #seq<byte>) =
        let header = bytes |> Seq.head
        let content = bytes |> Seq.tail |> Seq.toArray

        match header |> LanguagePrimitives.EnumOfValue with
        | MsgHeader.Ping -> content |> BitConverter.ToInt32 |> Ping |> Some
        | MsgHeader.RegisterGame ->
            content
            |> Text.Encoding.ASCII.GetString
            |> RegisterGame
            |> Some
        | _ -> None
