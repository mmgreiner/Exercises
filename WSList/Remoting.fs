namespace WSList

open System
open WebSharper

[<JavaScript>]
type Person = 
    {
        Name: string
        Birthday: DateTime
    }
    static member init name year month day =
        {
            Name = name
            Birthday = DateTime(year, month, day)
        }

module Server =

    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }

    let persons = 
        [
            Person.init "Donald Knuth" 1938 1 10
            Person.init "Barbara Liskov" 1039 11 7
            Person.init "Tony Hoare" 1934 1 11
            Person.init "Niklaus Wirth" 1934 2 15
            Person.init "Susan L. Graham" 1942 9 16 
        ]

    [<Remote>]
    let GetPersons () =
        async {
            let! sl = Async.Sleep 2000 
            return persons
        }