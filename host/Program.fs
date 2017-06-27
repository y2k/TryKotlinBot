module RX = Observable
module T  = Telegram
module I  = Interactive

module Domain =
    let private help = "Compile & Run simple Kotlin code\n\nSource code (MIT): https://github.com/y2k/TryKotlinBot\nKotlin Slack bot: @SlackToTelegramBot"
    let private inputLimit = 150
    let private outputLimit = 300
    
    let formatIn (message: string) =
        match message with
        | "/start"                     -> Error help
        | x when x.Length > inputLimit -> Error (sprintf "Code too long (limit is %O characters)" inputLimit)
        | _                            -> Ok (message.Replace('”', '"').Replace('“', '"'))
    
    let formatOut (message: string) =
        match message.Trim(' ', '\r', '\n') with
        | ""                            -> "[ERROR] Empty output"
        | x when x.Length > outputLimit -> 
            message.Substring(0, outputLimit) + "...\n\n[RESULT TOO LONG (" + (string message.Length) + ")]"
        | _                             -> message
        |> sprintf "```\n%O\n```"

[<EntryPoint>]
let main argv =
    printfn "Warmup Kotlin daemon..."
    I.callKotlinService "0" -1 |> Async.RunSynchronously |> ignore
    printfn "Kotlin daemon is ready"

    T.listenForMessages argv.[0]
        |> RX.add (fun x -> 
            async {
                let pm = Domain.formatIn x.text
                let! resp = match pm with
                            | Error e   -> async.Return e
                            | Ok script -> async {
                                               do! T.setProgress argv.[0] x.user
                                               let! result = I.callKotlinService script 2000
                                               return Domain.formatOut result
                                           }
                do! T.send argv.[0] x.user resp
            } |> Async.Start)

    printfn "Listening for telegram updates..."
    System.Threading.Thread.Sleep(-1)
    0