using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameLevel
{
    Level1,
    Level2,
    Level3,
    Level4,
    Level5
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;//单例
    public PlayerManager Instance=> instance;//玩家控制器

    //玩家管理器
    public int CurrentHealth=10;//玩家当前血量
    public int MaxHealth = 10;//玩家最大血量
    public int PlayerCurrentGold=0;//玩家当前金币数量
    public int PlayerParts = 0;//玩家当前修复零件数量
    public float CurrentFixProcess = 0f;//当前修复进度
    public int MaxCardAmount = 5;//最大卡牌数量
    public GameLevel CurrentLevel = GameLevel.Level1;//当前关卡

    public bool IsObtainCalculatorSkill = false;//是否获得计算技能
    public bool IsObtainShowGoldSkill = true;//是否获得金币显示
    public bool IsObtainExtraCardSkill = false;//是否获得额外卡牌技能

    [SerializeField]private List<int> LevelCangetGoldList = new List<int>() { 1, 5, 10, 25, 50 };//每个难度可获得的金币数量
    public List<int> AdvanceLevelNeedMoney=new List<int>() { 20, 100, 250, 500 };//升级所需金币数量

    public int CurrentLevelCanGetGold = 1;//当前难度可获得金币数量

    public int CurrenWinningStreak = 0;//当前连胜数

    public void Awake()
    {
        if (instance == null)
            instance = this;

        CurrentHealth= MaxHealth;//初始化血量
        CurrenWinningStreak = 0;
        //设置等级
    }

    //玩家胜利函数
    public void PlayerWin(Card Card)
    {
        //增加连胜数
        CurrenWinningStreak++;
        if (CurrenWinningStreak >= 3)//判断零件嘉奖规则
            AddPart();//三连胜获得零件,之后每胜利获得一个零件

        //发放钱币奖励
        GetGoldArea.Instance.CreateGold(CurrentLevelCanGetGold);
        PlayerManager.instance.ChangeGold(CurrentLevelCanGetGold);
        //回收两张牌然后创建新牌
        RecycleCurrentPushCard(Card);
    }


    private void RecycleCurrentPushCard(Card Card)
    {
        RecycleArea.Instance.RecycleCard(Card);
        RecycleArea.Instance.RecycleObj(EnemyCard.CurrentEnemyCard.transform);
        EnemyCard.CurrentEnemyCard = null;
        HandCardManger.Instance.GetEnemyCard();//重新获取敌人卡牌

    }

    //玩家失败函数
    public void PlayerLose(Card Card)
    {
        CurrenWinningStreak = 0;//失败连胜数归零
        //扣两滴血
        ChangeHealth(-2);
        RecycleArea.Instance.RecycleCard(Card);
        //失败屏幕晃动！
        // CameraControl.Instance.AddCameraShake(0.5f, 0.6f);
        //回收两张牌然后创建新牌
    }

  
    public void AddPart()
    {
        PlayerParts++;
        //可能后面要做更新ui啊和播放动画子类的
    }

    public void ChangeGold(int Value)
    {
        PlayerCurrentGold += Value;
        //更新金币显示
        UI_ShowGold.Instance.UpdateGold(PlayerCurrentGold);
    }

    public void SetObtainCalculatorSkill()
    {
        IsObtainCalculatorSkill = true;
       //自动计算
    }

    public void ChangeHealth(int value)
    {
        CurrentHealth += value;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
        else if (CurrentHealth < 0)
            CurrentHealth = 0;
        UI_healthslider.instance.UpdateHeathBar(CurrentHealth, MaxHealth);
    }


    public void AddGameLevel()
    {
        CurrentLevel++;
        UImanager.Instance.GetPanel<televisionPanel>().ChangeLevelText(CurrentLevel);
        //刷新一些数据，加载一些根据等级改变而改变的数据
        CardNumberInfo.Instance.ChangeLevelData(CurrentLevel);//改变数据的来源
        //刷新一下手牌数据
        ReFreshCardNumber();
        //当前胜利的金币获取数
        CurrentLevelCanGetGold = LevelCangetGoldList[(int)CurrentLevel];
    }

    public void ReFreshCardNumber()
    {
        foreach (var card in HandCardManger.Instance.HandCardList)
        {
            card.GetComponent<Card>().RefreshData();//刷新玩家手牌数据
        }

        //敌人数据刷新
        EnemyCard.CurrentEnemyCard.RefreshCard();
    }

}
