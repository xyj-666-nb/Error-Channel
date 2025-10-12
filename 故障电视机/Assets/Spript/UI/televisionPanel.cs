using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class televisionPanel : BasePanel
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void ClickButton(string controlName)
    {
        base.ClickButton(controlName);

        switch (controlName)
        {
            case "GetCardButton":
                HandCardManger.Instance.CreatCard();//创建卡牌
                break;
        
        }

    }

    public override void HideMe(UnityAction callback)
    {
        base.HideMe(callback);
    }

    public override void ShowMe(bool IsNeedDefalutAnimator = true)//要是需要特殊动画就改false,然后加入特殊的标识结束的时候在动画控制器里面调用！
    {
        base.ShowMe(IsNeedDefalutAnimator);
    }

    public override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    //提供给外部一个控制出牌按钮的接口
    public void SetGetCardButtonState(bool IsCanUse)
    {
       Button GetCardButton=controlDic["GetCardButton"]  as Button;
        GetCardButton.interactable = IsCanUse;//设置按钮是否可以点击
        //设置按钮的颜色
        Color TargetColor = IsCanUse ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0f);
        transform.DOKill();//先停止之前的动画
        GetCardButton.GetComponent<Image>().DOColor(TargetColor,0.5f);//渐变颜色
        GetCardButton.GetComponentInChildren<TextMeshProUGUI>().DOColor(TargetColor, 0.5f);//渐变颜色
    }
}
