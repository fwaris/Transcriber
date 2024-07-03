#r "nuget: Llamasharp"
#r "nuget: Llamasharp.backend.cuda12"
#r "nuget: FSharp.Control.AsyncSeq"
open System
open System.Text
open System.IO
open LLama
open LLama.Common
open FSharp.Control
open LLama.Sampling
open LLama.Native
let f:string  = @"E:\s\models\llava-llama-3-8b-v1_1-f16.gguf"
let m = @"E:\s\models\llava-llama-3-8b-v1_1-mmproj-f16.gguf"
let img = @"E:\s\genai\00000_120125 4- Port Antenna - DWG_CellMax.pdf.0_0.jpeg"

let w  = LLavaWeights.LoadFromFile(m)

let parms = ModelParams(f)
let model = LLamaWeights.LoadFromFile(parms)

let emb= w.CreateImageEmbeddings(File.ReadAllBytes(img))

let exec = new Batched.BatchedExecutor(model,parms)
let conversation = exec.Create()
conversation.Prompt(emb)
task{
    let! c = exec.Infer() 
    while exec.BatchedTokenCount > 0 do
        let! c = exec.Infer()
        ()
} |> Async.AwaitTask |> Async.RunSynchronously

let decoder = new StreamingTokenDecoder(exec.Context)
let sampler = DefaultSamplingPipeline()
sampler.TopK <- 1

let prompt = model.Tokenize( "\nUSER: Provide a full description of the image.\nASSISTANT: ", true, false, Encoding.UTF8)
conversation.Prompt(prompt)
task{
    for i in 0 .. 100 do
        let! m = exec.Infer()
        let token = sampler.Sample(exec.Context.NativeHandle, conversation.Sample(), Array.Empty<LLamaToken>())
        decoder.Add(token)
        printfn "%s" (decoder.Read())
        conversation.Prompt(token)

}
|> Async.AwaitTask |> Async.RunSynchronously

printfn "%s" (decoder.Read())