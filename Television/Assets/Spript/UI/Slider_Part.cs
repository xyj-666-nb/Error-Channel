using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Slider_Part : MonoBehaviour
{
    private static Slider_Part Instance;
    public static Slider_Part instance=>Instance;

    public Slider partSlider; // 零件获取的进度条
    private float TargetValue; // 目标值
    private float lastValue; // 记录上一次的值
    [SerializeField] private Image FillIamge; // 填充条的颜色图片
    private Color OriginalColor;
    private bool IsCanGetPart;
    private bool isRestoringColor; 

    [SerializeField] GameObject PartPrefab; // 零件的预制体
    [SerializeField] Transform StartPartPos; // 零件开始产出的位置
    [SerializeField] Transform EnterPlayerAccountPos; // 进入玩家账户的位置

    private void Awake()
    {
        Instance= this;
        OriginalColor = FillIamge.color;
        IsCanGetPart = false;
        lastValue = partSlider.value;
        isRestoringColor = false;

        // 新增：初始化DOTween最大容量
        DOTween.SetTweensCapacity(300, 50);
    }

    public void UpdateGetProcess(float Value)
    {
        float oldTarget = TargetValue;
        TargetValue = Value;

        // 关键：创建新颜色动画前，先销毁当前Image上已有的颜色动画
        FillIamge.DOKill(true); // true表示允许杀死正在运行的动画

        // 判断增减并设置颜色
        if (Value > lastValue)
        {
            FillIamge.DOColor(Color.green, 0.5f);
            isRestoringColor = false; // 取消恢复标记
        }
        else if (Value < lastValue)
        {
            FillIamge.DOColor(Color.red, 0.5f);
            isRestoringColor = false; // 取消恢复标记
        }

        lastValue = Value;
        IsCanGetPart = (TargetValue >= 1);
    }

    private void Update()
    {
        // 用阈值判断进度条是否接近目标值
        if (Mathf.Abs(partSlider.value - TargetValue) > 0.001f)
        {
            partSlider.value = math.lerp(partSlider.value, TargetValue, 5 * Time.deltaTime);
            isRestoringColor = false; // 移动时不恢复颜色
        }
        else
        {
            // 进度条稳定后，只执行一次恢复原色
            if (!isRestoringColor)
            {
                FillIamge.DOKill(true); // 先销毁旧动画
                FillIamge.DOColor(OriginalColor, 0.5f)
                    .OnComplete(() =>
                    {
                        isRestoringColor = false; // 动画结束后重置标记
                    });
                isRestoringColor = true; // 标记正在恢复，避免重复执行
            }
            IsCanGetPart = (partSlider.value >= 1);
        }
    }

    public void GetPart(int Amount)
    {
        if (!IsCanGetPart || PartPrefab == null) return;

        var Part = PoolManage.Instance.GetObj(PartPrefab);
        Part.transform.position = StartPartPos.position;

        Part.transform.DOKill(true);
        Part.GetComponent<Image>().DOKill(true);

        Part.transform.DOMove(EnterPlayerAccountPos.position, 1)
            .OnComplete(() =>
            {
                Part.GetComponent<Image>().DOFade(0, 0.3f)
                    .OnComplete(() =>
                    {
                        Part.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                        PoolManage.Instance.PushObj(PartPrefab, Part);
                    });
            });

        UI_ShowPart.Instance.SetPromptText(true, Amount);//开启提示
        Debug.Log("已经开始提示");
    }
}