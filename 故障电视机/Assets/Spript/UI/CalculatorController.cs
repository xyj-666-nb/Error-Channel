using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalculatorController : MonoBehaviour
{
    [Header("关联滑动窗口")]
    public NumberScroller[] numberScrollers;

    [Header("控制按钮")]
    public Button randomButton;

    [Header("动画参数")]
    public float accelerationSpeed = 100f; // 加速度
    public float maxScrollSpeed = 800f; // 最大滚动速度
    public float completeDisplayDuration = 3f; // Complete状态显示时间

    private bool isCalculating = false;
    private bool isInCompleteState = false;
    private float completeStateTimer = 0f;

    // 记录初始速度
    private float[] initialSpeeds;
    private Coroutine accelerationCoroutine;
    private AutoCalculatorpanel panel;

    void Start()
    {
        // 保存初始速度
        initialSpeeds = new float[numberScrollers.Length];
        for (int i = 0; i < numberScrollers.Length; i++)
        {
            if (numberScrollers[i] != null)
                initialSpeeds[i] = numberScrollers[i].scrollSpeed;
        }

        if (randomButton != null)
        {
            randomButton.onClick.AddListener(StartCalculation);
        }

        // 获取面板引用
        panel = UImanager.Instance.GetPanel<AutoCalculatorpanel>();

        UpdateButtonState();
    }

    /// <summary>
    /// 开始计算流程
    /// </summary>
    private void StartCalculation()
    {
        if (isCalculating) return;

        if (EnemyCard.CurrentEnemyCard == null)
        {
            UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "当前的敌人牌还未刷新！请稍后重试。");
            return;
        }

        isCalculating = true;
        isInCompleteState = false;
        UpdateButtonState();

        // 1. 重置滚动器状态
        ResetScrollers();

        // 2. 开始计算动画
        if (panel != null)
        {
            panel.StartCalculationAnimation();
        }

        // 3. 立即开始加速滚轮（在Ing状态期间持续加速）
        if (accelerationCoroutine != null)
            StopCoroutine(accelerationCoroutine);
        accelerationCoroutine = StartCoroutine(AccelerationRoutine());
    }

    /// <summary>
    /// 加速协程 - 在Ing状态期间持续加速
    /// </summary>
    private IEnumerator AccelerationRoutine()
    {
        Debug.Log("开始加速滚轮");

        // 在Ing状态期间持续加速
        while (panel != null && panel.IsCalculating())
        {
            // 加速所有滚动器
            foreach (var scroller in numberScrollers)
            {
                if (scroller != null && scroller.scrollSpeed < maxScrollSpeed)
                {
                    scroller.scrollSpeed += accelerationSpeed * Time.deltaTime;
                    scroller.scrollSpeed = Mathf.Min(scroller.scrollSpeed, maxScrollSpeed);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 当计算动画完成时调用（由AutoCalculatorpanel调用）
    /// </summary>
    public void OnCalculationAnimationComplete()
    {

        // 停止加速协程
        if (accelerationCoroutine != null)
        {
            StopCoroutine(accelerationCoroutine);
            accelerationCoroutine = null;
        }

        // 停止在目标数字
        StopAtTargetNumber();

        // 进入Complete状态计时
        isInCompleteState = true;
        completeStateTimer = 0f;

        // 开始Complete状态计时协程
        StartCoroutine(CompleteStateCountdown());
    }

    /// <summary>
    /// Complete状态倒计时
    /// </summary>
    private IEnumerator CompleteStateCountdown()
    {
        completeStateTimer = 0f;

        while (completeStateTimer < completeDisplayDuration)
        {
            completeStateTimer += Time.deltaTime;
            UpdateButtonText();
            yield return null;
        }

        // Complete状态结束，回到Idle
        ReturnToIdleState();
    }

    /// <summary>
    /// 返回到Idle状态
    /// </summary>
    private void ReturnToIdleState()
    {
        isCalculating = false;
        isInCompleteState = false;

        // 重置面板状态
        if (panel != null)
        {
            panel.ResetToIdle();
        }

        // 重置滚动器
        ResetScrollers();

        UpdateButtonState();
    }

    /// <summary>
    /// 重置所有滚动器
    /// </summary>
    private void ResetScrollers()
    {
        for (int i = 0; i < numberScrollers.Length; i++)
        {
            if (numberScrollers[i] != null)
            {
                numberScrollers[i].scrollSpeed = initialSpeeds[i];
                numberScrollers[i].RestartScrolling();
            }
        }
    }

    /// <summary>
    /// 停止在目标数字
    /// </summary>
    private void StopAtTargetNumber()
    {
        if (EnemyCard.CurrentEnemyCard == null)
        {
            // 显示默认数字
            for (int i = 0; i < Mathf.Min(numberScrollers.Length, 4); i++)
            {
                if (numberScrollers[i] != null)
                {
                    numberScrollers[i].StopAtNumber(0);
                }
            }
            return;
        }

        int targetNumber = EnemyCard.CurrentEnemyCard.Number;
        int[] digitWeights = { 1000, 100, 10, 1 };

        for (int i = 0; i < Mathf.Min(numberScrollers.Length, 4); i++)
        {
            if (numberScrollers[i] != null)
            {
                int digit = (targetNumber / digitWeights[i]) % 10;
                numberScrollers[i].StopAtNumber(digit);
            }
        }
    }

    void Update()
    {
        // 如果需要实时更新按钮文本，可以在这里处理
    }

    /// <summary>
    /// 更新按钮状态
    /// </summary>
    private void UpdateButtonState()
    {
        if (randomButton == null) return;

        randomButton.interactable = !isCalculating;
        UpdateButtonText();
    }

    /// <summary>
    /// 更新按钮文本
    /// </summary>
    private void UpdateButtonText()
    {
        var text = randomButton.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null) return;

        if (isCalculating && isInCompleteState)
        {
            int remainingTime = Mathf.CeilToInt(completeDisplayDuration - completeStateTimer);
            text.text = $"显示中({remainingTime}s)";
        }
        else if (isCalculating)
        {
            text.text = "计算中...";
        }
        else
        {
            text.text = "开始计算";
        }
    }
}