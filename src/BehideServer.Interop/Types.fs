namespace BehideServer.Types

open System

type Id = Guid
module Id =
    let CreateOf (x: Id -> _) = Guid.NewGuid() |> x

    let TryParse (str: string) : Id option =
        match Guid.TryParse str with
        | true, id -> Some id
        | false, _ -> None

    let TryParseBytes (bytes: byte []) : Id option =
        try
            new Guid(bytes) |> Some
        with
        | _ -> None

    let ToBytes (id: Id) = id.ToByteArray()

type GuidHelper =
    static member TryParseBytes (bytes: byte [], out: Guid outref) : bool =
        match bytes |> Id.TryParseBytes with
        | Some id -> out <- id; true
        | None -> false

type RoomId =
    private | RoomId of string

    static member private possibilities =
        [| 'A'; 'B'; 'C'; 'D'; 'E'; 'F'; 'G'; 'H'; 'I'; 'J'; 'K'; 'L';
           'M'; 'N'; 'O'; 'P'; 'Q'; 'R'; 'S'; 'T'; 'U'; 'V'; 'W'; 'X';
           'Y'; 'Z'; '0'; '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; |]
    static member possibilitiesByte = RoomId.possibilities |> Array.indexed |> Array.map (fst >> byte)

    static member Create() : RoomId =
        [| 0..3 |]
        |> Array.map (fun _ ->
            let charIndex =
                Random().NextDouble()
                * (float <| RoomId.possibilities.Length - 1)
                |> Math.Round
                |> int

            RoomId.possibilities |> Array.item charIndex)
        |> String
        |> RoomId

    override this.ToString() = this |> function RoomId rawRoomId -> rawRoomId.ToUpper()

    static member TryParse(str: string) =
        let str = str.ToUpper()

        str
        |> Seq.forall (fun char -> Array.contains char RoomId.possibilities)
        |> function
            | true -> Some (RoomId str)
            | false -> None

    static member TryParse(str: string, out: RoomId outref) =
        match str |> RoomId.TryParse with
        | Some roomId -> out <- roomId; true
        | None -> false

    static member ToBytes(RoomId id) =
        id.ToCharArray()
        |> Array.map (fun char -> Array.findIndex ((=) char) RoomId.possibilities)
        |> Array.map (fun i -> RoomId.possibilitiesByte.[i])

    static member TryParseBytes(bytes: byte []) =
        bytes
        |> Array.map (fun byte -> Array.tryFindIndex ((=) byte) RoomId.possibilitiesByte)
        |> Array.map (Option.map (fun charIndex -> Array.item charIndex RoomId.possibilities))
        |> fun x ->
            match x |> Array.contains None with
            | true -> None
            | false ->
                x
                |> Array.map Option.get
                |> String
                |> RoomId
                |> Some

    static member TryParseBytes(bytes: byte [], out: RoomId outref) =
        match bytes |> RoomId.TryParseBytes with
        | Some roomId -> out <- roomId; true
        | None -> false

type Room =
    { Id: RoomId
      EpicId: Id }