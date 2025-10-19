using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvanceShopPanel : BasePanel
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void ClickButton(string controlName)
    {
        base.ClickButton(controlName);
        switch (controlName)
        { 
         case "AddLevel_Button":
                //先判断当前的升级难度需要的金币是否足够
                if (PlayerManager.instance.PlayerCurrentGold >= PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel])
                {
                    //扣除金币
                    GetGoldArea.Instance.UseGoldInAdvanceShop(PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel]);//使用金币动画
                    PlayerManager.instance.ChangeGold(-PlayerManager.instance.AdvanceLevelNeedMoney[(int)PlayerManager.instance.CurrentLevel]);//扣除金币
                    PlayerManager.instance.AddGameLevel();//提升游戏难度
                }

                break;

        }

    }

    public override void Start()
    {
        base.Start();
    }
}
