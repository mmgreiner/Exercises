namespace Exercise1

open System
open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Server

type MainTemplate = Templating.Template<"main.html">

[<JavaScript>]
module Client =

    open WebSharper.UI.Client
    open WebSharper.JavaScript

    [<Struct>]
    type PrimaryColors = 
        | Red
        | Yellow
        | Blue 



    let t1 = Var.Create ""
    let n1 = Var.Create 0
    let d1 = Var.Create ""
    let tel = Var.Create ""
    let pw = Var.Create ""
    let check = Var.Create true
    let color = Var.Create ""
    let primary = Var.Create (sprintf "%A" PrimaryColors.Blue)
    let n2: Var<Client.CheckedInput<int>> = Var.Create (Client.CheckedInput<int>.Blank "0")

    //let primaryColors = 
        ///FSharp.Reflection.FSharpType.GetUnionCases typeof<PrimaryColors>
        // |> Array.map (fun uc -> uc.Name)

    let primaryColorOptions =
        [ Red; Yellow; Blue ]
        |> Seq.map (sprintf "%A")
        |> Seq.map (fun col -> Tags.option [] [text col])

    let MainHTMLTemplate () =
        MainTemplate.InputField()
            .T1(t1)
            .N1(n1)
            .D1(d1)
            .Tel(tel)
            .PW(pw)
            .Check(check)
            .Color(color)
            .PrimaryColor(primary)
            .PrimaryColorList(primaryColorOptions)
            .N2(n2)
            .LoadFile(fun ev -> 
                printfn "LoadFile: ev = %A, %A" ev.Target ev.Event
                
                // https://developer.mozilla.org/en-US/docs/Web/API/File/Using_files_from_web_applications
                // https://developer.mozilla.org/en-US/docs/Web/API/File/Using_files_from_web_applications
                // https://www.w3docs.com/learn-javascript/file-and-filereader.html
                
                let inputElem = ev.Target :?> HTMLInputElement
                let files = inputElem.Files
                for i in 0 .. files.Length-1 do
                    let file = files.[i]
                    printfn "file = %s, %i" file.Name file.Size
                    let reader = new TextFileReader()
                    reader.Onload <- (fun _ -> printfn "reader = %s" reader.Result)
                    reader.Onerror <- (fun _ -> sprintf "cannot read %s" file.Name |> JS.Alert)
                    reader.ReadAsText(file)

                ()
                //JavaScript.JS.
            )
            .OnInput(fun ev -> printfn "OnInput: ev = %A" ev.Target)
            .Doc()

    let MainWS () =
        let vText = Var.Create "please enter name"
        let vInt = CheckedInput<int>.Blank "give int" |> Var.Create
        let vFloat = CheckedInput<float>.Valid(Math.PI, Math.PI.ToString()) |> Var.Create

        let inline viewChecked (chk: CheckedInput<'t>) =
            match chk with
            | Valid(v, _) -> string v
            | Blank(_) -> "no text given"
            | Invalid(t) -> "invalid input " + t 

        let vColor = Var.Create Blue
        let vColorRadio = Var.Create Red
        let showColor (c: PrimaryColors) = sprintf "%A" c
        let dColor = Doc.Select [] showColor [ Blue; Red; Yellow ] vColor

        div [] [
            label [] [
                text "Text:"
                Doc.Input [] vText
            ]
            label [] [
                text "Checked Int"
                Doc.IntInput [] vInt
            ]
            label [] [
                text "checked float"
                Doc.FloatInput [] vFloat
            ]
            label [] [
                text "selects"
                dColor
            ]
            div [] [
                label [] [text "red"; Doc.Radio [] Red vColorRadio]
                label [] [text "yellow"; Doc.Radio [] Yellow vColorRadio]
                label [] [text "blue"; Doc.Radio [] Blue vColorRadio]
            ]
            p [] [text ("text = " + vText.V)]
            p [] [text ("int = " + (vInt.V |> viewChecked))]
            p [attr.step "0.01"] [text ("float = " + (vFloat.V |> viewChecked))]
            p [] [text ("color = " + (vColor.V |> showColor))]
            p [] [text ("radio = " + (vColorRadio.V |> showColor))]
        ]

module Site =
    let mainPage ctx =
        let temp1 = 
            MainTemplate.Template1()
                .Name("first template")
                .Doc()
        let temp2 =
            MainTemplate.Template2()
                .Name("child template")
                .Doc()

        let items =
            [ 0 .. 10 ]
            |> Seq.map string
            |> Seq.map (fun i -> 
                MainTemplate.ListItem()
                    .ItemName(i)
                    .Doc()
            )
            |> Doc.Concat

        Content.Page(
            MainTemplate()
                .Title("Exercise 1")
                .Body(p [] [text "Hallo Markus"])
                .Hole1(temp1)
                .Hole2(temp2)
                .ListItems(items)
                .InputFields(div [] [client <@ Client.MainHTMLTemplate() @>])
                .InputFields2(client <@ Client.MainWS() @>)
                .Doc()
        )

    [<Website>]
    let Main = 
        Application.SinglePage (fun ctx -> 
            mainPage ctx
        )