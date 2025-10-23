using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyCard : MonoBehaviour
{
    public static EnemyCard CurrentEnemyCard;
    //敌人卡牌
    [SerializeField]private SpriteRenderer BackGround;
    public TextMeshProUGUI NumberText;
    public int Number=0;

    private void Start()
    {
        CurrentEnemyCard = this;
        GetNumber();//获取数字
    }

    public void GetNumber()
    {
        CardNumberInfo.Instance.GetEnemyNumber(this);//获取敌人数字
    }

    public  void SetHideOrShowCurrentCard(bool IsShow)
    {
        this.gameObject.SetActive(IsShow);
    }


    public void RefreshCard()
    {
        //刷新当前的卡牌数据
        Color color = NumberText.color;
        DOTween.Sequence().Append(NumberText.DOFade(0, 0.3f)).OnComplete(
        () => {
            CardNumberInfo.Instance.GetEnemyNumber(this);//重新刷新当前数据
            NumberText.DOColor(color, 0.3f);
        });
    }

}
