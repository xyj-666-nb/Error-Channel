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
            // 单选逻辑（保持不变）
            int playerResult = Card.MyNumber.Number;
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
                    ShowPlayerReult.Instance.SetResult(playerResult);
                    if (EnemyCard.CurrentEnemyCard.Number > playerResult)
                        PlayerManager.instance.PlayerLose(Card);
                    else
                        PlayerManager.instance.PlayerWin(Card);
                });
        }
        else if (PlayerManager.instance.CurrentMaxSelectCardMount == 2)
        {
            Card.CanInitCar = false;
            CurrentPushCardNumber++;
            transform.DOKill();

            if (CurrentPushCardNumber == 1)
                Card.Push();
            else
            {
                Card.CanInitCar = true;
                Card.Push();
                Card.CurrentCanSelectCardNumber = 1;
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
                 // 获取敌人卡牌信息
                 var enemyCard = EnemyCard.CurrentEnemyCard;
                 CardDesignMode enemyMode = enemyCard.MyMode;
                 int enemyNumber = enemyCard.Number;

                 // 核心：判断是否开启花色计算（IsCanDesignModeCalculator为true时才计算花色）
                 bool isDesignModeEnabled = PlayerManager.instance.IsCanDesignModeCalculator;
                 int playerTotal;
                 int adjustedEnemyNumber = enemyNumber; // 默认使用敌人原始数值

                 if (!isDesignModeEnabled)
                 {
                     // 1. 未激活：仅原始数值相加（无花色效果）
                     playerTotal = FirstPushCard.MyNumber.Number + Card.MyNumber.Number;
                     // 保留结果强化
                     if (PlayerManager.instance.CurrentPlayerStrengthenReult > 0)
                     {
                         playerTotal += PlayerManager.instance.CurrentPlayerStrengthenReult;
                     }
                 }
                 else
                 {
                     // 2. 已激活：执行完整花色计算
                     // 处理敌人梅花效果：玩家第二张牌变负
                     bool isEnemyPlum = enemyMode == CardDesignMode.BlackPlumBlossom;
                     int secondCardValue = Card.MyNumber.Number;
                     if (isEnemyPlum)
                     {
                         secondCardValue = -secondCardValue;
                     }

                     // 处理玩家自身梅花效果：叠加变负
                     int firstCardFinal = FirstPushCard.MyDesignMode == CardDesignMode.BlackPlumBlossom
                         ? -FirstPushCard.MyNumber.Number
                         : FirstPushCard.MyNumber.Number;

                     int secondCardFinal = Card.MyDesignMode == CardDesignMode.BlackPlumBlossom
                         ? -secondCardValue
                         : secondCardValue;

                     // 基础总和
                     playerTotal = firstCardFinal + secondCardFinal;

                     // 玩家两张黑桃：结果乘二
                     bool isPlayerTwoSpades = FirstPushCard.MyDesignMode == CardDesignMode.Spades
                         && Card.MyDesignMode == CardDesignMode.Spades;
                     if (isPlayerTwoSpades)
                     {
                         playerTotal *= 2;
                     }

                     // 敌人红桃：玩家结果减10%
                     bool isEnemyRedHeart = enemyMode == CardDesignMode.RedHeart;
                     if (isEnemyRedHeart)
                     {
                         playerTotal = Mathf.FloorToInt(playerTotal * 0.9f);
                     }

                     // 玩家两张红桃：敌人结果减10%
                     bool isPlayerTwoRedHearts = FirstPushCard.MyDesignMode == CardDesignMode.RedHeart
                         && Card.MyDesignMode == CardDesignMode.RedHeart;
                     if (isPlayerTwoRedHearts)
                     {
                         adjustedEnemyNumber = Mathf.FloorToInt(enemyNumber * 0.9f);
                     }

                     // 结果强化
                     if (PlayerManager.instance.CurrentPlayerStrengthenReult > 0)
                     {
                         playerTotal += PlayerManager.instance.CurrentPlayerStrengthenReult;
                     }

                     // 玩家两张方片：恢复1生命值
                     bool isPlayerTwoDiamonds = FirstPushCard.MyDesignMode == CardDesignMode.RedAngularShape
                         && Card.MyDesignMode == CardDesignMode.RedAngularShape;
                     if (isPlayerTwoDiamonds)
                     {
                         PlayerManager.instance.ChangeHealth(1);
                     }
                 }

                 // 显示结果并判断胜负（共用逻辑）
                 ShowPlayerReult.Instance.SetResult(playerTotal);
                 bool isPlayerWin = playerTotal >= adjustedEnemyNumber;

                 if (isPlayerWin)
                 {
                     PlayerManager.instance.PlayerWin(FirstPushCard, Card);
                 }
                 else
                 {
                     // 敌人方片效果：失败额外扣1血（仅在花色计算开启时生效）
                     int extraDamage = isDesignModeEnabled && enemyMode == CardDesignMode.RedAngularShape ? 1 : 0;
                     PlayerManager.instance.PlayerLose(FirstPushCard, Card, extraDamage);
                 }

                 // 重置状态
                 HandCardManger.Instance.UpdateCardPosition();
                 CurrentPushCardNumber = 0;
                 FirstPushCard = null;
                 Card.CurrentCanSelectCardNumber = 2;
             }
         });
        }
    }

    private void Update()
    {
        AreaImage.color = Color.white;
    }
}