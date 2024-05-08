namespace Transcriber.Server

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Hosting
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Server
open Transcriber
open Whisper.net.Ggml
open Whisper.net
open FSharp.Control
open NAudio.Wave
open NAudio.Wave.SampleProviders
open FFMpegCore

module Whisper = 
    let ggmlType = GgmlType.Base

    let ensureDir (file:string) = 
        let dir = Path.GetDirectoryName file
        if Directory.Exists dir |> not then Directory.CreateDirectory dir |> ignore

    let download file = 
        task {
            if File.Exists file |> not then 
                printfn $"downloading model {file}"
                ensureDir file
                use! mstr = WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType)
                use str = File.OpenWrite(file)
                do! mstr.CopyToAsync(str)
            else
                printfn $"file found {file}"
        }      

    let converToWave (inputFile:string) =        
        let path = Path.GetTempFileName()
        try 
            let r = 
                FFMpegArguments
                    .FromFileInput(inputFile)
                    .OutputToFile($"{path}", true,
                        fun options -> 
                            options
                                .ForceFormat("wav")
                                .WithAudioSamplingRate(16000)
                            |>ignore)
                    .ProcessSynchronously()
            if r then 
                path
            else
                failwith "unable to convert to wav file"
        with ex -> 
            printfn "%A" ex.Message
            raise ex

    let transcribe (contentRoot:string) (mp3File:string) = 
        async {
            let modelFile = Path.Combine(contentRoot,"wwwroot","model","ggml-base.bin")
            ensureDir modelFile
            do! download modelFile |> Async.AwaitTask            
            let wavFile = converToWave mp3File
            use factory = WhisperFactory.FromPath modelFile
            use processor = factory.CreateBuilder().WithLanguage("auto").Build()
            use wavStr = File.OpenRead(wavFile)
            let xs = 
                processor.ProcessAsync(wavStr)
                |> AsyncSeq.ofAsyncEnum
                |> AsyncSeq.map(fun r -> r.Text) //sprintf $"{r.Start}->{r.End}: {r.Text}")
                |> AsyncSeq.toBlockingSeq
            return String.Join(" ",xs)
        }
        
type TranscriptionService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Model.TranscribeService>()

    override this.Handler =
        {
            getTranscription = fun (file:string) -> async {
                
                let audioFile = Path.Combine(env.ContentRootPath,"wwwroot","files",file)
                let! txt =  Whisper.transcribe env.ContentRootPath audioFile
                try File.Delete audioFile with ex -> printfn $"{ex.Message}"
                return txt
            }

            transcribeTestFile = fun () -> async {
                let audioFile = Path.Combine(env.ContentRootPath,"wwwroot","test_files","kennedy.mp3")
                return! Whisper.transcribe env.ContentRootPath audioFile                
            }

        }
