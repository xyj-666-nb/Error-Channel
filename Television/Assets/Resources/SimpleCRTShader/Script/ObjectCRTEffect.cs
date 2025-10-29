using UnityEngine;

[ExecuteInEditMode]
public class ObjectCRTEffect : MonoBehaviour
{
    [Header("CRT材质")]
    public Material crtMaterial;

    [Header("效果设置")]
    public int whiteNoiseFrequency = 1;
    public float whiteNoiseLength = 0.1f;
    private float whiteNoiseTimeLeft;

    public int screenJumpFrequency = 1;
    public float screenJumpLength = 0.2f;
    public float screenJumpMinLevel = 0.1f;
    public float screenJumpMaxLevel = 0.9f;
    private float screenJumpTimeLeft;

    public float flickeringStrength = 0.002f;
    public float flickeringCycle = 111f;

    public bool isSlippage = true;
    public bool isSlippageNoise = true;
    public float slippageStrength = 0.005f;
    public float slippageInterval = 1f;
    public float slippageScrollSpeed = 33f;
    public float slippageSize = 11f;

    public float chromaticAberrationStrength = 0.005f;
    public bool isChromaticAberration = true;

    public bool isMultipleGhost = true;
    public float multipleGhostStrength = 0.01f;

    public bool isScanline = true;
    public bool isMonochrome = false;

    public bool isFilmDirt = false;
    public Texture2D filmDirtTex;

    public bool isDecalTex = false;
    public Texture2D decalTex;
    public Vector2 decalTexPos;
    public Vector2 decalTexScale;

    // 渲染纹理相关
    private RenderTexture renderTexture;
    private Camera renderCamera;
    private Renderer objectRenderer;
    private Material originalMaterial;
    private Material crtMaterialInstance;

    #region Shader属性ID
    private int _WhiteNoiseOnOff;
    private int _ScanlineOnOff;
    private int _MonochormeOnOff;
    private int _ScreenJumpLevel;
    private int _FlickeringStrength;
    private int _FlickeringCycle;
    private int _SlippageStrength;
    private int _SlippageSize;
    private int _SlippageInterval;
    private int _SlippageScrollSpeed;
    private int _SlippageNoiseOnOff;
    private int _SlippageOnOff;
    private int _ChromaticAberrationStrength;
    private int _ChromaticAberrationOnOff;
    private int _MultipleGhostOnOff;
    private int _MultipleGhostStrength;
    private int _DecalTex;
    private int _DecalTexOnOff;
    private int _DecalTexPos;
    private int _DecalTexScale;
    private int _FilmDirtOnOff;
    private int _FilmDirtTex;
    #endregion

    void Start()
    {
        InitializeShaderProperties();
        SetupCRTRenderSystem();
    }

    void InitializeShaderProperties()
    {
        _WhiteNoiseOnOff = Shader.PropertyToID("_WhiteNoiseOnOff");
        _ScanlineOnOff = Shader.PropertyToID("_ScanlineOnOff");
        _MonochormeOnOff = Shader.PropertyToID("_MonochormeOnOff");
        _ScreenJumpLevel = Shader.PropertyToID("_ScreenJumpLevel");
        _FlickeringStrength = Shader.PropertyToID("_FlickeringStrength");
        _FlickeringCycle = Shader.PropertyToID("_FlickeringCycle");
        _SlippageStrength = Shader.PropertyToID("_SlippageStrength");
        _SlippageSize = Shader.PropertyToID("_SlippageSize");
        _SlippageInterval = Shader.PropertyToID("_SlippageInterval");
        _SlippageScrollSpeed = Shader.PropertyToID("_SlippageScrollSpeed");
        _SlippageNoiseOnOff = Shader.PropertyToID("_SlippageNoiseOnOff");
        _SlippageOnOff = Shader.PropertyToID("_SlippageOnOff");
        _ChromaticAberrationStrength = Shader.PropertyToID("_ChromaticAberrationStrength");
        _ChromaticAberrationOnOff = Shader.PropertyToID("_ChromaticAberrationOnOff");
        _MultipleGhostOnOff = Shader.PropertyToID("_MultipleGhostOnOff");
        _MultipleGhostStrength = Shader.PropertyToID("_MultipleGhostStrength");
        _DecalTex = Shader.PropertyToID("_DecalTex");
        _DecalTexOnOff = Shader.PropertyToID("_DecalTexOnOff");
        _DecalTexPos = Shader.PropertyToID("_DecalTexPos");
        _DecalTexScale = Shader.PropertyToID("_DecalTexScale");
        _FilmDirtOnOff = Shader.PropertyToID("_FilmDirtOnOff");
        _FilmDirtTex = Shader.PropertyToID("_FilmDirtTex");
    }

