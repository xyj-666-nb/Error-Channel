using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RecycleArea : MonoBehaviour
{
    //只有这一个回收区域用单例
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

    public void RecycleCard(Card Card)//回收卡牌
    {
        Card.IsAnimator = true;//打开动画状态
        Card.Push();
        HandCardManger.Instance.HandCardList.Remove(Card.gameObject);
        HandCardManger.Instance.UpdateCardPosition();
        Card.CurrentSelectedCard = null;

        MyPanel.RecycleArea.color = Color.white;
        MyPanel.SetRecycleAreaActive(false);
        IsTrigger = false;

        RecycleObj(Card.transform);//回收对象
    }


    public void RecycleObj(Transform Obj)
    {
        // 创建动画序列，组合移动、缩放、旋转
        transform.DOKill();
        DOTween.Sequence()
            // 移动到回收点 
            .Append(Obj.DOMove(RecyclePos.position, 0.8f))
            // 同时缩小到0
            .Join(Obj.DOScale(Vector3.zero, 0.8f))
            // 同时旋转
            .Join(Obj.DORotate(Vector3.forward * 720f, 0.8f, RotateMode.FastBeyond360))
            // 动画完成后销毁卡牌
            .OnComplete(() =>
            {
                Destroy(Obj.gameObject);
            });
    }


}
