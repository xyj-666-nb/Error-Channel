using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class televisionPanel : BasePanel
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
            case "GetCardButton":
                HandCardManger.Instance.CreatCard();//��������
                break;
        
        }

    }

    public override void HideMe(UnityAction callback)
    {
        base.HideMe(callback);
    }

    public override void ShowMe(bool IsNeedDefalutAnimator = true)//Ҫ����Ҫ���⶯���͸�false,Ȼ���������ı�ʶ������ʱ���ڶ���������������ã�
    {
        base.ShowMe(IsNeedDefalutAnimator);
    }

    public override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    //�ṩ���ⲿһ�����Ƴ��ư�ť�Ľӿ�
    public void SetGetCardButtonState(bool IsCanUse)
    {
       Button GetCardButton=controlDic["GetCardButton"]  as Button;
        GetCardButton.interactable = IsCanUse;//���ð�ť�Ƿ���Ե��
        //���ð�ť����ɫ
        Color TargetColor = IsCanUse ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0f);
        transform.DOKill();//��ֹ֮ͣǰ�Ķ���
        GetCardButton.GetComponent<Image>().DOColor(TargetColor,0.5f);//������ɫ
        GetCardButton.GetComponentInChildren<TextMeshProUGUI>().DOColor(TargetColor, 0.5f);//������ɫ
    }
}
