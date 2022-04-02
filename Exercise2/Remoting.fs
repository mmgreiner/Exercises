namespace Exercise2

open System
open WebSharper
open FSharp.Core.Option
open FSharp.Collections

module DTO =

    [<JavaScript>]
    type Person = 
        {
            Id: int
            FirstName: string
            LastName: string
            Birthday: DateTime
        }

    let mutable id = 0

    let init first last year month day =
        id <- id + 1
        { Id = id; FirstName = first; LastName = last; Birthday = DateTime(year, month, day)}

    let data = System.Collections.Concurrent.ConcurrentDictionary<int, Person>()
    
    let add first last year month day =
        let p = init first last year month day
        data.TryAdd(p.Id, p) |> ignore
    let addPerson (person: Person) = 
        add person.FirstName person.LastName person.Birthday.Year person.Birthday.Month person.Birthday.Day

    add "Markus" "Greiner" 1965 11 10 
    add "Cornelia" "Schmidt" 1070 1 3 
    add "Clara" "Greiner" 1993 6 27 
    add "Lukas" "Greiner" 1995 11 2


module Server =

    [<Remote>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }

    [<Remote>]
    let getPersons (): Async<DTO.Person []> =
        async {
            return 
                DTO.data
                |> Seq.map (fun kv -> kv.Value)
                |> Seq.toArray
        }

    [<Remote>]
    let getPerson id =
        async {
            return
                match DTO.data.TryGetValue id with
                | true, p -> Some p
                | _ -> None
        }

    [<Remote>]
    let addPerson person =
        async {
            // get hightest key
            let top = DTO.data.Keys |> Seq.max
            return DTO.addPerson person
        }

    [<Remote>]
    let deletePerson id =
        async {
            return
                if DTO.data.ContainsKey id then
                    match DTO.data.TryRemove id with
                    | true, _ -> true
                    | _ -> false
                else false
        }
