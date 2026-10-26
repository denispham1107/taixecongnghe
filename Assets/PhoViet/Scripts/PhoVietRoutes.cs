using System;
using System.Collections.Generic;
using UnityEngine;
namespace PhoViet
{
    public partial class PhoVietGame
    {
        [Serializable] public sealed class RoadRow
        {
            public Transform root;public List<Transform> tiles=new List<Transform>();
            [NonSerialized] public TextMesh[] streetLabels;
        }
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
        Vector3 turnControl1,turnControl2,turnControl3,turnControl4,turnEnd;
        readonly float[] turnArcLengths=new float[129];
        Vector3 turnStart;Quaternion crashTurnRotation;
        string junctionStreet;
        bool routePolice;float policeRouteAt,policeFallYaw,turnEndAt;
        readonly List<RoutePoint> routeTrail=new List<RoutePoint>();
        struct RoutePoint {public float at;public Vector3 point;public float yaw;public RoutePoint(float a,Vector3 p,float y){at=a;point=p;yaw=y;}}
        bool RoutesReady=>roadRows!=null&&roadRows.Count==5;
        void ResetRoutes()
        {
            IsTurning=false;QueuedTurn=0;CompletedTurns=0;TurnYaw=0;roadOffset=0;cameraTurnBlend=0;routePolice=false;oldRiderZ=0;
            ResetExitTraffic();lateTurn=false;turnLength=0;
            mainRow=0;RouteStreet="Trần Hưng Đạo";visitedStreets.Clear();visitedStreets.Add(RouteStreet);routeTrail.Clear();
            for(int i=-160;i<=0;i+=2)routeTrail.Add(new RoutePoint(i,new Vector3(LaneX[2],0,i),0));
            if(!RoutesReady)return;
            for(int i=0;i<roadRows.Count;i++)
            {
                var row=roadRows[i];row.root.localPosition=Vector3.zero;row.root.localRotation=Quaternion.identity;row.root.gameObject.SetActive(i==0);
                for(int j=0;j<row.tiles.Count;j++){row.tiles[j].localPosition=new Vector3(0,0,(j-(i==0?1:0))*80);row.tiles[j].localRotation=Quaternion.identity;}
            }
            foreach(var row in roadRows)CacheStreetLabels(row);
            blocks=new List<Transform>(roadRows[0].tiles);AssignRiverRow(roadRows[0].tiles,2);LabelRow(mainRow,RouteStreet);PrepareBranches(70);
        }
        string PickStreet()
        {
            var candidates=new List<string>();foreach(string n in RouteNames)if(!visitedStreets.Contains(n))candidates.Add(n);
            if(candidates.Count>0)return candidates[rng.Next(candidates.Count)];
            return "Đường " +RouteNames[CompletedTurns%RouteNames.Length]+" nối dài "+(CompletedTurns/RouteNames.Length+1);
        }
        void LabelRow(int index,string street)
        {
            var row=roadRows[index];CacheStreetLabels(row);string label=street.ToUpperInvariant();
            foreach(var text in row.streetLabels)if(text!=null&&text.text!=label)text.text=label;
        }
        static void CacheStreetLabels(RoadRow row)
        {
            if(row.streetLabels!=null)return;
            var labels=new List<TextMesh>();
            foreach(var text in row.root.GetComponentsInChildren<TextMesh>(true))
                if(text.transform.parent!=null&&text.transform.parent.name.StartsWith("Bảng tên đường",StringComparison.Ordinal)&&text.name!="ĐƯỜNG")labels.Add(text);
            row.streetLabels=labels.ToArray();
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
                AssignRiverRow(row.tiles,1);
                LabelRow(index,junctionStreet);
            }
            CityJunctionPrepared();
            HideBuildingsOverRiver();
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
            ScrollTankFire(delta);
            ScrollDinosaurs(delta);
            foreach(var b in blocks){b.localPosition-=Vector3.forward*delta;if(b.localPosition.z<-160){b.localPosition+=Vector3.forward*80*blocks.Count;AssignRecycledRiverBlock(b);}}
            for(int i=0;i<routeTrail.Count;i++){var p=routeTrail[i];p.point-=Vector3.forward*delta;routeTrail[i]=p;}
            // Phố còn cuộn khi té/rơi sông (TickRiderCrash, SinkRider): ẩn lại nhà đường ngang đứng trên sông theo vị trí mới (21.52).
            HideBuildingsOverRiver();
        }
        void BeginOrContinueTurn(float delta)
        {
            if(!RoutesReady)return;
            // The wider car starts closer to the open junction centre so its
            // rear corner does not clip sidewalk planters on the inside lane.
            float entry=IsPlayerCar?9f:11.65f;
            if(!IsTurning&&QueuedTurn!=0&&NextJunctionDistance<=entry)
            {
                if(!CanTurnFromLane(QueuedTurn,CurrentLane)||Mathf.Abs(rider.localPosition.x-LaneX[CurrentLane])>.65f){QueuedTurn=0;return;}
                // The world has already scrolled this frame. Spend only the part of
                // its distance after the entry line on the curve, without a dead frame.
                float afterEntry=Mathf.Clamp(entry-NextJunctionDistance,0,delta);
                StartJunctionTurnAt(false,NextJunctionDistance+afterEntry);
                delta=afterEntry;
            }
            if(!IsTurning)return;
            turnTravel+=Mathf.Max(0,delta);
            float curvedDistance=Mathf.Min(turnLength,turnTravel);
            int i=1;while(i<turnArcLengths.Length-1&&turnArcLengths[i]<curvedDistance)i++;
            float fraction=Mathf.InverseLerp(turnArcLengths[i-1],turnArcLengths[i],curvedDistance);
            float t=(i-1+fraction)/(turnArcLengths.Length-1);
            Vector3 tangent=TurnTangent(t);
            TurnYaw=Mathf.Atan2(tangent.x,tangent.z)*Mathf.Rad2Deg;
            // Carry the last frame's unused distance straight onto the destination
            // road. Clamping speed here used to brake the bike just before rebasing.
            float exitDistance=Mathf.Max(0,turnTravel-turnLength);
            rider.localPosition=TurnPoint(t)+Vector3.right*turnSide*exitDistance-Vector3.forward*turnTravel;
            rider.localRotation=Quaternion.Euler(0,TurnYaw,-turnSide*(IsPlayerCar?5:lateTurn?16:18)*Mathf.Sin(t*Mathf.PI));
            lastX=rider.localPosition.x;
        }
        void StartJunctionTurn(bool late)
        {StartJunctionTurnAt(late,NextJunctionDistance);}
        void StartJunctionTurnAt(bool late,float junctionDistance)
        {
            // Thế giới sắp quay 90°: hủy đạn/cảnh báo đang bay (21.38).
            ClearShells();
            IsTurning=true;turnSide=QueuedTurn;QueuedTurn=0;turnStart=rider.localPosition;turnStart.y=0;
            turnRadius=Mathf.Max(4,junctionDistance-turnSide*LaneX[CurrentLane]);turnTravel=0;TurnYaw=0;lateTurn=late;
            if(late)
            {
                // A late input needs room to align with the same exit lane. A tangent-
                // continuous curve keeps the bike inside the union of the two roads.
                turnControl1=turnStart+Vector3.forward*(IsPlayerCar?.5f:3);
                turnEnd=new Vector3(turnSide*20,0,junctionDistance-turnSide*LaneX[CurrentLane]);
                turnControl2=turnEnd-Vector3.right*turnSide*6;
            }
            else
            {
                // A quintic curve has zero curvature at both straight-road joins.
                // Arc-length sampling keeps speed independent of its eased steering.
                float handle=turnRadius*.27f;Vector3 exit=Vector3.right*turnSide;
                turnEnd=turnStart+(Vector3.forward+exit)*turnRadius;
                turnControl1=turnStart+Vector3.forward*handle;
                turnControl2=turnStart+Vector3.forward*(2*handle);
                turnControl3=turnEnd-exit*(2*handle);
                turnControl4=turnEnd-exit*handle;
            }
            Vector3 previous=turnStart;turnArcLengths[0]=0;
            for(int j=1;j<turnArcLengths.Length;j++)
            {
                Vector3 p=TurnPoint(j/(float)(turnArcLengths.Length-1));
                turnArcLengths[j]=turnArcLengths[j-1]+Vector3.Distance(previous,p);previous=p;
            }
            turnLength=turnArcLengths[turnArcLengths.Length-1];
            if(IsChased&&!routePolice){routePolice=true;policeRouteAt=Distance-PoliceDistance;}
            PreviewJunctionNavigation();
        }
        Vector3 TurnPoint(float t)
        {
            float u=1-t,u2=u*u,t2=t*t;
            if(lateTurn)return u2*u*turnStart+3*u2*t*turnControl1+3*u*t2*turnControl2+t2*t*turnEnd;
            return u2*u2*u*turnStart+5*u2*u2*t*turnControl1+10*u2*u*t2*turnControl2+
                10*u2*t2*t*turnControl3+5*u*t2*t2*turnControl4+t2*t2*t*turnEnd;
        }
        Vector3 TurnTangent(float t)
        {
            float u=1-t,u2=u*u,t2=t*t;
            if(lateTurn)return 3*u2*(turnControl1-turnStart)+6*u*t*(turnControl2-turnControl1)+3*t2*(turnEnd-turnControl2);
            return 5*(u2*u2*(turnControl1-turnStart)+4*u2*u*t*(turnControl2-turnControl1)+
                6*u2*t2*(turnControl3-turnControl2)+4*u*t2*t*(turnControl4-turnControl3)+t2*t2*(turnEnd-turnControl4));
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
            rider.GetComponent<CarJumpJetVisual>()?.Clear();
            Quaternion exit=Quaternion.Euler(0,turnSide*90,0),inverse=Quaternion.Inverse(exit);
            Vector3 origin=rider.localPosition-kickAppliedOffset-exit*new Vector3(LaneX[CurrentLane],0,0);origin.y=0;
            RebaseAssistedKick(inverse,origin);
            RebaseDinosaurs(inverse,origin);
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
                if(v.riverFalling){v.riverVelocity=inverse*v.riverVelocity;v.riverSpin=inverse*v.riverSpin;v.riverRestRotation=inverse*v.riverRestRotation;}
                RebaseTrafficRecovery(v,inverse,origin);
                v.roadTurnStart=inverse*(v.roadTurnStart-origin);v.roadTurnForward=inverse*v.roadTurnForward;v.roadTurnSide=inverse*v.roadTurnSide;
                v.routeDetached=true;v.routeVelocity=v.motionVelocity;
                if(v.crashEffect!=null){v.crashEffect.localPosition=inverse*(v.crashEffect.localPosition-origin);v.crashEffect.localRotation=inverse*v.crashEffect.localRotation;v.routeEffectOffset=v.crashEffect.localPosition-v.model.localPosition;}
            }
            if(policeBike!=null){policeBike.localPosition=inverse*(policeBike.localPosition-origin);policeBike.localRotation=inverse*policeBike.localRotation;}
            if(PoliceInRiver){policeBody.previousPosition=inverse*(policeBody.previousPosition-origin);policeBody.riverVelocity=inverse*policeBody.riverVelocity;policeBody.riverSpin=inverse*policeBody.riverSpin;policeBody.riverRestRotation=inverse*policeBody.riverRestRotation;}
            policeFallYaw-=turnSide*90;
            rideCamera.transform.localPosition=inverse*(rideCamera.transform.localPosition-origin);rideCamera.transform.localRotation=inverse*rideCamera.transform.localRotation;
            for(int i=0;i<routeTrail.Count;i++){var p=routeTrail[i];p.point=inverse*(p.point-origin);p.yaw-=turnSide*90;routeTrail[i]=p;}
            int oldMain=mainRow,unused=turnSide<0?rightRow:leftRow;
            int selected=turnSide<0?leftRow:rightRow;mainRow=selected;var chosen=roadRows[mainRow];
            var positions=new Vector3[chosen.tiles.Count];for(int i=0;i<positions.Length;i++)positions[i]=chosen.root.localPosition+chosen.root.localRotation*chosen.tiles[i].localPosition;
            chosen.root.localPosition=Vector3.zero;chosen.root.localRotation=Quaternion.identity;
            for(int i=0;i<positions.Length;i++){chosen.tiles[i].localPosition=positions[i];chosen.tiles[i].localRotation=Quaternion.identity;}
            blocks=new List<Transform>(chosen.tiles);roadOffset=-Distance-positions[0].z;
            rider.localPosition=new Vector3(LaneX[CurrentLane],jumpHeight,0)+kickAppliedOffset;rider.localRotation=Quaternion.identity;lean=0;ClearLaneMotion();steerYaw=0;lastX=rider.localPosition.x;oldRiderZ=rider.localPosition.z;
            if(daylight!=null)daylight.Rebase(inverse,origin);
            TurnCityJunction(turnSide);RouteStreet=junctionStreet;visitedStreets.Add(RouteStreet);CompletedTurns++;IsTurning=false;TurnYaw=0;cameraTurnBlend=.7f;turnEndAt=Distance;
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
