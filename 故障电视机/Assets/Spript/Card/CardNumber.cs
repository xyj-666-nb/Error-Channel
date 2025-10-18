using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardNumber : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI EquationText;
    public int Number;
    [SerializeField] private TextMeshProUGUI NumberText;

    public void SetNumber(int _Number,string _NumberText)
    {
        Number= _Number;
        EquationText.text = _NumberText;
        UpdateCardNumber();
    }

    public void UpdateCardNumber()
    {
        //更新卡牌上的数字显示
        if (PlayerManager.instance.IsObtainCalculatorSkill)
        {
            NumberText.text = Number.ToString();
        }
        else
        {
            NumberText.text = "?";
        }
    }
}
