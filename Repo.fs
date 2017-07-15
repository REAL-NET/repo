﻿(* Copyright 2017 Yurii Litvinov
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License. *)

namespace RepoExperimental.FacadeLayer

open RepoExperimental

/// Wrapper around repository from data layer. Maintains a repository of models and creates new models if needed.
type Repo(repo: DataLayer.IRepo) =
    let modelRepository = ModelRepository(repo)

    let unwrap (model: IModel) = (model :?> Model).UnderlyingModel

    member this.UnderlyingRepo = repo

    interface RepoExperimental.IRepo with
        member this.Models
            with get () =
                repo.Models |> Seq.map modelRepository.GetModel

        member this.CreateModel(name, metamodel) =
            let underlyingModel = repo.CreateModel(name, unwrap metamodel)
            modelRepository.GetModel underlyingModel
        
        member this.DeleteModel model =
            repo.DeleteModel (unwrap model)
            // TODO: Remove all elements from this model too.
            modelRepository.DeleteModel model