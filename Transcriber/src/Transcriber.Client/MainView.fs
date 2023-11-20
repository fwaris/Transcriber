module Transcriber.Client.View.Main
open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Microsoft.AspNetCore.Components
open Transcriber.Client.Model

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

let homePage model dispatch =
    div {
        attr.``class`` "content"

        p { "Transcribe audio using whisper" }

        div {
            attr.``class`` "level"
            div {
                attr.``class`` "level-left"
                div {
                    attr.``class`` "level-item"
                    button {
                        on.click (fun e -> dispatch ToggleRecord)
                        attr.disabled model.isTranscribing
                        attr.``class`` "button"
                        span {
                            attr.``class`` "icon is-large"                                      
                            match model.rstate with 
                            | Recording -> i { attr.``class`` "mdil mdil-24px mdil-stop"}
                            | Ready   -> i { attr.``class`` "mdil mdil-24px mdil-microphone"}
                        }
                    }
                }
                div {
                    attr.``class`` "level-item"
                    audio {
                        "controls" => ""
                        "autoplay" => ""
                        attr.src model.mUrl
                    }
                }
            }
        }
        h1 {attr.empty()}
        div {
            attr.``class`` "level"
            div {
                attr.``class`` "level-left"
                div {
                    attr.``class`` "level-item"
                    button {
                        on.click (fun e -> dispatch Transcribe)
                        attr.disabled model.isTranscribing
                        attr.``class`` "button is-primary"
                        attr.disabled (model.mUrl.Length=0)
                        "Transcribe"
                    }
                }
                div {
                    attr.``class`` "level-item"
                    button {
                        attr.disabled model.isTranscribing
                        on.click (fun e -> dispatch TranscribeTest)
                        attr.``class`` "button is-info"                
                        "Transcribe test file 'kennedy.mp3' (20.0 sec)"
                    }
                }
            }
        }
        h1{attr.empty()}
        div {
            
            textarea {
                attr.style "width:75%;"
                attr.rows 10
                model.transcribedText
            }
        }
        div {
            span {
                "Transcription time (sec): "
                $"{(model.endTranscribeTime - model.startTranscribeTime).TotalSeconds}"
            }
        }
    }

let counterPage model dispatch = div {attr.empty()}

let errorNotification errorText closeCallback =
    div {
        attr.``class`` "notification is-warning"
        cond closeCallback <| function
        | None -> empty()
        | Some closeCallback -> button { attr.``class`` "delete"; on.click closeCallback }
        text errorText
    }

let view model dispatch =
    div {
        attr.``class`` "columns"
        div {
            attr.``class`` "column"
            section {
                attr.``class`` "section"
                cond model.page <| function
                | Home -> homePage model dispatch
                | Counter -> counterPage model dispatch
                div {
                    attr.id "notification-area"
                    cond model.error <| function
                    | None -> empty()
                    | Some err -> errorNotification err (Some (fun _ -> dispatch ClearError))
                }
            }
        }
    }


