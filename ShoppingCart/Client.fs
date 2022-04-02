namespace ShoppingCart

open System

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.UI.Html

open DTO

(*
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
*)

[<JavaScript>]
module Client =
    open DTO

    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    type CartItem =
        {
            Item: Product
            Quantity: int
        }
        member this.Amount () = this.Item.Price * this.Quantity
        static member Amount (item: CartItem) = item.Amount ()

    type Cart = ListModel<string, CartItem>

    (*
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
    *)

    let store =
        Server.getStoreInfoSync ()

    let rvStore = Var.Create<ProductFamily list> []
    let loadStore () =
        async {
            let! store = Server.getStoreInfo()
            printfn "server returned %i families" (store.Length)
            rvStore := store |> Array.toList 
            return ()
        }

    // https://forums.websharper.com/topic/87102

    // setup empty cart
    let cart = ListModel.Create (fun (item: CartItem) -> item.Item.Id) []

    let viewProduct (product: Product) =
        IndexTemplate.Product()
            .ProductTitle(product.Title)
            .ProductPrice(string product.Price)
            .ProductImgSource(product.ImageSrc)
            .ProductQuantity(string 1)
            .AddToCart(fun e ->
                if cart.ContainsKey(product.Id) then
                    let item = cart.Lens(product.Id)
                    let quantity = item.LensAuto (fun item -> item.Quantity)
                    quantity.Value <- quantity.Value + int e.Vars.ProductQuantity.Value
                elif (int e.Vars.ProductQuantity.Value) <> 0 then
                    cart.Add { Item = product; Quantity = int e.Vars.ProductQuantity.Value }
                else
                    ()
            )
            .Doc()

    let viewProductFamily (family: ProductFamily): Doc =
        IndexTemplate.Family()
            .FamilyTitle(family.Title)
            .Products(
                family.Products
                |> List.map viewProduct
            )
            .Doc()

    let viewItemsToSellAsync: Doc =
        IndexTemplate.ItemsToSell()
            .Families(
                rvStore.View
                |> View.WithInitOption
                |> Doc.BindView (function
                    | None ->
                        p [] [text "loading"]
                    | Some pl ->
                        pl
                        |> List.map viewProductFamily
                        |> Doc.Concat
                )
            )
            .Doc()

    let viewItemsToSell: Doc =
        IndexTemplate.ItemsToSell()
            .Families(
                rvStore.Value
                |> List.map viewProductFamily
            )
            .Doc()


    let viewCartItem (item: CartItem): Doc =
        IndexTemplate.CartItem()
            .Name(item.Item.Title)
            .Amount(item.Quantity * item.Item.Price |> string)
            .Quantity(string item.Quantity)
            .Increment(fun e ->
                cart.Lens(item.Item.Id) := { item with Quantity = item.Quantity + 1 }
            )
            .Decrement(fun e ->
                if item.Quantity <= 1 then
                    cart.RemoveByKey(item.Item.Id)
                else
                    cart.Lens(item.Item.Id) := { item with Quantity = item.Quantity - 1 }
            )
            .Remove(fun e -> cart.RemoveByKey(item.Item.Id))
            .Doc()

    let viewTotalAmount =
        cart.View
        |> View.Map (Seq.sumBy CartItem.Amount)
        |> View.Map string

    let viewCartItems =
        IndexTemplate.ShoppingCart()
            .CartItems(
                cart.View.DocSeqCached viewCartItem
            )
            .TotalAmount(viewTotalAmount)
            .Checkout(fun _ -> JS.Alert("Checkout was called")
            )
            .EmptyCart(fun _ -> cart.Clear()
            )
            .Doc()


    [<SPAEntryPoint>]
    let Main () =
        printfn "loading store"
        loadStore ()
        |> Async.Start

        printfn "after loadStore"

        IndexTemplate()
            .Title("My Shop")
            .Footer("MyShoppingCart - simple Websharper demo")
            .Main(viewItemsToSellAsync)
            .Sidebar(viewCartItems)
            .Bind()
        
