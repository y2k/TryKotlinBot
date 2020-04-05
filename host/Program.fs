module Domain =
    let private help = "Compile & Run simple Kotlin code\n\nSource code (MIT): https://github.com/y2k/TryKotlinBot\nKotlin Slack bot: @SlackToTelegramBot"
    let private inputLimit = 1000
    let private outputLimit = 500
    
    let formatIn (message: string) =
        match message with
        | null | "/start"              -> Error help
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
let main _ =
    printfn "Warmup Kotlin daemon..."
    Interactive.callKotlinService "0" -1 |> Async.RunSynchronously |> ignore
    printfn "Kotlin daemon is ready"

    Telegram.listenForMessages ()
        |> Observable.add (fun x -> 
            async {
                printfn "Get message = %s" x.text
                let pm = Domain.formatIn x.text
                let! resp = match pm with
                            | Error e   -> async.Return e
                            | Ok script -> async {
                                               do! Telegram.setProgress x.user
                                               let! result = Interactive.callKotlinService script 3000
                                               return Domain.formatOut result
                                           }
                do! Telegram.send x.user resp
            } |> Async.Start)

    printfn "Listening for telegram updates..."
    System.Threading.Thread.Sleep -1
    0