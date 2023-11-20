namespace Transcriber.Server
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open System.IO

type FileUploadController(cfg:IHostEnvironment) =
    inherit ControllerBase()


    [<HttpPost("upload")>]
    member this.UploadAsync(file:IFormFile)  =
        task{
            let path = Path.Combine(cfg.ContentRootPath,"wwwroot","files",file.FileName)
            let dir = Path.GetDirectoryName(path)
            if Directory.Exists(dir) |> not then Directory.CreateDirectory(dir) |> ignore
            use str = File.Create path
            do! file.CopyToAsync(str)
            return JsonResult(file.FileName)
        }


