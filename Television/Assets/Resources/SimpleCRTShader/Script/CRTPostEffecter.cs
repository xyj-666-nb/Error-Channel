using DG.Tweening;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CRTPostEffecter : MonoBehaviour
{
    private static CRTPostEffecter Instance;
    public static CRTPostEffecter instance => Instance;

    // 效果核心参数
    public Material material;
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

    public bool isLetterBox = false;
    public bool isLetterBoxEdgeBlur = false;
    public LeterBoxType letterBoxType;
    public enum LeterBoxType
    {
        Black,
        Blur
    }

    public bool isFilmDirt = false;
    public Texture2D filmDirtTex;

    public bool isDecalTex = false;
    public Texture2D decalTex;
    public Vector2 decalTexPos;
    public Vector2 decalTexScale;

    public bool isLowResolution = true;
    public Vector2Int resolutions;

    // UV范围参数（效果作用区域）
    public Rect effectRange = new Rect(0.2f, 0.2f, 0.6f, 0.6f);

    // 调试可视化参数
    [Header("调试设置")]
    public bool showDebugRange = true;
    public Color debugRangeColor = new Color(1, 0, 0, 0.3f);

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
    private int _LetterBoxOnOff;
    private int _LetterBoxType;
    private int _LetterBoxEdgeBlurOnOff;
    private int _DecalTex;
    private int _DecalTexOnOff;
    private int _DecalTexPos;
    private int _DecalTexScale;
    private int _FilmDirtOnOff;
    private int _FilmDirtTex;
    private int _EffectRange;
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 初始化Shader属性ID
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
        _LetterBoxOnOff = Shader.PropertyToID("_LetterBoxOnOff");
        _LetterBoxType = Shader.PropertyToID("_LetterBoxType");
        _LetterBoxEdgeBlurOnOff = Shader.PropertyToID("_LetterBoxEdgeBlur");
        _DecalTex = Shader.PropertyToID("_DecalTex");
        _DecalTexOnOff = Shader.PropertyToID("_DecalTexOnOff");
        _DecalTexPos = Shader.PropertyToID("_DecalTexPos");
        _DecalTexScale = Shader.PropertyToID("_DecalTexScale");
        _FilmDirtOnOff = Shader.PropertyToID("_FilmDirtOnOff");
        _FilmDirtTex = Shader.PropertyToID("_FilmDirtTex");
        _EffectRange = Shader.PropertyToID("_EffectRange");
    }

    // 修复效果：保留中等复古感
    public void FixCRTEffect_MediumVintage()
    {
        isSlippage = false;
        isFilmDirt = false;
        isMultipleGhost = false;
        isDecalTex = false;
        screenJumpFrequency = 0;
        screenJumpLength = 0.001f;

        isScanline = true;
        isChromaticAberration = true;
        chromaticAberrationStrength = 0.001f;

        whiteNoiseFrequency = 2;
        whiteNoiseLength = 0.1f;

        flickeringStrength = 0.0005f;
        flickeringCycle = 150f;

        isLowResolution = true;
        resolutions = new Vector2Int(960, 540);

        if (material != null)
        {
            material.SetInteger(_SlippageOnOff, 0);
            material.SetInteger(_FilmDirtOnOff, 0);
            material.SetInteger(_MultipleGhostOnOff, 0);
            material.SetFloat(_ScreenJumpLevel, 0);
            material.SetInteger(_ScanlineOnOff, 1);
            material.SetFloat(_ChromaticAberrationStrength, chromaticAberrationStrength);
            material.SetFloat(_FlickeringStrength, flickeringStrength);
            material.SetInteger(_WhiteNoiseOnOff, whiteNoiseFrequency > 0 ? 1 : 0);
        }

        Debug.Log("修复完成：保留中等强度复古感，明显的老式电视质感，无故障杂乱");
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material == null)
        {
            Graphics.Blit(src, dest);
            return;
        }

        // 传递UV范围给Shader
        material.SetVector(_EffectRange, new Vector4(
            effectRange.x,
            effectRange.y,
            effectRange.width,
            effectRange.height
        ));

        // 白噪音逻辑
        whiteNoiseTimeLeft -= 0.01f;
        if (whiteNoiseTimeLeft <= 0)
        {
            material.SetInteger(_WhiteNoiseOnOff, Random.Range(0, 1000) < whiteNoiseFrequency ? 1 : 0);
            whiteNoiseTimeLeft = whiteNoiseLength;
        }

        // letterBox设置
        material.SetInteger(_LetterBoxOnOff, isLetterBox ? 0 : 1);
        material.SetInteger(_LetterBoxEdgeBlurOnOff, isLetterBoxEdgeBlur ? 1 : 0);
        material.SetInteger(_LetterBoxType, (int)letterBoxType);

        // 基础效果参数
        material.SetInteger(_ScanlineOnOff, isScanline ? 1 : 0);
        material.SetInteger(_MonochormeOnOff, isMonochrome ? 1 : 0);
        material.SetFloat(_FlickeringStrength, flickeringStrength);
        material.SetFloat(_FlickeringCycle, flickeringCycle);
        material.SetFloat(_ChromaticAberrationStrength, chromaticAberrationStrength);
        material.SetInteger(_ChromaticAberrationOnOff, isChromaticAberration ? 1 : 0);
        material.SetInteger(_MultipleGhostOnOff, isMultipleGhost ? 1 : 0);
        material.SetFloat(_MultipleGhostStrength, multipleGhostStrength);
        material.SetInteger(_FilmDirtOnOff, isFilmDirt ? 1 : 0);
        material.SetTexture(_FilmDirtTex, filmDirtTex);

        // 滑动效果参数
        material.SetInteger(_SlippageOnOff, isSlippage ? 1 : 0);
        material.SetFloat(_SlippageInterval, slippageInterval);
        material.SetFloat(_SlippageNoiseOnOff, isSlippageNoise ? Random.Range(0, 1f) : 1);
        material.SetFloat(_SlippageScrollSpeed, slippageScrollSpeed);
        material.SetFloat(_SlippageStrength, slippageStrength);
        material.SetFloat(_SlippageSize, slippageSize);

        // 屏幕跳动逻辑
        screenJumpTimeLeft -= 0.01f;
        if (screenJumpTimeLeft <= 0)
        {
            float level = Random.Range(0, 1000) < screenJumpFrequency
                ? Random.Range(screenJumpMinLevel, screenJumpMaxLevel)
                : 0;
            material.SetFloat(_ScreenJumpLevel, level);
            screenJumpTimeLeft = screenJumpLength;
        }

        // 贴图参数
        material.SetTexture(_DecalTex, decalTex);
        material.SetInteger(_DecalTexOnOff, isDecalTex ? 1 : 0);
        material.SetVector(_DecalTexPos, decalTexPos);
        material.SetVector(_DecalTexScale, decalTexScale);

        // 低分辨率处理
        if (isLowResolution)
        {
            var target = RenderTexture.GetTemporary(src.width / 2, src.height / 2);
            Graphics.Blit(src, target);
            Graphics.Blit(target, dest, material);
            RenderTexture.ReleaseTemporary(target);
        }
        else
        {
            Graphics.Blit(src, dest, material);
        }

        // 绘制调试范围（编辑器模式）
        if (showDebugRange && Application.isEditor)
        {
            DrawDebugRange();
        }
    }

    // 绘制调试范围框
    private void DrawDebugRange()
    {
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // 转换UV范围到屏幕像素坐标
        float x = effectRange.x * screenWidth;
        float y = effectRange.y * screenHeight;
        float width = effectRange.width * screenWidth;
        float height = effectRange.height * screenHeight;

        // 绘制矩形（使用GL）
        GL.PushMatrix();
        GL.LoadOrtho(); // 正交投影（2D屏幕空间）
        GL.invertCulling = true;

        // 使用内置纯色Shader
        var debugMat = new Material(Shader.Find("Hidden/Internal-Colored"));
        debugMat.SetPass(0);

        // 填充半透明区域
        GL.Begin(GL.QUADS);
        GL.Color(debugRangeColor);
        GL.Vertex3(x / screenWidth, y / screenHeight, 0);
        GL.Vertex3((x + width) / screenWidth, y / screenHeight, 0);
        GL.Vertex3((x + width) / screenWidth, (y + height) / screenHeight, 0);
        GL.Vertex3(x / screenWidth, (y + height) / screenHeight, 0);
        GL.End();

        // 绘制边框（红色实线）
        GL.Begin(GL.LINES);
        GL.Color(new Color(1, 0, 0, 1));
        // 下边框
        GL.Vertex3(x / screenWidth, y / screenHeight, 0);
        GL.Vertex3((x + width) / screenWidth, y / screenHeight, 0);
        // 上边框
        GL.Vertex3(x / screenWidth, (y + height) / screenHeight, 0);
        GL.Vertex3((x + width) / screenWidth, (y + height) / screenHeight, 0);
        // 左边框
        GL.Vertex3(x / screenWidth, y / screenHeight, 0);
        GL.Vertex3(x / screenWidth, (y + height) / screenHeight, 0);
        // 右边框
        GL.Vertex3((x + width) / screenWidth, y / screenHeight, 0);
        GL.Vertex3((x + width) / screenWidth, (y + height) / screenHeight, 0);
        GL.End();

        GL.invertCulling = false;
        GL.PopMatrix();

        // 释放临时材质
        DestroyImmediate(debugMat);
    }
}