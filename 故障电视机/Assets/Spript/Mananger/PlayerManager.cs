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
    public static PlayerManager instance;//����
    public PlayerManager Instance=> instance;//��ҿ�����

    //��ҹ�����
    public int CurrentHealth=10;//��ҵ�ǰѪ��
    public int MaxHealth = 10;//������Ѫ��
    public int PlayerCurrentGold=0;
    public GameLevel CurrentLevel = GameLevel.Level1;//��ǰ�ؿ�

    public bool IsObtainCalculatorSkill = false;//�Ƿ��ü��㼼��
    public bool IsObtainShowGoldSkill = true;//�Ƿ��ý����ʾ

    public void Awake()
    {
        if (instance == null)
            instance = this;

        CurrentHealth= MaxHealth;//��ʼ��Ѫ��
    }

    public void ChangeGold(int Value)
    {
        PlayerCurrentGold += Value;
        //���½����ʾ
        UI_ShowGold.Instance.UpdateGold(PlayerCurrentGold);
    }

    public void SetObtainCalculatorSkill()
    {
        IsObtainCalculatorSkill = true;
        foreach(var Card in HandCardManger.Instance.HandCardList)
        {
            Card.GetComponent<Card>().MyNumber.UpdateCardNumber();//���ø���
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
