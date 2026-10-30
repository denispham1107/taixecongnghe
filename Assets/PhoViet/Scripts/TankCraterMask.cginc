#ifndef PV_TANK_CRATER_MASK
#define PV_TANK_CRATER_MASK
int _PVCraterCount;
float4 _PVCraters[64]; // game-local X, Z, radius, outline seed
float _PVCraterYaw[64];
float4x4 _PVCraterWorldToGame;
void PVCutTankCrater(float3 world)
{
    if(_PVCraterCount==0)return;
    float3 p=mul(_PVCraterWorldToGame,float4(world,1)).xyz;
    if(p.y>.16||p.y<-.55)return;
    [loop] for(int i=0;i<_PVCraterCount;i++)
    {
        float4 c=_PVCraters[i];float2 d=p.xz-c.xy;
        if(abs(d.x)>c.z*1.07||abs(d.y)>c.z*1.07)continue;
        float a=atan2(d.y,d.x)-_PVCraterYaw[i];
        float edge=1+.035*sin(a*7+c.w)+.025*sin(a*13-c.w*3);
        clip(length(d)-c.z*edge);
    }
}
#endif
