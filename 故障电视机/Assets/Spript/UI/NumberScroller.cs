using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class NumberScroller : MonoBehaviour
{
    public GameObject numberPrefab; // 单个数字预制体（仅需1个）
    public float scrollSpeed = 100f; // 滚动速度
    public float stopDuration = 0.5f; // 停止时的缓动时间（让滚动慢慢停下）

    private RectTransform contentRect;
    private RectTransform viewportRect; // 滑动窗口的Viewport
    private List<RectTransform> numberRects = new List<RectTransform>(); // 存储0-9的RectTransform
    private float singleHeight; // 单个数字预制体的高度
    private bool isScrolling = true; // 是否正在滚动
    private Coroutine scrollCoroutine; // 滚动协程（用于中途停止）

    void Start()
    {
        contentRect = GetComponent<RectTransform>();
        viewportRect = GetComponentInParent<RectTransform>(); // 从父级获取Viewport

        // 动态生成0-9的数字预制体
        GenerateNumbers();

        // 启动自动滚动
        scrollCoroutine = StartCoroutine(AutoScroll());
    }

    // 动态生成0-9的数字预制体（仅用1个预制体实例化10次）
    void GenerateNumbers()
    {
        // 清除Content中已有的子物体（防止重复生成）
        foreach (Transform child in contentRect)
        {
            Destroy(child.gameObject);
        }
        numberRects.Clear();

        // 实例化0-9的数字
        for (int i = 0; i < 10; i++)
        {
            GameObject numObj = Instantiate(numberPrefab, contentRect);
            numObj.GetComponent<TextMeshProUGUI>().text = i.ToString(); // 设置数字文本
            RectTransform numRect = numObj.GetComponent<RectTransform>();
            numberRects.Add(numRect);
        }

        // 获取单个数字的高度（假设所有预制体大小一致）
        singleHeight = numberRects[0].sizeDelta.y;

        // 配置Content自适应高度（垂直排列+高度适配）
        if (!contentRect.GetComponent<VerticalLayoutGroup>())
        {
            VerticalLayoutGroup layout = contentRect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 0; // 数字间无间距
        }
        if (!contentRect.GetComponent<ContentSizeFitter>())
        {
            ContentSizeFitter fitter = contentRect.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; // 高度自适应子物体
        }
    }

    // 自动滚动协程（循环滚动效果）
    IEnumerator AutoScroll()
    {
        while (isScrolling)
        {
            // 持续向上滚动（修改Content的y坐标）
            Vector2 pos = contentRect.anchoredPosition;
            pos.y += scrollSpeed * Time.deltaTime;
            contentRect.anchoredPosition = pos;

            // 循环逻辑：当滚动超过10个数字高度时，重置位置（实现无限滚动）
            if (pos.y >= singleHeight * 10)
            {
                pos.y -= singleHeight * 10; // 减去10个数字的总高度，回到初始滚动状态
                contentRect.anchoredPosition = pos;
            }

            yield return null;
        }
    }

    // 外部调用：停止滚动并显示指定数字（num范围0-9）
    public void ShowTargetNumber(int num)
    {
        if (num < 0 || num > 9) return; // 校验数字合法性

        isScrolling = false; // 停止自动滚动
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);

        // 计算目标数字需要滚动到的位置（让目标数字居中显示在Viewport中）
        float targetY = CalculateTargetPosition(num);

        // 缓动到目标位置（让停止更平滑）
        StartCoroutine(MoveToTarget(targetY));
    }

    // 计算目标数字的最终位置
    private float CalculateTargetPosition(int targetNum)
    {
        // Viewport的高度（滑动窗口可视区域高度）
        float viewportHeight = viewportRect.sizeDelta.y;
        // 目标数字在Content中的本地y坐标（相对于Content的位置）
        float numLocalY = numberRects[targetNum].anchoredPosition.y;
        // 最终需要让目标数字居中，所以Content的y坐标需要偏移到：- (numLocalY - 可视区域一半 + 数字自身一半)
        return -(numLocalY - viewportHeight / 2 + singleHeight / 2);
    }

    // 缓动移动到目标位置
    IEnumerator MoveToTarget(float targetY)
    {
        float startY = contentRect.anchoredPosition.y;
        float elapsed = 0;

        while (elapsed < stopDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / stopDuration;
            t = Mathf.SmoothStep(0, 1, t); // 平滑插值

            contentRect.anchoredPosition = new Vector2(
                contentRect.anchoredPosition.x,
                Mathf.Lerp(startY, targetY, t)
            );
            yield return null;
        }

        // 最终精确对齐
        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, targetY);
    }
}