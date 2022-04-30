namespace WSList

open System
open WebSharper
open WebSharper.UI
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.UI.Html
open WebSharper
open WebSharper.UI.Client

[<JavaScript>]
module Templates =

    type MainTemplate = Templating.Template<"Main.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

[<JavaScript>]
module Client =

    let renderPerson (p: Person) =
        Templates.MainTemplate.PersonEntry()
            .Name(p.Name)
            .Birthday(p.Birthday.ToString())
            .Doc()

        

    let Main () =
        let rvReversed = Var.Create ""
        Templates.MainTemplate.MainForm()
            .OnSend(fun e ->
                async {
                    let! res = Server.DoSomething e.Vars.TextToReverse.Value
                    rvReversed := res
                }
                |> Async.StartImmediate
            )
            .Reversed(rvReversed.View)
            .Doc()


    let personDoc =
    // https://try.websharper.com/snippet/adam.granicz/00003b
        let persons =
            async {
                let! ps = Server.GetPersons ()
                return
                    ps
                    |> List.map (fun p ->
                        Templates.MainTemplate.PersonEntry()
                            .Name(p.Name)
                            .Birthday(p.Birthday.ToShortDateString())
                            .Doc()
                    )
                    |> Doc.Concat
            }
        persons
        |> View.Const
        |> View.MapAsync id
        |> View.WithInit (p [] [text "loading..."]) 
        |> Doc.EmbedView

    // https://intellifactory.com/user/jankoa/20161207-distributed-web-applications-in-f-with-websharper?msclkid=a86e06f1bd5211ecb5c128a5fffb668a

    let ListMain () =
        Doc.Concat [
            h2 [] [text "Testing lists"]
            Templates.MainTemplate.TableForm()
                .PersonEntries(personDoc)
                .Doc()
        ]
       