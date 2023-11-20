module Transcriber.Client.App

open System
open Elmish
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Client
open Microsoft.AspNetCore.Components
open Transcriber.Client.Model
open Transcriber.Client.Update
open Transcriber.Client.View.Main
open Microsoft.JSInterop


type MyApp() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val Jsr : IJSRuntime  = Unchecked.defaultof<_> with get, set

    [<JSInvokable>]
    member this.OnAudioUrl(vUrl:string) = 
        task {
            this.Dispatch (SetMUrl vUrl)
        }

    override this.Program =
        this.Jsr.InvokeVoidAsync("BlazorAudioRecorder.Initialize", DotNetObjectReference.Create(this)) |> ignore
        let bookService = this.Remote<TranscribeService>()
        let update = update this.Jsr bookService
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
