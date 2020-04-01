(*
    Copyright (C) 2020  Contributors as noted in the AUTHORS.md file
    This file is part of hf-vault, an Eternal Twin preservation project.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*)

namespace HfVault.Optics
#nowarn "62"
#light "off"

open System
open Aether

module HtmlWeb = begin
  open HtmlAgilityPack

  let get_ (url:System.Uri) : Prism<HtmlWeb, _> =
    ( (fun web -> try Some (web.Load(url)) with _ -> None)
    , (fun _ web -> web)
    )
end

module HtmlDocument = begin
  open HtmlAgilityPack

  let root_ : Prism<HtmlDocument, _> =
    ( ( fun doc ->
          if Seq.length doc.ParseErrors > 0
          then None
          else Some doc.DocumentNode
      )
    , (fun root _ -> root.OwnerDocument)
    )
end

module HtmlNode = begin
  open HtmlAgilityPack

  let nodes_ (xpath:Xml.XPath.XPathExpression) : Lens<HtmlNode, _> =
    ( (fun doc -> doc.SelectNodes(xpath) :> HtmlNode seq)
    , fun _ _ -> failwith "unimplemented"
    )

  let innerText_ : Lens<HtmlNode, string> =
    ( (fun xx -> Web.HttpUtility.HtmlDecode xx.InnerText)
    , (fun text xx -> xx.InnerHtml <- Web.HttpUtility.HtmlEncode text; xx)
    )
end

module String = begin
  let int32_ : Epimorphism<string, _> =
    ( ( fun s ->
          let mutable result = Unchecked.defaultof<_> in
          if Int32.TryParse(s, &result)
          then Some result
          else None
      )
    , (fun i -> i.ToString())
    )
end