module Transcriber.Client.Model

open System
open Bolero
open Bolero.Remoting

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/counter">] Counter

type RState = Ready | Recording 
/// The Elmish application's model.
type Model =
    {
        page: Page
        counter: int
        error: string option
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
        rstate : RState
        mUrl : string
        transcribedText : string
        isTranscribing : bool
        startTranscribeTime : DateTime
        endTranscribeTime : DateTime
    }


let initModel =
    {
        page = Home
        counter = 0
        error = None
        username = ""
        password = ""
        signedInAs = None
        signInFailed = false
        rstate = Ready
        mUrl = ""
        transcribedText = ""
        isTranscribing = false
        startTranscribeTime = DateTime.Now
        endTranscribeTime = DateTime.Now
    }

/// Remote service definition.
type TranscribeService =
    {
        /// Get the list of all books in the collection.
        getTranscription: string -> Async<string>

        transcribeTestFile : unit -> Async<string>
    }

    interface IRemoteService with
        member this.BasePath = "/transcribe"

/// The Elmish application's update messages.
type Message =
    | ToggleRecord 
    | SetMUrl of string
    | Transcribe
    | TranscribeTest
    | Uploaded of string
    | GotTranscription of string
    | SetPage of Page
    | Error of exn
    | ClearError
