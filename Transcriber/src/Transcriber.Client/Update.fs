module Transcriber.Client.Update

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Microsoft.AspNetCore.Components
open Transcriber.Client.Model
open Microsoft.JSInterop

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

let update (jsr:IJSRuntime) remote message model =
    printfn "message: %A" message

    let toggleRecording model =        
        match model.rstate with 
        | Ready     -> jsr.InvokeVoidAsync("BlazorAudioRecorder.StartRecord") |> ignore; {model with rstate=Recording}
        | Recording -> jsr.InvokeVoidAsync("BlazorAudioRecorder.StopRecord")  |> ignore; {model with rstate=Ready}
        ,Cmd.none

    let uploadFile (jsr:IJSRuntime,model) = 
        task{
            let! file = jsr.InvokeAsync<string> ("BlazorAudioRecorder.UploadBlob",model.mUrl)
            return file
        }

    match message with
    | ToggleRecord -> toggleRecording model
    | SetMUrl murl -> {model with mUrl=murl},Cmd.none
    | Transcribe -> {model with startTranscribeTime=DateTime.Now; endTranscribeTime=DateTime.Now; isTranscribing=true},Cmd.OfTask.either uploadFile (jsr,model) Uploaded Error
    | Uploaded fn -> model,Cmd.OfAsync.either remote.getTranscription fn GotTranscription Error
    | GotTranscription tx -> {model with transcribedText = tx; endTranscribeTime=DateTime.Now; isTranscribing=false},Cmd.none
    | TranscribeTest -> {model with startTranscribeTime=DateTime.Now; endTranscribeTime=DateTime.Now; isTranscribing=true},Cmd.OfAsync.either remote.transcribeTestFile () GotTranscription Error
    | SetPage page -> { model with page = page }, Cmd.none
    | Error RemoteUnauthorizedException ->{ model with error = Some "You have been logged out."; signedInAs = None }, Cmd.none
    | Error exn -> { model with error = Some exn.Message }, Cmd.none
    | ClearError -> { model with error = None }, Cmd.none

