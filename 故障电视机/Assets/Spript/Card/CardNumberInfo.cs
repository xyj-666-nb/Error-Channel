
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
class NumberInfo
{
   public int Number;//数字
   public string EquationString;//等式字符串
}

public class CardNumberInfo : MonoBehaviour
{
    //单例
    private static CardNumberInfo instance;
    public static CardNumberInfo Instance => instance;

    //装载不同的关卡数字信息
    [SerializeField] private List<NumberInfo> NumberInfoList_Level1 = new List<NumberInfo>();
    [SerializeField] private List<NumberInfo> NumberInfoList_Level2 = new List<NumberInfo>();
    [SerializeField] private List<NumberInfo> NumberInfoList_Level3 = new List<NumberInfo>();
    [SerializeField] private List<NumberInfo> NumberInfoList_Level4 = new List<NumberInfo>();
    [SerializeField] private List<NumberInfo> NumberInfoList_Level5 = new List<NumberInfo>();

    [Space(10)]
    //敌人的数字信息
    [SerializeField] private List<int> EnemyNumberInfoList= new List<int>();

    private List<int> AlreadyUsedIndexList = new List<int>();//防止重复
     private float repeatProbability = 0.2f;//重复获取一个值的概率

    private void Awake()
    {
        instance = this;
    }

    //根据关卡和数字获取等式字符串
    public void GetEquationString(Card card)
    {
        List<NumberInfo> currentList = null;
        switch (PlayerManager.instance.CurrentLevel)
        {
            case GameLevel.Level1:
                currentList = NumberInfoList_Level1;
                break;
            case GameLevel.Level2:
                currentList = NumberInfoList_Level2;
                break;
            case GameLevel.Level3:
                currentList = NumberInfoList_Level3;
                break;
            case GameLevel.Level4:
                currentList = NumberInfoList_Level4;
                break;
            case GameLevel.Level5:
                currentList = NumberInfoList_Level5;
                break;
        }
        var Index = GetIndex(currentList);
        card.MyNumber.SetNumber(currentList[Index].Number, currentList[Index].EquationString);//设置一下卡片
    }

    public int GetEnemyNumber() 
    {
        return EnemyNumberInfoList[Random.Range(0, EnemyNumberInfoList.Count)];
    }

    private int GetIndex(List<NumberInfo> list)
    {
        if (AlreadyUsedIndexList.Count >= list.Count)
            AlreadyUsedIndexList.Clear();

        int index;
        index = Random.Range(0, list.Count);

        if (AlreadyUsedIndexList.Contains(index))
        {
            if (Random.value < repeatProbability)
            {
                AlreadyUsedIndexList.Remove(index); // 移除“已用标记”，避免下次判定为已用
                return index;
            }
            else
            {
                do
                {
                    index = Random.Range(0, list.Count);
                } while (AlreadyUsedIndexList.Contains(index));
            }
        }

        AlreadyUsedIndexList.Add(index);
        return index;
    }
}
