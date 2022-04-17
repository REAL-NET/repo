﻿(* Copyright 2017-2018 REAL.NET group
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

namespace Repo.Metametamodels

open Repo.DataLayer
open Repo.CoreSemanticLayer
open Repo.InfrastructureSemanticLayer

/// Initializes repository with test model conforming to Query Metamodel, actual program that can be written by end-user.
type QueryTestModelBuilder() =
    interface IModelBuilder with
        member this.Build(repo: IRepo): unit =
            let infrastructure = InfrastructureSemantic(repo)
            let metamodel = Repo.findModel repo "QueryMetamodel"

            let metamodelAbstractQueryBlock = Model.findNode metamodel "AbstractQueryBlock"
            let metamodelMaterializationPlank = Model.findNode metamodel "MaterializationPlank"
            let metamodelSort = Model.findNode metamodel "Sort"
            let metamodelAggregate = Model.findNode metamodel "Aggregate"
            let metamodelJoin = Model.findNode metamodel "Join"
            let metamodelFilter = Model.findNode metamodel "Filter"
            let metamodelRead = Model.findNode metamodel "Read"
            let metamodelOperatorInternals = Model.findNode metamodel "OperatorInternals"

            let link = Model.findAssociationWithSource metamodelAbstractQueryBlock "target"

            let model = repo.CreateModel("QueryTestModel", metamodel)

            let materializationPlank = infrastructure.Instantiate model metamodelMaterializationPlank

            let sort = infrastructure.Instantiate model metamodelSort
            let sortNode = Model.findNode model "aSort"
            sortNode.Name <- "sort"
            infrastructure.Element.SetAttributeValue sort "children" "aggregate"
            infrastructure.Element.SetAttributeValue sort "connectionType" "local"

            let operatorInternals1 = infrastructure.Instantiate model metamodelOperatorInternals
            infrastructure.Element.SetAttributeValue operatorInternals1 "contents" "aggregate"

            let aggregate = infrastructure.Instantiate model metamodelAggregate
            let aggregateNode = Model.findNode model "aAggregate"
            aggregateNode.Name <- "aggregate"
            infrastructure.Element.SetAttributeValue aggregate "children" "join"
            infrastructure.Element.SetAttributeValue aggregate "parent" "sort"
            infrastructure.Element.SetAttributeValue aggregate "connectionType" "local"

            let operatorInternals2 = infrastructure.Instantiate model metamodelOperatorInternals
            infrastructure.Element.SetAttributeValue operatorInternals2 "contents" "join, read1, read2"

            let join = infrastructure.Instantiate model metamodelJoin
            let joinNode = Model.findNode model "aJoin"
            joinNode.Name <- "join"
            infrastructure.Element.SetAttributeValue join "children" "read1, read2"
            infrastructure.Element.SetAttributeValue join "parent" "aggregate"
            infrastructure.Element.SetAttributeValue join "connectionType" "local"

            let read1 = infrastructure.Instantiate model metamodelRead
            let readNode1 = Model.findNode model "aRead"
            readNode1.Name <- "read1"
            infrastructure.Element.SetAttributeValue read1 "parent" "join"
            infrastructure.Element.SetAttributeValue read1 "connectionType" "local"
            infrastructure.Element.SetAttributeValue read1 "argument" "d_datekey"

            let read2 = infrastructure.Instantiate model metamodelRead
            let readNode2 = Model.findNode model "aRead"
            readNode2.Name <- "read2"
            infrastructure.Element.SetAttributeValue read2 "parent" "join"
            infrastructure.Element.SetAttributeValue read2 "connectionType" "remote"
            infrastructure.Element.SetAttributeValue read2 "argument" "lo_orderdate"

            let filter = infrastructure.Instantiate model metamodelFilter
            let filterNode = Model.findNode model "aFilter"
            filterNode.Name <- "filter"
            infrastructure.Element.SetAttributeValue filter "children" "read3"

            let read3 = infrastructure.Instantiate model metamodelRead
            let readNode3 = Model.findNode model "aRead"
            readNode3.Name <- "read3"
            infrastructure.Element.SetAttributeValue read3 "parent" "filter"
            infrastructure.Element.SetAttributeValue read3 "argument" "p_category"

            let (-->) (src: IElement) dst =
                let aLink = infrastructure.Instantiate model link :?> IAssociation
                aLink.Source <- Some src
                aLink.Target <- Some dst
                dst

            sort --> aggregate --> join |> ignore
            join --> read1 |> ignore
            join --> read2 |> ignore
            filter --> read3 |> ignore

            ()
