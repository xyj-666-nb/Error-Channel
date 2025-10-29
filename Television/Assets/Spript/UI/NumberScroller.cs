using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class NumberScroller : MonoBehaviour
{
    [Header("配置参数")]
    public GameObject numberPrefab;
    public float scrollSpeed = 300f;
    public float stopSmoothTime = 0.3f;

    [Header("调试信息")]
    [SerializeField] private int currentDisplayNum;
    [SerializeField] private int numberCount;
    [SerializeField] private float singleHeightDebug;

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
        singleHeight = numberPrefab.GetComponent<RectTransform>().sizeDelta.y;
        singleHeightDebug = singleHeight;

        GenerateNumberSequence();
        scrollCoroutine = StartCoroutine(AutoScrollLoop());
    }

    private void GenerateNumberSequence()
    {
        foreach (Transform child in contentRect) Destroy(child.gameObject);
        numberRects.Clear();

        for (int i = 0; i < 10; i++)
        {
            GameObject numObj = Instantiate(numberPrefab, contentRect);
            numObj.GetComponent<TextMeshProUGUI>().text = i.ToString();
            RectTransform numRect = numObj.GetComponent<RectTransform>();
            numRect.anchoredPosition = new Vector2(0, -i * singleHeight);
            numRect.sizeDelta = numberPrefab.GetComponent<RectTransform>().sizeDelta;
            numberRects.Add(numRect);
        }

        GameObject extraZero = Instantiate(numberPrefab, contentRect);
        extraZero.GetComponent<TextMeshProUGUI>().text = "0";
        RectTransform extraRect = extraZero.GetComponent<RectTransform>();
        extraRect.anchoredPosition = new Vector2(0, -10 * singleHeight);
        extraRect.sizeDelta = numberPrefab.GetComponent<RectTransform>().sizeDelta;
        numberRects.Add(extraRect);

        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, singleHeight * 11);
        numberCount = numberRects.Count;
    }

    private IEnumerator AutoScrollLoop()
    {
        while (isScrolling)
        {
            Vector2 currentPos = contentRect.anchoredPosition;
            currentPos.y += scrollSpeed * Time.deltaTime;
            contentRect.anchoredPosition = currentPos;

            if (currentPos.y >= singleHeight * 10)
            {
                currentPos.y -= singleHeight * 10;
                contentRect.anchoredPosition = currentPos;
            }

            currentDisplayNum = Mathf.FloorToInt(currentPos.y / singleHeight) % 10;
            yield return null;
        }
    }

    public void StopAtNumber(int targetNum)
    {
        if (targetNum < 0 || targetNum > 9) return;

        isScrolling = false;
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);

        float targetY = targetNum * singleHeight;
        targetY = Mathf.Clamp(targetY, 0, singleHeight * 9);

        StartCoroutine(SmoothMoveToTarget(targetY));
    }

    private IEnumerator SmoothMoveToTarget(float targetY)
    {
        float startY = contentRect.anchoredPosition.y;
        float elapsedTime = 0;

        float distance = Mathf.Abs(targetY - startY);
        if (distance > singleHeight * 5)
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
    }

    public void RestartScrolling()
    {
        scrollSpeed = 300; // 重置速度
        isScrolling = true;
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
        scrollCoroutine = StartCoroutine(AutoScrollLoop());
    }
}