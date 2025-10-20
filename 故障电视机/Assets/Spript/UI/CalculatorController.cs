using UnityEngine;
using UnityEngine.UI;

public class CalculatorController : MonoBehaviour
{
    [Header("关联滑动窗口")]
    public NumberScroller[] numberScrollers; // 4个滑动窗口的NumberScroller组件

    [Header("控制按钮")]
    public Button randomButton; // 触发随机停止的按钮
    private bool IsTartTime;//是否开始计时
    [SerializeField] private float ReFreshTime = 3f;//刷新时间间隔
    private float CurrenTime;

    void Start()
    {
        // 绑定按钮点击事件
        if (randomButton != null)
        {
            randomButton.onClick.AddListener(OnRandomButtonClick);
        }
    }

    /// <summary>
    /// 按钮点击：让所有滑动窗口随机停止在0-9的数字上
    /// </summary>
    private void OnRandomButtonClick()
    {
        foreach (var scroller in numberScrollers)
        {
            if (scroller != null)
            {
                int randomNum = Random.Range(0, 10); // 随机生成0-9的数字
                scroller.StopAtNumber(randomNum); // 控制窗口停止在该数字
            }
        }

        IsTartTime = true;//开始计时
        CurrenTime = 0f;//重置计时器
    }


    private void Update()
    {
        if (IsTartTime)
        {
            CurrenTime += Time.deltaTime;
            if (CurrenTime >= ReFreshTime)
            {
                CurrenTime = 0f;
                IsTartTime = false;
                //重新重置计算器
                foreach (var scroller in numberScrollers)
                {
                    if (scroller != null)
                    {
                        scroller.RestartScrolling(); // 重置滑动窗口
                    }
                }
            }
        }
    }

}