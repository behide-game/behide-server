namespace BehideServer.Types

[<RequireQualifiedAccess>]
type ResponseHeader =
    | Ping = 1uy
    | FailedToParseMsg = 2uy

    | BadServerVersion = 3uy
    | CorrectServerVersion = 4uy

    /// Contain RoomId
    | RoomCreated = 5uy
    | RoomNotCreated = 6uy
    | RoomGet = 7uy
    | RoomNotGet = 8uy
    | RoomDeleted = 9uy
    | RoomNotDeleted = 10uy

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
        | ResponseHeader.BadServerVersion, [||] -> createResponse ResponseHeader.BadServerVersion [||]
        | ResponseHeader.CorrectServerVersion, [||] -> createResponse ResponseHeader.CorrectServerVersion [||]
        | ResponseHeader.FailedToParseMsg, [||] -> createResponse ResponseHeader.FailedToParseMsg [||]

        | ResponseHeader.RoomCreated, content when content.Length = 4 (* The length of a RoomId *) -> createResponse ResponseHeader.RoomCreated content
        | ResponseHeader.RoomNotCreated, [||] -> createResponse ResponseHeader.RoomNotCreated [||]

        | ResponseHeader.RoomGet, content when content.Length = 16 (* The length of a Guid *) -> createResponse ResponseHeader.RoomGet content
        | ResponseHeader.RoomNotGet, [||] -> createResponse ResponseHeader.RoomNotGet [||]

        | ResponseHeader.RoomDeleted, [||] -> createResponse ResponseHeader.RoomDeleted [||]
        | ResponseHeader.RoomNotDeleted, [||] -> createResponse ResponseHeader.RoomNotDeleted [||]

        | _ -> None

    static member TryParse(bytes: byte [], out: Response outref) : bool =
        let parsedResponseOpt = bytes |> Response.TryParse

        match parsedResponseOpt with
        | Some x -> out <- x; true
        | None -> false