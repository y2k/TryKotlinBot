module Telegram

open System
open Telegram.Bot
open Telegram.Bot.Types

let private token = Environment.GetEnvironmentVariable "TELEGRAM_TOKEN"

type Message = { text: string; user: string }

let setProgress (user: string) =
    let bot = TelegramBotClient token
    bot.SendChatActionAsync(ChatId.op_Implicit user, Types.Enums.ChatAction.Typing) |> Async.AwaitTask

let listenForMessages () =
    let bot = TelegramBotClient token
    let result = bot.OnUpdate 
                 |> Observable.map (fun x -> x.Update)
                 |> Observable.map (fun x -> { text = x.Message.Text; user = string x.Message.From.Id })
    bot.StartReceiving()
    result

let send (user: string) message =
    async {
        try
            let bot = TelegramBotClient token
            let! _ = bot.SendTextMessageAsync(ChatId.op_Implicit user, message, parseMode = Types.Enums.ParseMode.Markdown) |> Async.AwaitTask
            ()
        with
        | e -> printfn "Log error: %O" e
    }