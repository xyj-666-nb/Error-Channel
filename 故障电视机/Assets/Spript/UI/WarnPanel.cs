using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WarnPanel : BasePanel
{
    [SerializeField] private TextMeshProUGUI TopicText;//警告的主题
    [SerializeField] private TextMeshProUGUI ContentText;//警告的内容

    //警告面板
    public override void Awake()
    {
        base.Awake();
    }

    public override void ClickButton(string controlName)
    {
        base.ClickButton(controlName);
        if(controlName=="CertainButton")
        {
            //点击确认按钮
            UImanager.Instance.HidePanel<WarnPanel>();//隐藏警告面板
        }
    }

    public void SetText(string Topic,string Content)
    {
        TopicText.text = Topic;
        ContentText.text = Content;
    }

    public override void HideMe(UnityAction callback)
    {
        foreach (var UI in UImanager.Instance.GetPanel<televisionPanel>().controlDic.Values)
        {
            if (UI is Button button)
            {
                button.interactable = true;
            }

        }
        //显示一下敌人卡牌
        EnemyCard.CurrentEnemyCard.SetHideOrShowCurrentCard(true);
        //关闭时间暂停
        Time.timeScale = 1f;
        base.HideMe(callback);
    }

    public override void ShowMe(bool IsNeedDefalutAnimator = true)
    {
        //先隐藏一下敌人卡牌
        EnemyCard.CurrentEnemyCard.SetHideOrShowCurrentCard(false);
        //打开时间暂停
        Time.timeScale = 0f;
        //获取主面板上面的所有按钮把交互改为不交互
        foreach(var UI in UImanager.Instance.GetPanel<televisionPanel>().controlDic.Values)
        {
            if(UI is Button button)
            {
                button.interactable = false;
            }

        }
        base.ShowMe(IsNeedDefalutAnimator);
    }

    public override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }
}
