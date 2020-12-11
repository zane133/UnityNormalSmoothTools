
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NormalSmoothProcessor : Editor
{

    [MenuItem("Tools/ConvertNormalToColor")]
    public static void ConvertNormalToColor()
    {

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        var trans = Selection.activeTransform;

        string NewMeshPath = $"Assets/Models/xxx/{trans.name}.asset";

        // 获取Mesh
        //获取Mesh
        Mesh mesh = new Mesh();
        if (trans.GetComponent<SkinnedMeshRenderer>())
        {
            mesh = trans.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        }
        if (trans.GetComponent<MeshFilter>())
        {
            mesh = trans.GetComponent<MeshFilter>().sharedMesh;
        }
        Debug.Log(mesh.name);

        //声明一个Vector3数组，长度与mesh.normals一样，用于存放
        //与mesh.vertices中顶点一一对应的光滑处理后的法线值
        Vector3[] smoothedNormals = new Vector3[mesh.normals.Length];

        var count = smoothedNormals.Length * smoothedNormals.Length;
        var normalslen = smoothedNormals.Length;
        //var trangle = mesh.triangles;

        ////开始一个循环，循环的次数 = mesh.normals.Length = mesh.vertices.Length = meshNormals.Length
        //for (int i = 0; i < normalslen; i++)
        //{
        //    //定义一个零值法线
        //    Vector3 smoothedNormal = new Vector3(0, 0, 0);
        //    //遍历mesh.vertices数组，如果遍历到的值与当前序号顶点值相同，则将其对应的法线与Normal相加
        //    for (int j = 0; j < normalslen; j++)
        //    {
        //        int v = i * smoothedNormals.Length + j;
        //        EditorUtility.DisplayProgressBar("smooth", v.ToString(), (float)v / count);
        //        if (mesh.vertices[j] == mesh.vertices[i])
        //        {
        //            smoothedNormal += mesh.normals[j];
        //        }
        //    }
        //    //归一化Normal并将meshNormals数列对应位置赋值为Normal,到此序号为i的顶点的对应法线光滑处理完成
        //    //此时求得的法线为模型空间下的法线
        //    smoothedNormal.Normalize();
        //    smoothedNormals[i] = smoothedNormal;
        //}

        //EditorUtility.ClearProgressBar();

        // 空间换时间
        Dictionary<Vector3, List<int>> vertexDic = new Dictionary<Vector3, List<int>>();
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            if (!vertexDic.ContainsKey(mesh.vertices[i]))
            {
                List<int> vertexIndexs = new List<int>();
                vertexIndexs.Add(i);
                vertexDic.Add(mesh.vertices[i], vertexIndexs);
            }
            else
            {
                vertexDic[mesh.vertices[i]].Add(i);
            }
        }

        foreach (var item in vertexDic)
        {
            Vector3 smoothedNormal = new Vector3(0, 0, 0);
            foreach (var index in item.Value)
            {
                smoothedNormal += mesh.normals[index];
            }
            foreach (var index in item.Value)
            {
                smoothedNormals[index] = smoothedNormal;
            }
        }



        //构建模型空间→切线空间的转换矩阵
        List<Vector3[]> OtoTMatrixs = new List<Vector3[]>();
        //ArrayList OtoTMatrixs = new ArrayList();
        for (int i = 0; i < mesh.normals.Length; i++)
        {
            Vector3[] OtoTMatrix = new Vector3[3];
            OtoTMatrix[0] = new Vector3(mesh.tangents[i].x, mesh.tangents[i].y, mesh.tangents[i].z);
            OtoTMatrix[1] = Vector3.Cross(mesh.normals[i], OtoTMatrix[0]) * mesh.tangents[i].w;
            OtoTMatrix[2] = mesh.normals[i];
            OtoTMatrixs.Add(OtoTMatrix);
        }

        //将meshNormals数组中的法线值一一与矩阵相乘，求得切线空间下的法线值
        for (int i = 0; i < smoothedNormals.Length; i++)
        {
            Vector3 tNormal;
            tNormal = Vector3.zero;
            tNormal.x = Vector3.Dot(((Vector3[])OtoTMatrixs[i])[0], smoothedNormals[i]);
            tNormal.y = Vector3.Dot(((Vector3[])OtoTMatrixs[i])[1], smoothedNormals[i]);
            tNormal.z = Vector3.Dot(((Vector3[])OtoTMatrixs[i])[2], smoothedNormals[i]);
            smoothedNormals[i] = tNormal;
        }

        //新建一个颜色数组把光滑处理后的法线值存入其中
        Color[] meshColors = new Color[smoothedNormals.Length];
        for (int i = 0; i < meshColors.Length; i++)
        {
            meshColors[i].r = smoothedNormals[i].x * 0.5f + 0.5f;
            meshColors[i].g = smoothedNormals[i].y * 0.5f + 0.5f;
            meshColors[i].b = smoothedNormals[i].z * 0.5f + 0.5f;
            if (mesh.colors.Length != 0)
            {
                meshColors[i].a = mesh.colors[i].a;
            }
        }

        //新建一个mesh，将之前mesh的所有信息copy过去
        Mesh newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.normals = mesh.normals;
        newMesh.tangents = mesh.tangents;
        newMesh.uv = mesh.uv;
        newMesh.uv2 = mesh.uv2;
        newMesh.uv3 = mesh.uv3;
        newMesh.uv4 = mesh.uv4;
        newMesh.uv5 = mesh.uv5;
        newMesh.uv6 = mesh.uv6;
        newMesh.uv7 = mesh.uv7;
        newMesh.uv8 = mesh.uv8;
        //将新模型的颜色赋值为计算好的颜色
        newMesh.colors = meshColors;
        //newMesh.colors32 = new Color32(meshColors);
        newMesh.bounds = mesh.bounds;
        newMesh.indexFormat = mesh.indexFormat;
        newMesh.bindposes = mesh.bindposes;
        newMesh.boneWeights = mesh.boneWeights;
                        
        AssetDatabase.CreateAsset(newMesh, NewMeshPath);
        AssetDatabase.SaveAssets();
        Debug.Log("Done");

        sw.Stop();

        Debug.Log(sw.ElapsedMilliseconds);

    }
}


