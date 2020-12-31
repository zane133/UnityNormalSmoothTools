using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class CommandBufferCapture : MonoBehaviour
{
    private Camera m_Camera;
    private Mesh m_quad;

    public static RenderTexture CaptureRT;

    //TextureFlipper m_VFlipper = new TextureFlipper();

    private CommandBuffer m_CbCopyFB;
    private Material captureMaterial;
    private Shader shader;

    void Start()
    {
        // 延迟渲染
        var displayGO = new GameObject();
        displayGO.name = "CameraHostGO-" + displayGO.GetInstanceID();
        //displayGO.transform.parent = session.recorderGameObject.transform;
        var camera = displayGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Nothing;
        camera.cullingMask = 0;
        camera.renderingPath = RenderingPath.DeferredShading;
        camera.targetDisplay = 0;
        camera.rect = new Rect(0, 0, 1, 1);
        camera.depth = float.MaxValue;


        //m_Camera = GetComponent<Camera>();
        m_Camera = camera;
        m_quad = CreateFullscreenQuad();

        CaptureRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32)
        {
            wrapMode = TextureWrapMode.Repeat
        };
        CaptureRT.Create();

        shader = Shader.Find("Hidden/Recorder/Inputs/CBRenderTexture/CopyFB");
        captureMaterial = new Material(shader);
        //captureMaterial.EnableKeyword("OFFSCREEN");
        captureMaterial.EnableKeyword("TRANSPARENCY_ON");

        var tid = Shader.PropertyToID("_TmpFrameBuffer");
        m_CbCopyFB = new CommandBuffer { name = "Capture Screen ##############" };
        m_CbCopyFB.GetTemporaryRT(tid, -1, -1, 0, FilterMode.Bilinear);
        m_CbCopyFB.Blit(BuiltinRenderTextureType.CurrentActive, tid);
        m_CbCopyFB.SetRenderTarget(CaptureRT);
        m_CbCopyFB.DrawMesh(m_quad, Matrix4x4.identity, captureMaterial, 0, 0);
        m_CbCopyFB.ReleaseTemporaryRT(tid);
        m_Camera.AddCommandBuffer(CameraEvent.AfterEverything, m_CbCopyFB);
    }

    private Texture2D GetTex2D()
    {
        int width = CaptureRT.width;
        int height = CaptureRT.height;

        Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture.active = CaptureRT;
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();
        return texture2D;
    }

    private void CaptureToDesktop()
    {
        Texture2D texture2D = GetTex2D();
        SaveRenderTextureToPNG(
            texture2D,
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            DateTime.Now.ToString("yyyyMMddHHmmssffff_" + "_"));
    }

    void Update()
    {
        //m_VFlipper.Flip(CaptureRT);
        // 截图
        if (Input.GetKey(KeyCode.Space))
        {
            CaptureToDesktop();
            Debug.Log("截图");
        }
    }

    static Mesh CreateFullscreenQuad()
    {
        var vertices = new[]
        {
                new Vector3(1.0f, 1.0f, 0.0f),
                new Vector3(-1.0f, 1.0f, 0.0f),
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f),
            };
        var indices = new[] { 0, 1, 2, 2, 3, 0 };

        var r = new Mesh
        {
            vertices = vertices,
            triangles = indices
        };
        return r;
    }

    public static bool SaveRenderTextureToPNG(Texture2D spng, string contents, string pngName)
    {
        string r = contents + "/" + pngName + ".png";
        Debug.Log($"save {r}");
        Texture2D png = new Texture2D(spng.width, spng.height, TextureFormat.ARGB32, false);
        png.ReadPixels(new Rect(0, 0, spng.width, spng.height), 0, 0);
        byte[] bytes = spng.EncodeToPNG();
        if (!Directory.Exists(contents))
            Directory.CreateDirectory(contents);
        FileStream file = File.Open(r, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        Texture2D.DestroyImmediate(png);
        png = null;
        return true;

    }
}
