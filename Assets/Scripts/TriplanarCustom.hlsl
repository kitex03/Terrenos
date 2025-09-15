#ifndef _TRIPLANA_CUSTOM_FUCNTIONS_HLSL_
#define _TRIPLANA_CUSTOM_FUCNTIONS_HLSL_

void TriplanarCustom_float(

    UnityTexture2D WallTex,
    UnityTexture2D GroundTex,
    float ScaleWall,
    float ScaleGround,
    float BlendFactor,
    float3 WorldPos,
    float3 WorldNormal,

    out float4 Out)
{
    
    float4 c =  tex2D(GroundTex, WorldPos.xz * ScaleGround);
    float4 c2 = tex2D(WallTex, WorldPos.zy * ScaleWall);
    float4 c3 = tex2D(WallTex, WorldPos.xy * ScaleWall);

    WorldNormal = abs(WorldNormal);
    WorldNormal = pow(WorldNormal,BlendFactor);
    float auxNormal = WorldNormal.x + WorldNormal.y + WorldNormal.z;
    WorldNormal = WorldNormal / auxNormal;
    Out =c * WorldNormal.y + c2 * WorldNormal.x + c3 * WorldNormal.z;
}

float Snowlevel(float3 WorldNormal, float snowAmount)
{
    float3 y = float3(0,1,0);
    float c = saturate(dot(WorldNormal , y));

    float aplicar = step(snowAmount,c);
    float cantidad_nieve = c * aplicar ;
    
    return cantidad_nieve;
}

void ApplySnowColor_float(
    float4 color,
    float snowBlend,
    float snowAmount,
    float snowminHeight,
    float3 WorldNormal,
    float3 WorldPos,
    
    out float4 Out)
{
    
    WorldNormal = abs(WorldNormal);
    WorldNormal = pow(WorldNormal,snowBlend);
    float auxNormal = WorldNormal.x + WorldNormal.y + WorldNormal.z;
    WorldNormal = WorldNormal / auxNormal;

    int aplicar = step(snowminHeight,WorldPos.y);
    
    float cantidad_nieve = Snowlevel( WorldNormal, snowAmount);
    float factor = pow(cantidad_nieve * aplicar, 0.5f);
    Out = lerp(color, float4(1, 1, 1, color.a), factor);

}

void CreateSnow_float(
    float3 WolrdNormal,
    float3 WorldPos,
    float snowAmount,
    float snowminHeight,
    float SnowDisplacement,
    out float3 Out)
{
    
    float cantidad_nieve = Snowlevel( WolrdNormal, snowAmount);
    Out = WorldPos;
    int aplicar = step(snowminHeight,WorldPos.y);
    float factor = pow(cantidad_nieve * aplicar, 0.2f); 
    Out += WolrdNormal * (factor * SnowDisplacement );
    
}

void ApplyWaterColor_float(
    float4 color,
    float minWaterHeight,
    float3 WorldPos,
    out float4 Out)
{
    int aplicar = step(WorldPos.y,minWaterHeight);
    Out =lerp(color,float4(0,0,1,1),aplicar) ;
}


void BounceEffect_float(float3 WorldPos,
    float3 WorldNormal,
    float3 BounceImpactPosition,
    float BounceFrecuence,
    float BounceAmplitud,
    float BounceAtenuation,
    float BounceDistance,
    float BounceTime,
    float BounceImpactTime,
    out float3 Out)
{
    float3 dir =WorldPos- BounceImpactPosition ;
    float distance = length(dir);
    float tiempo = saturate(1.0-(BounceImpactTime / BounceTime));
    float distancia = saturate(1.0 - (distance / BounceDistance));
    float phase = sin( BounceFrecuence * tiempo);
    float positiveSine = sin(phase +1) * 0.5;
    float atenuacion = saturate(pow(distancia,BounceAtenuation)*tiempo);

    float bounce = positiveSine * atenuacion * BounceAmplitud;
    int aplicar = step(distance,BounceDistance);
    Out = WorldPos + WorldNormal * bounce * aplicar;
}
#endif
 