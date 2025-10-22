using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FixProcess : MonoBehaviour
{
    private static FixProcess instance;
    public static FixProcess Instance => instance;

    [SerializeField] private Slider MyFixProcess;
    [SerializeField] private TextMeshProUGUI ShowText;
    [SerializeField] private Image Fillimage;

    private float currentValue = 0f;
    private float targetValue = 0f;
    private bool isUpdating = false;
    private Coroutine updateCoroutine;

    private void Awake()
    {
        instance = this;
        currentValue = 0f;
        targetValue = 0f;
        MyFixProcess.value = 0f;
        ShowText.text = "修复进度：" + (int)(currentValue * 100) + "%";
    }

    public void UpdateFixProcess(float value)
    {
        // 颜色动画
        Fillimage.DOColor(Color.green, 0.5f).OnComplete(() =>
        {
            Fillimage.DOColor(Color.white, 0.5f);
        });

        // 更新目标值
        targetValue = Mathf.Clamp01(currentValue + value);

        // 如果已经在更新，停止之前的协程
        if (isUpdating && updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }

        // 开始新的更新协程
        updateCoroutine = StartCoroutine(UpdateProgress());
    }

    IEnumerator UpdateProgress()
    {
        isUpdating = true;

        // 计算需要增加的步数（每0.01为一步）
        int totalSteps = Mathf.RoundToInt((targetValue - currentValue) * 100);

        for (int step = 1; step <= totalSteps; step++)
        {
            // 更新当前值
            currentValue = currentValue + 0.01f;

            // 更新UI
            MyFixProcess.value = currentValue;
            ShowText.text = "修复进度：" + (int)(currentValue * 100) + "%";

            // 等待
            yield return new WaitForSeconds(0.02f);
        }

        // 确保最终值准确
        currentValue = targetValue;
        MyFixProcess.value = currentValue;
        ShowText.text = "修复进度：" + (int)(currentValue * 100) + "%";

        isUpdating = false;
        updateCoroutine = null;
    }

    // 重置进度
    public void ResetProgress()
    {
        currentValue = 0f;
        targetValue = 0f;
        MyFixProcess.value = 0f;
        ShowText.text = "修复进度：0%";

        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
        isUpdating = false;
    }
}