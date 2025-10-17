using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;//����
    public PlayerManager Instance=> instance;//��ҿ�����

    //��ҹ�����
    public int CurrentHealth=10;//��ҵ�ǰѪ��
    public int MaxHealth = 10;//������Ѫ��

    public void Awake()
    {
        if (instance == null)
            instance = this;

        CurrentHealth= MaxHealth;//��ʼ��Ѫ��
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
