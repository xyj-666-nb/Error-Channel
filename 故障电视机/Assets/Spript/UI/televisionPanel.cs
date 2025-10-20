using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class televisionPanel : BasePanel
{
    public Image RecycleArea_Image;//回收区域图片
    public Image pushCardArea;//打出卡牌区域图片
    [SerializeField] private TextMeshProUGUI ShowCurrentLevelText;
    [SerializeField] GameObject CallAutoButton;//呼唤计算器的按钮
    public override void Awake()
    {
        base.Awake();
        //最开始设置回收区域
        RecycleArea_Image.color = new Color(RecycleArea_Image.color.r, RecycleArea_Image.color.g, RecycleArea_Image.color.b, 0.4f);//设置为半透明
        CallAutoButton.SetActive(false);//最开始未激活
    }

    public void SetCallAutoButtonActive(bool IsActive)
    {
        if (IsActive)
        {
            CallAutoButton.SetActive(true);
            CallAutoButton.GetComponent<Image>().DOFade(1f, 0.5f);
        }
        else
            CallAutoButton.GetComponent<Image>().DOFade(0f, 0.5f).OnComplete(() =>{ CallAutoButton.SetActive(false); });

    }


    public override void ClickButton(string controlName)
    {
        base.ClickButton(controlName);

        switch (controlName)
        {
            case "PassButton":
                //判断玩家当前有没有选择手牌
               if(Card.CurrentSelectedCard!=null)
                {
                    //随机抽出牌来回收
                    RecycleArea.Instance.RecycleCard(Card.CurrentSelectedCard);
                    HandCardManger.Instance.CreatCard();//创建卡牌                          
                    PassPromptText.Instance.TriggerPass();//触发pass文本更新
                }
               else
               {
                    //触发警告
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "请选择你要更换的手牌");
                }
             
                break;
            case "PushCardButton":
                HandCardManger.Instance.PushCard(PushType.track);//打出卡牌
                break;
                case "AdvanceButton":
                if (UImanager.Instance.GetPanel<AdvanceShopPanel>())
                {
                    controlDic["AdvanceButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Advance Shop";
                    UImanager.Instance.HidePanel<AdvanceShopPanel>();//隐藏高级商店
                }
                else
                {
                    controlDic["AdvanceButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
                    CloseShop<FixShopPanel>("Fix Shop", "FixShopButton");
                    UImanager.Instance.ShowPanel<AdvanceShopPanel>();//显示高级商店
                }
               
                break;
            case "FixShopButton":
                if (UImanager.Instance.GetPanel<FixShopPanel>())
                {
                    controlDic["FixShopButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Fix Shop";
                    UImanager.Instance.HidePanel<FixShopPanel>();//隐藏高级商店
                }
                else
                {
                    controlDic["FixShopButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
                    CloseShop<AdvanceShopPanel>("Advance Shop", "AdvanceButton");
                    UImanager.Instance.ShowPanel<FixShopPanel>();//显示高级商店
                }
                break;
            case "ExitButton":
                break;
            case "CallAutoCalculatorButton":
                UImanager.Instance.ShowPanel<AutoCalculatorpanel>();//自动调用计算器面板
                //失活自己
                controlDic["CallAutoCalculatorButton"].gameObject.SetActive(false);
                break;
        }

    }

    public void ChangeLevelText(GameLevel Level)
    {
        ShowCurrentLevelText.text="当前游戏难度：" +Level.ToString();
    }

    public void  CloseShop<T>(string CloseName,string ButtonName) where T:BasePanel
    {
        if (UImanager.Instance.GetPanel<T>())
        {
            UImanager.Instance.HidePanel<T>();//隐藏高级商店
            controlDic[ButtonName].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = CloseName;
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

    public void SetRecycleAreaActive(bool IsActive)
    {
        Color color = RecycleArea_Image.color;
        if (IsActive)
            RecycleArea_Image.DOColor(new Color(color.r, color.g, color.b, 1f), 0.5f); // 高亮
        else
            RecycleArea_Image.DOColor(new Color(color.r, color.g, color.b, 0.4f), 0.5f); // 半透明
    }

    public void SetpushCardAreaActive(bool IsActive)
    {
        Color color = RecycleArea_Image.color;
        if (IsActive)
            pushCardArea.DOColor(new Color(color.r, color.g, color.b, 1f), 0.3f); // 高亮
        else
            pushCardArea.DOColor(new Color(color.r, color.g, color.b, 0.4f), 0.3f); // 半透明
    }
}
