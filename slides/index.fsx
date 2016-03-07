(**
- title : Property Based Testing
- description : Property Based Testing
- author : Clément Bouillier
- theme : night
- transition : defaultArg

***
*)
(*** hide ***)
#r "../packages/xunit.assert/lib/dotnet/xunit.assert.dll"
#r "../packages/xunit.abstractions/lib/net35/xunit.abstractions.dll"
#r "../packages/xunit.extensibility.core/lib/dotnet/xunit.core.dll"
#r "../packages/xunit.extensibility.execution/lib/dotnet/xunit.execution.dotnet.dll"
#r "../packages/FsUnit.xUnit/lib/net45/NHamcrest.dll"
#r "../packages/FsUnit.xUnit/lib/net45/FsUnit.Xunit.dll"
#r "../packages/FsCheck/lib/net45/FsCheck.dll"
#r "../packages/xunit.runner.console/tools/xunit.console.exe"
#load "../Diamond.fs"
open Diamond
open FsUnit.Xunit
open Xunit

(**
## Property Based Testing
### Human Talks - March 2016

<small>Clément Bouillier - [DevCrafting](http://www.devcrafting.com) - [@clem_bouillier](http://twitter.com/clem_bouillier)</small>

***

## Inspired by [@ScottWlaschin](https://twitter.com/ScottWlaschin) and [@ploeh](http://blog.ploeh.dk/2016/02/10/types-properties-software/)

- Blog post ["Introduction to Property Based Testing"]()
- Blog posts serie ["Types + Properties = Software"](http://blog.ploeh.dk/2016/02/10/types-properties-software/)

<small>NB : only in F#, but F# is really great and easy to understand ! ;)</small>

***

## When "doing TDD well" is <br/>not enough...

---

### The EDFH <br/>(Enterprise Developer From Hell)

![EDFH](images/EDFH.png)

> A rule of thumb : write the minimal code that will pass the test

---

### Trivial example : add
*)

(*** include:edfhTest1 ***)

(** EDFH would simply implement it with: *)
let add x y = 3

(*** define:edfhTest1 ***)
[<Fact>]
let ``When I add 1 and 2, I expect 3`` () =
    add 1 2 |> should equal 3

(**
---

### First step : generate arbitrary data

Using an arbitrary data generator, the EDFH has to find a non trivial implementation.
*)

let rand = System.Random()
let randInt() = rand.Next()

[<Fact>]
let ``When I add two numbers, it returns the sum`` () =
    let x = randInt()
    let y = randInt()
    add x y |> should equal (x + y)

(**
But "problem", we are using "+", i.e tests depends on an implementation, we have to find a better way...
Note: it is sometimes acceptable ("test oracle" technique)

---

### More complex : have you ever tried Diamond kata ?

> Given a letter, display a diamond with letters from A to the given letter

---

### Example of Diamond Kata

*)

(*** define-output:diamondOfE ***)
let c = 'E'
printfn "Diamond of %c:\n%s" c (diamondOf c)

(*** include-output:diamondOfE ***)

(**
---

### Pretty sure you tried it this way...
*)
[<Fact>]
let ``Diamond of A, return A`` () =
    diamondOf 'A' | should equal "A"

[<Fact>]
let ``Diamond of B, return A-BB-A`` () =
    diamondOf 'B' | should equal " A \nB B\n A "

(**
And so on, but it becomes more and more difficult...need to find another way again.

***

## Then, let's try Property Based Testing

---

### Trivial example : add

Remind the example, rather than depending on an implementation in test, let's find some properties that describe a sum...keeping idea of arbitrary data

Recall your math/algebra lessons...

![Math/algebra lessons were hard...](images/algebra.jpg)

---

### Sum is commutative
*)
let x = randInt()
let y = randInt()
add x y |> should equal add y x
(**
Oups, the EDFH can always give malicious answer :)
*)
let add x y = 0
(**
---

### Sum has 0 as identity element
*)
add x 0 |> should equal x
(**
Test fails with the malicious implementation, the EDFH has to implement "correctly"

---

### Diamond Kata again

Some properties of a Diamond:

- First and last lines contains only one A
*)
(diamondOf c |> Seq.head).Trim() |> should equal "A"
(diamondOf c |> Seq.rev |> Seq.head).Trim() |> should equal "A"
(**
- Diamond is as wide as high...
- Diamond lines are symetric...
- Diamond lines always contains 2 letter (but A)...

---

### Some tools to generate arbitrary data

QuickCheck (Haskell) is known as the original library, so you will find lots of parents : FsCheck, ScalaCheck, JUnit-QuickCheck...

Goal of this libraries is to simplify generation of arbitrary data, even based on complex types.

It becomes even more interesting with Type Driven Development...

---

### Example with Tennis Kata

Defining Tennis game states : [make illegal states unrepresentable](http://fsharpforfunandprofit.com/posts/designing-with-types-making-illegal-states-unrepresentable) (Type Driven Development)
*)
type Player = PlayerOne | PlayerTwo
type Score =
| Points of PointsData
| Forty of FortyData
| Deuce
| Advantage of Player
| Game of Player
(**
*Ref: [blog posts from @ploeh](http://blog.ploeh.dk/2016/02/16/types-properties-software-composition/)*

---

### Then, we can generate <br/>arbitraty "complex" data
*)
[<Property>]
let ``A game with less than four balls isn't over`` (wins : Player list) =
    let actual : Score = wins |> Seq.truncate 3 |> scoreSeq
    test <@ actual |> (not << isGame) @>

[<Property>]
let ``A game where one player wins all balls is over in four balls`` (player) =
    let fourWins = Seq.init 4 (fun _ -> player)
    scoreSeq fourWins |> should equal (Game player)
(**
***

## More...

For those who go to DevoxxFR 2016, don't miss Univertisity of [@malk_zameth](http://www.twitter.com/@malk_zameth) et [@cyriux](http://www.twitter.com/@cyriux)

Read more on the web, lots of resources...

Go deeper with Type Driven Development, see for example an introduction in ["Types + Properties = Software"](http://blog.ploeh.dk/2016/02/10/types-properties-software/)

TDD next time ? But Type, not Test ! ;)

### Thanks

---

### For the fun, have a look at [this slides source code](https://github.com/devcrafting/DiamondKataPropertyBasedTesting) :)

Thanks [FsReveal](http://fsprojects.github.io/FsReveal/)

Based on [RevealJS](F# Formatting) and [F# Formatting](https://tpetricek.github.io/FSharp.Formatting/)

Examples in F# are executed to produce output integrated this slides, for ex: diamond example

*)
