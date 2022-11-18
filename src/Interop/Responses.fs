namespace BehideServer.Types

[<RequireQualifiedAccess>]
type private ResponseHeader =
    | Ping = 1uy
    | PlayerRegistered = 2uy
    | PlayerNotRegistered = 3uy
    | RoomRegistered = 4uy
    | RoomNotRegistered = 5uy
    | BadServerVersion = 254uy
    | FailedToParseMsg = 255uy

[<RequireQualifiedAccess>]
type Response =
    | Ping
    | PlayerRegistered of PlayerId
    | PlayerNotRegistered
    | RoomRegistered of RoomId
    | RoomNotRegistered
    | BadServerVersion
    | FailedToParseMsg

    static member ToBytes msg =
        let header =
            match msg with
            | Ping -> ResponseHeader.Ping
            | PlayerRegistered _ -> ResponseHeader.PlayerRegistered
            | PlayerNotRegistered -> ResponseHeader.PlayerNotRegistered
            | RoomRegistered _ -> ResponseHeader.RoomRegistered
            | RoomNotRegistered -> ResponseHeader.RoomNotRegistered
            | BadServerVersion -> ResponseHeader.BadServerVersion
            | FailedToParseMsg -> ResponseHeader.FailedToParseMsg
            |> byte

        let content =
            match msg with
            | PlayerRegistered x -> x |> PlayerId.ToBytes
            | RoomRegistered x -> x |> RoomId.ToBytes
            | _ -> [||]

        Array.append [| header |] content

    member this.ToBytes() = this |> Response.ToBytes

    static member TryParse(bytes: #seq<byte>, out: Response outref) : bool = // Use outref instead of option because this function is mainly used by the game so in C#
        let header = bytes |> Seq.head
        let content = bytes |> Seq.tail |> Seq.toArray

        let parsedResponseOpt =
            match header |> LanguagePrimitives.EnumOfValue, content with
            | ResponseHeader.Ping, [||] -> Some Ping
            | ResponseHeader.PlayerRegistered, content ->
                content
                |> PlayerId.TryParseBytes
                |> Option.map PlayerRegistered
            | ResponseHeader.RoomRegistered, content ->
                content
                |> RoomId.TryParseBytes
                |> Option.map RoomRegistered
            | ResponseHeader.BadServerVersion, [| |] -> Some BadServerVersion
            | ResponseHeader.FailedToParseMsg, [| |] -> Some FailedToParseMsg
            | _ -> None

        match parsedResponseOpt with
        | Some x -> out <- x; true
        | None -> false