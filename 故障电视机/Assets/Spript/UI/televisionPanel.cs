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
            case "PushCardButton":
                HandCardManger.Instance.PushCard(PushType.track);//�������
                break;
                case "AdvanceButton":
                if (UImanager.Instance.GetPanel<AdvanceShopPanel>())
                {
                    controlDic["AdvanceButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Advance Shop";
                    UImanager.Instance.HidePanel<AdvanceShopPanel>();//���ظ߼��̵�
                }
                else
                {
                    controlDic["AdvanceButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
                    if (UImanager.Instance.GetPanel<FixShopPanel>())
                        UImanager.Instance.HidePanel<FixShopPanel>();//���ظ߼��̵�
                    UImanager.Instance.ShowPanel<AdvanceShopPanel>();//��ʾ�߼��̵�
                }
               
                break;
            case "FixShopButton":
                if (UImanager.Instance.GetPanel<FixShopPanel>())
                {
                    controlDic["FixShopButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Fix Shop";
                    UImanager.Instance.HidePanel<FixShopPanel>();//���ظ߼��̵�
                }
                else
                {
                    controlDic["FixShopButton"].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
                    if(UImanager.Instance.GetPanel<AdvanceShopPanel>())
                        UImanager.Instance.HidePanel<AdvanceShopPanel>();//���ظ߼��̵�
                    UImanager.Instance.ShowPanel<FixShopPanel>();//��ʾ�߼��̵�
                }
                break;
            case "ExitButton":
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
