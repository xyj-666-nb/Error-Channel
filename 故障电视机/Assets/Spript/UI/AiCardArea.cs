using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AiCardArea : MonoBehaviour
{
    // 单例实例
    private static AiCardArea _instance;
    public static AiCardArea Instance => _instance;

    [SerializeField] private televisionPanel MyPanel;
    [SerializeField] private Transform PushPos;

    private Image AreaImage;
    public bool IsTrigger;

    private int CurrentPushCardNumber = 0;
    private Card FirstPushCard;// 记录第一张打出的牌
    private void Awake()
    {
        _instance = this;
        IsTrigger = false;
        AreaImage = GetComponent<Image>();
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

    public void PushCard(Card Card)// 打出卡牌
    {
        Card.IsAnimator = true;// 打开动画状态
        HandCardManger.Instance.HandCardList.Remove(Card.gameObject);// 移除手牌
        MyPanel.SetpushCardAreaActive(false);
        IsTrigger = false;

        if (PlayerManager.instance.CurrentMaxSelectCardMount == 1)
        {
            // 单选逻辑：计算玩家最终结果（含强化）
            int playerResult = Card.MyNumber.Number;
            // 如果解锁了结果强化，加5
            if (PlayerManager.instance.CurrentPlayerStrengthenReult > 0)
            {
                playerResult += PlayerManager.instance.CurrentPlayerStrengthenReult;
            }

            Card.Push();
            HandCardManger.Instance.UpdateCardPosition();
            Card.CurrentSelectedCard_1 = null;
            DOTween.Sequence()
                .Append(Card.transform.DOMove(PushPos.position, 0.8f))
                .OnComplete(() =>
                {
                    ShowPlayerReult.Instance.SetResult(playerResult); // 显示强化后的结果
                    if (EnemyCard.CurrentEnemyCard.Number > playerResult) // 用强化后结果比较
                        PlayerManager.instance.PlayerLose(Card);
                    else
                        PlayerManager.instance.PlayerWin(Card);
                });
        }
        // 在AiCardArea.cs的双牌逻辑部分（CurrentMaxSelectCardMount == 2分支）修改如下
        else if (PlayerManager.instance.CurrentMaxSelectCardMount == 2)
        {
            Card.CanInitCar = false;// 不允许初始化卡牌
            CurrentPushCardNumber++;// 标记打出的牌数量
            transform.DOKill();

            if (CurrentPushCardNumber == 1)
                Card.Push();
            else
            {
                Card.CanInitCar = true;// 允许初始化卡牌
                Card.Push();
                Card.CurrentCanSelectCardNumber = 1;// 玩家暂时只能选1张
            }

            DOTween.Sequence()
         .Append(Card.transform.DOMove(PushPos.position, 0.8f))
         .OnComplete(() =>
         {
             if (CurrentPushCardNumber == 1)
             {
                 FirstPushCard = Card;
             }
             else
             {
                 // 获取敌人当前牌信息（花色和数值）
                 var enemyCard = EnemyCard.CurrentEnemyCard;
                 CardDesignMode enemyMode = enemyCard.MyMode;
                 int enemyNumber = enemyCard.Number; // 敌人黑桃已在数据中处理乘二，直接使用

                 // 1. 处理敌人梅花效果：玩家第二张牌变为负数
                 bool isEnemyPlum = enemyMode == CardDesignMode.BlackPlumBlossom;
                 int secondCardValue = Card.MyNumber.Number;
                 if (isEnemyPlum)
                 {
                     secondCardValue = -secondCardValue; // 敌人梅花：第二张牌变负
                 }

                 // 2. 处理玩家自身梅花效果：带有梅花的牌数据变负（与敌人梅花效果可叠加）
                 int firstCardFinal = FirstPushCard.MyDesignMode == CardDesignMode.BlackPlumBlossom
                     ? -FirstPushCard.MyNumber.Number
                     : FirstPushCard.MyNumber.Number;

                 int secondCardFinal = Card.MyDesignMode == CardDesignMode.BlackPlumBlossom
                     ? -secondCardValue // 若自身是梅花，叠加变负（先处理敌人效果再处理自身）
                     : secondCardValue;

                 // 3. 计算玩家基础总和
                 int playerTotal = firstCardFinal + secondCardFinal;

                 // 4. 处理玩家两张黑桃效果：玩家结果乘二
                 bool isPlayerTwoSpades = FirstPushCard.MyDesignMode == CardDesignMode.Spades
                     && Card.MyDesignMode == CardDesignMode.Spades;
                 if (isPlayerTwoSpades)
                 {
                     playerTotal *= 2;
                 }

                 // 5. 处理敌人红桃效果：玩家结果减百分之十（向下取整）
                 bool isEnemyRedHeart = enemyMode == CardDesignMode.RedHeart;
                 if (isEnemyRedHeart)
                 {
                     playerTotal = Mathf.FloorToInt(playerTotal * 0.9f);
                 }

                 // 6. 处理玩家两张红桃效果：敌人结果减百分之十（向下取整）
                 bool isPlayerTwoRedHearts = FirstPushCard.MyDesignMode == CardDesignMode.RedHeart
                     && Card.MyDesignMode == CardDesignMode.RedHeart;
                 int adjustedEnemyNumber = enemyNumber;
                 if (isPlayerTwoRedHearts)
                 {
                     adjustedEnemyNumber = Mathf.FloorToInt(enemyNumber * 0.9f);
                 }

                 // 7. 处理玩家结果强化（原逻辑保留）
                 if (PlayerManager.instance.CurrentPlayerStrengthenReult > 0)
                 {
                     playerTotal += PlayerManager.instance.CurrentPlayerStrengthenReult;
                 }

                 // 8. 处理玩家两张方片（菱形）效果：恢复1生命值
                 bool isPlayerTwoDiamonds = FirstPushCard.MyDesignMode == CardDesignMode.RedAngularShape
                     && Card.MyDesignMode == CardDesignMode.RedAngularShape;
                 if (isPlayerTwoDiamonds)
                 {
                     PlayerManager.instance.ChangeHealth(1); 
                 }

                 // 显示玩家最终结果
                 ShowPlayerReult.Instance.SetResult(playerTotal);

                 // 9. 比较结果并处理胜负
                 bool isPlayerWin = playerTotal >= adjustedEnemyNumber;
                 if (isPlayerWin)
                 {
                     PlayerManager.instance.PlayerWin(FirstPushCard, Card);
                 }
                 else
                 {
                     // 10. 处理敌人方片（菱形）效果：失败额外扣1血
                     bool isEnemyDiamond = enemyMode == CardDesignMode.RedAngularShape;
                     PlayerManager.instance.PlayerLose(FirstPushCard, Card, isEnemyDiamond ? 1 : 0);
                     // 需修改PlayerLose方法，增加额外扣血参数
                 }

                 // 重置状态
                 HandCardManger.Instance.UpdateCardPosition();
                 CurrentPushCardNumber = 0;
                 FirstPushCard = null;
                 Card.CurrentCanSelectCardNumber = 2;// 恢复双选能力
             }
         });
        }
    }

    private void Update()
    {
        AreaImage.color = Color.white;
    }
}