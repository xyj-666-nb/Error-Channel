using UnityEngine;
using UnityEngine.UI;

public class CalculatorController : MonoBehaviour
{
    public NumberScroller[] scrollers; // 4个滑动窗口的NumberScroller组件
    public Button randomButton; // 触发随机显示的按钮

    void Start()
    {
        // 按钮点击时，给4个窗口随机显示0-9的数字
        randomButton.onClick.AddListener(() =>
        {
            foreach (var scroller in scrollers)
            {
                int randomNum = Random.Range(0, 10); // 随机0-9
                scroller.ShowTargetNumber(randomNum); // 让窗口显示该数字
            }
        });
    }
}