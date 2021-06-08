using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class OutlineDirInfo
{
    public DefaultAsset Folder;
}

[CreateAssetMenu]
public class OutlineDirSetting : ScriptableObject
{
    public List<OutlineDirInfo> DirInfos;
    
    // private string age = string.Empty;
    // private string name = string.Empty;

    // public void Print()
    // {
    //     for (int i = 0, iMax = dirInfos.Count; i < iMax; i++)
    //     {
    //         Debug.Log("Name:" + dirInfos[i].name + "Age:" + dirInfos[i].age);
    //     }
    // }
    //
    public void AddItem()
    {
        DirInfos.Add(new OutlineDirInfo {Folder = null});
    }
    
    public void RemoveItem()
    {
        DirInfos.Remove(DirInfos[DirInfos.Count - 1]);
    }

}

[CustomEditor(typeof(OutlineDirSetting))]
public class OutlineDirSettingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var script = (OutlineDirSetting)target;
        
        GUILayout.Space(10);
        if (GUILayout.Button("添加新目录"))
        {
            script.AddItem();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("删除最后一个目录"))
        {
            script.RemoveItem();
        }

        // if (GUILayout.Button("保存配置"))
        // {
        //     // EditorUtility.SetDirty(target);
        //     // AssetDatabase.SaveAssets();
        //     // AssetImportSetting.ReloadInstance();
        // }
    }
}
