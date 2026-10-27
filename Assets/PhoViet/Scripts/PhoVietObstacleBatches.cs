using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
 public partial class PhoVietGame
 {
  sealed class ObstacleBatch {public Transform root;public Bounds bounds;public readonly List<StreetObstacleSet> sets=new List<StreetObstacleSet>();}
  readonly List<ObstacleBatch> obstacleBatches=new List<ObstacleBatch>();
  StreetObstacleSet[] obstacleBatchSource;StreetObstacleSet.Obstacle[][] obstacleBatchArrays;
  void EnsureObstacleBatches()
  {
   bool same=obstacleBatchSource==streetObstacles&&obstacleBatchArrays!=null&&obstacleBatchArrays.Length==streetObstacles.Length;
   if(same)for(int i=0;i<streetObstacles.Length;i++)if(streetObstacles[i]!=null&&obstacleBatchArrays[i]!=streetObstacles[i].obstacles){same=false;break;}
   if(same)return;
   obstacleBatches.Clear();obstacleBatchSource=streetObstacles;obstacleBatchArrays=new StreetObstacleSet.Obstacle[streetObstacles.Length][];
   var roots=new HashSet<Transform>();if(roadRows!=null)foreach(var row in roadRows)foreach(var tile in row.tiles)roots.Add(tile);
   var map=new Dictionary<Transform,ObstacleBatch>();
   for(int i=0;i<streetObstacles.Length;i++)
   {
    var set=streetObstacles[i];if(set==null)continue;obstacleBatchArrays[i]=set.obstacles;
    var root=set.transform;while(root!=null&&!roots.Contains(root))root=root.parent;if(root==null)root=set.transform;
    bool exists=map.TryGetValue(root,out var batch);if(!exists){batch=new ObstacleBatch{root=root};map.Add(root,batch);obstacleBatches.Add(batch);}
    var bounds=set.LocalBounds;var matrix=root.worldToLocalMatrix*set.transform.localToWorldMatrix;
    for(int corner=0;corner<8;corner++)
    {
     var p=matrix.MultiplyPoint3x4(new Vector3((corner&1)==0?bounds.min.x:bounds.max.x,(corner&2)==0?bounds.min.y:bounds.max.y,(corner&4)==0?bounds.min.z:bounds.max.z));
     if(!exists&&corner==0)batch.bounds=new Bounds(p,Vector3.zero);else batch.bounds.Encapsulate(p);
    }
    batch.sets.Add(set);
   }
  }
  bool NearbyObstacleBatch(ObstacleBatch batch,Vector3 from,Vector3 to)
  {
   if(batch.root==null||!batch.root.gameObject.activeInHierarchy)return false;
   Vector3 centre=transform.InverseTransformPoint(batch.root.TransformPoint(batch.bounds.center));
   float radius=batch.bounds.extents.magnitude*batch.root.lossyScale.magnitude+Mathf.Sqrt(PlayerHalfLength*PlayerHalfLength+PlayerHalfWidth*PlayerHalfWidth)+.1f;
   return Mathf.Abs(centre.x-to.x)<=radius+Mathf.Abs(to.x-from.x)&&Mathf.Abs(centre.z-to.z)<=radius+Mathf.Abs(to.z-from.z);
  }
 }
}
