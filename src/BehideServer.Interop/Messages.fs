namespace BehideServer.Types

open Smoosh

[<RequireQualifiedAccess>]
type Msg =
    | FailedToParse

    | Ping
    /// Send the server version (string) and the player's username (string)
    | RegisterPlayer of string * string
    /// Send the PlayerId and the EpicId
    | CreateRoom of PlayerId * Id
    /// Send the RoomId
    | DeleteRoom of RoomId
    /// Send the RoomId
    | GetRoom of RoomId

    static member private encoder = Encoder.mkEncoder<Msg>()
    static member private decoder = Decoder.mkDecoder<Msg>()

    static member ToBytes msg = Msg.encoder msg |> Array.ofSeq
    member this.ToBytes() = this |> Msg.ToBytes

    static member TryParse(bytes: #seq<byte>) =
        try Msg.decoder (bytes |> Seq.toArray) |> Some
        with | _ -> None
        |> function
            | Some FailedToParse | None -> None
            | msgOpt -> msgOpt