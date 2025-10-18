using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiCardArea : MonoBehaviour
{
    //ֻ����һ�����������õ���
    private static AiCardArea _instance;
    public static AiCardArea Instance => _instance;

    [SerializeField] private televisionPanel MyPanel;
    [SerializeField] private Transform PushPos;

    public bool IsTrigger;

    private void Awake()
    {
        _instance = this;
        IsTrigger = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Card"))
        {
            IsTrigger = true;
            MyPanel.SetpushCardAreaActive(true);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Card"))
        {
            IsTrigger = false;
            MyPanel.SetpushCardAreaActive(false);
        }
    }

    public void PushCard(Card Card)//�������
    {
        Card.IsAnimator = true;//�򿪶���״̬
        Card.Push();
        HandCardManger.Instance.HandCardList.Remove(Card.gameObject);
        HandCardManger.Instance.UpdateCardPosition();
        Card.CurrentSelectedCard = null;

        //����ƺ���г�ʼ������
        MyPanel.SetpushCardAreaActive(false);
        IsTrigger = false;
        // �����������У�����ƶ������š���ת
        transform.DOKill();
        DOTween.Sequence()
            // �ƶ��������
            .Append(Card.transform.DOMove(PushPos.position, 0.8f))
            .OnComplete(() =>
            {
                //�����Ƚ�ϵͳ
                if(EnemyCard.CurrentEnemyCard.Number> Card.MyNumber.Number)
                {
                    //ʧ�ܿ�Ѫ����Ļ�ζ���
                    PlayerManager.instance.ChangeHealth(-1);
                    CameraControl.Instance.AddCameraShake(0.5f, 0.6f);
                    //����������Ȼ�󴴽�����
                    RecycleArea.Instance.RecycleCard(Card);
                    RecycleArea.Instance.RecycleObj(EnemyCard.CurrentEnemyCard.transform);
                    EnemyCard.CurrentEnemyCard= null;
                    HandCardManger.Instance.GetEnemyCard();//���»�ȡ���˿���
                }
                else
                {
                    //����Ǯ�ҽ���
                    GetGoldArea.Instance.CreateGold(10);
                    PlayerManager.instance.ChangeGold(10);
                    //����������Ȼ�󴴽�����
                    RecycleArea.Instance.RecycleCard(Card);
                    RecycleArea.Instance.RecycleObj(EnemyCard.CurrentEnemyCard.transform);
                    EnemyCard.CurrentEnemyCard = null;
                    HandCardManger.Instance.GetEnemyCard();//���»�ȡ���˿���
                }
            });
    }
}
