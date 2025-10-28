using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdvanceShopPanel : BasePanel
{
    private GameLevel previousLevel;
    private bool IsGetMax = false;

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
                // 难度升级逻辑保持不变
                Debug.Log($"开始升级检查 - 当前等级: {PlayerManager.instance.CurrentLevel}, 索引: {(int)PlayerManager.instance.CurrentLevel}");
                Debug.Log($"AdvanceLevelNeedMoney 列表长度: {PlayerManager.instance.AdvanceLevelNeedMoney?.Count}");

                if (!IsValidLevelIndex((int)PlayerManager.instance.CurrentLevel))
                {
                    Debug.LogError($"无效的等级索引: {(int)PlayerManager.instance.CurrentLevel}, 列表长度: {PlayerManager.instance.AdvanceLevelNeedMoney?.Count}");
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("错误", "升级配置错误，请联系开发人员");
                    return;
                }

                int needMoney = PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel];

                if (PlayerManager.instance.PlayerCurrentGold >= needMoney)
                {
                    MusicManager.Instance.PlayEffectMusic("Music/成功在商店购买", false);
                    GetGoldArea.Instance.UseGoldInAdvanceShop(needMoney);
                    PlayerManager.instance.ChangeGold(-needMoney);

                    GameLevel oldLevel = PlayerManager.instance.CurrentLevel;

                    if (PlayerManager.instance.CurrentLevel == GameLevel.Level5)
                    {
                        controlDic["AddLevel_Button"].GetComponent<Button>().interactable = false;
                        controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "达到最高上限";
                        return;
                    }
                    else
                        PlayerManager.instance.AddGameLevel();

                    Debug.Log($"升级完成 - 旧等级: {oldLevel}, 新等级: {PlayerManager.instance.CurrentLevel}, 新索引: {(int)PlayerManager.instance.CurrentLevel}");

                    if (PlayerManager.instance.CurrentLevel == GameLevel.Level5)
                    {
                        controlDic["AddLevel_Button"].GetComponent<Button>().interactable = false;
                        controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "达到最高上限";
                        controlDic["AddLevel_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "MAX";
                    }
                    else
                    {
                        int nextLevelIndex = (int)PlayerManager.instance.CurrentLevel;
                        if (IsValidLevelIndex(nextLevelIndex))
                        {
                            controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "难度" + PlayerManager.instance.CurrentLevel.ToString();
                            controlDic["AddLevel_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.AdvanceLevelNeedMoney[nextLevelIndex].ToString() + "$";
                        }
                        else
                        {
                            Debug.LogError($"升级后索引无效: {nextLevelIndex}, 列表长度: {PlayerManager.instance.AdvanceLevelNeedMoney?.Count}");
                            controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "难度" + PlayerManager.instance.CurrentLevel.ToString();
                            controlDic["AddLevel_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "N/A";
                        }
                    }

                    // 升级后刷新结果强化按钮状态
                    UpdateOutComeStrengthenButton();
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "金币数量不够");

                break;

            case "AddCardAmount_Button":
                // 手牌数量升级逻辑保持不变
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (PlayerManager.instance.IsCanAdvanceCardAmount && PlayerManager.instance.PlayerCurrentGold >= PlayerManager.instance.AvancedCardAmount_NeedMoney)
                {
                    MusicManager.Instance.PlayEffectMusic("Music/成功在商店购买", false);
                    GetGoldArea.Instance.UseGoldInAdvanceShop(PlayerManager.instance.AvancedCardAmount_NeedMoney);
                    PlayerManager.instance.ChangeGold(-PlayerManager.instance.AvancedCardAmount_NeedMoney);
                    PlayerManager.instance.MaxCardAmount++;
                    PlayerManager.instance.CurrentAdvanceCardAmount++;
                    HandCardManger.Instance.AddCardMount();
                    PlayerManager.instance.AvancedCardAmount_NeedMoney = PlayerManager.instance.AvancedCardAmount_NeedMoney * 2;

                    if ((int)PlayerManager.instance.CurrentLevel < PlayerManager.instance.CurrentAdvanceCardAmount)
                    {
                        (controlDic["AddCardAmount_Button"] as Button).interactable = false;
                        controlDic["AddCardAmount_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "提升难度后激活";
                        controlDic["AddCardAmount_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "###$";
                    }
                    else
                    {
                        controlDic["AddCardAmount_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.AvancedCardAmount_NeedMoney.ToString() + "$";
                    }
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "金币数量不够");
                break;

            case "OutComeStrengthen_Button":
                // 修复后的结果强化逻辑
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                int currentLevelIndex = (int)PlayerManager.instance.CurrentLevel;
                int strengthenPrice = PlayerManager.instance.GetCurrentLevelStrengthenPrice();

                // 检查是否已经购买过当前等级的强化
                if (PlayerManager.instance.HasPurchasedStrengthenForCurrentLevel())
                {
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "当前等级的结果强化已购买");
                    return;
                }

                // 检查金币是否足够
                if (PlayerManager.instance.PlayerCurrentGold >= strengthenPrice)
                {
                    MusicManager.Instance.PlayEffectMusic("Music/成功在商店购买", false);
                    GetGoldArea.Instance.UseGoldInAdvanceShop(strengthenPrice);
                    PlayerManager.instance.ChangeGold(-strengthenPrice);

                    // 购买强化
                    PlayerManager.instance.PurchaseStrengthenForCurrentLevel();

                    // 更新按钮状态
                    UpdateOutComeStrengthenButton();

                    Debug.Log($"结果强化购买成功，等级{currentLevelIndex}");
                }
                else
                {
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "金币数量不够");
                }
                break;

            case "AutoCalculator_Button":
                // 自动计算器逻辑保持不变
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (PlayerManager.instance.GetAutoCulCaltorNeedMoney <= PlayerManager.instance.PlayerCurrentGold)
                {
                    MusicManager.Instance.PlayEffectMusic("Music/成功在商店购买", false);
                    PlayerManager.instance.ActiveAutoButton();
                    controlDic["AutoCalculator_Button"].GetComponent<Button>().interactable = false;
                    controlDic["AutoCalculator_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "成功购买";
                    GetGoldArea.Instance.UseGoldInAdvanceShop(PlayerManager.instance.GetAutoCulCaltorNeedMoney);
                    PlayerManager.instance.ChangeGold(-PlayerManager.instance.GetAutoCulCaltorNeedMoney);
                    //召唤对话
                    Main.Instance.InitDia.StartDialogue(9);
                }
                else
                {
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "当前金币不够");
                }
                break;

            case "ExitButton":
                UImanager.Instance.HidePanel<AdvanceShopPanel>();
                UImanager.Instance.GetPanel<televisionPanel>().controlDic["AdvanceButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "升级商店";
                break;

            case "RealHealthButton":
                if (PlayerManager.instance.RealHealthNeedMoney <= PlayerManager.instance.PlayerCurrentGold)
                {
                    MusicManager.Instance.PlayEffectMusic("Music/成功在商店购买", false);
                    GetGoldArea.Instance.UseGoldInAdvanceShop(PlayerManager.instance.RealHealthNeedMoney);
                    PlayerManager.instance.ChangeGold(-PlayerManager.instance.RealHealthNeedMoney);
                    PlayerManager.instance.ChangeHealth(2);
                }
                else
                {
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "当前金币不够");
                }
                break;
        }
    }

    /// <summary>
    /// 更新结果强化按钮状态
    /// </summary>
    private void UpdateOutComeStrengthenButton()
    {
        Button strengthenButton = controlDic["OutComeStrengthen_Button"].GetComponent<Button>();
        TextMeshProUGUI buttonText = controlDic["OutComeStrengthen_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = controlDic["OutComeStrengthen_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        int currentLevelIndex = (int)PlayerManager.instance.CurrentLevel;

        if (PlayerManager.instance.IsAllStrengthenPurchased())
        {
            // 所有等级强化都已购买
            strengthenButton.interactable = false;
            buttonText.text = "达到上限";
            priceText.text = "MAX";
            IsGetMax = true;
        }
        else if (PlayerManager.instance.HasPurchasedStrengthenForCurrentLevel())
        {
            // 当前等级强化已购买
            strengthenButton.interactable = false;
            buttonText.text = "已购买";
            priceText.text = "---";
        }
        else
        {
            // 当前等级强化可购买
            strengthenButton.interactable = true;
            buttonText.text = "结果强化";
            priceText.text = PlayerManager.instance.GetCurrentLevelStrengthenPrice().ToString() + "$";
        }
    }

    /// <summary>
    /// 检查等级索引是否有效
    /// </summary>
    private bool IsValidLevelIndex(int levelIndex)
    {
        return PlayerManager.instance.AdvanceLevelNeedMoney != null &&
               levelIndex >= 0 &&
               levelIndex < PlayerManager.instance.AdvanceLevelNeedMoney.Count;
    }

    public override void HideMe(UnityAction callback)
    {
        previousLevel = PlayerManager.instance.CurrentLevel;
        base.HideMe(callback);
    }

    public void SetAddCardAmount_Button_interactable()
    {
        (controlDic["AddCardAmount_Button"] as Button).interactable = true;
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
        if (PlayerManager.instance.CurrentLevel != previousLevel)
        {
            if (!PlayerManager.instance.IsCanAdvanceCardAmount)
            {
                SetAddCardAmount_Button_interactable();
            }
        }
    }

    public override void Start()
    {
        base.Start();

        // 更新手牌升级按钮
        controlDic["AddCardAmount_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.AvancedCardAmount_NeedMoney.ToString() + "$";

        // 更新难度升级按钮
        if (PlayerManager.instance.CurrentLevel == GameLevel.Level5)
        {
            controlDic["AddLevel_Button"].GetComponent<Button>().interactable = false;
            controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "达到最高上限";
            controlDic["AddLevel_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "MAX";
        }
        else
        {
            if (IsValidLevelIndex((int)PlayerManager.instance.CurrentLevel))
            {
                controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.CurrentLevel.ToString();
                controlDic["AddLevel_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel].ToString() + "$";
            }
            else
            {
                Debug.LogError($"Start方法中索引无效: {(int)PlayerManager.instance.CurrentLevel}");
                controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.CurrentLevel.ToString();
                controlDic["AddLevel_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "N/A";
            }
        }

        // 更新自动计算器按钮
        controlDic["AutoCalculator_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.GetAutoCulCaltorNeedMoney.ToString() + "$";

        if (PlayerManager.instance.IsGetAutoCulCaltorSkill)
        {
            controlDic["AutoCalculator_Button"].GetComponent<Button>().interactable = false;
            controlDic["AutoCalculator_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "成功购买";
        }

        // 更新结果强化按钮状态
        UpdateOutComeStrengthenButton();
    }
}