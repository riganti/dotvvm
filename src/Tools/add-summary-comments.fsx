#!/usr/bin/fsharpi

#r "System.Xml.Linq"
open System
open System.Xml.Linq
let rootElements = Set.ofList [ "example"; "code"; "exception"; "include";  "returns"; "param"; "permission"; "remark"; "seealso"; "value"; "typeparam" ]
let filePath : string option =
    if fsi.CommandLineArgs.Length <> 2 then
        None
    else
        Some fsi.CommandLineArgs.[1]
let xml =
    match filePath with
    | Some file -> XDocument.Load(file)
    | None -> XDocument.Parse(Console.In.ReadToEnd())
let n = XName.Get
// get elements that does not contain <summary>
let rawTexts = xml.Root.Element(n "members").Elements(n "member") |> Seq.filter (fun e -> e.Elements(n "summary") |> Seq.isEmpty)
for doccomment in rawTexts do
    let nodes =
        doccomment.Nodes()
        |> Seq.filter (function
                   | :? XElement as element ->
                       // filter non-global elements
                       not <| Set.contains element.Name.LocalName rootElements
                   | :? XText as text ->
                       not <| String.IsNullOrEmpty text.Value
                   | _ -> true)
        |> Seq.toArray
    for n in nodes do
        n.Remove()

    if nodes.Length > 0 then
        doccomment.AddFirst(
            XElement(n "summary", box nodes)
        )

match filePath with
| Some file -> xml.Save(file)
| None -> xml.Save(Console.Out)
