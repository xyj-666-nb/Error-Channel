using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FixShopPanel : BasePanel
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void ClickButton(string controlName)
    {
        base.ClickButton(controlName);
        switch(controlName)
        {
            case "CorrectShowGetMoneyButton"://玩家胜利获取的金币

                if (PlayerManager.instance.PlayerParts >= PlayerManager.instance.StartShowPrompttext_GoldneedPart)
                {
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.StartShowPrompttext_GoldneedPart);
                    PlayerManager.instance.IsStartShowPrompttext_Gold = true;
                    //停止交互
                    controlDic["CorrectShowGetMoneyButton"].GetComponent<Button>().interactable = false;
                    controlDic["CorrectShowGetMoneyButton"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "已修复";
                    //增加修复进度
                    FixProcess.Instance.UpdateFixProcess(0.1f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;
            case "CorrectShowFixAmountButton"://显示当前修复零件数量
                if (PlayerManager.instance.PlayerParts >= PlayerManager.instance.FixPart_NeedParts)
                {
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.FixPart_NeedParts);
                    UI_ShowPart.Instance.FixMe();//修复显示零件
                    controlDic["CorrectShowFixAmountButton"].GetComponent<Button>().interactable = false;
                    controlDic["CorrectShowFixAmountButton"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "已修复";
                    FixProcess.Instance.UpdateFixProcess(0.15f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;
            case "CorrectShowGetFixAmountButton"://玩家胜利获取的修复零件
                if(PlayerManager.instance.PlayerParts >= PlayerManager.instance.StartShowPrompttext_PartneedPart)
                {
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.StartShowPrompttext_PartneedPart);
                    PlayerManager.instance.IsStartShowPrompttext_part = true;
                    controlDic["CorrectShowGetFixAmountButton"].GetComponent<Button>().interactable = false;
                    controlDic["CorrectShowGetFixAmountButton"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "已修复";
                    FixProcess.Instance.UpdateFixProcess(0.1f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;
            case "CountCorrectButton"://显示当前的生命

                break;
            case "FixVolumeButton"://修复音量问题

                break;
            case "CorrectShowShopButton"://显示玩家当前金币数量
                if (PlayerManager.instance.PlayerParts >= PlayerManager.instance.IsFixShowGold_NeedPart)
                {
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.IsFixShowGold_NeedPart);
                    UI_ShowGold.Instance.FixMe();//修复显示金币
                    controlDic["CorrectShowShopButton"].GetComponent<Button>().interactable = false;
                    controlDic["CorrectShowShopButton"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "已修复";
                    FixProcess.Instance.UpdateFixProcess(0.1f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;
            case "FixTelevisionCatonButton"://修复电视机的显示问题

                break;
            case "FixExitGameButton"://修复退出游戏按钮
                if(PlayerManager.instance.PlayerParts >= PlayerManager.instance.IsFixExitButton_NeedParts)
                {
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.IsFixExitButton_NeedParts);
                    PlayerManager.instance.IsFixExitButton = true;//修复退出按钮
                    UImanager.Instance.GetPanel<televisionPanel>().SetFIxExitButton();//设置退出按钮可用
                    controlDic["FixExitGameButton"].GetComponent<Button>().interactable = false;
                    controlDic["FixExitGameButton"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "已修复";
                    FixProcess.Instance.UpdateFixProcess(0.2f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;

        }
    }

    public override void Start()
    {
        base.Start();
        //更新零件数量的显示
        controlDic["FixExitGameButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =PlayerManager.instance.IsFixExitButton_NeedParts.ToString()+"P";
        controlDic["CorrectShowShopButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.IsFixShowGold_NeedPart.ToString() + "P";
        controlDic["CorrectShowGetFixAmountButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.StartShowPrompttext_PartneedPart.ToString() + "P";
        controlDic["CorrectShowGetMoneyButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.StartShowPrompttext_GoldneedPart.ToString() + "P";
        controlDic["CorrectShowFixAmountButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.FixPart_NeedParts.ToString() + "P";
    }
}
