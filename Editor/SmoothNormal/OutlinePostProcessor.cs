using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Unity.Jobs;
using System.IO;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;


public class OutlinePostProcessor : AssetPostprocessor
{
    const string assetsScriptsNewOutlineDirSettingAsset = "Assets/Settings/OutlineDirSetting.asset";

    private Mesh m_BakedMesh = new Mesh();
    // 在模型导入前调用
    // void OnPreprocessModel()
    // {
    //     var exampleAsset =
    //         AssetDatabase.LoadAssetAtPath<OutlineDirSetting>
    //             (assetsScriptsNewOutlineDirSettingAsset);
    //
    //     bool IsProcess = false;
    //     IsProcess = Filter(exampleAsset, IsProcess);
    //
    //     if (!IsProcess)
    //         return;
    //     
    //     
    //     var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
    //     
    //     Debug.Log(go.name);
    //     
    //     
    //     
    //     // 更改导入设置，使用Unity自带算法平滑模型，会自动合并重合顶点
    //     ModelImporter model = assetImporter as ModelImporter;
    //     model.importNormals = ModelImporterNormals.Calculate;
    //     model.normalCalculationMode = ModelImporterNormalCalculationMode.AngleWeighted;
    //     model.normalSmoothingAngle = 0;
    //     // model.importAnimation = false;
    //     // model.materialImportMode = ModelImporterMaterialImportMode.None;
    // }
    
    void OnPostprocessModel(GameObject g)
    {
        var exampleAsset =
            AssetDatabase.LoadAssetAtPath<OutlineDirSetting>
                (assetsScriptsNewOutlineDirSettingAsset);

        bool IsProcess = false;
        IsProcess = Filter(exampleAsset, IsProcess);
        if(!IsProcess)
            return;

        Dictionary<string, Mesh> originalMesh = GetMesh(g), smoothedMesh = GetMesh(g);

        foreach (var item in originalMesh)
        {
            var mesh = item.Value;
            mesh.SetUVs(3, SmoothNormals(mesh));
        }
        
        // set layer
        foreach(Transform trans in g.GetComponentsInChildren<Transform>())
        {
            trans.gameObject.layer = LayerMask.NameToLayer("Outline");
        }

    }
    
    List<Vector3> SmoothNormals(Mesh mesh) {

        // Group vertices by location
        var groups = mesh.vertices
            .Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index))
            .GroupBy(pair => pair.Key);

        // Copy normals to a new list
        var smoothNormals = new List<Vector3>(mesh.normals);

        // Average normals for grouped vertices
        foreach (var group in groups) {
            
            // Skip single vertices
            // 不注掉的话 角色平滑后的法线再平滑会出问题 神奇的问题
            // 有大佬知道为啥请告诉我 ??? mail: wssgg13@qq.com
            // if (group.Count() == 1) {
            //     continue;
            // }

            // Calculate the average normal
            var smoothNormal = Vector3.zero;

            foreach (var pair in group) {
                smoothNormal += mesh.normals[pair.Value];
                // smoothNormal += mesh.normals[pair.Value];
            }

            smoothNormal.Normalize();

            // Assign smooth normal to each vertex
            foreach (var pair in group) {
                
                var normals = mesh.normals;
                var tangents = mesh.tangents;
                
                var binormal = (Vector3.Cross(normals[pair.Value], tangents[pair.Value]) * tangents[pair.Value].w).normalized;
                
                var tbn = new Matrix4x4(
                    tangents[pair.Value],
                    binormal,
                    normals[pair.Value],
                    Vector4.zero);
                tbn = tbn.transpose;
                
                smoothNormals[pair.Value] = tbn.MultiplyVector(smoothNormal).normalized;
            }
        }

        return smoothNormals;
    }
    

    private bool Filter(OutlineDirSetting exampleAsset, bool IsProcess)
    {
        foreach (var dirInfo in exampleAsset.DirInfos)
        {
            var path = AssetDatabase.GetAssetPath(dirInfo.Folder);
            // Debug.Log(Path.GetDirectoryName(assetPath) + " " +path.Replace("/","\\"));
            string a = Path.GetDirectoryName(assetPath);
            string b = path.Replace("/", "\\");
            if (a.Contains(b + "\\")||a == b)
            {
                Debug.Log("Smoothing Normal To UV3......: " + assetPath);
                IsProcess = true;
                break;
            }
        }

        return IsProcess;
    }

    Dictionary<string, Mesh> GetMesh(GameObject go)
    {
        Dictionary<string, Mesh> dic = new Dictionary<string, Mesh>();
        foreach (var item in go.GetComponentsInChildren<MeshFilter>())
            dic.Add(item.name, item.sharedMesh);
        if (dic.Count == 0)
            foreach (var item in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                dic.Add(item.name, item.sharedMesh);
        return dic;
    }

}