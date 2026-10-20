using System;
using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
    public partial class PhoVietGame
    {
        [Serializable] public sealed class RoadRow {public Transform root;public List<Transform> tiles=new List<Transform>();}
        public List<RoadRow> roadRows=new List<RoadRow>();
        public bool IsTurning {get;private set;}
        public int QueuedTurn {get;private set;}
        public int CompletedTurns {get;private set;}
        public float TurnYaw {get;private set;}
        public float NextJunctionDistance=>junctionAt-Distance;
        public string NextStreet=>junctionStreet;
        public float RoadProgress=>Distance+roadOffset;
        public string RouteStreet {get;private set;}="Trần Hưng Đạo";
        static readonly string[] RouteNames={"Trần Hưng Đạo","Quang Trung","Hai Bà Trưng","Lê Lợi","Lý Thường Kiệt","Lý Nam Đế","Đinh Bộ Lĩnh","Nguyễn Huệ","Nguyễn Trãi","Lê Duẩn","Điện Biên Phủ","Võ Thị Sáu","Nguyễn Thị Minh Khai","Cách Mạng Tháng Tám","Pasteur","Nam Kỳ Khởi Nghĩa","Nguyễn Đình Chiểu","Phan Đình Phùng","Trương Định","Bà Triệu","Ngô Quyền","Lê Hồng Phong","Trần Phú","Nguyễn Văn Cừ","Hoàng Diệu","Đề Thám","Tôn Đức Thắng","Bến Vân Đồn","Hàm Nghi","Đồng Khởi","Lạc Long Quân","Âu Cơ","Trần Quang Khải","Phan Chu Trinh","Nguyễn Tri Phương","Trần Bình Trọng","Võ Văn Tần","Nguyễn Hữu Cảnh","Phạm Ngũ Lão","Mai Chí Thọ","Hoàng Văn Thụ","Nguyễn Văn Trỗi","Nguyễn Bỉnh Khiêm","Lê Văn Sỹ","Hồng Bàng","Trần Não","Phan Văn Trị","Nguyễn Xí"};
        readonly HashSet<string> visitedStreets=new HashSet<string>();
        int mainRow,leftRow=1,rightRow=2,turnSide;
        float junctionAt,roadOffset,turnRadius,turnTravel,cameraTurnBlend,oldRiderZ;
        float turnLength;bool lateTurn;
        Vector3 lateControl1,lateControl2,lateEnd;
        readonly float[] lateArcLengths=new float[65];
        Vector3 turnStart,crashTurnPosition;Quaternion crashTurnRotation;
        string junctionStreet;
        bool turnCrash,routePolice;float policeRouteAt,policeFallYaw,turnEndAt;
        readonly List<RoutePoint> routeTrail=new List<RoutePoint>();
        struct RoutePoint {public float at;public Vector3 point;public float yaw;public RoutePoint(float a,Vector3 p,float y){at=a;point=p;yaw=y;}}
        bool RoutesReady=>roadRows!=null&&roadRows.Count==5;
        void ResetRoutes()
        {
            IsTurning=false;QueuedTurn=0;CompletedTurns=0;TurnYaw=0;roadOffset=0;cameraTurnBlend=0;turnCrash=false;routePolice=false;oldRiderZ=0;
            ResetExitTraffic();lateTurn=false;turnLength=0;
            mainRow=0;RouteStreet="Trần Hưng Đạo";visitedStreets.Clear();visitedStreets.Add(RouteStreet);routeTrail.Clear();
            for(int i=-160;i<=0;i+=2)routeTrail.Add(new RoutePoint(i,new Vector3(LaneX[2],0,i),0));
            if(!RoutesReady)return;
            for(int i=0;i<roadRows.Count;i++)
            {
                var row=roadRows[i];row.root.localPosition=Vector3.zero;row.root.localRotation=Quaternion.identity;row.root.gameObject.SetActive(i==0);
                for(int j=0;j<row.tiles.Count;j++){row.tiles[j].localPosition=new Vector3(0,0,(j-(i==0?1:0))*80);row.tiles[j].localRotation=Quaternion.identity;}
            }
            blocks=new List<Transform>(roadRows[0].tiles);LabelRow(mainRow,RouteStreet);PrepareBranches(70);
        }
        string PickStreet()
        {
            var candidates=new List<string>();foreach(string n in RouteNames)if(!visitedStreets.Contains(n))candidates.Add(n);
            if(candidates.Count>0)return candidates[rng.Next(candidates.Count)];
            return "Đường " +RouteNames[CompletedTurns%RouteNames.Length]+" nối dài "+(CompletedTurns/RouteNames.Length+1);
        }
        void LabelRow(int index,string street)
        {
            foreach(var text in roadRows[index].root.GetComponentsInChildren<TextMesh>(true))
                if(text.transform.parent!=null&&text.transform.parent.name.StartsWith("Bảng tên đường",StringComparison.Ordinal)&&text.name!="ĐƯỜNG")text.text=street.ToUpperInvariant();
        }
        void PrepareBranches(float distance)
        {
            junctionAt=Distance+distance;junctionStreet=GridJunctionStreet();int first=-1,second=-1;
            for(int i=0;i<5;i++)if(i!=mainRow&&i!=leftRow&&i!=rightRow){if(first<0)first=i;else second=i;}
            // On reset all four spare rows are empty; subsequent calls keep the last junction behind us.
            if(first<0||second<0){first=(mainRow+1)%5;second=(mainRow+2)%5;}
            leftRow=first;rightRow=second;
            foreach(int index in new[]{leftRow,rightRow})
            {
                float side=index==leftRow?-1:1;var row=roadRows[index];row.root.gameObject.SetActive(true);
                row.root.localPosition=new Vector3(side*10,0,distance);row.root.localRotation=Quaternion.Euler(0,side*90,0);
                for(int j=0;j<row.tiles.Count;j++){row.tiles[j].localPosition=new Vector3(0,0,j*80);row.tiles[j].localRotation=Quaternion.identity;}
                LabelRow(index,junctionStreet);
            }
            CityJunctionPrepared();
        }
        public static bool CanTurnFromLane(int direction,int lane)=>lane>=0&&lane<4&&(direction<0?lane<=1:direction>0&&lane>=2);
        bool SelectJunctionTurn(int direction)
        {
            if(!RoutesReady||direction==0)return false;if(IsTurning)return true;
            if(QueuedTurn==Math.Sign(direction)&&CanTurnFromLane(direction,CurrentLane))return true;
            float near=NextJunctionDistance;
            // Accept an unqueued turn throughout the actual open intersection.
            bool atEdge=direction<0?rider.localPosition.x<=-.65f:rider.localPosition.x>=.65f;
            if(near>=-7&&near<=7&&atEdge)
            {
                CurrentLane=direction<0?(rider.localPosition.x<=-3.1f?0:1):(rider.localPosition.x>=3.1f?3:2);
                QueuedTurn=Math.Sign(direction);StartJunctionTurn(true);return true;
            }
            bool lane=CanTurnFromLane(direction,CurrentLane);
            if(near>=12&&near<=30&&lane&&Mathf.Abs(rider.localPosition.x-LaneX[CurrentLane])<.6f)
            {
                QueuedTurn=Math.Sign(direction);notice=(direction<0?"ĐÃ CHỌN RẼ TRÁI · ":"ĐÃ CHỌN RẼ PHẢI · ")+junctionStreet.ToUpperInvariant();noticeTimer=2;return true;
            }
            QueuedTurn=0;return false;
        }
        void ScrollRoutes(float delta)
        {
            if(RoutesReady)foreach(var row in roadRows)if(row!=roadRows[mainRow]&&row.root.gameObject.activeSelf)row.root.localPosition-=Vector3.forward*delta;
            foreach(var b in blocks){b.localPosition-=Vector3.forward*delta;if(b.localPosition.z<-160)b.localPosition+=Vector3.forward*80*blocks.Count;}
            for(int i=0;i<routeTrail.Count;i++){var p=routeTrail[i];p.point-=Vector3.forward*delta;routeTrail[i]=p;}
        }
        void BeginOrContinueTurn(float delta)
        {
            if(!RoutesReady)return;
            if(!IsTurning&&QueuedTurn!=0&&NextJunctionDistance<=11.65f)
            {
                if(!CanTurnFromLane(QueuedTurn,CurrentLane)||Mathf.Abs(rider.localPosition.x-LaneX[CurrentLane])>.65f){QueuedTurn=0;return;}
                StartJunctionTurn(false);
                return;
            }
            if(!IsTurning)return;
            turnTravel=Mathf.Min(turnLength,turnTravel+delta);
            if(lateTurn)
            {
                int i=1;while(i<lateArcLengths.Length-1&&lateArcLengths[i]<turnTravel)i++;
                float fraction=Mathf.InverseLerp(lateArcLengths[i-1],lateArcLengths[i],turnTravel),t=(i-1+fraction)/64;
                float u=1-t;
                Vector3 tangent=3*u*u*(lateControl1-turnStart)+6*u*t*(lateControl2-lateControl1)+3*t*t*(lateEnd-lateControl2);
                TurnYaw=Mathf.Atan2(tangent.x,tangent.z)*Mathf.Rad2Deg;
                rider.localPosition=LateTurnPoint(t)-Vector3.forward*turnTravel;
                rider.localRotation=Quaternion.Euler(0,TurnYaw,-turnSide*16*Mathf.Sin(t*Mathf.PI));
            }
            else
            {
                float angle=turnTravel/turnRadius;TurnYaw=turnSide*angle*Mathf.Rad2Deg;
                rider.localPosition=new Vector3(turnStart.x+turnSide*turnRadius*(1-Mathf.Cos(angle)),0,turnStart.z+turnRadius*Mathf.Sin(angle)-turnTravel);
                rider.localRotation=Quaternion.Euler(0,TurnYaw,-turnSide*18*Mathf.Sin(angle*2));
            }
            lastX=rider.localPosition.x;
        }
        void StartJunctionTurn(bool late)
        {
            IsTurning=true;turnSide=QueuedTurn;QueuedTurn=0;turnStart=rider.localPosition;turnStart.y=0;
            turnRadius=Mathf.Max(4,NextJunctionDistance-turnSide*LaneX[CurrentLane]);turnTravel=0;TurnYaw=0;lateTurn=late;
            turnLength=turnRadius*Mathf.PI*.5f;
            if(late)
            {
                // A late input needs room to align with the same exit lane. A tangent-
                // continuous curve keeps the bike inside the union of the two roads.
                lateControl1=turnStart+Vector3.forward*3;
                lateEnd=new Vector3(turnSide*20,0,NextJunctionDistance-turnSide*LaneX[CurrentLane]);
                lateControl2=lateEnd-Vector3.right*turnSide*6;
                Vector3 previous=turnStart;lateArcLengths[0]=0;
                for(int i=1;i<65;i++){Vector3 p=LateTurnPoint(i/64f);lateArcLengths[i]=lateArcLengths[i-1]+Vector3.Distance(previous,p);previous=p;}
                turnLength=lateArcLengths[64];
            }
            if(IsChased&&!routePolice){routePolice=true;policeRouteAt=Distance-PoliceDistance;}
            PreviewJunctionNavigation();
        }
        Vector3 LateTurnPoint(float t)
        {
            float u=1-t;return u*u*u*turnStart+3*u*u*t*lateControl1+3*u*t*t*lateControl2+t*t*t*lateEnd;
        }
        void EndRouteFrame(float dt)
        {
            if(!RoutesReady)return;
            if(routeTrail.Count==0||Distance-routeTrail[routeTrail.Count-1].at>.3f)routeTrail.Add(new RoutePoint(Distance,rider.localPosition,TurnYaw));
            while(routeTrail.Count>2&&routeTrail[1].at<Distance-180)routeTrail.RemoveAt(0);
            if(IsTurning&&turnTravel>=turnLength-.0001f){FinishJunctionTurn();return;}
            if(!IsTurning&&NextJunctionDistance< -18){PassCityJunction();PrepareBranches(NextJunctionDistance+80);}
            cameraTurnBlend=Mathf.Max(0,cameraTurnBlend-dt);
        }
        void FinishJunctionTurn()
        {
            if(boostSmoke!=null)boostSmoke.Rebase();
            Quaternion exit=Quaternion.Euler(0,turnSide*90,0),inverse=Quaternion.Inverse(exit);
            Vector3 origin=rider.localPosition-exit*new Vector3(LaneX[CurrentLane],0,0);origin.y=0;
            foreach(var row in roadRows)
            {
                if(row==roadRows[mainRow])
                {
                    // Its tiles already scroll individually; put them under the common rigid rebase.
                    row.root.localPosition=Vector3.zero;row.root.localRotation=Quaternion.identity;
                }
                row.root.localPosition=inverse*(row.root.localPosition-origin);row.root.localRotation=inverse*row.root.localRotation;
            }
            foreach(var v in vehicles)
            {
                if(!v.active)continue;
                v.model.localPosition=inverse*(v.model.localPosition-origin);v.model.localRotation=inverse*v.model.localRotation;
                v.previousPosition=inverse*(v.previousPosition-origin);v.previousYaw-=turnSide*90;v.initialYaw-=turnSide*90;
                v.knockVelocity=inverse*v.knockVelocity;v.motionVelocity=inverse*v.motionVelocity;v.crashEuler.y-=turnSide*90;
                v.roadTurnStart=inverse*(v.roadTurnStart-origin);v.roadTurnForward=inverse*v.roadTurnForward;v.roadTurnSide=inverse*v.roadTurnSide;
                v.routeDetached=true;v.routeVelocity=v.motionVelocity;
                if(v.crashEffect!=null){v.crashEffect.localPosition=inverse*(v.crashEffect.localPosition-origin);v.crashEffect.localRotation=inverse*v.crashEffect.localRotation;v.routeEffectOffset=v.crashEffect.localPosition-v.model.localPosition;}
            }
            if(policeBike!=null){policeBike.localPosition=inverse*(policeBike.localPosition-origin);policeBike.localRotation=inverse*policeBike.localRotation;}
            policeFallYaw-=turnSide*90;
            rideCamera.transform.localPosition=inverse*(rideCamera.transform.localPosition-origin);rideCamera.transform.localRotation=inverse*rideCamera.transform.localRotation;
            for(int i=0;i<routeTrail.Count;i++){var p=routeTrail[i];p.point=inverse*(p.point-origin);p.yaw-=turnSide*90;routeTrail[i]=p;}
            int oldMain=mainRow,unused=turnSide<0?rightRow:leftRow;
            int selected=turnSide<0?leftRow:rightRow;mainRow=selected;var chosen=roadRows[mainRow];
            var positions=new Vector3[chosen.tiles.Count];for(int i=0;i<positions.Length;i++)positions[i]=chosen.root.localPosition+chosen.root.localRotation*chosen.tiles[i].localPosition;
            chosen.root.localPosition=Vector3.zero;chosen.root.localRotation=Quaternion.identity;
            for(int i=0;i<positions.Length;i++){chosen.tiles[i].localPosition=positions[i];chosen.tiles[i].localRotation=Quaternion.identity;}
            blocks=new List<Transform>(chosen.tiles);roadOffset=-Distance-positions[0].z;
            rider.localPosition=new Vector3(LaneX[CurrentLane],jumpHeight,0);rider.localRotation=Quaternion.identity;lastX=rider.localPosition.x;oldRiderZ=0;
            TurnCityJunction(turnSide);RouteStreet=junctionStreet;visitedStreets.Add(RouteStreet);CompletedTurns++;IsTurning=false;TurnYaw=0;turnCrash=false;cameraTurnBlend=.7f;turnEndAt=Distance;
            float nearest=float.MaxValue;foreach(var b in blocks){float z=b.localPosition.z+70;if(z>12)nearest=Mathf.Min(nearest,z);}
            nextIntersectionDistance=Distance+nearest;leftRow=oldMain;rightRow=unused;PrepareBranches(nearest);notice="ĐÃ VÀO ĐƯỜNG "+RouteStreet.ToUpperInvariant();noticeTimer=2;
            QueueExitTraffic();
        }
        void RouteTurnHud()
        {
            if(!RoutesReady||State!=RideState.Riding)return;
            string value=IsTurning?"ĐANG RẼ · "+junctionStreet:QueuedTurn!=0?"ĐÃ CHỌN RẼ "+(QueuedTurn<0?"TRÁI":"PHẢI")+" · "+junctionStreet:
                NextJunctionDistance>=-7&&NextJunctionDistance<=7?"HAI LÀN TRÁI RẼ TRÁI · HAI LÀN PHẢI RẼ PHẢI":
                NextJunctionDistance<=30&&NextJunctionDistance>12?"THEO MŨI TÊN: BẤM / VUỐT ĐỂ CHỌN RẼ":"";
            if(value.Length==0)return;float w=portrait?420:540;Panel(new Rect((uiW-w)/2,portrait?115:100,w,38),StreetArt.Ink);Txt(value,(uiW-w)/2+10,portrait?123:108,w-20,24,portrait?11:13,StreetArt.Cream,TextAnchor.MiddleCenter);
        }
    }
}
