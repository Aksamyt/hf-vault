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

namespace Api.Machine.Forum
#nowarn "62"
#light "off"

open Freya.Core
open Freya.Core.Operators
open Freya.Machines.Http
open Freya.Machines.Http.Cors
open Freya.Types.Http
open Thoth.Json.Net
open Api

[<AutoOpen>]
module private __ThemesImpl__ = begin
  let ``200`` = freya
  { let! realm = Option.get <!> Machine.Pervasives.realm in
    let! themes = Db.``select all themes from realm`` realm |> Freya.fromJob in
    let inline toDto t =
      Domain.List.dto_<Dto.Theme, Domain.Theme>()
      |> snd
      <| t
    in
    let json = themes
            |> toDto
            |> Dto.List.jsonEncoder<Dto.Theme>
            |> Encode.toString 0 in
    return { Data=System.Text.Encoding.UTF8.GetBytes json
           ; Description={ Charset=Some Charset.Utf8
                         ; Encodings=None
                         ; MediaType=Some MediaType.Json
                         ; Languages=None
                         }
           }
  }

  let ``404`` =
    Represent.text "Allowed realm values are “FR”, “EN”, or “ES”"

  let machine = freyaMachine
  { exists (Option.isSome <!> Machine.Pervasives.realm)
  ; handleOk ``200``
  ; handleNotFound ``404``
  ; cors
  }
end

type ThemesMachine = Themes with
  static member Pipeline(_) = HttpMachine.Pipeline(machine)
end
