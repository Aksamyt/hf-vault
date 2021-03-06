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

namespace HfVault
#nowarn "62"
#light "off"

namespace HfVault.Dto
module Theme = begin
  type T = T of (struct (int * string * string))

  type DbRepr = { id : int
                ; name : string
                ; description : string
                }

  let toDb (T (struct (id, name, desc))) = {id=id;name=name;description=desc}

  let fromDb repr = T (struct (repr.id, repr.name, repr.description))
end

namespace HfVault.Domain
module Theme = begin
  type T = { id : int
           ; name : string
           ; desc : string
           }

  let new_ = {id=0;name=null;desc=null}
  let id_ = (fun {id=i} -> i), (fun i t -> {t with id=i})
  let name_ = (fun {name=n} -> n), (fun n t -> {t with name=n})
  let desc_ = (fun {desc=d} -> d), (fun d t -> {t with desc=d})

  let dto_ =
  ( ( fun (HfVault.Dto.Theme.T (struct (id, name, desc))) ->
        {id=id;name=name;desc=desc}
    )
  , (fun t -> HfVault.Dto.Theme.T (struct (t.id, t.name, t.desc)))
  )
end
