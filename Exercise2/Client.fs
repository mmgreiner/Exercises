namespace Exercise2

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.UI.Html


type MainTemplate = Templating.Template<"Main.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

[<JavaScript>]
module Client =

    let vPersons = Var.Create<DTO.Person list> []

    let loadPersons () =
        async {
            let! persons = Server.getPersons ()
            vPersons := persons |> Array.toList
            return ()
        }

    let renderPerson (person: DTO.Person) =
        MainTemplate.TableRow()
            .First(person.FirstName)
            .Last(person.LastName)
            .Birthday(person.Birthday.ToShortDateString())
            .Doc()

    let renderText txt =
        p [] [text txt]

    
    
    let Main () =

        loadPersons ()
        |> Async.Start

        vPersons.View
        |> View.WithInitOption
        |> Doc.BindView (function 
            | None -> 
                p [] [text "loading"]
            | Some persons ->
                persons
                |> List.map renderPerson
                |> Doc.Concat
        )
    