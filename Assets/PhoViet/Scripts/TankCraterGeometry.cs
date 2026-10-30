using System;using System.Collections.Generic;using UnityEngine;
namespace PhoViet
{
 // Shared geometry is built once, never tessellated or allocated per explosion.
 public static class TankCraterGeometry
 {
  public static float Outline(float angle,float seed)=>1+.035f*Mathf.Sin(angle*7+seed)+.025f*Mathf.Sin(angle*13-seed*3);
  public static Mesh Build(int variant)
  {
   float seed=variant*1.73f+.41f;var random=new System.Random(9187+variant*71);
   var vertices=new List<Vector3>();var colors=new List<Color>();var indices=new List<int>();
   void Triangle(Vector3 a,Vector3 b,Vector3 c,Color color)
   {
    int start=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);colors.Add(color);colors.Add(color);colors.Add(color);
    if(Vector3.Cross(b-a,c-a).y<0){indices.Add(start);indices.Add(start+2);indices.Add(start+1);}else{indices.Add(start);indices.Add(start+1);indices.Add(start+2);}
   }
   float Next()=> (float)random.NextDouble();
   const int sides=56;
   float[] rings={0,.38f,.72f,.94f,1.025f,1.12f};float[] heights={-.40f,-.38f,-.27f,-.09f,.115f,.009f};
   for(int ring=0;ring<rings.Length-1;ring++)for(int i=0;i<sides;i++)
   {
    Vector3 P(int j,int k){float a=j*Mathf.PI*2/sides;float r=rings[k]*Outline(a,seed);return new Vector3(Mathf.Cos(a)*r,heights[k]+(k>0?.012f*Mathf.Sin(a*11+seed):0),Mathf.Sin(a)*r);}
    var col=Color.Lerp(new Color(.105f,.092f,.077f),new Color(.27f,.255f,.23f),ring/4f)*( .88f+Next()*.22f);
    if(ring==0)Triangle(P(i,0),P(i,1),P(i+1,1),col);
    else{Triangle(P(i,ring),P(i+1,ring),P(i+1,ring+1),col);Triangle(P(i,ring),P(i+1,ring+1),P(i,ring+1),col);}
   }
   // Narrow branching fissures, laid on the untouched asphalt outside the rim.
   for(int i=0;i<16;i++)
   {
    float a=i*Mathf.PI*2/16+Next()*.19f;Vector3 p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*1.07f;
    for(int s=0;s<3;s++)
    {
     float b=a+(Next()-.5f)*.8f;var q=p+new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*(.09f+Next()*.09f);
     var normal=new Vector3(-(q-p).z,0,(q-p).x).normalized*(.014f-s*.003f);p.y=q.y=.012f+s*.001f;
     Triangle(p-normal,p+normal,q+normal,new Color(.065f,.061f,.056f));Triangle(p-normal,q+normal,q-normal,new Color(.065f,.061f,.056f));p=q;
    }
   }
   // Angular slabs around the rim; faces share the same single draw as the bowl.
   for(int i=0;i<18;i++)
   {
    float a=Next()*Mathf.PI*2;var centre=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(1.01f+Next()*.15f);
    var u=new Vector3(Mathf.Cos(a+.7f),0,Mathf.Sin(a+.7f))*(.025f+Next()*.045f);var w=new Vector3(-u.z,0,u.x)*.7f;
    var peak=centre+Vector3.up*(.07f+Next()*.11f);var c=new Color(.24f,.23f,.21f)*(.8f+Next()*.5f);
    Triangle(centre-u-w,centre+u-w,peak,c);Triangle(centre+u-w,centre+u+w,peak,c*.82f);Triangle(centre+u+w,centre-u+w,peak,c*.9f);Triangle(centre-u+w,centre-u-w,peak,c*.7f);
   }
   var mesh=new Mesh{name="PV_TankCrater_"+variant};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
  }
  public static Mesh Debris()
  {
   var mesh=new Mesh{name="PV_AsphaltShards"};mesh.vertices=new[]{Vector3.up*.5f,Vector3.down*.32f,Vector3.left*.5f,Vector3.right*.45f,Vector3.forward*.37f,Vector3.back*.4f};
   mesh.triangles=new[]{0,4,3,0,3,5,0,5,2,0,2,4,1,3,4,1,5,3,1,2,5,1,4,2};
   mesh.colors=new[]{new Color(.3f,.28f,.25f),new Color(.13f,.12f,.1f),new Color(.22f,.21f,.19f),new Color(.26f,.25f,.23f),new Color(.19f,.18f,.17f),new Color(.23f,.22f,.2f)};mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
  }
 }
}
