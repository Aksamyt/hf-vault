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

module HfVault.Db
#nowarn "62"
#light "off"

open Aether.Operators

let connRuntime =
  try
    let stream = System.IO.File.OpenRead("config.json") in
    let root = System.Text.Json.JsonDocument.Parse(stream) in
    root.RootElement.GetProperty("ConnectionStrings")
                    .GetProperty("hf-vault")
                    .GetString()
  with
  | _ -> failwith "Configuration file ‘config.json’ not found or invalid"

let ``insert new HfTheme and Theme`` hfTheme = async
{ use cmd = DbTypes.Db.CreateCommand<"""
           WITH new_theme AS (INSERT INTO theme (name, description)
                                   VALUES (@name, @description)
                                RETURNING id)
    INSERT INTO hf_theme (hfid, realm, theme)
         VALUES (@hfid, @realm, (SELECT id FROM new_theme))
      RETURNING theme
  """, SingleRow=true>(connRuntime) in
  let dto = hfTheme |> (snd Domain.HfTheme.dto_) |> Dto.HfTheme.toDb in
  let themeId_ = Domain.HfTheme.theme_ >-> Domain.Theme.id_ in
  try
    let! id =
      cmd.AsyncExecute
        ( name=dto.theme.name
        , hfid=dto.hfid
        , realm=dto.realm
        , description=dto.theme.description
        )
    in
    return id |> Option.map (fun id -> hfTheme |> id^=themeId_)
  with
  | _ -> return None
}

let ``insert HfTheme and Theme or get id`` hfTheme = async
{ let dto = hfTheme |> (snd Domain.HfTheme.dto_) |> Dto.HfTheme.toDb in
  let themeId_ = Domain.HfTheme.theme_ >-> Domain.Theme.id_ in
  use cmd = DbTypes.Db.CreateCommand<"""
    SELECT theme.id
     FROM theme
          INNER JOIN hf_theme
          ON hf_theme.theme = theme.id
    WHERE hf_theme.hfid = @hfid
          AND hf_theme.realm = @realm
    LIMIT 1
  """, SingleRow=true>(connRuntime) in
  match! cmd.AsyncExecute(hfid=dto.hfid, realm=dto.realm) with
  | Some id -> return Some (hfTheme |> id^=themeId_)
  | _       -> return! ``insert new HfTheme and Theme`` hfTheme
}

let ``insert HfUser and Author or get id`` hfUser = async
{ let dto = hfUser |> (snd Domain.HfUser.dto_) |> Dto.HfUser.toDb in
  let userId_ = Domain.HfUser.author_ >-> Domain.Author.id_ in
  use selectId = DbTypes.Db.CreateCommand<"""
    SELECT author.id
      FROM author
           INNER JOIN hf_user
           ON hf_user.author = author.id
     WHERE hf_user.hfid = @hfid
           AND hf_user.realm = @realm
     LIMIT 1
  """, SingleRow=true>(connRuntime) in
  match! selectId.AsyncExecute(hfid=dto.hfid, realm=dto.realm) with
  | Some id -> return Some (hfUser |> id^=userId_)
  | _       -> use insert = DbTypes.Db.CreateCommand<"""
                        WITH new_user AS (INSERT INTO author (id, name)
                                               VALUES (DEFAULT, @name)
                                            RETURNING id)
                 INSERT INTO hf_user (hfid, realm, author)
                      VALUES (@hfid, @realm, (SELECT id FROM new_user))
                   RETURNING author
               """, SingleRow=true>(connRuntime) in
               match!
                 insert.AsyncExecute
                   ( hfid=dto.hfid
                   , realm=dto.realm
                   , name=dto.author.name
                   )
               with
               | Some id -> return Some (hfUser |> id^=userId_)
               | _       -> return None
}

let ``insert new HfThread and Thread`` hfThread = async
{ let threadId_ = Domain.HfThread.thread_ >-> Domain.Thread.id_ in
  let dto = hfThread |> (snd Domain.HfThread.dto_) |> Dto.HfThread.toDb in
  use insert = DbTypes.Db.CreateCommand<"""
           WITH new_thread AS (INSERT INTO thread (id, author, created_at,
                                                   updated_at, theme, name,
                                                   open, sticky)
                                    VALUES (DEFAULT, @author, @created_at,
                                            @updated_at, @theme, @name, @open,
                                            @sticky)
                                 RETURNING id)
    INSERT INTO hf_thread (hfid, realm, thread)
         VALUES (@hfid, @realm, (SELECT id FROM new_thread))
      RETURNING thread
  """, SingleRow=true>(connRuntime) in
  match!
    insert.AsyncExecute
      ( hfid=dto.hfid
      , realm=dto.realm
      , author=dto.thread.author.id
      , created_at=dto.thread.createdAt
      , updated_at=dto.thread.updatedAt
      , theme=dto.thread.theme.id
      , name=dto.thread.name
      , ``open``=dto.thread.``open``
      , sticky=dto.thread.sticky
      )
  with
  | Some id -> return Some (hfThread |> id^=threadId_)
  | _       -> return None
}

let ``insert HfThread and Thread or get id`` hfThread = async
{ let dto = hfThread |> (snd Domain.HfThread.dto_) |> Dto.HfThread.toDb in
  let threadId_ = Domain.HfThread.thread_ >-> Domain.Thread.id_ in
  use cmd = DbTypes.Db.CreateCommand<"""
    SELECT thread.id
     FROM thread
          INNER JOIN hf_thread
          ON hf_thread.thread = thread.id
    WHERE hf_thread.hfid = @hfid
          AND hf_thread.realm = @realm
    LIMIT 1
  """, SingleRow=true>(connRuntime) in
  match! cmd.AsyncExecute(hfid=dto.hfid, realm=dto.realm) with
  | Some id -> return Some (hfThread |> id^=threadId_)
  | _       -> return! ``insert new HfThread and Thread`` hfThread
}

let ``insert new HfPost and Post`` hfPost = async
{ let postId_ = Domain.HfPost.post_ >-> Domain.Post.id_ in
  let dto = hfPost |> (snd Domain.HfPost.dto_) |> Dto.HfPost.toDb in
  use insert = DbTypes.Db.CreateCommand<"""
           WITH new_post AS (INSERT INTO post (id, author, created_at,
                                               message, thread)
                                  VALUES (DEFAULT, @author, @created_at,
                                          @message, @thread)
                               RETURNING id)
    INSERT INTO hf_post (realm, post)
         VALUES (@realm, (SELECT id FROM new_post))
      RETURNING post
  """, SingleRow=true>(connRuntime) in
  match!
    insert.AsyncExecute
      ( realm=dto.realm
      , author=dto.post.author.id
      , created_at=dto.post.createdAt
      , message=dto.post.message
      , thread=dto.post.thread
      )
  with
  | Some id -> return Some (hfPost |> id^=postId_)
  | _       -> return None
}
