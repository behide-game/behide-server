namespace BehideServer.Types

[<RequireQualifiedAccess>]
type ResponseHeader =
    | Ping = 1uy
    | BadServerVersion = 254uy
    | FailedToParseMsg = 255uy

    /// Contain PlayerId
    | PlayerRegistered = 2uy
    | PlayerNotRegistered = 3uy

    /// Contain RoomId
    | RoomCreated = 4uy
    | RoomNotCreated = 5uy
    | RoomDeleted = 6uy
    | RoomNotDeleted = 7uy

    /// Contain Room
    | RoomJoined = 8uy
    | RoomNotJoined = 9uy

    | RoomLeaved = 10uy
    | RoomNotLeaved = 11uy

[<RequireQualifiedAccess>]
type Response =
    { Header: ResponseHeader
      Content: byte [] }

    static member ToBytes response = Array.append [| response.Header |> byte |] response.Content
    member this.ToBytes() = this |> Response.ToBytes

    static member TryParse(bytes: byte []) : Response option =
        let header = bytes |> Seq.head
        let content = bytes |> Seq.tail |> Seq.toArray

        let createResponse header content =
            { Header = header
              Content = content }
            |> Some

        match header |> LanguagePrimitives.EnumOfValue, content with
        | ResponseHeader.Ping, [||] -> createResponse ResponseHeader.Ping [||]
        | ResponseHeader.PlayerNotRegistered, [||] -> createResponse ResponseHeader.PlayerNotRegistered [||]
        | ResponseHeader.RoomDeleted, [||] -> createResponse ResponseHeader.RoomDeleted [||]
        | ResponseHeader.RoomLeaved, [||] -> createResponse ResponseHeader.RoomLeaved [||]
        | ResponseHeader.RoomNotCreated, [||] -> createResponse ResponseHeader.RoomNotCreated [||]
        | ResponseHeader.RoomNotDeleted, [||] -> createResponse ResponseHeader.RoomNotDeleted [||]
        | ResponseHeader.RoomNotJoined, [||] -> createResponse ResponseHeader.RoomNotJoined [||]
        | ResponseHeader.RoomNotLeaved, [||] -> createResponse ResponseHeader.RoomNotLeaved [||]
        | ResponseHeader.BadServerVersion, [||] -> createResponse ResponseHeader.BadServerVersion [||]
        | ResponseHeader.FailedToParseMsg, [||] -> createResponse ResponseHeader.FailedToParseMsg [||]
        | ResponseHeader.RoomJoined, content -> createResponse ResponseHeader.RoomJoined content
        | ResponseHeader.PlayerRegistered, content when content.Length = 16 (* The length of a Guid *) -> createResponse ResponseHeader.PlayerRegistered content
        | ResponseHeader.RoomCreated, content when content.Length = 4 (* The length of a RoomId *) -> createResponse ResponseHeader.RoomCreated content
        | _ -> None

    static member TryParse(bytes: byte [], out: Response outref) : bool =
        let parsedResponseOpt = bytes |> Response.TryParse

        match parsedResponseOpt with
        | Some x -> out <- x; true
        | None -> false