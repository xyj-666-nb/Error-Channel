using DG.Tweening;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CRTPostEffecter : MonoBehaviour
{
    private static CRTPostEffecter Instance;
    public static CRTPostEffecter instance => Instance;

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

    // 渲染范围（UV坐标，x=左边界, y=下边界, width=宽度, height=高度，取值0-1）
    public Rect effectRange = new Rect(0.2f, 0.2f, 0.6f, 0.6f);

    #region Properties in shader
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

    // ====================================== 新增：修复效果函数 ======================================
    /// <summary>
    /// 调用后减弱CRT效果（调整指定参数至低强度状态）
    /// </summary>
    /// <summary>
    /// 直接关闭大部分效果，仅保留极淡基础效果（立刻见效）
    /// </summary>
    /// <summary>
    /// 修复后效果：保留老式电视机复古感（淡扫描线/弱色差），去除故障抖动/杂乱
    /// </summary>
    /// <summary>
    /// 修复后效果：保留明显的老式电视复古感（中等强度），无故障杂乱
    /// </summary>
    public void FixCRTEffect_MediumVintage()
    {
        // ====================================== 1. 坚决关闭“故障类效果”（不允许杂乱回归）
        isSlippage = false;          // 关闭画面滑动/偏移（故障核心，必须关）
        isFilmDirt = false;          // 关闭胶片污渍（避免脏污感）
        isMultipleGhost = false;     // 关闭多重鬼影（防止画面叠影混乱）
        isDecalTex = false;          // 关闭额外贴图干扰
        screenJumpFrequency = 0;     // 彻底关闭屏幕跳动
        screenJumpLength = 0.001f;   // 防误触发


        // ====================================== 2. 增强“复古核心效果”（比之前稍重，保持自然）
        // ① 扫描线（老式电视最核心标志，调至中等密度，清晰可见但不刺眼）
        isScanline = true;
        // （若Shader支持扫描线强度，可在Shader中调大“线密度”，比如从10→20，让线更明显）

        // ② 色差（边缘偏色更明显，增强复古感，但不杂乱）
        isChromaticAberration = true;
        chromaticAberrationStrength = 0.001f;  // 比之前的0.0003f稍强（原故障值0.005f）

        // ③ 底噪（轻微白噪音，模拟老式电视的“沙沙”底噪感，偶尔出现）
        whiteNoiseFrequency = 2;     // 低频率（1000帧里出现2次）
        whiteNoiseLength = 0.1f;     // 每次持续0.1秒（极短，不干扰观看）

        // ④ 闪烁（轻微电流波动，比之前稍明显，但稳定）
        flickeringStrength = 0.0005f;  // 比之前的0.0001f稍强（原故障值0.002f）
        flickeringCycle = 150f;        // 周期适中，避免高频闪烁


        // ====================================== 3. 可选：轻微保留低分辨率（增强复古颗粒感）
        isLowResolution = true;
        resolutions = new Vector2Int(960, 540);  // 半高清（比原故障的更低分辨率清晰，保留颗粒感）


        // ====================================== 4. 同步Shader参数，确保效果生效
        if (material != null)
        {
            // 故障类效果：强制关闭
            material.SetInteger(_SlippageOnOff, 0);
            material.SetInteger(_FilmDirtOnOff, 0);
            material.SetInteger(_MultipleGhostOnOff, 0);
            material.SetFloat(_ScreenJumpLevel, 0);

            // 复古类效果：同步增强后的参数
            material.SetInteger(_ScanlineOnOff, 1);
            material.SetFloat(_ChromaticAberrationStrength, chromaticAberrationStrength);
            material.SetFloat(_FlickeringStrength, flickeringStrength);
            material.SetInteger(_WhiteNoiseOnOff, whiteNoiseFrequency > 0 ? 1 : 0);
        }

        Debug.Log("修复完成：保留中等强度复古感，明显的老式电视质感，无故障杂乱");
    }
    // ==============================================================================================

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material == null)  // 防止material未赋值导致报错
        {
            Graphics.Blit(src, dest);
            return;
        }

        material.SetVector(_EffectRange, new Vector4(
            effectRange.x,
            effectRange.y,
            effectRange.width,
            effectRange.height
        ));

        ///////White noise
        whiteNoiseTimeLeft -= 0.01f;
        if (whiteNoiseTimeLeft <= 0)
        {
            if (Random.Range(0, 1000) < whiteNoiseFrequency)
            {
                material.SetInteger(_WhiteNoiseOnOff, 1);
                whiteNoiseTimeLeft = whiteNoiseLength;
            }
            else
            {
                material.SetInteger(_WhiteNoiseOnOff, 0);
            }
        }
        //////

        material.SetInteger(_LetterBoxOnOff, isLetterBox ? 0 : 1);
        material.SetInteger(_LetterBoxEdgeBlurOnOff, isLetterBoxEdgeBlur ? 1 : 0);
        material.SetInteger(_LetterBoxType, (int)letterBoxType);

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

        //////Slippage
        material.SetInteger(_SlippageOnOff, isSlippage ? 1 : 0);
        material.SetFloat(_SlippageInterval, slippageInterval);
        material.SetFloat(_SlippageNoiseOnOff, isSlippageNoise ? Random.Range(0, 1f) : 1);
        material.SetFloat(_SlippageScrollSpeed, slippageScrollSpeed);
        material.SetFloat(_SlippageStrength, slippageStrength);
        material.SetFloat(_SlippageSize, slippageSize);
        //////

        //////Screen Jump Noise
        screenJumpTimeLeft -= 0.01f;
        if (screenJumpTimeLeft <= 0)
        {
            if (Random.Range(0, 1000) < screenJumpFrequency)
            {
                var level = Random.Range(screenJumpMinLevel, screenJumpMaxLevel);
                material.SetFloat(_ScreenJumpLevel, level);
                screenJumpTimeLeft = screenJumpLength;
            }
            else
            {
                material.SetFloat(_ScreenJumpLevel, 0);
            }
        }
        //////

        //////Decal Texture
        material.SetTexture(_DecalTex, decalTex);
        material.SetInteger(_DecalTexOnOff, isDecalTex ? 1 : 0);
        material.SetVector(_DecalTexPos, decalTexPos);
        material.SetVector(_DecalTexScale, decalTexScale);
        //////

        //////Low resolution
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
        //////
    }
}