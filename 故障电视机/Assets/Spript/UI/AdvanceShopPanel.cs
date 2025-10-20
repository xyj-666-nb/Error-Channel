using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdvanceShopPanel : BasePanel
{
    private GameLevel previousLevel;//记录之前的难度等级

    public override void Awake()
    {
        base.Awake();
    }


    public override void ClickButton(string controlName)
    {
        base.ClickButton(controlName);
        switch (controlName)
        { 
         case "AddLevel_Button":
                //先判断当前的升级难度需要的金币是否足够
                if (PlayerManager.instance.PlayerCurrentGold >= PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel])
                {
                    //扣除金币
                    GetGoldArea.Instance.UseGoldInAdvanceShop(PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel]);//使用金币动画
                    PlayerManager.instance.ChangeGold(-PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel]);//扣除金币
                    PlayerManager.instance.AddGameLevel();//提升游戏难度
                    controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "难度"+PlayerManager.instance.CurrentLevel.ToString();
                    controlDic["AddLevel_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel].ToString() + "$";
                }

          break;
            case "AddCardAmount_Button":
                //先判断当前的升级额外卡牌数量需要的金币是否足够
                if (PlayerManager.instance.IsCanAdvanceCardAmount && PlayerManager.instance.PlayerCurrentGold >= PlayerManager.instance.AvancedCardAmount_NeedMoney)
                {
                    //扣除金币
                    GetGoldArea.Instance.UseGoldInAdvanceShop(PlayerManager.instance.AvancedCardAmount_NeedMoney);//使用金币动画
                    PlayerManager.instance.ChangeGold(-PlayerManager.instance.AvancedCardAmount_NeedMoney);//扣除金币
                    PlayerManager.instance.MaxCardAmount++;//提升最大卡牌数量
                    PlayerManager.instance.CurrentAdvanceCardAmount++;//当前已升级的额外卡牌数量+1
                    HandCardManger.Instance.AddCardMount();//手牌管理器增加手牌数量
                    PlayerManager.instance. AvancedCardAmount_NeedMoney = PlayerManager.instance.AvancedCardAmount_NeedMoney * 2;//每次升级所需金币翻倍
                    //调用更新
                    if ((int)PlayerManager.instance.CurrentLevel<PlayerManager.instance.CurrentAdvanceCardAmount)//如果升级上限就禁用
                    {
                        (controlDic["AddCardAmount_Button"] as Button).interactable = false;//无法交互
                                                                                            //进行提示
                        controlDic["AddCardAmount_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "提升难度后激活";
                        controlDic["AddCardAmount_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "###$";
                    }
                    else
                    {
                        controlDic["AddCardAmount_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.AvancedCardAmount_NeedMoney.ToString() + "$";
                    }
                 
                }
                break;

            case "OutComeStrengthen_Button":
                //结果强化按钮

                break;

            case "AutoCalculator_Button":
                //自动计算器按钮
                //生成自动计算机,显示当前敌人的数值
                PlayerManager.instance.ActiveAutoButton();
                //变为不可交互
                (controlDic["AutoCalculator_Button"] as Button).interactable = false;
                break;

        }

    }

    public override void HideMe(UnityAction callback)
    {
        previousLevel = PlayerManager.instance.CurrentLevel;//记录之前的难度等级
        base.HideMe(callback);
    }

    public void SetAddCardAmount_Button_interactable()
    {
        (controlDic["AddCardAmount_Button"] as Button).interactable = true;//可以交互
        //进行提示
        controlDic["AddCardAmount_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "更多手牌";
        controlDic["AddCardAmount_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.AvancedCardAmount_NeedMoney.ToString() + "$";
    }

    public override void ShowMe(bool IsNeedDefalutAnimator = true)
    {
        CheckUpdate();
        base.ShowMe(IsNeedDefalutAnimator);
    }

    public void CheckUpdate()
    {
        //每次调用自动检查更新
        if (PlayerManager.instance.CurrentLevel != previousLevel)
        {
            //难度提升了，检查是否可以激活额外卡牌数量按钮
            if (!PlayerManager.instance.IsCanAdvanceCardAmount)
            {
                SetAddCardAmount_Button_interactable();
            }
        }
    }

    public override void Start()
    {
        base.Start();
        //更新显示
        controlDic["AddCardAmount_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.AvancedCardAmount_NeedMoney.ToString() + "$";
        //更新显示当前难度
        controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "难度" + PlayerManager.instance.CurrentLevel.ToString();
        controlDic["AddLevel_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel].ToString() + "$";
    }
}
