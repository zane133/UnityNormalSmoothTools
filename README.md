# UnityNormalSmoothTools
A small tool for smoothing vertices and put smoothed normal in the vertex color

![20201105225250751.png](https://i.loli.net/2020/12/11/NmUjz8ClnD4dWMq.png)

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
// 这里最好单位化一下，不然绑定骨骼的时候会出现奇怪的法线偏移
vertNormal = normalize(vertNormal);
o.vertex = UnityObjectToClipPos(v.vertex + vertNormal *_OutlineWidth);
....
```
