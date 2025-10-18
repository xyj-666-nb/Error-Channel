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
    public int PlayerCurrentGold=0;
    public GameLevel CurrentLevel = GameLevel.Level1;//当前关卡

    public bool IsObtainCalculatorSkill = false;//是否获得计算技能
    public bool IsObtainShowGoldSkill = true;//是否获得金币显示

    public void Awake()
    {
        if (instance == null)
            instance = this;

        CurrentHealth= MaxHealth;//初始化血量
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
        foreach(var Card in HandCardManger.Instance.HandCardList)
        {
            Card.GetComponent<Card>().MyNumber.UpdateCardNumber();//调用更新
        }
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


}
