module Transcriber.Server.Index

open Bolero
open Bolero.Html
open Bolero.Server.Html
open Transcriber

let page = doctypeHtml {
    head {
        meta { attr.charset "UTF-8" }
        meta { attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0" }
        title { "Whisper Transcription" }
        ``base`` { attr.href "/" }
        link { attr.rel "stylesheet"; attr.href @"https://cdn.jsdelivr.net/npm/bulma@0.9.4/css/bulma.min.css"}
        link {attr.rel "stylesheet"; attr.href @"https://cdn.jsdelivr.net/npm/@mdi/light-font@0.2.63/css/materialdesignicons-light.min.css"}
        link { attr.rel "stylesheet"; attr.href "css/index.css" }
        link { attr.rel "stylesheet"; attr.href "Transcriber.Client.styles.css" }

        script {attr.src "scripts/AudioRecorder.js" }
    }
    body {
        nav {
            attr.``class`` "navbar is-dark"
            "role" => "navigation"
            attr.aria "label" "main navigation"
            div {
                attr.``class`` "navbar-brand"
                span {
                    attr.``class`` "navbar-item has-text-weight-bold is-size-5"
                    text "Whisper Transcriber"
                }
            }
        }
        div { attr.id "main"; comp<Client.App.MyApp> }

        boleroScript

        script{"navigator.serviceWorker.register('service-worker.js');" }
    }
}
