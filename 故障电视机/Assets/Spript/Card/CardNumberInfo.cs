
using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class NumberInfo
{
   public int Number;//数字
   public string EquationString;//等式字符串
}
[System.Serializable]
public class levelNumberInfo
{
  public List<NumberInfo> PlayerNumberInfo;
  public List<NumberInfo> EnemyNumberInfo;
}


public class CardNumberInfo : MonoBehaviour
{
    //单例
    private static CardNumberInfo instance;
    public static CardNumberInfo Instance => instance;

    //4种不同等级的数字信息列表
    [SerializeField] private levelNumberInfo Level1Info;
    [SerializeField] private levelNumberInfo Level2Info;
    [SerializeField] private levelNumberInfo Level3Info;
    [SerializeField] private levelNumberInfo Level4Info;
    [SerializeField] private levelNumberInfo Level5Info;

    [SerializeField] private levelNumberInfo CurrentLeveInfo;

    private List<int> AlreadyUsedIndexList_player = new List<int>();//防止重复
    private List<int> AlreadyUsedIndexList_enemy = new List<int>();//防止重复
    private float repeatProbability = 0.2f;//重复获取一个值的概率

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ChangeLevelData(GameLevel.Level1);
    }

    //根据关卡和数字获取等式字符串
    public void GetEquationString(Card card)
    {

        var Index = GetIndex(CurrentLeveInfo.PlayerNumberInfo, AlreadyUsedIndexList_player);
        card.MyNumber.SetNumber(CurrentLeveInfo.PlayerNumberInfo[Index].Number, CurrentLeveInfo.PlayerNumberInfo[Index].EquationString);//设置一下卡片
    }

    public void GetEnemyNumber(EnemyCard Card) 
    {
        var Index = GetIndex(CurrentLeveInfo.EnemyNumberInfo, AlreadyUsedIndexList_enemy);
        Card.Number = CurrentLeveInfo.EnemyNumberInfo[Index].Number;
        Card.NumberText.text = CurrentLeveInfo.EnemyNumberInfo[Index].EquationString.ToString();
    }
    public void ChangeLevelData(GameLevel Level)
    {
        switch(Level)
        {
            case GameLevel.Level1:
                CurrentLeveInfo = Level1Info;
                break;
            case GameLevel.Level2:
                CurrentLeveInfo = Level2Info;
                break;
            case GameLevel.Level3:
                CurrentLeveInfo = Level3Info;
                break;
            case GameLevel.Level4:
                CurrentLeveInfo= Level4Info;
                break;
            case GameLevel.Level5:
                CurrentLeveInfo = Level5Info;
                break;
        }
    }

    private int GetIndex(List<NumberInfo> list,List<int> _AlreadyUsedIndexList)
    {
        if (_AlreadyUsedIndexList.Count >= list.Count)
            _AlreadyUsedIndexList.Clear();

        int index;
        index = Random.Range(0, list.Count);

        if (_AlreadyUsedIndexList.Contains(index))
        {
            if (Random.value < repeatProbability)
            {
                _AlreadyUsedIndexList.Remove(index); // 移除“已用标记”，避免下次判定为已用
                return index;
            }
            else
            {
                do
                {
                    index = Random.Range(0, list.Count);
                } while (_AlreadyUsedIndexList.Contains(index));
            }
        }

        _AlreadyUsedIndexList.Add(index);
        return index;
    }
}
