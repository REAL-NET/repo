namespace Repo.DeepMetamodel.Details.Elements

open System
open Repo
open Repo.AttributeMetamodel
open Repo.LanguageMetamodel
open Repo.DeepMetamodel

[<AbstractClass>]
type DeepElement(element: ILanguageElement, pool: DeepPool, repo: ILanguageRepository, level: int, potency: int) =
    inherit DeepContext(level, potency)
    
    let languageMetamodel =
        repo.Model Consts.deepMetamodel
        
    let attributeMetatype =
        languageMetamodel.Node Consts.attribute
        
    let slotMetatype =
        languageMetamodel.Node Consts.slot
        
    let typeAssociationMetatype =
        languageMetamodel.Association Consts.typeRelationship
    
    let attributesAssociationMetatype =
        languageMetamodel.Association Consts.attributesRelationship
        
    let slotsAssociationMetatype =
        languageMetamodel.Association Consts.slotsRelationship
        
    let attributeAssociationMetatype =
        languageMetamodel.Association Consts.attributeRelationship
        
    let valueAssociationMetatype =
        languageMetamodel.Association Consts.valueRelationship
             
    let (--->) source (target, metatype) =
        element.Model.InstantiateAssociation source target metatype Map.empty
        |> ignore
    
    let wrap = pool.Wrap
    
    let mutable myName: string = "name"
    
    member this.UnderlyingElement = element
        
    interface IDeepElement with
        member this.Name
            with get() = myName
            and set value = myName <- value
        
        member this.OutgoingEdges =
            element.OutgoingEdges
            |> Seq.map pool.Wrap
            |> Seq.cast<IDeepRelationship>
        
        member this.OutgoingAssociations =
            element.OutgoingAssociations
            |> Seq.map (fun e -> pool.WrapAssociation e 0 0 0 0 0 0)    
            |> Seq.cast<IDeepAssociation>

        member this.IncomingAssociations =
            element.IncomingAssociations
            |> Seq.map (fun e -> pool.WrapAssociation e 0 0 0 0 0 0)
            |> Seq.cast<IDeepAssociation>

        member this.DirectSupertypes =
            element.OutgoingEdges
            |> Seq.filter (fun e -> e :? IDeepGeneralization)
            |> Seq.map (fun e -> e.Target)
            |> Seq.map (fun e -> wrap e 0 0)

        member this.Attributes =
             let selfAttributes =
                element.OutgoingAssociations
                |> Seq.filter (fun a -> a.Metatype = (attributesAssociationMetatype :> ILanguageElement))
                |> Seq.map (fun a -> a.Target)
                |> Seq.map (fun e -> pool.WrapAttribute e 0 0) 
                |> Seq.cast<IDeepAttribute>
                
             (this :> IDeepElement).DirectSupertypes
            |> Seq.map (fun e -> e.Attributes)
            |> Seq.concat
            |> Seq.append selfAttributes
            
        member this.AddAttribute name ``type`` level potency =
            if (this :> IDeepElement).Attributes
               |> Seq.filter (fun a -> a.Name = name)
               |> Seq.length = 1 then
                raise <| AmbiguousAttributesException(name)
            let attributeNode = element.Model.InstantiateNode name attributeMetatype Map.empty
            attributeNode ---> ((``type`` :?> DeepElement).UnderlyingElement, typeAssociationMetatype)
            element ---> (attributeNode, attributesAssociationMetatype)
            pool.WrapAttribute attributeNode level potency
 
        member this.Slots =
            element.OutgoingAssociations
            |> Seq.filter (fun a -> a.Metatype = (slotsAssociationMetatype :> ILanguageElement))
            |> Seq.map (fun a -> a.Target)
            |> Seq.map (fun e -> pool.WrapSlot e 0 0 )
            
        member this.AddSlot attribute value level potency =
            let name = "Slot." + attribute.Name + Guid.NewGuid().ToString()
            let slotNode = element.Model.InstantiateNode name slotMetatype Map.empty
            slotNode ---> ((attribute :?> DeepAttribute).UnderlyingAttribute, attributeAssociationMetatype)
            slotNode ---> ((value :?> DeepElement).UnderlyingElement, valueAssociationMetatype)
            element ---> (slotNode, slotsAssociationMetatype)
            pool.WrapSlot slotNode level potency

        member this.Model: IDeepModel =
            pool.WrapModel element.Model

        member this.HasMetatype =
            failwith "Not implemented"

        member this.Metatype =
            pool.Wrap element.Metatype 0 0 
    