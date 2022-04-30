namespace SpaList

open System
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

module Server =

    [<JavaScript>]
    type Person =
        {
            Name: string
            Birthday: DateTime
        }
        static member Init (name, year, month, day) = { Name = name; Birthday = DateTime(year, month, day) }
        static member Init (name, date) = 
            {
                Name = name
                Birthday = DateTime.Parse(date)
            }
        static member Init (name, date) = { Name = name; Birthday = date }

    let persons = new System.Collections.Generic.List<Person>()
    persons.Add(Person.Init ("Markus", 1965, 11, 10))

    [<Remote>]
    let getAll () =
        async {
            do! Async.Sleep 2000
            return persons
        }

    [<Remote>]
    let add (p: Person) =
        async {
            printfn "Server: adding %A" p
            persons.Add(p)
            return ()
        }

[<JavaScript>]
module Client =

    open WebSharper.UI.Html

    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>


    let People = 
        ListModel.Create (fun (p: Server.Person) -> p.Name) Seq.empty<Server.Person>

    [<SPAEntryPoint>]
    let Main () =
        let newName = Var.Create ""
        let newBirthday = Var.Create ""

        async {
            let! people = Server.getAll()
            People.Set people
            return ()
        }
        |> Async.Start

        let peopleDoc =
            async {
                let! people = Server.getAll() 
                return
                    people 
                    |> Seq.map (fun person ->
                        IndexTemplate.ListItem()
                            .Name(person.Name)
                            .Doc()
                    )
                    |> Doc.Concat
            }
            |> View.Const
            |> View.MapAsync id
            |> View.WithInit (p [] [text "loading..."])
            |> Doc.EmbedView

        IndexTemplate.Main()
            .ListContainer(
                People.View
                //|> View.WithInit (seq { Server.Person.Init ("loading...", DateTime.Now) })
                |> Doc.BindView(fun people ->
                    people
                    |> Seq.map (fun p ->
                        IndexTemplate.ListItem()
                            .Name(p.Name)
                            .Birthday(p.Birthday.ToShortDateString())
                            .Doc()
                    )
                    |> Doc.Concat
                )
                //|> View.WithInit (p [] [text "loading..."])
            )
            
            //.ListContainer(peopleDoc)
            .Name(newName)
            .Birthday(newBirthday)
            .Add(fun _ ->
                let person = Server.Person.Init (newName.Value, newBirthday.Value)
                People.Add(person)
                newName.Value <- ""
                newBirthday.Value <- ""
                async {
                    do! Server.add(person)
                    return ()
                }
                |> Async.Start
            )
            .Doc()
        |> Doc.RunById "main"
