using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiCardArea : MonoBehaviour
{
    //只有这一个回收区域用单例
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

    public void PushCard(Card Card)//打出卡牌
    {
        Card.IsAnimator = true;//打开动画状态
        Card.Push();
        HandCardManger.Instance.HandCardList.Remove(Card.gameObject);
        HandCardManger.Instance.UpdateCardPosition();
        Card.CurrentSelectedCard = null;

        //打出牌后进行初始化区域
        MyPanel.SetpushCardAreaActive(false);
        IsTrigger = false;
        // 创建动画序列，组合移动、缩放、旋转
        transform.DOKill();
        DOTween.Sequence()
            // 移动到打出点
            .Append(Card.transform.DOMove(PushPos.position, 0.8f))
            .OnComplete(() =>
            {
                //触发比较系统
                if(EnemyCard.CurrentEnemyCard.Number> Card.MyNumber.Number)
                {
                    //失败扣血加屏幕晃动！
                    PlayerManager.instance.ChangeHealth(-1);
                    CameraControl.Instance.AddCameraShake(0.5f, 0.6f);
                    //回收两张牌然后创建新牌
                    RecycleArea.Instance.RecycleCard(Card);
                    RecycleArea.Instance.RecycleObj(EnemyCard.CurrentEnemyCard.transform);
                    EnemyCard.CurrentEnemyCard= null;
                    HandCardManger.Instance.GetEnemyCard();//重新获取敌人卡牌
                }
                else
                {
                    //发放钱币奖励
                    GetGoldArea.Instance.CreateGold(10);
                    PlayerManager.instance.ChangeGold(10);
                    //回收两张牌然后创建新牌
                    RecycleArea.Instance.RecycleCard(Card);
                    RecycleArea.Instance.RecycleObj(EnemyCard.CurrentEnemyCard.transform);
                    EnemyCard.CurrentEnemyCard = null;
                    HandCardManger.Instance.GetEnemyCard();//重新获取敌人卡牌
                }
            });
    }
}
