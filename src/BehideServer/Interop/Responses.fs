namespace BehideServer.Types

[<RequireQualifiedAccess>]
type ResponseHeader =
    | Ping = 1uy
    /// Contain PlayerId <see cref="PlayerId"/>
    | PlayerRegistered = 2uy
    | PlayerNotRegistered = 3uy
    /// Contain RoomId <see cref="RoomId"/>
    | RoomCreated = 4uy
    | RoomNotCreated = 5uy
    | RoomDeleted = 6uy
    | RoomNotDeleted = 7uy
    | BadServerVersion = 254uy
    | FailedToParseMsg = 255uy

[<RequireQualifiedAccess>]
type Response =
    { Header: ResponseHeader
      Content: byte [] }

    static member ToBytes response =
        Array.append ([| response.Header |> byte |]) response.Content

    member this.ToBytes() = this |> Response.ToBytes

    static member TryParse(bytes: byte []) : Response option =
        let header = bytes |> Seq.head
        let content = bytes |> Seq.tail |> Seq.toArray

        let createResponse header content =
            { Header = (header |> LanguagePrimitives.EnumOfValue)
              Content = content }
            |> Some

        match header |> LanguagePrimitives.EnumOfValue, content with
        | ResponseHeader.Ping, [||]
        | ResponseHeader.PlayerNotRegistered, [||]
        | ResponseHeader.RoomNotCreated, [||]
        | ResponseHeader.RoomDeleted, [||]
        | ResponseHeader.RoomNotDeleted, [||]
        | ResponseHeader.BadServerVersion, [||]
        | ResponseHeader.FailedToParseMsg, [||] -> createResponse header [||]
        | ResponseHeader.PlayerRegistered, content when content.Length = 16 -> // 16 is the length of a guid
            createResponse header content
        | ResponseHeader.RoomCreated, content when content.Length = 4 -> createResponse header content
        | _ -> None

    static member TryParse(bytes: byte [], out: Response outref) : bool =
        let parsedResponseOpt = bytes |> Response.TryParse

        match parsedResponseOpt with
        | Some x ->
            out <- x
            true
        | None -> false