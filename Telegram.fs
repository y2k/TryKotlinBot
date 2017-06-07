module Telegram

open System
open Telegram.Bot
module RX = Observable

type Message = { text: string; user: string }

let setProgress (token: string) (user: string) =
    let bot = TelegramBotClient(token)
    bot.SendChatActionAsync(user, Types.Enums.ChatAction.Typing) |> Async.AwaitTask

let listenForMessages (token: string) =
    let bot = TelegramBotClient(token)
    let result = bot.OnUpdate 
                 |> RX.map (fun x -> x.Update)
                 |> RX.map (fun x -> { text = x.Message.Text; user = string x.Message.From.Id })
    bot.StartReceiving()
    result

let send (token: string) (user: string) message =
    async {
        try
            let bot = TelegramBotClient(token)
            let! _ = bot.SendTextMessageAsync(user, message, parseMode = Types.Enums.ParseMode.Markdown) |> Async.AwaitTask
            ()
        with
        | e -> printfn "Log error: %O" e
    }