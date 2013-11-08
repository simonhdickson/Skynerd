namespace Skynerd
open System
open System.Collections.Generic
open System.IO
open System.Text
open Owin
open System.Threading.Tasks
open Monads.State

module Server =
    type ContentType =
        | PlainText
    
    let requestTarget (enviroment:IDictionary<string,obj>) =
        (enviroment.["owin.RequestMethod"] :?> string, enviroment.["owin.RequestPath"] :?> string)

    let headers (enviroment:IDictionary<string,obj>) =
        enviroment.["owin.ResponseHeaders"] :?> IDictionary<string, string[]>

    let requestBody (enviroment:IDictionary<string,obj>)=
        (new StreamReader (enviroment.["owin.RequestBody"] :?> Stream)).ReadToEnd()

    let responseBody (enviroment:IDictionary<string,obj>) =
        enviroment.["owin.ResponseBody"] :?> Stream

    let addResponseHeader (contentLength:int) contentType enviroment =
        let responseHeaders = headers enviroment
        responseHeaders.Add("Content-Length", [|contentLength.ToString()|])
        match contentType with
        | PlainText -> responseHeaders.Add("Content-Type", [|"text/plain"|])

    let get f =
        state {
            let! (_,enviroment) = getState
            return f enviroment
        }

    let next () =
        state {
            let! (next,enviroment) = getState
            return Task.Run (fun () -> next enviroment)
        }

    let respondWith (response:string) =
        state {
            let! (_,enviroment) = getState
            let responseBytes = ASCIIEncoding.UTF8.GetBytes response
            let responseStream = responseBody enviroment
            addResponseHeader responseBytes.Length PlainText enviroment
            return responseStream.AsyncWrite(responseBytes, 0, responseBytes.Length) |> Async.StartAsTask :> Task
        }

    let useRequestProcessor verb url processor =
        state {
            let! target = get requestTarget
            match target with
            | (verb, url) ->
                let! request = get requestBody
                let response = processor request
                return! respondWith response
            | _ ->
                return! next ()
        }

    let Func2 (x:Func<_,_>) y = x.Invoke(y)

    type IAppBuilder with
        member x.UseRequestProcessor((verb:string), (url:string), processor) =
            let processor (x:IDictionary<string,obj>->Task) y =
                let (t,_) = executeState (useRequestProcessor verb url processor) (x,y)
                t
            x.Use(fun next -> processor (Func2 next))
