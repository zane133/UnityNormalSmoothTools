# UnityNormalSmoothTools
A small tool for smoothing vertices and put smoothed normal in the vertex color

```
....
float3 vertNormal = v.vertexColor.rgb * 2 - 1;
float3 binormal = cross(v.normal, v.tangent) * v.tangent.w;
float3x3 TtoO = float3x3(
                v.tangent.xyz,
                binormal.xyz,
                v.normal.xyz);
TtoO = transpose(TtoO);
vertNormal = mul(TtoO, vertNormal);			
o.vertex = UnityObjectToClipPos(v.vertex + vertNormal *_OutlineWidth);
....
```
