
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
class NumberInfo
{
   public int Number;//����
   public string EquationString;//��ʽ�ַ���
}

public class CardNumberInfo : MonoBehaviour
{
    //����
    private static CardNumberInfo instance;
    public static CardNumberInfo Instance => instance;

    //װ�ز�ͬ�Ĺؿ�������Ϣ
    [SerializeField] private List<NumberInfo> NumberInfoList_Level1 = new List<NumberInfo>();
    [SerializeField] private List<NumberInfo> NumberInfoList_Level2 = new List<NumberInfo>();
    [SerializeField] private List<NumberInfo> NumberInfoList_Level3 = new List<NumberInfo>();
    [SerializeField] private List<NumberInfo> NumberInfoList_Level4 = new List<NumberInfo>();
    [SerializeField] private List<NumberInfo> NumberInfoList_Level5 = new List<NumberInfo>();

    [Space(10)]
    //���˵�������Ϣ
    [SerializeField] private List<int> EnemyNumberInfoList= new List<int>();

    private List<int> AlreadyUsedIndexList = new List<int>();//��ֹ�ظ�
     private float repeatProbability = 0.2f;//�ظ���ȡһ��ֵ�ĸ���

    private void Awake()
    {
        instance = this;
    }

    //���ݹؿ������ֻ�ȡ��ʽ�ַ���
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
        card.MyNumber.SetNumber(currentList[Index].Number, currentList[Index].EquationString);//����һ�¿�Ƭ
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
                AlreadyUsedIndexList.Remove(index); // �Ƴ������ñ�ǡ��������´��ж�Ϊ����
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
