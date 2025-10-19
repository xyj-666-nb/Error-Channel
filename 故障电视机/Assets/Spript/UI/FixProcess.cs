using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FixProcess : MonoBehaviour
{
    private static FixProcess instance;
    public static FixProcess Instance=> instance;


    [SerializeField] private Slider MyFixProcess;//我的修复进度
    [SerializeField] private TextMeshProUGUI ShowText;//显示的文本
    private float TargetValue = 0f;//目标值

    private void Awake()
    {
        instance = this;
        TargetValue = 0f;
        MyFixProcess.value = 0;
        ShowText.text = "修复进度：" + (int)(TargetValue * 100) + "%";
    }

    public void UpdateFixProcess(float value)
    {
       ShowText.text = "修复进度：" + (int)(value * 100) + "%";
       TargetValue = value;
    }

    private void Update()
    {
        if(TargetValue != MyFixProcess.value)
        {
            MyFixProcess.value = Mathf.Lerp(MyFixProcess.value, TargetValue, 0.1f*Time.deltaTime);
        }
    }
}
