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

    [SerializeField] Transform RecycleGoldPos; // 回收金币位置

    [Header("动画参数")]
    [Tooltip("单个金币变化的时间（秒）")]
    public float timePerGold = 0.2f;
    [Tooltip("动画缓动曲线（控制过渡平滑度）")]
    public Ease easeType = Ease.OutQuad;
    [Tooltip("调用UpdateGold后延迟执行的时间（秒）")]
    public float delayTime = 0.5f; // 新增：延迟时间

    private Tween goldTween;
    private Coroutine delayGoldCoroutine; // 存储当前的延迟协程（用于取消）

    private void Awake()
    {
        instance = this;
        goldText = GetComponent<TextMeshProUGUI>();
        currentGold = 0;
        targetGold = 0;
    }

    public void UpdateGold(int gold)
    {
        if (!PlayerManager.instance.IsObtainShowGoldSkill)
        {
            // 未获得技能：立即显示未知，终止所有动画和延迟
            goldText.text = "金币：##?";
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
                    goldText.text = $"金币：{currentGold}";
                },
                targetGold,
                totalDuration
            )
            .SetEase(easeType)
            .OnComplete(() =>
            {
                currentGold = targetGold;
                goldText.text = $"金币：{currentGold}";
            });

        // 协程结束，清空引用
        delayGoldCoroutine = null;
    }

    public void RecycleGold(GameObject Gold, GameObject Prefabs)
    {
        Gold.transform.DOMove(RecycleGoldPos.position, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                PoolManage.Instance.PushObj(Prefabs, Gold);
            });
    }
}