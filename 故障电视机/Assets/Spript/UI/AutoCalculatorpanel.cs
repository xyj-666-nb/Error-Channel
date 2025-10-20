using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AutoCalculatorpanel : BasePanel
{
    private RectTransform rectTransform;

    [Header("位置配置（关键！）")]
    [Tooltip("面板隐藏时的起始锚点位置（初始位置）")]
    public Vector2 startAnchoredPos = new Vector2(0, 0);
    [Tooltip("面板显示时的目标锚点位置")]
    public Vector2 targetAnchoredPos = new Vector2(0, 100);

    [Header("动画参数")]
    public float moveDuration = 0.5f; // 动画时长

    private Coroutine moveCoroutine;

    public override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = startAnchoredPos; // 初始化到起始位置
    }

    public override void ShowMe(bool IsNeedDefalutAnimator = false)
    {
        base.ShowMe(IsNeedDefalutAnimator);

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        Vector2 currentStartPos = rectTransform.anchoredPosition;

        moveCoroutine = StartCoroutine(MoveSmoothly(currentStartPos, targetAnchoredPos));
    }

    // 核心修改：HideMe时移动回起始位置
    public override void HideMe(UnityAction callback)
    {
        base.HideMe(callback);

        // 停止当前可能运行的动画
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        // 获取当前位置（作为隐藏动画的起始点）
        Vector2 currentPos = rectTransform.anchoredPosition;

        // 启动“移动回起始位置”的协程，动画结束后执行回调
        moveCoroutine = StartCoroutine(MoveToStartAndCallback(currentPos, callback));
    }

    /// <summary>
    /// 移动回起始位置并执行回调
    /// </summary>
    private IEnumerator MoveToStartAndCallback(Vector2 currentPos, UnityAction callback)
    {
        // 调用通用移动方法，从当前位置→起始位置
        yield return StartCoroutine(MoveSmoothly(currentPos, startAnchoredPos));

        // 动画结束后执行回调（比如真正隐藏面板）
        if (callback != null)
            callback.Invoke();

        moveCoroutine = null;
    }

    /// <summary>
    /// 通用平滑移动协程（可复用）
    /// </summary>
    private IEnumerator MoveSmoothly(Vector2 start, Vector2 target)
    {
        float startTime = Time.time;

        while (true)
        {
            float t = (Time.time - startTime) / moveDuration;
            if (t >= 1)
            {
                t = 1;
                break;
            }

            t = Mathf.SmoothStep(0, 1, t);
            rectTransform.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        rectTransform.anchoredPosition = target; // 强制对齐目标位置
    }

    public override void ClickButton(string controlName)
    {
        base.ClickButton(controlName);
        if (controlName == "ExitButton")
        {
            UImanager.Instance.HidePanel<AutoCalculatorpanel>();
            UImanager.Instance.GetPanel<televisionPanel>().controlDic["CallAutoCalculatorButton"].gameObject.SetActive(true);
        }
    }

    public override void Start() => base.Start();
    protected override void Update() => base.Update();
}