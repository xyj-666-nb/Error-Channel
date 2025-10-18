using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyCard : MonoBehaviour
{
    public static EnemyCard CurrentEnemyCard;
    //���˿���
    [SerializeField]private SpriteRenderer BackGround;
    [SerializeField] private SpriteRenderer ContentImage;
    [SerializeField] private TextMeshProUGUI NumberText;
    public int Number=0;

    private void Start()
    {
        CurrentEnemyCard = this;
        GetNumber();//��ȡ����
    }

    public void GetNumber()
    {
        Number= CardNumberInfo.Instance.GetEnemyNumber();
        NumberText.text = Number.ToString();
    }

}
