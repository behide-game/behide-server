open System

// This file is used to do some test and debug.
// It isn't used by the server or even in CI/CD.

type RoomId = RoomId of string

let possibilities =
    [| 'A'; 'B'; 'C'; 'D'; 'E'; 'F'; 'G'; 'H'; 'I'; 'J'; 'K'; 'L';
       'M'; 'N'; 'O'; 'P'; 'Q'; 'R'; 'S'; 'T'; 'U'; 'V'; 'W'; 'X';
       'Y'; 'Z'; '0'; '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; |]

let possibilitiesByte =
    [| 0uy; 1uy; 2uy; 3uy; 4uy; 5uy; 6uy; 7uy; 8uy; 9uy; 10uy; 11uy;
       12uy; 13uy; 14uy; 15uy; 16uy; 17uy; 18uy; 19uy; 20uy; 21uy; 22uy; 23uy;
       24uy; 25uy; 26uy; 27uy; 28uy; 29uy; 30uy; 31uy; 32uy; 33uy; 34uy; 35uy; |]

let Create() : RoomId =
    [| 0..3 |]
    |> Array.map (fun _ ->
        let charIndex =
            Random().NextDouble()
            * (float <| possibilities.Length - 1)
            |> Math.Round
            |> int

        possibilities |> Array.item charIndex)
    |> String
    |> RoomId

let ToBytes(RoomId id) =
    id.ToCharArray()
    |> Array.map (fun char -> Array.findIndex ((=) char) possibilities)
    |> Array.map (fun i -> possibilitiesByte.[i])

let TryParseBytes(bytes: byte []) =
    bytes
    |> Array.map (fun byte -> Array.tryFindIndex ((=) byte) possibilitiesByte)
    |> Array.map (Option.map (fun charIndex -> Array.item charIndex possibilities))
    |> fun x ->
        match x |> Array.contains None with
        | true -> None
        | false ->
            x
            |> Array.map Option.get
            |> String
            |> RoomId
            |> Some

let x = Create()

x
|> ToBytes
|> TryParseBytes