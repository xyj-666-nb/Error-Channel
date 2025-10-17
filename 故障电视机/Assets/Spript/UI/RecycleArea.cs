using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RecycleArea : MonoBehaviour
{
    //ֻ����һ�����������õ���
    private static RecycleArea _instance;
    public static RecycleArea Instance=> _instance;
    
    [SerializeField]private televisionPanel MyPanel;
    [SerializeField] private Transform RecyclePos;

    public bool IsTrigger;

    private void Awake()
    {
        _instance = this;
        IsTrigger = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Card"))
        {
            MyPanel.RecycleArea.color = Color.red;
            MyPanel.SetRecycleAreaActive(true);
            IsTrigger=true;
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Card"))
        {
            MyPanel.RecycleArea.color = Color.white;
            MyPanel.SetRecycleAreaActive(false);
            IsTrigger = false;
        }
    }

    public void RecycleCard(Card Card)//���տ���
    {
        Card.IsAnimator = true;//�򿪶���״̬
        Card.Push();
        HandCardManger.Instance.HandCardList.Remove(Card.gameObject);
        HandCardManger.Instance.UpdateCardPosition();
        Card.CurrentSelectedCard = null;

        MyPanel.RecycleArea.color = Color.white;
        MyPanel.SetRecycleAreaActive(false);
        IsTrigger = false;

        // �����������У�����ƶ������š���ת
        DOTween.Sequence()
            // �ƶ������յ�
            .Append(Card.transform.DOMove(RecyclePos.position, 0.8f))
            // ͬʱ��С��0
            .Join(Card.transform.DOScale(Vector3.zero, 0.8f))
            // ͬʱ��ת
            .Join(Card.transform.DORotate(Vector3.forward * 720f, 0.8f, RotateMode.FastBeyond360))
            // ������ɺ����ٿ���
            .OnComplete(() =>
            { 
                Destroy(Card.gameObject);
            });
    }


}