    void SetupCRTRenderSystem()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError("ObjectCRTEffect需要Renderer组件！");
            return;
        }

        // 保存原始材质
        originalMaterial = objectRenderer.material;

        // 创建渲染纹理
        renderTexture = new RenderTexture(512, 512, 24);
        renderTexture.name = $"{name}_CRTTexture";

        // 创建CRT材质实例
        if (crtMaterial != null)
        {
            crtMaterialInstance = new Material(crtMaterial);
            crtMaterialInstance.mainTexture = renderTexture;
            objectRenderer.material = crtMaterialInstance;
        }

        // 创建渲染摄像机
        CreateRenderCamera();
    }

    void CreateRenderCamera()
    {
        GameObject cameraObj = new GameObject("CRTRenderCamera");
        cameraObj.transform.SetParent(transform);
        cameraObj.transform.localPosition = new Vector3(0, 0, -1);
        cameraObj.transform.localRotation = Quaternion.identity;

        renderCamera = cameraObj.AddComponent<Camera>();
        renderCamera.targetTexture = renderTexture;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = Color.black;
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = 1;
        renderCamera.nearClipPlane = 0.1f;
        renderCamera.farClipPlane = 10f;
        renderCamera.depth = -100; // 确保在其他摄像机之前渲染
    }

    void Update()
    {
        if (crtMaterialInstance == null) return;

        UpdateDynamicEffects();
        UpdateMaterialProperties();
    }

    void UpdateDynamicEffects()
    {
        // 白噪声效果
        whiteNoiseTimeLeft -= Time.deltaTime;
        if (whiteNoiseTimeLeft <= 0)
        {
            if (Random.Range(0, 1000) < whiteNoiseFrequency)
            {
                crtMaterialInstance.SetInteger(_WhiteNoiseOnOff, 1);
                whiteNoiseTimeLeft = whiteNoiseLength;
            }
            else
            {
                crtMaterialInstance.SetInteger(_WhiteNoiseOnOff, 0);
            }
        }

        // 屏幕跳动效果
        screenJumpTimeLeft -= Time.deltaTime;
        if (screenJumpTimeLeft <= 0)
        {
            if (Random.Range(0, 1000) < screenJumpFrequency)
            {
                var level = Random.Range(screenJumpMinLevel, screenJumpMaxLevel);
                crtMaterialInstance.SetFloat(_ScreenJumpLevel, level);
                screenJumpTimeLeft = screenJumpLength;
            }
            else
            {
                crtMaterialInstance.SetFloat(_ScreenJumpLevel, 0);
            }
        }
    }

    void UpdateMaterialProperties()
    {
        crtMaterialInstance.SetInteger(_ScanlineOnOff, isScanline ? 1 : 0);
        crtMaterialInstance.SetInteger(_MonochormeOnOff, isMonochrome ? 1 : 0);
        crtMaterialInstance.SetFloat(_FlickeringStrength, flickeringStrength);
        crtMaterialInstance.SetFloat(_FlickeringCycle, flickeringCycle);

        crtMaterialInstance.SetInteger(_SlippageOnOff, isSlippage ? 1 : 0);
        crtMaterialInstance.SetFloat(_SlippageNoiseOnOff, isSlippageNoise ? Random.Range(0f, 1f) : 1f);
        crtMaterialInstance.SetFloat(_SlippageStrength, slippageStrength);
        crtMaterialInstance.SetFloat(_SlippageSize, slippageSize);
        crtMaterialInstance.SetFloat(_SlippageInterval, slippageInterval);
        crtMaterialInstance.SetFloat(_SlippageScrollSpeed, slippageScrollSpeed);

        crtMaterialInstance.SetFloat(_ChromaticAberrationStrength, chromaticAberrationStrength);
        crtMaterialInstance.SetInteger(_ChromaticAberrationOnOff, isChromaticAberration ? 1 : 0);

        crtMaterialInstance.SetInteger(_MultipleGhostOnOff, isMultipleGhost ? 1 : 0);
        crtMaterialInstance.SetFloat(_MultipleGhostStrength, multipleGhostStrength);

        crtMaterialInstance.SetInteger(_FilmDirtOnOff, isFilmDirt ? 1 : 0);
        crtMaterialInstance.SetTexture(_FilmDirtTex, filmDirtTex);

        crtMaterialInstance.SetTexture(_DecalTex, decalTex);
        crtMaterialInstance.SetInteger(_DecalTexOnOff, isDecalTex ? 1 : 0);
        crtMaterialInstance.SetVector(_DecalTexPos, decalTexPos);
        crtMaterialInstance.SetVector(_DecalTexScale, decalTexScale);
    }

    // 公共控制方法
    public void EnableEffect(bool enable)
    {
        if (objectRenderer != null)
        {
            if (enable && crtMaterialInstance != null)
            {
                objectRenderer.material = crtMaterialInstance;
            }
            else if (originalMaterial != null)
            {
                objectRenderer.material = originalMaterial;
            }
        }
    }

    public void SetRenderTextureSize(int width, int height)
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture.width = width;
            renderTexture.height = height;
            renderTexture.Create();
        }
    }

    void OnDestroy()
    {
        // 恢复原始材质
        if (objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }

        // 清理资源
        if (renderTexture != null)
        {
            renderTexture.Release();
            if (Application.isPlaying)
                Destroy(renderTexture);
            else
                DestroyImmediate(renderTexture);
        }

        if (crtMaterialInstance != null)
        {
            if (Application.isPlaying)
                Destroy(crtMaterialInstance);
            else
                DestroyImmediate(crtMaterialInstance);
        }
    }

    // 在编辑器中可视化渲染摄像机
    void OnDrawGizmos()
    {
        if (renderCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(2, 2, 0.1f));
        }
    }
}