using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public PlayerManager Instance => instance;

    // 玩家基础属性
    public int CurrentHealth = 10;
    public int MaxHealth = 10;
    public int PlayerCurrentGold = 0;
    public int PlayerParts = 0;
    public float CurrentFixProcess = 0f;
    public int MaxCardAmount = 5;
    public GameLevel CurrentLevel = GameLevel.Level1;

    // 修复相关
    public bool IsObtainShowPart = true;
    public int IsFixShowGold_NeedPart = 10;
    public int FixPart_NeedParts = 10;
    public bool IsFixExitButton = false;
    public int IsFixExitButton_NeedParts = 20;
    public bool IsObtainShowGoldSkill = true;

    // 升级相关
    public int AvancedCardAmount_NeedMoney = 100;
    public bool IsCanAdvanceCardAmount = true;
    public int CurrentAdvanceCardAmount = 0;
    public bool IsGetAutoCulCaltorSkill;
    public int GetAutoCulCaltorNeedMoney = 500;

    // 结果强化系统 - 修复部分
    public List<bool> IsGetStrengthenReultPerLevel = new List<bool>() { false, false, false, false, false };
    public List<int> GetStrengthenReultList_NeedMoney = new List<int>() { 30, 50, 100, 150, 300 };
    public List<int> GetStrengthenReultList = new List<int>() { 10, 60, 100, 150, 200 };
    public int RealHealthNeedMoney = 50;
    public int CurrentPlayerStrengthenReult;

    [SerializeField] private List<int> LevelCangetGoldList = new List<int>() { 1, 5, 10, 25, 50 };
    public List<int> AdvanceLevelNeedMoney = new List<int> {
        100,  // Level1
        200,  // Level2  
        300,  // Level3
        400,  // Level4
        500   // Level5 
    };
    public int CurrentLevelCanGetGold = 1;
    public int CurrenWinningStreak = 0;

    public bool IsStartShowPrompttext_Gold = false;
    public int StartShowPrompttext_GoldneedPart = 5;
    public bool IsStartShowPrompttext_part = false;
    public int StartShowPrompttext_PartneedPart = 5;
    public bool IsFixShowPlayerHealth = false;
    public int FixShowPlayerHealthNeedPart = 5;
    public bool FixCcenceEffector = false;
    public int FixCcenceEffector_NeedPart = 10;
    public bool FixVolume = false;
    public int FixVolumeNeedPart = 5;
    public int CurrentMaxSelectCardMount = 1;
    public bool IsCanDesignModeCalculator = false;

    private bool FirstTriggerPeomptHealth = true;

    public void Awake()
    {
        if (instance == null)
            instance = this;

        CurrentHealth = MaxHealth;
        CurrenWinningStreak = 0;
        CurrentAdvanceCardAmount = 0;
        CurrentPlayerStrengthenReult = 0;

        // 初始化结果强化状态
        InitializeStrengthenResults();
    }

    /// <summary>
    /// 初始化结果强化状态
    /// </summary>
    private void InitializeStrengthenResults()
    {
        // 确保列表有正确的长度
        if (IsGetStrengthenReultPerLevel.Count < 5)
        {
            IsGetStrengthenReultPerLevel = new List<bool>() { false, false, false, false, false };
        }
        if (GetStrengthenReultList_NeedMoney.Count < 5)
        {
            GetStrengthenReultList_NeedMoney = new List<int>() { 30, 50, 100, 150, 300 };
        }
        if (GetStrengthenReultList.Count < 5)
        {
            GetStrengthenReultList = new List<int>() { 10, 60, 100, 150, 200 };
        }
    }

    public void ActiveAutoButton()
    {
        IsGetAutoCulCaltorSkill = true;
        UImanager.Instance.GetPanel<televisionPanel>().SetCallAutoButtonActive(true);
    }

    // 玩家胜利函数
    public void PlayerWin(Card Card, Card Card2 = null)
    {
        CurrenWinningStreak++;
        switch (CurrenWinningStreak)
        {
            case 1:
                Slider_Part.instance.UpdateGetProcess(0.3f);
                break;
            case 2:
                Slider_Part.instance.UpdateGetProcess(0.7f);
                break;
        }

        if (CurrenWinningStreak >= 3)
        {
            Slider_Part.instance.UpdateGetProcess(1f);
            AddPart();
            Slider_Part.instance.GetPart(1);
            UI_ShowPart.Instance.UpdatePartText();
            MusicManager.Instance.PlayEffectMusic("Music/成功收集到零件", false);
        }

        MusicManager.Instance.PlayEffectMusic("Music/对比成功", false);
        GetGoldArea.Instance.CreateGold(CurrentLevelCanGetGold);
        PlayerManager.instance.ChangeGold(CurrentLevelCanGetGold);
        RecycleCurrentPushCard(Card);
        if (CurrentMaxSelectCardMount == 2 && Card2 != null)
            RecycleArea.Instance.RecycleCard(Card2);
    }

    private void RecycleCurrentPushCard(Card Card)
    {
        RecycleArea.Instance.RecycleCard(Card);
        RecycleArea.Instance.RecycleObj(EnemyCard.CurrentEnemyCard.transform);
        EnemyCard.CurrentEnemyCard = null;
        HandCardManger.Instance.GetEnemyCard();
    }

    // 玩家失败函数
    public void PlayerLose(Card Card, Card Card2 = null)
    {
        CurrenWinningStreak = 0;
        Slider_Part.instance.UpdateGetProcess(0);
        ChangeHealth(-2);
        RecycleCurrentPushCard(Card);
        if (CurrentMaxSelectCardMount == 2 && Card2 != null)
            RecycleArea.Instance.RecycleCard(Card2);
        Debug.Log("触发失败震动");
        CameraControl.Instance.AddCameraShake(0.5f, 0.6f);
        MusicManager.Instance.PlayEffectMusic("Music/玩家失败错误", false);
    }

    public void AddPart()
    {
        PlayerParts++;
    }

    public void ChangeGold(int Value)
    {
        PlayerCurrentGold += Value;
        UI_ShowGold.Instance.UpdateGold(PlayerCurrentGold);
        if (Value < 0)
            UI_ShowGold.Instance.SetPromptText(false, Value);
        else
            UI_ShowGold.Instance.SetPromptText(true, Value);
    }

    public void ChangeHealth(int value)
    {
        CurrentHealth += value;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
        else if (CurrentHealth < 0)
        {
            CurrentHealth = 0;
            Main.Instance.InitDia.StartDialogue(11);
        }
        UI_healthslider.instance.UpdateHeathBar();

        if (CurrentHealth == 2 && FirstTriggerPeomptHealth)
        {
            FirstTriggerPeomptHealth = false;
            Main.Instance.InitDia.StartDialogue(7);
        }
    }

    public void AddGameLevel()
    {
        CurrentLevel++;
        RefreshLevel();
        ReFreshCardNumber();

        if (UImanager.Instance.GetPanel<AdvanceShopPanel>())
        {
            UImanager.Instance.GetPanel<AdvanceShopPanel>().SetAddCardAmount_Button_interactable();
        }

        if (CurrentLevel == GameLevel.Level3)
        {
            CurrentMaxSelectCardMount = 2;
            Main.Instance.InitDia.StartDialogue(5);
        }
        else if (CurrentLevel == GameLevel.Level5)
        {
            IsCanDesignModeCalculator = true;
            Main.Instance.InitDia.StartDialogue(6);
        }

    }

    public void RefreshLevel()
    {
        UImanager.Instance.GetPanel<televisionPanel>().ChangeLevelText(CurrentLevel);
        CardNumberInfo.Instance.ChangeLevelData(CurrentLevel);
        CurrentLevelCanGetGold = LevelCangetGoldList[(int)CurrentLevel];
    }

    public void ReFreshCardNumber()
    {
        // 关键修改：创建手牌列表的副本进行遍历，避免枚举时修改原集合
        if (HandCardManger.Instance != null && RecycleArea.Instance != null)
        {
            var handCardsCopy = HandCardManger.Instance.HandCardList.ToArray();
            foreach (var card in handCardsCopy)
            {
                RecycleArea.Instance.RecycleCard(card.GetComponentInChildren<Card>());
            }

            // 清空手牌列表，因为所有卡牌都已被回收
            HandCardManger.Instance.HandCardList.Clear();
        }

        RecycleArea.Instance.RecycleObj(EnemyCard.CurrentEnemyCard.transform);
        EnemyCard.CurrentEnemyCard = null;
        HandCardManger.Instance.GetEnemyCard();

        // 重新初始化所有卡牌
        if (HandCardManger.Instance != null)
        {
            HandCardManger.Instance.InitCard();
        }
    }

    /// <summary>
    /// 检查当前等级是否已经购买了结果强化
    /// </summary>
    public bool HasPurchasedStrengthenForCurrentLevel()
    {
        int currentLevelIndex = (int)CurrentLevel;
        if (currentLevelIndex >= 0 && currentLevelIndex < IsGetStrengthenReultPerLevel.Count)
        {
            return IsGetStrengthenReultPerLevel[currentLevelIndex];
        }
        return false;
    }

    public void PlayerLose(Card Card, Card Card2, int extraDamage)
    {
        CurrenWinningStreak = 0;
        Slider_Part.instance.UpdateGetProcess(0);
        // 基础扣2血 + 额外扣血（敌人方片效果）
        ChangeHealth(-(2 + extraDamage));
        RecycleCurrentPushCard(Card);
        if (CurrentMaxSelectCardMount == 2 && Card2 != null)
            RecycleArea.Instance.RecycleCard(Card2);
        Debug.Log("触发失败震动");
        CameraControl.Instance.AddCameraShake(0.5f, 0.6f);
        MusicManager.Instance.PlayEffectMusic("Music/玩家失败错误", false);
    }

    /// <summary>
    /// 购买当前等级的结果强化
    /// </summary>
    public void PurchaseStrengthenForCurrentLevel()
    {
        int currentLevelIndex = (int)CurrentLevel;
        if (currentLevelIndex >= 0 && currentLevelIndex < IsGetStrengthenReultPerLevel.Count)
        {
            IsGetStrengthenReultPerLevel[currentLevelIndex] = true;
            CurrentPlayerStrengthenReult += GetStrengthenReultList[currentLevelIndex];
            Debug.Log($"结果强化购买成功，等级{currentLevelIndex}，强化值+{GetStrengthenReultList[currentLevelIndex]}，总强化值：{CurrentPlayerStrengthenReult}");
        }
    }

    /// <summary>
    /// 获取当前等级的结果强化价格
    /// </summary>
    public int GetCurrentLevelStrengthenPrice()
    {
        int currentLevelIndex = (int)CurrentLevel;
        if (currentLevelIndex >= 0 && currentLevelIndex < GetStrengthenReultList_NeedMoney.Count)
        {
            return GetStrengthenReultList_NeedMoney[currentLevelIndex];
        }
        return 0;
    }

    /// <summary>
    /// 检查是否所有等级的结果强化都已购买
    /// </summary>
    public bool IsAllStrengthenPurchased()
    {
        foreach (bool purchased in IsGetStrengthenReultPerLevel)
        {
            if (!purchased) return false;
        }
        return true;
    }
}