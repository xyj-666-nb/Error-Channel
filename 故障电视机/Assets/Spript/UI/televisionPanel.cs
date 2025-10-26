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
    [SerializeField] AnimatorControl AnimatorControl_Pass;
    [SerializeField] AnimatorControl AnimatorControl_FixShop;
    [SerializeField] AnimatorControl AnimatorControl_advance;
    [SerializeField] AnimatorControl AnimatorControl_Exit;
    public override void Awake()
    {
        base.Awake();
        //最开始设置回收区域
        RecycleArea_Image.color = new Color(RecycleArea_Image.color.r, RecycleArea_Image.color.g, RecycleArea_Image.color.b, 0.4f);//设置为半透明
        CallAutoButton.gameObject.SetActive(false);//最开始隐藏呼唤计算器按钮
        controlDic["ExitButton"].GetComponent<Button>().interactable = false;//不可交互

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
                    MusicManager.Instance.PlayEffectMusic("Music/单次拿牌", false);
                    PassPromptText.Instance.TriggerPass();//触发pass文本更新
                    AnimatorControl_Pass.SetAnimatorStart(); //调用pass按钮的特效
                    MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                }
               else
               {
                    AnimatorControl_Pass.SetAnimatorStart(); //调用pass按钮的特效
                    //触发警告
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "请选择你要更换的手牌");
                }

                break;
            case "PushCardButton":

                HandCardManger.Instance.PushCard(PushType.track);//打出卡牌
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                break;
                case "AdvanceButton":
                AnimatorControl_advance.SetAnimatorStart();
                if (UImanager.Instance.GetPanel<AdvanceShopPanel>())
                {
                    controlDic["AdvanceButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Advance Shop";
                    UImanager.Instance.HidePanel<AdvanceShopPanel>();//隐藏高级商店
                    MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                }
                else
                {
                    controlDic["AdvanceButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
                    CloseShop<FixShopPanel>("Fix Shop", "FixShopButton");
                    UImanager.Instance.ShowPanel<AdvanceShopPanel>();//显示高级商店
                }
               
                break;
            case "FixShopButton":
                AnimatorControl_FixShop.SetAnimatorStart();
                if (UImanager.Instance.GetPanel<FixShopPanel>())
                {
                    controlDic["FixShopButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Fix Shop";
                    UImanager.Instance.HidePanel<FixShopPanel>();//隐藏高级商店
                    MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                }
                else
                {
                    controlDic["FixShopButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
                    CloseShop<AdvanceShopPanel>("Advance Shop", "AdvanceButton");
                    UImanager.Instance.ShowPanel<FixShopPanel>();//显示高级商店
                }
                break;
            case "ExitButton":
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                AnimatorControl_Exit.SetAnimatorStart();
                break;
            case "CallAutoCalculatorButton":
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                UImanager.Instance.ShowPanel<AutoCalculatorpanel>();//自动调用计算器面板
                //失活自己
                controlDic["CallAutoCalculatorButton"].gameObject.SetActive(false);
                break;
            case "FlipCardButton":
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (Card.CurrentSelectedCard!=null)
                    Card.CurrentSelectedCard.Flip();//进行翻转
                break;

            case "FlipAllCardButton":
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                StartCoroutine(FlipAllCard());
                break;
        }

    }

    IEnumerator FlipAllCard()
    {
        foreach(var Card in HandCardManger.Instance.HandCardList)
        {
            Card.GetComponentInChildren<Card>()?.Flip();
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void SetCanShowCardFlip(bool IsShow)//显示卡片翻转的按钮
    {
        // 终止所有相关动画
        var flipButton = controlDic["FlipCardButton"].gameObject;
        var buttonImage = flipButton.GetComponent<Image>();
        var buttonText = flipButton.GetComponentInChildren<TextMeshProUGUI>();

        buttonImage.DOKill();    // 终止按钮图片动画
        buttonText.DOKill();     // 终止文本动画

        if (IsShow)
        {
            // 显示时：先激活按钮，再执行淡入
            if (!flipButton.activeSelf)
                flipButton.SetActive(true);

            buttonImage.DOFade(1, 0.6f);
            buttonText.DOFade(1, 0.6f);
        }
        else
        {
            //  隐藏时：淡入完成后再禁用按钮
            // 使用Sequence确保透明度动画同步完成
            DOTween.Sequence()
                .Append(buttonImage.DOFade(0, 0.6f))
                .Join(buttonText.DOFade(0, 0.6f))
                .OnComplete(() =>
                {
                    if (flipButton.activeSelf)
                        flipButton.SetActive(false);
                });
        }
    }


    public void ChangeLevelText(GameLevel Level)
    {
        ShowCurrentLevelText.text=Level.ToString();
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
        //开局隐藏退出按钮
    }

    public void SetFIxExitButton()
    {
        PlayerManager.instance.IsFixExitButton = true;
        controlDic["ExitButton"].gameObject.GetComponent<Image>().DOFade(1f, 1f);
        controlDic["ExitButton"].GetComponentInChildren<TextMeshProUGUI>().DOFade(1f, 1f);
        controlDic["ExitButton"].GetComponent<Button>().interactable = true;//可以交互
    }

    protected override void Update()
    {
        base.Update();
        if (Card.CurrentSelectedCard != null && controlDic["FlipCardButton"].gameObject.activeSelf == false)
            SetCanShowCardFlip(true);
        else if (Card.CurrentSelectedCard == null && controlDic["FlipCardButton"].gameObject.activeSelf == true)
            SetCanShowCardFlip(false);
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
