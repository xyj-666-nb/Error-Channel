using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AutoCalculatorpanel : BasePanel
{
    private RectTransform rectTransform;

    [Header("位置配置")]
    public Vector2 startAnchoredPos = new Vector2(0, 0);
    public Vector2 targetAnchoredPos = new Vector2(0, 100);

    [Header("动画参数")]
    public float moveDuration = 0.5f;

    private Coroutine moveCoroutine;
    public Animator MyAnimator;
    [SerializeField] CalculatorController MyController;

    // 动画状态枚举
    public enum AnimState
    {
        Idle,
        Calculating,
        Complete
    }

    private AnimState currentAnimState = AnimState.Idle;

    public override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = startAnchoredPos;
        MyAnimator = GetComponent<Animator>();

        // 初始化为Idle状态
        SetAnimationState(AnimState.Idle);
    }

    /// <summary>
    /// 动画事件调用的方法 - Ing动画播放完成时调用
    /// </summary>
    public void SetAnimatorComplete()
    {
        Debug.Log("Ing动画播放完成，切换到Complete状态");
        SetAnimationState(AnimState.Complete);

        // 通知CalculatorController动画完成，可以显示数字了
        if (MyController != null)
        {
            MyController.OnCalculationAnimationComplete();
        }
    }

    /// <summary>
    /// 设置动画状态（确保互斥）
    /// </summary>
    private void SetAnimationState(AnimState newState)
    {
        // 如果状态没有变化，直接返回
        if (currentAnimState == newState) return;

        currentAnimState = newState;

        // 重置所有布尔状态
        MyAnimator.SetBool("IsIng", false);
        MyAnimator.SetBool("IsComplete", false);
        MyAnimator.SetBool("IsIdle", false);

        // 根据新状态设置对应的布尔
        switch (newState)
        {
            case AnimState.Idle:
                MyAnimator.SetBool("IsIdle", true);
                break;
            case AnimState.Calculating:
                MyAnimator.SetBool("IsIng", true);
                break;
            case AnimState.Complete:
                MyAnimator.SetBool("IsComplete", true);
                break;
        }

        Debug.Log($"动画状态切换到: {newState}");
    }

    // 开始计算动画
    public void StartCalculationAnimation()
    {
        SetAnimationState(AnimState.Calculating);
    }

    // 重置到待机状态
    public void ResetToIdle()
    {
        SetAnimationState(AnimState.Idle);
    }

    // 获取当前动画状态
    public AnimState GetCurrentAnimState()
    {
        return currentAnimState;
    }

    // 检查是否正在播放计算动画
    public bool IsCalculating()
    {
        return currentAnimState == AnimState.Calculating;
    }

    public override void ShowMe(bool IsNeedDefalutAnimator = false)
    {
        base.ShowMe(IsNeedDefalutAnimator);

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveSmoothly(rectTransform.anchoredPosition, targetAnchoredPos));
    }

    public override void HideMe(UnityAction callback)
    {
        base.HideMe(callback);

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveToStartAndCallback(rectTransform.anchoredPosition, callback));
    }

    private IEnumerator MoveToStartAndCallback(Vector2 currentPos, UnityAction callback)
    {
        yield return StartCoroutine(MoveSmoothly(currentPos, startAnchoredPos));
        callback?.Invoke();
        moveCoroutine = null;
    }

    private IEnumerator MoveSmoothly(Vector2 start, Vector2 target)
    {
        float startTime = Time.time;

        while (true)
        {
            float t = (Time.time - startTime) / moveDuration;
            if (t >= 1) break;

            t = Mathf.SmoothStep(0, 1, t);
            rectTransform.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        rectTransform.anchoredPosition = target;
    }

    public override void ClickButton(string controlName)
    {
        base.ClickButton(controlName);
        if (controlName == "ExitButton")
        {
            MusicManager.Instance.PlayEffectMusic("Music/点击", false);
            UImanager.Instance.HidePanel<AutoCalculatorpanel>();
            UImanager.Instance.GetPanel<televisionPanel>().controlDic["CallAutoCalculatorButton"].gameObject.SetActive(true);
        }
    }
}