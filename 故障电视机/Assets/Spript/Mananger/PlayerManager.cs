using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;//单例
    public PlayerManager Instance=> instance;//玩家控制器

    //玩家管理器
    public int CurrentHealth=10;//玩家当前血量
    public int MaxHealth = 10;//玩家最大血量

    public void Awake()
    {
        if (instance == null)
            instance = this;

        CurrentHealth= MaxHealth;//初始化血量
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
