using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class PassPromptText : MonoBehaviour
{
    private static PassPromptText instance;
    public static PassPromptText Instance=> instance;

    [SerializeField] private TextMeshProUGUI PromptText;//换牌提示文本

    private bool IsFree;//是否免费
    private void Awake()
    {
        instance = this;
        PromptText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        InitPassText();
    }

    public void InitPassText()
    {
        IsFree = true;//每轮第一次免费
        PromptText.text = "免费";
    }

    public void TriggerPass()//触发换牌
    {
        if (!IsFree)//第一次免费跳过
            PlayerManager.instance.ChangeHealth(-1);
        PromptText.text = "消耗1滴血";
        IsFree = false;
    }
}
