namespace Exercise2

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About



module Site =
    open WebSharper.UI.Html

    let homePage ctx =
        MainTemplate()
            .TableRows(client <@ Client.Main() @>)
            .Doc()

        |> Content.Page


   
    [<Website>]
    let Main =
        Application.SinglePage (fun ctx ->
            homePage ctx            
        )
