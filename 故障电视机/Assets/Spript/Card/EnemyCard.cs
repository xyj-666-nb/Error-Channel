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
    [SerializeField]private SpriteRenderer FrontSprite;
    [SerializeField] private SpriteRenderer BackSprite ;
    public CardDesignMode MyMode;
    public TextMeshProUGUI NumberText;
    public int Number=0;
    private bool IsFilp;

    private void Start()
    {
        CurrentEnemyCard = this;
        GetNumber();//获取数字
        IsFilp = true;
    }

   public void Filp()
   {
        IsFilp = !IsFilp;
        FrontSprite.transform.DOKill();

        Vector3 currentLocalRot = FrontSprite.transform.localEulerAngles;
        Vector3 targetLocalRot = new Vector3(
            currentLocalRot.x,  // 保持原X轴旋转
            IsFilp ? 0f : 180f, // 只修改Y轴
            currentLocalRot.z   // 保持原Z轴旋转
        );
        FrontSprite.transform.DORotate(targetLocalRot, 0.25f, (RotateMode)Space.Self);

        // 音效空值保护
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayEffectMusic("Music/扑克翻牌", false);
        }
   }
    public void SetSprite(Sprite Front,Sprite Back)
    {
        FrontSprite.sprite = Front;
        BackSprite.sprite = Back;
    }

    public void GetNumber()
    {
        // 原有逻辑
        CardNumberInfo.Instance.GetEnemyNumber(this);
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
