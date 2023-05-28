namespace BehideServer.Types

open Smoosh

[<RequireQualifiedAccess>]
type Msg =
    | FailedToParse // Placed first because it's the default value that smoosh take when it fails to parse.
    | Ping

    /// Send the server version (string)
    | CheckServerVersion of string

    /// Send the EpicId
    | CreateRoom of Id

    /// Send the RoomId
    | GetRoom of RoomId

    /// Send the RoomId
    | DeleteRoom of RoomId


    static member private encoder = Encoder.mkEncoder<Msg>()
    static member private decoder = Decoder.mkDecoder<Msg>()

    static member ToBytes msg = Msg.encoder msg |> Array.ofSeq
    member this.ToBytes() = this |> Msg.ToBytes

    static member TryParse(bytes: #seq<byte>) =
        try
            bytes
            |> Seq.toArray
            |> Msg.decoder
            |> Some
        with _ -> None