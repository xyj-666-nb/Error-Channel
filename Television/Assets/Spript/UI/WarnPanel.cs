using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class WarnPanel : BasePanel
{
    [SerializeField] private TextMeshProUGUI TopicText;//警告的主题
    [SerializeField] private TextMeshProUGUI ContentText;//警告的内容

    // Canvas 相关核心引用
    [SerializeField] private Canvas subCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    [Header("关键引用")]
    [SerializeField] private Camera mainCamera; // 手动拖入场景主摄像机

    //警告面板
    public override void Awake()
    {
        base.Awake();
        // 初始化主摄像机引用
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public override void Start()
    {
        base.Start();
        SetupSubCanvas();
    }

    /// <summary>
    /// 按截图数据配置Canvas和RectTransform
    /// </summary>
    private void SetupSubCanvas()
    {
        // 获取/创建 Canvas 组件
        if (subCanvas == null)
            subCanvas = gameObject.AddComponent<Canvas>();

        // 获取/创建 GraphicRaycaster（确保交互）
        if (graphicRaycaster == null)
            graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();

        // 渲染模式配置（与DialoguePanel一致）
        subCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        if (mainCamera != null)
            subCanvas.worldCamera = mainCamera;
        else
        {
            Debug.LogError("请为警告面板绑定主摄像机！");
            subCanvas.worldCamera = Camera.main;
        }
        subCanvas.planeDistance = 10f;

        // 层级配置
        subCanvas.overrideSorting = true;
        subCanvas.sortingOrder = 200;
        subCanvas.sortingLayerName = "FrontCard";

        // 核心：按截图中的RectTransform数据精确设置
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            // 锚点（截图显示为stretch模式）
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            // 偏移值（截图中Left=700, Top=260, Right=870, Bottom=515）
            rt.offsetMin = new Vector2(700, 515); // Left=700, Bottom=515
            rt.offsetMax = new Vector2(-870, -260); // Right=870 → -870，Top=260 → -260

            // 轴心点（截图中Pivot X=0.5, Y=0.5）
            rt.pivot = new Vector2(0.5f, 0.5f);

            // 旋转（截图中Z=0.56）
            rt.localEulerAngles = new Vector3(0, 0, 0.56f);

            // 缩放（截图中X=1, Y=1, Z=1）
            rt.localScale = Vector3.one;
        }
        else
        {
            Debug.LogError("警告面板缺少RectTransform组件！");
        }
    }

    public override void ClickButton(string controlName)
    {
        base.ClickButton(controlName);
        if (controlName == "CertainButton" || controlName == "ExitButton")
        {
            MusicManager.Instance.PlayEffectMusic("Music/点击", false);
            UImanager.Instance.HidePanel<WarnPanel>();//隐藏警告面板
        }
    }

    public void SetText(string Topic, string Content)
    {
        if (TopicText != null) TopicText.text = Topic;
        if (ContentText != null) ContentText.text = Content;
    }

    public override void HideMe(UnityAction callback)
    {
        // 恢复主面板按钮交互
        foreach (var UI in UImanager.Instance.GetPanel<televisionPanel>().controlDic.Values)
        {
            if (UI is Button button)
            {
                button.interactable = true;
            }
        }
        // 显示敌人卡牌
        if (EnemyCard.CurrentEnemyCard != null)
            EnemyCard.CurrentEnemyCard.SetHideOrShowCurrentCard(true);
        // 恢复时间流速
        Time.timeScale = 1f;
        base.HideMe(callback);
    }

    public override void ShowMe(bool IsNeedDefalutAnimator = true)
    {
        // 按截图数据重新校准
        SetupSubCanvas();
        if (subCanvas != null)
            subCanvas.sortingOrder = 200;

        // 原有显示逻辑
        MusicManager.Instance.PlayEffectMusic("Music/提示音", false);
        if (EnemyCard.CurrentEnemyCard != null)
            EnemyCard.CurrentEnemyCard.SetHideOrShowCurrentCard(false);
        Time.timeScale = 0f;
        foreach (var UI in UImanager.Instance.GetPanel<televisionPanel>().controlDic.Values)
        {
            if (UI is Button button)
            {
                button.interactable = false;
            }
        }

        transform.SetAsLastSibling();
        base.ShowMe(IsNeedDefalutAnimator);
    }

    protected override void Update()
    {
        base.Update();
        if (subCanvas != null && subCanvas.worldCamera == null)
        {
            subCanvas.worldCamera = Camera.main;
        }
    }

    private void OnDestroy()
    {
        if (TopicText != null) TopicText.DOKill();
        if (ContentText != null) ContentText.DOKill();
    }
}