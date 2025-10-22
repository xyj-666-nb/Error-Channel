using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class UI_ShowPart : MonoBehaviour
{
    private static UI_ShowPart instance;
    public static UI_ShowPart Instance => instance;

    [SerializeField] private TextMeshProUGUI ShowPartText;
    [SerializeField] private Transform RecyclePartPos;
    [SerializeField] private Transform StartPartPos;
    [SerializeField] private GameObject PartEffectPrefab;
    [SerializeField] private TextMeshProUGUI PromptText;

    // 新增：跟踪当前运行的协程和动画
    private Coroutine currentPromptCoroutine;
    private Tween currentPromptTween;

    private void Awake()
    {
        instance = this;
        PromptText.DOFade(0, 0.01f); // 立即隐藏提示文本
    }

    void Start()
    {
        ShowPartText.text = "##?P";
    }

    public void UpdatePartText()
    {
        if (PlayerManager.instance.IsObtainShowPart)
            ShowPartText.text = PlayerManager.instance.PlayerParts.ToString() + "P";
    }

    public void FixMe()
    {
        PlayerManager.instance.IsObtainShowPart = true;
        UI_ShowPart.Instance.UpdatePartText();
    }

    public void ConsumePartEffect(int consumePartAmount)
    {
        StartCoroutine(ConsumePart(consumePartAmount));
        SetPromptText(false, consumePartAmount);
    }

    IEnumerator ConsumePart(int consumePartAmount)
    {
        for (int i = 0; i < consumePartAmount; i++)
        {
            GameObject part = PoolManage.Instance.GetObj(PartEffectPrefab);
            part.transform.position = StartPartPos.position;
            part.transform.DOMove(RecyclePartPos.position, 0.5f).OnComplete(() =>
            {
                PoolManage.Instance.PushObj(PartEffectPrefab, part);
            });
            yield return new WaitForSeconds(0.05f);
            PlayerManager.instance.PlayerParts--;
            UpdatePartText();
        }
    }

    public void SetPromptText(bool IsAdd, int Amount)
    {
        // 检查是否解锁提示文本显示
        if (!PlayerManager.instance.IsStartShowPrompttext_part)
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
            PromptText.text = "+" + Amount.ToString();
        }
        else
        {
            PromptText.color = Color.red;
            PromptText.text = "-" + Amount.ToString();
        }

        // 开始新的提示动画
        currentPromptCoroutine = StartCoroutine(PromptAnimation());
    }

    IEnumerator PromptAnimation()
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

    // 新增：强制立即隐藏提示文本的方法
    public void HidePromptImmediately()
    {
        if (currentPromptCoroutine != null)
        {
            StopCoroutine(currentPromptCoroutine);
            currentPromptCoroutine = null;
        }

        currentPromptTween?.Kill();
        PromptText.DOFade(0, 0.1f); // 快速隐藏
    }

    // 新增：检查提示文本是否正在显示
    public bool IsPromptShowing()
    {
        return PromptText.alpha > 0.1f;
    }
}