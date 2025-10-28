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
        switch (controlName)
        {
            case "CorrectShowGetMoneyButton"://玩家胜利获取的金币
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);

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
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (PlayerManager.instance.PlayerParts >= PlayerManager.instance.FixPart_NeedParts)
                {
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.FixPart_NeedParts);
                    UI_ShowPart.Instance.FixMe();//修复显示零件
                    SetCompleteFxi("CorrectShowFixAmountButton");
                    FixProcess.Instance.UpdateFixProcess(0.15f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;
            case "CorrectShowGetFixAmountButton"://玩家胜利获取的修复零件
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (PlayerManager.instance.PlayerParts >= PlayerManager.instance.StartShowPrompttext_PartneedPart)
                {
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.StartShowPrompttext_PartneedPart);
                    PlayerManager.instance.IsStartShowPrompttext_part = true;
                    SetCompleteFxi("CorrectShowGetFixAmountButton");
                    FixProcess.Instance.UpdateFixProcess(0.1f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;
            case "CountCorrectButton"://显示当前的生命
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (PlayerManager.instance.PlayerParts >= PlayerManager.instance.FixShowPlayerHealthNeedPart)
                {
                    //如果零件够
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.FixShowPlayerHealthNeedPart);//消耗
                    PlayerManager.instance.IsFixShowPlayerHealth = true;
                    //调用更新
                    UI_healthslider.instance.UpdateHeathBar();//更新血量
                    SetCompleteFxi("CountCorrectButton");
                    FixProcess.Instance.UpdateFixProcess(0.1f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;
            case "FixVolumeButton"://修复音量问题
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (PlayerManager.instance.PlayerParts >= PlayerManager.instance.FixVolumeNeedPart)
                {
                    //如果零件够
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.FixVolumeNeedPart);//消耗
                    PlayerManager.instance.FixVolume = true;
                    VoicevolumeButton.Instance.SetButton(true);
                    SetCompleteFxi("FixVolumeButton");
                    FixProcess.Instance.UpdateFixProcess(0.15f);
                }

                break;
            case "CorrectShowShopButton"://显示玩家当前金币数量
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (PlayerManager.instance.PlayerParts >= PlayerManager.instance.IsFixShowGold_NeedPart)
                {

                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.IsFixShowGold_NeedPart);
                    UI_ShowGold.Instance.FixMe();//修复显示金币
                    SetCompleteFxi("CorrectShowShopButton");
                    FixProcess.Instance.UpdateFixProcess(0.1f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;
            case "FixTelevisionCatonButton"://修复电视机的显示问题
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (PlayerManager.instance.PlayerParts >= PlayerManager.instance.FixCcenceEffector_NeedPart)
                {
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.FixCcenceEffector_NeedPart);
                    PlayerManager.instance.FixCcenceEffector = true;
                    SetCompleteFxi("FixTelevisionCatonButton");
                    //调用屏幕修复
                    CRTPostEffecter.instance.FixCRTEffect_MediumVintage();//减弱效果
                    FixProcess.Instance.UpdateFixProcess(0.1f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;
            case "FixExitGameButton"://修复退出游戏按钮
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (PlayerManager.instance.PlayerParts >= PlayerManager.instance.IsFixExitButton_NeedParts)
                {
                    UI_ShowPart.Instance.ConsumePartEffect(PlayerManager.instance.IsFixExitButton_NeedParts);
                    PlayerManager.instance.IsFixExitButton = true;//修复退出按钮
                    UImanager.Instance.GetPanel<televisionPanel>().SetFIxExitButton();//设置退出按钮可用
                    SetCompleteFxi("FixExitGameButton");
                    FixProcess.Instance.UpdateFixProcess(0.2f);
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "零件数量不够");
                break;

        }
    }

    private void SetCompleteFxi(string Name)
    {
        controlDic[Name].GetComponent<Button>().interactable = false;
        controlDic[Name].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "已修复";
    }

    public override void Start()
    {
        base.Start();
        //更新零件数量的显示
        controlDic["FixExitGameButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.IsFixExitButton_NeedParts.ToString() + "P";
        controlDic["CorrectShowShopButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.IsFixShowGold_NeedPart.ToString() + "P";
        controlDic["CorrectShowGetFixAmountButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.StartShowPrompttext_PartneedPart.ToString() + "P";
        controlDic["CorrectShowGetMoneyButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.StartShowPrompttext_GoldneedPart.ToString() + "P";
        controlDic["FixVolumeButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.FixVolumeNeedPart.ToString() + "P";
        controlDic["FixTelevisionCatonButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.FixCcenceEffector_NeedPart.ToString() + "P";
        controlDic["CountCorrectButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.FixShowPlayerHealthNeedPart.ToString() + "P";
        controlDic["CorrectShowFixAmountButton"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.FixPart_NeedParts.ToString() + "P";

        //最开始进行判断是否失活按钮
        if (PlayerManager.instance.IsObtainShowPart)//如果已经解锁了显示
            SetCompleteFxi("CorrectShowFixAmountButton");
        if (PlayerManager.instance.IsFixExitButton)//如果修复了退出按钮
            SetCompleteFxi("FixExitGameButton");
        if (PlayerManager.instance.IsObtainShowGoldSkill)//如果已经获得金币显示
            SetCompleteFxi("CorrectShowShopButton");
        if (PlayerManager.instance.IsStartShowPrompttext_Gold)
        {
            controlDic["CorrectShowGetMoneyButton"].GetComponent<Button>().interactable = false;
            controlDic["CorrectShowGetMoneyButton"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "已修复";
        }
        ;
        if (PlayerManager.instance.IsStartShowPrompttext_part)
            SetCompleteFxi("CorrectShowGetFixAmountButton");
        if (PlayerManager.instance.IsFixShowPlayerHealth)
            SetCompleteFxi("CountCorrectButton");
        if (PlayerManager.instance.FixCcenceEffector)
            SetCompleteFxi("FixTelevisionCatonButton");
        if (PlayerManager.instance.FixVolume)
            SetCompleteFxi("FixVolumeButton");
    }
}