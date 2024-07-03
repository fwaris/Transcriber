//paths need for the script to run
//cudnn: E:\s\cudnn\cudnn-windows-x86_64-8.4.1.50_cuda11.6-archive\bin
//cuda: %CUDA_PATH_V11_8%\bin

#r "nuget: NtvLibs.zlib.zlibwapi.runtime.win-x64"
#r "nuget: Microsoft.ML.OnnxRuntimeGenAI.Cuda"

open System
open Microsoft.ML.OnnxRuntimeGenAI

let modelPath = @"E:\s\models\Phi-3-vision-128k-instruct-onnx-cuda\cuda-fp16"
let systemPrompt = "You are an AI assistant that helps people find information. Answer questions using a direct style. Do not share more information that the requested by the users."
let userPrompt = "Describe the image, and return the string 'STOP' at the end."
let fullPrompt = $"<|system|>{systemPrompt}<|end|><|user|><|image_1|>{userPrompt}<|end|><|assistant|>"
let imgPath = @"E:\s\genai\00000_120125 4- Port Antenna - DWG_CellMax.pdf.0_0.jpeg"
let img = Images.Load(imgPath)

let model = new Model(modelPath);
let processor = new MultiModalProcessor(model);
let tokenizerStream = processor.CreateStream();
let inputTensors = processor.ProcessImages(fullPrompt, img)

let generatorParams = new GeneratorParams(model)
generatorParams.SetSearchOption("max_length", 3027)
generatorParams.SetInputs(inputTensors)

let generator = new Generator(model, generatorParams)

while (not(generator.IsDone())) do
    generator.ComputeLogits()
    generator.GenerateNextToken()
    let seq = 
        let ra =generator.GetSequence(0uL)
        ra.[(ra.Length-1)]
    Console.Write(tokenizerStream.Decode(seq))


    
