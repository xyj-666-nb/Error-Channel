using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardNumber : MonoBehaviour
{
    public TextMeshProUGUI EquationText;
    public int Number;

    public void SetNumber(int _Number,string _NumberText)
    {
        Number= _Number;
        EquationText.text = _NumberText;
    }

}
