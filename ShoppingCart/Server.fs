module ShoppingCart.Server

open System
open WebSharper
open WebSharper.Remoting

open DTO

let store =
    let item imageSrc (title, id, price) =
        {
            Id = id
            Title = title
            Price = price
            ImageSrc = imageSrc
        }
    let laptop product = item "/images/laptop.png" product
    let desktop product = item "/images/desktop.png" product
    let netbook product = item "/images/netbook.png" product
    [
        {
            Title = "Laptops"
            Products = 
                [
                    laptop ("Toshiba", "id1", 1299)
                    laptop ("HP", "id2", 1499)
                    laptop ("Dell", "id3", 999)
                    laptop ("Acer", "id4", 1199)
                ]
        }
        {
            Title = "Desktops"
            Products = 
                [
                    desktop ("Gamer", "id11", 699)
                    desktop ("Office", "id13", 599)
                ]
        }
    ]

[<Remote>]
let getStoreInfoSync () =
    store

[<Remote>]
let getStoreInfo () =
    async {
        return store |> List.toArray
    }
    
