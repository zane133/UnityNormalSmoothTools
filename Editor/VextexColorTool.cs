using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VextexColorTool : Editor
{

    [MenuItem("Tools/SmoothNormalToColor")]
    static void SmoothNormalToColor()
    {
        string NewMeshPath = "Assets/Models/Yuanshen/"; ;

        var trans = Selection.activeTransform;
        NewMeshPath += trans.name + "_" + System.DateTime.Now.ToString("yyyyMMddhhmmss") + ".asset";
        // 获取Mesh
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


        // 计算进度
        int sum = 6 * (mesh.vertices.Length / 3) + mesh.normals.Length;
        int count = 0;


        // 声明一个Vector3数组，长度与mesh.normals一样，用于存放
        // 与mesh.vertices中顶点一一对应的光滑处理后的法线值
        Vector3[] smoothedNormals = new Vector3[mesh.normals.Length];

        // 空间换时间 (存下每个法线的相同的顶点索引)  其实就是真正的共享顶点 立方体的话因该是24 / 3=8个共享顶点。
        // 这里用到了hash表 (可以去领扣刷刷hash表相关的题，游戏还是会用到的)
        Dictionary<Vector3, List<int>> vertexDic = new Dictionary<Vector3, List<int>>();
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            float jd = count++ / (float)sum;
            EditorUtility.DisplayProgressBar("smooth", count.ToString(), jd);


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
        // 平均化每个顶点
        foreach (var item in vertexDic)
        {
            float jd = count++ / (float)sum;
            EditorUtility.DisplayProgressBar("smooth", count.ToString(), jd);

            Vector3 smoothedNormal = new Vector3(0, 0, 0);
            foreach (var index in item.Value)
            {
                smoothedNormal += mesh.normals[index];
            }
            smoothedNormal.Normalize();
            foreach (var index in item.Value)
            {
                smoothedNormals[index] = smoothedNormal;
            }
        }

        // 遍历三角形
        //Vector3[] verts = mesh.vertices;
        //// triangles存的是顶点vertices的索引 
        //// https://forum.unity.com/threads/how-do-i-get-the-normal-of-each-triangle-in-mesh.101018/
        //int[] triangles = mesh.triangles;

        //// 计算normal
        //int triangNum = mesh.triangles.Length / 3;
        //for (int j = 0; j < triangNum; j++)
        //{
        //    Vector3 PN1 = smoothedNormals[triangles[j * 3]];
        //    Vector3 PN2 = smoothedNormals[triangles[j * 3 + 1]];
        //    Vector3 PN3 = smoothedNormals[triangles[j * 3 + 2]];

        //    var smoothedNormal = PN1 + PN2 + PN3;
        //    smoothedNormal.Normalize();

        //    smoothedNormals[triangles[j * 3]] = smoothedNormal;
        //    smoothedNormals[triangles[j * 3 + 1]] = smoothedNormal;
        //    smoothedNormals[triangles[j * 3 + 2]] = smoothedNormal;
        //}

        // 网上找的方法

        //smoothedNormals = NormalSolver.RecalculateNormals(mesh);



        // 新建一个颜色数组把光滑处理后的法线值存入其中
        Color[] meshColors = new Color[smoothedNormals.Length];
        for (int i = 0; i < smoothedNormals.Length; i++)
        {
            float jd = count++ / (float)sum;
            EditorUtility.DisplayProgressBar("smooth", count.ToString(), jd);

            // 构建模型空间→切线空间的转换矩阵
            Vector3[] OtoTMatrix = new Vector3[3];
            OtoTMatrix[0] = new Vector3(mesh.tangents[i].x, mesh.tangents[i].y, mesh.tangents[i].z);
            OtoTMatrix[1] = Vector3.Cross(mesh.normals[i], OtoTMatrix[0]) * mesh.tangents[i].w;
            OtoTMatrix[2] = mesh.normals[i];

            // 将meshNormals数组中的法线值一一与矩阵相乘，求得切线空间下的法线值
            Vector3 tNormal;
            tNormal = Vector3.zero;
            tNormal.x = Vector3.Dot(OtoTMatrix[0], smoothedNormals[i]);
            tNormal.y = Vector3.Dot(OtoTMatrix[1], smoothedNormals[i]);
            tNormal.z = Vector3.Dot(OtoTMatrix[2], smoothedNormals[i]);
            smoothedNormals[i] = tNormal;

            meshColors[i].r = smoothedNormals[i].x * 0.5f + 0.5f;
            meshColors[i].g = smoothedNormals[i].y * 0.5f + 0.5f;
            meshColors[i].b = smoothedNormals[i].z * 0.5f + 0.5f;
            if (mesh.colors.Length != 0)
            {
                meshColors[i].a = mesh.colors[i].a;
            }
        }

        //新建一个mesh，将之前mesh的所有信息copy过去
        Mesh newMesh = Object.Instantiate(mesh) as Mesh;
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
        //newMesh.colors32 = mesh.colors32;
        newMesh.bounds = mesh.bounds;
        newMesh.indexFormat = mesh.indexFormat;
        newMesh.bindposes = mesh.bindposes;
        newMesh.boneWeights = mesh.boneWeights;
        //将新mesh保存为.asset文件，路径可以是"Assets/Character/Shader/VertexColorTest/TestMesh2.asset"                          
        AssetDatabase.CreateAsset(newMesh, NewMeshPath);
        AssetDatabase.SaveAssets();
        Debug.Log("Done");


        EditorUtility.ClearProgressBar();
    }
}


