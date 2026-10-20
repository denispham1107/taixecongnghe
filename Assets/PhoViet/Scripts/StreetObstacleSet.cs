using System;
using UnityEngine;
namespace PhoViet
{
 // Lightweight collision proxies follow each recycled/rotated street tile. LOD
 // meshes remain unreadable and do not need thousands of MeshColliders on mobile.
 public sealed class StreetObstacleSet:MonoBehaviour
 {
  [Serializable] public struct Obstacle {public string kind;public Bounds bounds;public Obstacle(string k,Vector3 p,Vector3 size){kind=k;bounds=new Bounds(p,size);}}
  public Obstacle[] obstacles;
  Obstacle[] cached;Bounds localBounds;
  public Bounds LocalBounds
  {
   get
   {
    if(cached==obstacles)return localBounds;cached=obstacles;localBounds=new Bounds();
    if(obstacles!=null&&obstacles.Length>0){localBounds=obstacles[0].bounds;for(int i=1;i<obstacles.Length;i++)localBounds.Encapsulate(obstacles[i].bounds);}
    return localBounds;
   }
  }
 }
}
