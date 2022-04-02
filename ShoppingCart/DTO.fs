namespace ShoppingCart

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.UI.Notation

// Data Transfer Object

[<JavaScript>]
module DTO =
    type Product = 
        {
            Id: string
            Title: string
            Price: int
            ImageSrc: string
        }

    type ProductFamily =
        {
            Title: string
            Products: Product list
        }