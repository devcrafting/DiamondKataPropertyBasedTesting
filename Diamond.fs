module Diamond

open System
open System.Text

let diamondOf (c:char) =
    let stringBuilder = new StringBuilder()
    let arrayLen = int c - int 'A' + 1
    let line (x:StringBuilder) y =
        match y with
            | 'A' ->
                x.Append(String.replicate (arrayLen - 1) " ")
                  .Append("A")
                  .Append(String.replicate (arrayLen - 1) " ")
            | _ ->
                let positionOfYFromA = int y - int 'A' - 1
                let positionOfCFromY = arrayLen - positionOfYFromA - 2
                x.Append(String.replicate positionOfCFromY " ")
                  .Append(string y)
                  .Append(String.replicate (2*positionOfYFromA+1) " ")
                  .Append(string y)
                  .Append(String.replicate positionOfCFromY " ")
        x.AppendLine()
    let array = ['A'..c]
    array |> Seq.fold line stringBuilder
    array |> Seq.rev |> Seq.tail |> Seq.fold line stringBuilder
    stringBuilder.Remove(stringBuilder.Length - 2, 2)
    stringBuilder.ToString()

open FsCheck
open FsUnit.Xunit
open Xunit
open System

type Letter =
    static member Char() =
        Arb.Default.Char()
        |> Arb.filter (fun x -> x >= 'A' && x <= 'Z')


[<Fact>]
let ``check diamond is never empty`` () =
    Arb.register<Letter>()
    Check.QuickThrowOnFailure (fun x -> not (String.IsNullOrWhiteSpace (diamondOf x)))

[<Fact>]
let ``check diamond has A in first and last rows`` () =
    Arb.register<Letter>()
    Check.QuickThrowOnFailure (fun x ->
        let actual = diamondOf x
        let rows = actual.Split([| Environment.NewLine |], StringSplitOptions.None)
        let lastChar = (rows |> Seq.last).Trim() |> Seq.head
        let firstChar = (rows |> Seq.head).Trim() |> Seq.head
        lastChar = 'A' && firstChar = 'A')

[<Fact>]
let ``check there are 2*(N-1)+1 diamond lines where N is position in ABC`` () =
    Arb.register<Letter>()
    Check.QuickThrowOnFailure (fun x ->
        let actual = diamondOf x
        let rows = actual.Split([| Environment.NewLine |], StringSplitOptions.None)
        rows |> Seq.length = ((['A'..x] |> Seq.length) - 1) * 2 + 1)

[<Fact>]
let ``check diamond is as wide as high`` () =
    Arb.register<Letter>()
    Check.QuickThrowOnFailure (fun x ->
        let actual = diamondOf x
        let rows = actual.Split([| Environment.NewLine |], StringSplitOptions.None)
        let height = rows |> Seq.length
        rows |> Seq.forall (fun y ->
            y |> Seq.length = height))

[<Fact>]
let ``check diamond lines are symetric`` () =
    Arb.register<Letter>()
    Check.QuickThrowOnFailure (fun x ->
        let actual = diamondOf x
        let rows = actual.Split([| Environment.NewLine |], StringSplitOptions.None)
        rows |> Seq.forall (fun y ->
            let a = y.Substring(0, y.Length/2 + 1)
            let b = new String(y.Substring(y.Length/2) |> Seq.rev |> Seq.toArray)
            a = b))

[<Fact>]
let ``check diamond lines always contain each letter twice, but only one A`` () =
    Arb.register<Letter>()
    Check.QuickThrowOnFailure (fun x ->
        let actual = diamondOf x
        let rows = actual.Split([| Environment.NewLine |], StringSplitOptions.None)
        rows |> Seq.forall (fun y ->
            let trimmedLine = y.Replace(" ", "")
            match trimmedLine |> Seq.head with
            | 'A' -> trimmedLine = "A"
            | c -> trimmedLine = string c + string c))

[<Fact>]
let ``check distance between letter is 2*(N-1)+1 spaces, where N is position in ABC`` () =
    Arb.register<Letter>()
    Check.QuickThrowOnFailure (fun x ->
        let actual = diamondOf x
        let rows = actual.Split([| Environment.NewLine |], StringSplitOptions.None)
        rows |> Seq.forall (fun y ->
            let trimmedLine = y.Trim()
            match trimmedLine |> Seq.head with
            | 'A' -> trimmedLine = "A"
            | c -> trimmedLine = string c + (String.replicate (((['A'..c] |> Seq.length) - 2) * 2 + 1) " ") + string c))
