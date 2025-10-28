using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialoguePanel : BasePanel
{
    private TextMeshProUGUI mainText;
    private TextMeshProUGUI textName;

    [Header("打字机效果设置")]
    public float textSpeed = 0.05f;

    // 状态变量
    private string fullText;
    private bool isTextAnimating = false;
    public UnityAction AnimaEndCallBack;

    private Coroutine textCoroutine;
    private Coroutine promptCoroutine;

    [Header("提示文本设置")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private float promptFadeDuration = 0.8f;
    [SerializeField] private float promptWaitDuration = 0.2f;
    private bool isPromptActive = false;

    // 添加子Canvas组件引用
    [SerializeField] private Canvas subCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("关键引用")]
    [SerializeField] private Camera mainCamera; // 手动拖入场景中的主摄像机（非static摄像机）


    public override void Start()
    {
        base.Start();
        mainText = controlDic["MainText"] as TextMeshProUGUI;
        textName = controlDic["Text_Name"] as TextMeshProUGUI;

        // 初始化为空
        if (mainText != null) mainText.text = "";
        if (textName != null) textName.text = "";

        // 初始化提示文本
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
            promptText.alpha = 0f;
        }

        // 初始化子Canvas
        SetupSubCanvas();
    }

    /// <summary>
    /// 设置子Canvas以确保面板显示在最前面
    /// </summary>
    private void SetupSubCanvas()
    {
        //断开父对象
        transform.parent = null;
        // 1. 获取或创建Canvas组件
        subCanvas = GetComponent<Canvas>();
        if (subCanvas == null)
        {
            subCanvas = gameObject.AddComponent<Canvas>();
        }

        // 2. 获取或创建GraphicRaycaster（确保交互有效）
        graphicRaycaster = GetComponent<GraphicRaycaster>();
        if (graphicRaycaster == null)
        {
            graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        }

        // 3. 关键设置：ScreenSpaceCamera模式，绑定主摄像机
        subCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        // 强制绑定到主摄像机（避免使用Camera.main，防止动态切换时出错）
        if (mainCamera != null)
        {
            subCanvas.worldCamera = mainCamera;
        }
        else
        {
            Debug.LogError("请为主对话面板绑定主摄像机！");
            subCanvas.worldCamera = Camera.main; // 降级方案（不推荐）
        }

        // 4. 确保对话面板在3D物体和其他UI前面
        subCanvas.planeDistance = 10f; // 距离主摄像机10单位（根据场景调整，确保在所有3D物体前）

        // 5. 层级设置：覆盖所有UI
        subCanvas.overrideSorting = true;
        subCanvas.sortingOrder = 200; // 高于其他所有UI的sortingOrder（主Canvas设为100以下）
        subCanvas.sortingLayerName = "FrontCard";

        // 6. 强制RectTransform全屏（关键！确保霸占整个屏幕）
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
        else
        {
            Debug.LogError("对话面板缺少RectTransform组件！");
        }
    }

    /// <summary>
    /// 设置提示文本显示状态
    /// </summary>
    public void SetPromptText(bool isShow)
    {
        // 停止之前的提示协程
        if (promptCoroutine != null)
        {
            StopCoroutine(promptCoroutine);
            promptCoroutine = null;
        }

        if (promptText == null) return;

        isPromptActive = isShow;

        if (isShow)
        {
            // 显示提示文本并开始持续闪烁
            promptText.gameObject.SetActive(true);
            promptCoroutine = StartCoroutine(ContinuousPromptAnimation());
        }
        else
        {
            // 停止闪烁并隐藏提示文本
            promptText.DOKill(); // 停止所有DoTween动画
            promptText.alpha = 0f;
            promptText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 持续闪烁动画 - 在透明和不透明之间无限循环
    /// </summary>
    IEnumerator ContinuousPromptAnimation()
    {
        while (isPromptActive)
        {
            // 从透明到不透明
            promptText.DOFade(1f, promptFadeDuration);
            yield return new WaitForSeconds(promptFadeDuration);

            // 短暂保持不透明
            yield return new WaitForSeconds(promptWaitDuration);

            // 从不透明到透明
            promptText.DOFade(0f, promptFadeDuration);
            yield return new WaitForSeconds(promptFadeDuration);

            // 短暂保持透明
            yield return new WaitForSeconds(promptWaitDuration);
        }

        // 循环结束后确保提示文本隐藏
        if (promptText != null)
        {
            promptText.DOKill();
            promptText.alpha = 0f;
            promptText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 设置对话内容
    /// </summary>
    public void SetDialogue(string name, string text, UnityAction callback = null)
    {
        // 如果正在播放打字效果，先停止
        if (isTextAnimating && textCoroutine != null)
        {
            StopCoroutine(textCoroutine);
            isTextAnimating = false;
        }

        // 开始新的对话时隐藏提示文本
        SetPromptText(false);

        fullText = text ?? "";

        // 设置说话者名字
        if (!string.IsNullOrEmpty(name) && textName != null)
        {
            textName.text = name;
        }

        // 开始文本动画
        textCoroutine = StartCoroutine(TypeText(callback));
    }

    /// <summary>
    /// 打字机效果协程
    /// </summary>
    private IEnumerator TypeText(UnityAction callback = null)
    {
        isTextAnimating = true;

        if (mainText != null)
        {
            mainText.text = "";
            yield return null; // 等待一帧确保UI更新
        }

        // 逐个字符显示
        for (int i = 0; i < fullText.Length; i++)
        {
            if (mainText != null)
                mainText.text += fullText[i];

            yield return new WaitForSeconds(textSpeed);
        }

        isTextAnimating = false;

        // 打字完成后显示提示文本并开始持续闪烁
        SetPromptText(true);

        callback?.Invoke();
    }

    /// <summary>
    /// 跳过当前打字效果，立即显示完整文本
    /// </summary>
    public void SkipTypingEffect()
    {
        if (isTextAnimating && textCoroutine != null)
        {
            StopCoroutine(textCoroutine);
            if (mainText != null)
                mainText.text = fullText;
            isTextAnimating = false;

            // 跳过打字后也显示提示文本并开始持续闪烁
            SetPromptText(true);
        }
    }

    /// <summary>
    /// 动画完成回调（由动画事件调用）
    /// </summary>
    public void SetAnimationFinish()
    {
        AnimaEndCallBack?.Invoke();
        GetComponent<Animator>().SetBool("IsStart", false);
    }

    /// <summary>
    /// 显示动画
    /// </summary>
    public void ShowAnimator()
    {
        GetComponent<Animator>().SetBool("IsStart", true);
    }

    public override void HideMe(UnityAction callback)
    {
        // 停止所有协程
        if (textCoroutine != null)
        {
            StopCoroutine(textCoroutine);
            textCoroutine = null;
        }

        if (promptCoroutine != null)
        {
            StopCoroutine(promptCoroutine);
            promptCoroutine = null;
        }

        //打开交互
        foreach (var Card in HandCardManger.Instance.HandCardList)
        {
            Card.GetComponent<Card>().IsCanInteractive = true;
        }

        // 停止闪烁并隐藏提示文本
        SetPromptText(false);

        isTextAnimating = false;
        base.HideMe(callback);
    }

    private void OnDestroy()
    {
        // 清理协程和DoTween动画
        if (textCoroutine != null)
        {
            StopCoroutine(textCoroutine);
            textCoroutine = null;
        }

        if (promptCoroutine != null)
        {
            StopCoroutine(promptCoroutine);
            promptCoroutine = null;
        }

        // 停止所有DoTween动画
        if (promptText != null)
        {
            promptText.DOKill();
        }
    }

    public override void ShowMe(bool IsNeedDefalutAnimator = true)
    {
        // 显示前重新校准Canvas（防止摄像机切换后错位）
        SetupSubCanvas();
        GetComponent<Animator>().SetBool("IsStart", true);
        // 确保在所有UI顶层
        if (subCanvas != null)
        {
            subCanvas.sortingOrder = 200; // 强制最高层级
        }

        //关闭卡牌的交互
        foreach(var Card in HandCardManger.Instance.HandCardList)
        {
            Card.GetComponent<Card>().IsCanInteractive = false;
        }

        // 其他原有逻辑
        transform.SetAsLastSibling();
        if (mainText == null || textName == null)
        {
            mainText = controlDic["MainText"] as TextMeshProUGUI;
            textName = controlDic["Text_Name"] as TextMeshProUGUI;
        }
        base.ShowMe(IsNeedDefalutAnimator);
    }

    protected override void Update()
    {
        base.Update();

        // 确保摄像机引用始终有效
        if (subCanvas != null && subCanvas.worldCamera == null)
        {
            subCanvas.worldCamera = Camera.main;
        }
    }

    public override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }
}