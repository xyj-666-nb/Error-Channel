using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ShowGold : MonoBehaviour
{
    // 单例模式
    private static UI_ShowGold instance;
    public static UI_ShowGold Instance => instance;

    private TextMeshProUGUI goldText;
    private int targetGold; // 目标金币数值
    private int currentGold; // 当前显示的金币数值

    [SerializeField] private Transform RecycleGoldPos; // 回收金币位置
    [SerializeField] private Transform UseGoldPos_AdvanceShop; // 使用金币位置

    [Header("提示文本")]
    [SerializeField] private TextMeshProUGUI PromptText; // 新增：提示文本组件

    [Header("动画参数")]
    [Tooltip("单个金币变化的时间（秒）")]
    public float timePerGold = 0.05f;
    [Tooltip("动画缓动曲线（控制过渡平滑度）")]
    public Ease easeType = Ease.OutQuad;
    [Tooltip("调用UpdateGold后延迟执行的时间（秒）")]
    public float delayTime = 0.5f; // 新增：延迟时间

    private Tween goldTween;
    private Coroutine delayGoldCoroutine; // 存储当前的延迟协程（用于取消）

    // 新增：提示文本相关的变量
    private Coroutine currentPromptCoroutine;
    private Tween currentPromptTween;

    private void Awake()
    {
        instance = this;
        goldText = GetComponent<TextMeshProUGUI>();
        currentGold = PlayerManager.instance.PlayerCurrentGold;
        targetGold = PlayerManager.instance.PlayerCurrentGold;

        if (PlayerManager.instance.IsObtainShowGoldSkill)
            goldText.text = $"：{currentGold}$";
        else
            goldText.text = "：##?$";

        // 新增：初始化提示文本
        if (PromptText != null)
        {
            PromptText.alpha = 0f; // 初始隐藏
        }
    }

    public void FixMe()
    {
        PlayerManager.instance.IsObtainShowGoldSkill = true;
        currentGold = PlayerManager.instance.PlayerCurrentGold;
        targetGold = PlayerManager.instance.PlayerCurrentGold;
        goldText.text = $"：{currentGold}$";
    }

    public void UpdateGold(int gold)
    {
        if (!PlayerManager.instance.IsObtainShowGoldSkill)
        {
            // 未获得技能：立即显示未知，终止所有动画和延迟
            goldText.text = "：##?";
            targetGold = currentGold;
            goldTween?.Kill();
            if (delayGoldCoroutine != null)
                StopCoroutine(delayGoldCoroutine); // 取消延迟协程
            return;
        }

        // 目标值未变化：无需执行
        if (gold == targetGold)
        {
            // 取消可能存在的延迟协程（如果有的话）
            if (delayGoldCoroutine != null)
                StopCoroutine(delayGoldCoroutine);
            return;
        }

        // 取消之前的延迟协程（确保以最新调用为准）
        if (delayGoldCoroutine != null)
            StopCoroutine(delayGoldCoroutine);

        // 启动新的延迟协程：等待0.5秒后执行更新逻辑
        targetGold = gold; // 先记录目标值（延迟期间可能被多次修改，以最后一次为准）
        delayGoldCoroutine = StartCoroutine(DelayUpdateGold());
    }

    // 延迟协程：等待delayTime后执行金币更新动画
    private IEnumerator DelayUpdateGold()
    {
        yield return new WaitForSeconds(delayTime); // 等待0.5秒

        // 等待结束后，执行动画逻辑
        int delta = targetGold - currentGold;
        float totalDuration = Mathf.Abs(delta) * timePerGold;

        // 终止当前可能运行的动画（避免冲突）
        goldTween?.Kill();

        // 启动金币滚动动画
        goldTween = DOTween.To(
                () => currentGold,
                value =>
                {
                    currentGold = value;
                    goldText.text = $"：{currentGold}";
                },
                targetGold,
                totalDuration
            )
            .SetEase(easeType)
            .OnComplete(() =>
            {
                currentGold = targetGold;
                goldText.text = $"：{currentGold}";
            });

        // 协程结束，清空引用
        delayGoldCoroutine = null;
    }

    public void RecycleGold(GameObject Gold, GameObject Prefabs)
    {
        Gold.transform.DOMove(RecycleGoldPos.position, Random.Range(0.3f, 0.6f))
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                PoolManage.Instance.PushObj(Prefabs, Gold);
            });
    }

    public void UseGoldInAdvanceShop(GameObject Gold, GameObject Prefabs)
    {
        Gold.transform.DOMove(UseGoldPos_AdvanceShop.position, 0.5f)
           .SetEase(Ease.InBack)
           .OnComplete(() =>
           {
               PoolManage.Instance.PushObj(Prefabs, Gold);
           });
    }

    // 新增：设置提示文本的方法
    public void SetPromptText(bool IsAdd, int Amount)
    {
        // 检查是否解锁提示文本显示
        if (!PlayerManager.instance.IsStartShowPrompttext_Gold)
            return;

        // 取消之前正在运行的提示动画
        if (currentPromptCoroutine != null)
        {
            StopCoroutine(currentPromptCoroutine);
            currentPromptCoroutine = null;
        }

        // 取消之前的Tween动画
        currentPromptTween?.Kill();

        // 设置文本内容和颜色
        if (IsAdd)
        {
            PromptText.color = Color.green;
            PromptText.text = "+" + Amount.ToString() + "$";
        }
        else
        {
            PromptText.color = Color.red;
            PromptText.text =  Amount.ToString() + "$";
        }

        // 开始新的提示动画
        currentPromptCoroutine = StartCoroutine(PromptAnimation());
    }

    // 新增：提示文本动画协程
    private IEnumerator PromptAnimation()
    {
        // 淡入显示
        currentPromptTween = PromptText.DOFade(1, 0.2f);
        yield return currentPromptTween.WaitForCompletion();

        // 等待1秒
        yield return new WaitForSeconds(1f);

        // 淡出隐藏
        currentPromptTween = PromptText.DOFade(0, 1f);
        yield return currentPromptTween.WaitForCompletion();

        // 动画完成，清空引用
        currentPromptCoroutine = null;
        currentPromptTween = null;
    }

    public void HidePromptImmediately()
    {
        if (currentPromptCoroutine != null)
        {
            StopCoroutine(currentPromptCoroutine);
            currentPromptCoroutine = null;
        }

        currentPromptTween?.Kill();
        if (PromptText != null)
        {
            PromptText.DOFade(0, 0.1f); // 快速隐藏
        }
    }

    // 新增：在回收金币时显示提示文本
    public void RecycleGoldWithPrompt(GameObject Gold, GameObject Prefabs, int goldAmount)
    {
        RecycleGold(Gold, Prefabs);
        SetPromptText(true, goldAmount);
    }

    // 新增：在使用金币时显示提示文本
    public void UseGoldWithPrompt(GameObject Gold, GameObject Prefabs, int goldAmount)
    {
        UseGoldInAdvanceShop(Gold, Prefabs);
        SetPromptText(false, goldAmount);
    }

}