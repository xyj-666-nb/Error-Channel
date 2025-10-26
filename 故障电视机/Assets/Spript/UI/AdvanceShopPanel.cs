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
                // 添加详细的调试信息
                Debug.Log($"开始升级检查 - 当前等级: {PlayerManager.instance.CurrentLevel}, 索引: {(int)PlayerManager.instance.CurrentLevel}");
                Debug.Log($"AdvanceLevelNeedMoney 列表长度: {PlayerManager.instance.AdvanceLevelNeedMoney?.Count}");

                // 添加安全检查
                if (!IsValidLevelIndex((int)PlayerManager.instance.CurrentLevel))
                {
                    Debug.LogError($"无效的等级索引: {(int)PlayerManager.instance.CurrentLevel}, 列表长度: {PlayerManager.instance.AdvanceLevelNeedMoney?.Count}");
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("错误", "升级配置错误，请联系开发人员");
                    return;
                }

                // 关键修复：使用当前等级作为索引（因为列表存储的是从当前等级升级到下一级需要的金币）
                int needMoney = PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel];

                if (PlayerManager.instance.PlayerCurrentGold >= needMoney)
                {
                    MusicManager.Instance.PlayEffectMusic("Music/成功在商店购买", false);
                    //扣除金币
                    GetGoldArea.Instance.UseGoldInAdvanceShop(needMoney);//使用金币动画
                    PlayerManager.instance.ChangeGold(-needMoney);//扣除金币

                    // 保存升级前的等级
                    GameLevel oldLevel = PlayerManager.instance.CurrentLevel;

                    if (PlayerManager.instance.CurrentLevel == GameLevel.Level5)
                    {
                        controlDic["AddLevel_Button"].GetComponent<Button>().interactable = false;
                        controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "达到最高上限";
                        return;
                    }
                    else
                        PlayerManager.instance.AddGameLevel();//提升游戏难度

                    Debug.Log($"升级完成 - 旧等级: {oldLevel}, 新等级: {PlayerManager.instance.CurrentLevel}, 新索引: {(int)PlayerManager.instance.CurrentLevel}");

                    // 关键修复：升级后检查是否达到最高等级
                    if (PlayerManager.instance.CurrentLevel == GameLevel.Level5)
                    {
                        controlDic["AddLevel_Button"].GetComponent<Button>().interactable = false;
                        controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "达到最高上限";
                        controlDic["AddLevel_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "MAX";
                    }
                    else
                    {
                        // 关键修复：升级后，下一级升级需要的金币是列表中新等级的索引
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
                }
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "金币数量不够");

                break;
            case "AddCardAmount_Button":
                //先判断当前的升级额外卡牌数量需要的金币是否足够
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                if (PlayerManager.instance.IsCanAdvanceCardAmount && PlayerManager.instance.PlayerCurrentGold >= PlayerManager.instance.AvancedCardAmount_NeedMoney)
                {
                    MusicManager.Instance.PlayEffectMusic("Music/成功在商店购买", false);
                    //扣除金币
                    GetGoldArea.Instance.UseGoldInAdvanceShop(PlayerManager.instance.AvancedCardAmount_NeedMoney);//使用金币动画
                    PlayerManager.instance.ChangeGold(-PlayerManager.instance.AvancedCardAmount_NeedMoney);//扣除金币
                    PlayerManager.instance.MaxCardAmount++;//提升最大卡牌数量
                    PlayerManager.instance.CurrentAdvanceCardAmount++;//当前已升级的额外卡牌数量+1
                    HandCardManger.Instance.AddCardMount();//手牌管理器增加手牌数量
                    PlayerManager.instance.AvancedCardAmount_NeedMoney = PlayerManager.instance.AvancedCardAmount_NeedMoney * 2;//每次升级所需金币翻倍
                    //调用更新
                    if ((int)PlayerManager.instance.CurrentLevel < PlayerManager.instance.CurrentAdvanceCardAmount)//如果升级上限就禁用
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
                else
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "金币数量不够");
                break;

            case "OutComeStrengthen_Button":
                //结果强化按钮
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                break;

            case "AutoCalculator_Button":
                MusicManager.Instance.PlayEffectMusic("Music/点击", false);
                //自动计算器按钮
                //生成自动计算机,显示当前敌人的数值
                //先判断金币是否足够
                if (PlayerManager.instance.GetAutoCulCaltorNeedMoney <= PlayerManager.instance.PlayerCurrentGold)
                {
                    MusicManager.Instance.PlayEffectMusic("Music/成功在商店购买", false);
                    PlayerManager.instance.ActiveAutoButton();
                    //变为不可交互
                    controlDic["AutoCalculator_Button"].GetComponent<Button>().interactable = false;
                    controlDic["AutoCalculator_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "成功购买";
                    //扣除金币
                    GetGoldArea.Instance.UseGoldInAdvanceShop(PlayerManager.instance.GetAutoCulCaltorNeedMoney);//使用金币动画
                    PlayerManager.instance.ChangeGold(-PlayerManager.instance.GetAutoCulCaltorNeedMoney);//扣除金币
                }
                else
                {
                    UImanager.Instance.ShowPanel<WarnPanel>().SetText("注意！", "当前金币不够");
                }

                break;

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

        // 关键修复：先检查是否是最高等级
        if (PlayerManager.instance.CurrentLevel == GameLevel.Level5)
        {
            controlDic["AddLevel_Button"].GetComponent<Button>().interactable = false;
            controlDic["AddLevel_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "达到最高上限";
            controlDic["AddLevel_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "MAX";
        }
        else
        {
            // 只有在不是最高等级时才尝试访问列表
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

        controlDic["AutoCalculator_Button"].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerManager.instance.GetAutoCulCaltorNeedMoney.ToString() + "$";

        if (PlayerManager.instance.IsGetAutoCulCaltorSkill)
        {
            controlDic["AutoCalculator_Button"].GetComponent<Button>().interactable = false;
            controlDic["AutoCalculator_Button"].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "成功购买";
        }
    }
}