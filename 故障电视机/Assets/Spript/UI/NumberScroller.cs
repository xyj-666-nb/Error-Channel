using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class NumberScroller : MonoBehaviour
{
    [Header("配置参数")]
    public GameObject numberPrefab; // 数字预制体（尺寸与窗口匹配）
    public float scrollSpeed = 300f; // 滚动速度（建议≥200，数值越大越快）
    public float stopSmoothTime = 0.3f; // 停止过渡时间

    [Header("调试信息（运行时查看）")]
    [SerializeField] private int currentDisplayNum; // 当前显示数字
    [SerializeField] private int numberCount; // 生成的数字总数
    [SerializeField] private float singleHeightDebug; // 单个数字高度

    private RectTransform contentRect;
    private RectTransform viewportRect;
    private List<RectTransform> numberRects = new List<RectTransform>();
    private float singleHeight;
    private bool isScrolling = true;
    private Coroutine scrollCoroutine;

    void Start()
    {
        contentRect = GetComponent<RectTransform>();
        viewportRect = GetComponentInParent<RectTransform>();

        // 获取预制体原始高度（不修改）
        singleHeight = numberPrefab.GetComponent<RectTransform>().sizeDelta.y;
        singleHeightDebug = singleHeight; // 调试用

        GenerateNumberSequence();
        scrollCoroutine = StartCoroutine(AutoScrollLoop());

        Debug.Log("===== NumberScroller 初始化 =====");
        Debug.Log($"单个数字高度: {singleHeight}");
        Debug.Log($"生成数字总数: {numberRects.Count}");
    }

    private void GenerateNumberSequence()
    {
        foreach (Transform child in contentRect) Destroy(child.gameObject);
        numberRects.Clear();

        // 生成 0-9（10个数字）
        for (int i = 0; i < 10; i++)
        {
            GameObject numObj = Instantiate(numberPrefab, contentRect);
            numObj.GetComponent<TextMeshProUGUI>().text = i.ToString();
            RectTransform numRect = numObj.GetComponent<RectTransform>();
            numRect.anchoredPosition = new Vector2(0, -i * singleHeight);
            numRect.sizeDelta = numberPrefab.GetComponent<RectTransform>().sizeDelta;
            numberRects.Add(numRect);
        }

        // 生成额外的0（第11个数字，实现无缝循环）
        GameObject extraZero = Instantiate(numberPrefab, contentRect);
        extraZero.GetComponent<TextMeshProUGUI>().text = "0";
        RectTransform extraRect = extraZero.GetComponent<RectTransform>();
        extraRect.anchoredPosition = new Vector2(0, -10 * singleHeight);
        extraRect.sizeDelta = numberPrefab.GetComponent<RectTransform>().sizeDelta;
        numberRects.Add(extraRect);

        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, singleHeight * 11);
        numberCount = numberRects.Count; // 记录总数（调试用）
    }

    private IEnumerator AutoScrollLoop()
    {
        while (isScrolling)
        {
            Vector2 currentPos = contentRect.anchoredPosition;
            currentPos.y += scrollSpeed * Time.deltaTime;
            contentRect.anchoredPosition = currentPos;

            // 修正：当滚动高度 ≥ 10个数字高度时，重置位置实现0-9循环
            if (currentPos.y >= singleHeight * 10)
            {
                currentPos.y -= singleHeight * 10;
                contentRect.anchoredPosition = currentPos;
                Debug.Log("滚动重置！当前位置重置为: " + currentPos.y);
            }

            // 计算当前显示的数字（调试用）
            currentDisplayNum = Mathf.FloorToInt(currentPos.y / singleHeight) % 10;
            Debug.Log($"当前滚动位置: {currentPos.y}, 显示数字: {currentDisplayNum}");

            yield return null;
        }
    }

    public void StopAtNumber(int targetNum)
    {
        if (targetNum < 0 || targetNum > 9) return;

        isScrolling = false;
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);

        // 修正：目标位置应该是targetNum * singleHeight
        float targetY = targetNum * singleHeight;

        // 确保目标位置在有效范围内（0 - 9*singleHeight）
        targetY = Mathf.Clamp(targetY, 0, singleHeight * 9);

        StartCoroutine(SmoothMoveToTarget(targetY));
        Debug.Log($"停止滚动，目标数字: {targetNum}, 目标位置: {targetY}");
    }

    private IEnumerator SmoothMoveToTarget(float targetY)
    {
        float startY = contentRect.anchoredPosition.y;
        float elapsedTime = 0;

        // 修正：处理循环情况，确保平滑过渡
        float distance = Mathf.Abs(targetY - startY);
        if (distance > singleHeight * 5) // 如果距离较远，选择更短的路径
        {
            if (startY < targetY)
                startY += singleHeight * 10;
            else
                targetY += singleHeight * 10;
        }

        while (elapsedTime < stopSmoothTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / stopSmoothTime);
            float currentY = Mathf.Lerp(startY, targetY, t);

            // 应用循环逻辑
            if (currentY >= singleHeight * 10)
                currentY -= singleHeight * 10;

            contentRect.anchoredPosition = new Vector2(0, currentY);
            yield return null;
        }

        float finalY = targetY;
        if (finalY >= singleHeight * 10)
            finalY -= singleHeight * 10;

        contentRect.anchoredPosition = new Vector2(0, finalY);
        currentDisplayNum = Mathf.FloorToInt(finalY / singleHeight) % 10;
        Debug.Log($"停止完成，最终显示数字: {currentDisplayNum}");
    }

    // 可选：添加重新开始滚动的方法
    public void RestartScrolling()
    {
        isScrolling = true;
        scrollCoroutine = StartCoroutine(AutoScrollLoop());
    }
}