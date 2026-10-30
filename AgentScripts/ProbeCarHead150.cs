using System;using System.IO;using System.Linq;using UnityEngine;using PhoViet;
public static class ProbeCarHead150
{
 public static object Main()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();var rows=g.daylight.vehicles.Where(v=>v.root!=null&&v.head!=null).Select(v=>{
   var front=v.halos.Where(h=>!h.rear&&h.source!=null).ToArray();var car=v.root.GetComponent<MeshyCarVariants>();var player=v.root.GetComponent<PlayerCarVisual>();var p=v.root.InverseTransformPoint(v.head.position);
   return new{root=v.root.name,v.player,meshCar=car!=null,playerCar=player!=null,frontCount=front.Length,head=v.head.name,parent=v.head.parent.name,headAlias=front.Any(h=>h.source==v.head),localPosition=new[]{p.x,p.y,p.z},frontPositions=front.Select(h=>{var a=v.root.InverseTransformPoint(h.source.position);return new[]{a.x,a.y,a.z};}).ToArray(),npcAlias=car!=null&&car.frontHalos.Contains(car.headAnchor)};
  }).ToArray();var r=new{success=true,rows};File.WriteAllText("Captures/release150-head-before.json",Newtonsoft.Json.JsonConvert.SerializeObject(r,Newtonsoft.Json.Formatting.Indented));return r;
 }
}
