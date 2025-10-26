using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NumberInfo
{
    public int Number; // 存储“结果”
    public string EquationString; // 存储“式子”
}

[System.Serializable]
public class levelNumberInfo
{
    public List<NumberInfo> PlayerNumberInfo = new List<NumberInfo>();
    public List<NumberInfo> EnemyNumberInfo = new List<NumberInfo>();
}

public class CardNumberInfo : MonoBehaviour
{
    private static CardNumberInfo instance;
    public static CardNumberInfo Instance => instance;

    // 4个等级的数字信息（Level1/2需手动补充，这里先留空）
    [SerializeField] private levelNumberInfo Level1Info = new levelNumberInfo();
    [SerializeField] private levelNumberInfo Level2Info = new levelNumberInfo();
    [SerializeField] private levelNumberInfo Level3Info = new levelNumberInfo();
    [SerializeField] private levelNumberInfo Level4Info = new levelNumberInfo();
    [SerializeField] private levelNumberInfo Level5Info = new levelNumberInfo();

    [SerializeField] private levelNumberInfo CurrentLeveInfo;

    // 防止重复获取的索引列表
    private List<int> AlreadyUsedIndexList_player = new List<int>();
    private List<int> AlreadyUsedIndexList_enemy = new List<int>();
    private float repeatProbability = 0.2f;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // 关键：手动初始化所有关卡数据（Level3/4/5用你提供的式子和结果）
        InitAllLevelData();
    }

    private void Start()
    {
        ChangeLevelData(GameLevel.Level1);
    }


    #region 核心：手动填充关卡数据（按Level3/4/5分别整理）
    /// <summary>
    /// 初始化所有关卡的手动数据
    /// </summary>
    private void InitAllLevelData()
    {
        // 1. 初始化Level3数据（加减法，共30条）
        InitLevel3Data();
        // 2. 初始化Level4数据（乘除法，共25条）
        InitLevel4Data();
        // 3. 初始化Level5数据（带花色，仅用式子和结果，共30条）
        InitLevel5Data();

        // Level1/2若需要数据，可参考上面方法手动添加
        // InitLevel1Data();
        // InitLevel2Data();
    }

    /// <summary>
    /// 手动填充Level3数据（加减法）
    /// </summary>
    private void InitLevel3Data()
    {
        // 手动添加Level3的“式子”和“结果”（对应你提供的前30条数据）
        List<NumberInfo> level3Data = new List<NumberInfo>()
        {
            new NumberInfo{ Number=129, EquationString="73 + 56" },
            new NumberInfo{ Number=85, EquationString="62 + 23" },
            new NumberInfo{ Number=130, EquationString="74 + 56" },
            new NumberInfo{ Number=80, EquationString="53 + 27" },
            new NumberInfo{ Number=78, EquationString="95 - 17" },
            new NumberInfo{ Number=81, EquationString="54 + 27" },
            new NumberInfo{ Number=68, EquationString="102 - 34" },
            new NumberInfo{ Number=111, EquationString="143 - 32" },
            new NumberInfo{ Number=79, EquationString="118 - 39" },
            new NumberInfo{ Number=120, EquationString="68 + 52" },
            new NumberInfo{ Number=67, EquationString="45 + 22" },
            new NumberInfo{ Number=105, EquationString="128 - 23" },
            new NumberInfo{ Number=80, EquationString="43 + 37" },
            new NumberInfo{ Number=137, EquationString="79 + 58" },
            new NumberInfo{ Number=130, EquationString="149 - 19" },
            new NumberInfo{ Number=92, EquationString="45 + 47" },
            new NumberInfo{ Number=109, EquationString="72 + 37" },
            new NumberInfo{ Number=120, EquationString="147 - 27" },
            new NumberInfo{ Number=83, EquationString="49 + 34" },
            new NumberInfo{ Number=109, EquationString="58 + 51" },
            new NumberInfo{ Number=107, EquationString="121 - 14" },
            new NumberInfo{ Number=94, EquationString="131 - 37" },
            new NumberInfo{ Number=88, EquationString="124 - 36" },
            new NumberInfo{ Number=119, EquationString="146 - 27" },
            new NumberInfo{ Number=84, EquationString="106 - 22" },
            new NumberInfo{ Number=119, EquationString="79 + 40" },
            new NumberInfo{ Number=69, EquationString="104 - 35" },
            new NumberInfo{ Number=70, EquationString="90 - 20" },
            new NumberInfo{ Number=114, EquationString="140 - 26" },
            new NumberInfo{ Number=76, EquationString="30 + 46" }
        };

        // 玩家和敌人共用同一批数据（若需分开，可单独创建列表）
        Level3Info.PlayerNumberInfo = level3Data;
        Level3Info.EnemyNumberInfo = new List<NumberInfo>(level3Data); // 复制一份给敌人
    }

    /// <summary>
    /// 手动填充Level4数据（乘除法）
    /// </summary>
    private void InitLevel4Data()
    {
        // 手动添加Level4的“式子”和“结果”（对应你提供的第31-55条数据）
        List<NumberInfo> level4Data = new List<NumberInfo>()
        {
            new NumberInfo{ Number=300, EquationString="25 × 12" },
            new NumberInfo{ Number=179, EquationString="358 ÷ 2" },
            new NumberInfo{ Number=129, EquationString="387 ÷ 3" },
            new NumberInfo{ Number=132, EquationString="528 ÷ 4" },
            new NumberInfo{ Number=176, EquationString="22 × 8" },
            new NumberInfo{ Number=182, EquationString="364 ÷ 2" },
            new NumberInfo{ Number=300, EquationString="20 × 15" },
            new NumberInfo{ Number=360, EquationString="24 × 15" },
            new NumberInfo{ Number=125, EquationString="25 × 5" },
            new NumberInfo{ Number=350, EquationString="35 × 10" },
            new NumberInfo{ Number=406, EquationString="29 × 14" },
            new NumberInfo{ Number=171, EquationString="684 ÷ 4" },
            new NumberInfo{ Number=150, EquationString="30 × 5" },
            new NumberInfo{ Number=120, EquationString="24 × 5" },
            new NumberInfo{ Number=510, EquationString="34 × 15" },
            new NumberInfo{ Number=198, EquationString="18 × 11" },
            new NumberInfo{ Number=126, EquationString="378 ÷ 3" },
            new NumberInfo{ Number=192, EquationString="32 × 6" },
            new NumberInfo{ Number=169, EquationString="13 × 13" },
            new NumberInfo{ Number=111, EquationString="333 ÷ 3" },
            new NumberInfo{ Number=130, EquationString="26 × 5" },
            new NumberInfo{ Number=240, EquationString="24 × 10" },
            new NumberInfo{ Number=171, EquationString="513 ÷ 3" },
            new NumberInfo{ Number=124, EquationString="496 ÷ 4" },
            new NumberInfo{ Number=187, EquationString="935 ÷ 5" },
            new NumberInfo{ Number=135, EquationString="540 ÷ 4" },
            new NumberInfo{ Number=132, EquationString="22 × 6" },
            new NumberInfo{ Number=104, EquationString="13 × 8" },
            new NumberInfo{ Number=245, EquationString="35 × 7" },
            new NumberInfo{ Number=129, EquationString="645 ÷ 5" }
        };

        Level4Info.PlayerNumberInfo = level4Data;
        Level4Info.EnemyNumberInfo = new List<NumberInfo>(level4Data);
    }

    /// <summary>
    /// 手动填充Level5数据（带花色，仅用式子和结果）
    /// </summary>
    private void InitLevel5Data()
    {
        // 手动添加Level5的“式子”和“结果”（对应你提供的第56-85条数据）
        List<NumberInfo> level5Data = new List<NumberInfo>()
        {
            new NumberInfo{ Number=418, EquationString="38 × 11" },
            new NumberInfo{ Number=270, EquationString="292 - 22" },
            new NumberInfo{ Number=377, EquationString="29 × 13" },
            new NumberInfo{ Number=264, EquationString="12 × 11" },
            new NumberInfo{ Number=480, EquationString="48 × 10" },
            new NumberInfo{ Number=564, EquationString="94 × 6" },
            new NumberInfo{ Number=490, EquationString="49 × 10" },
            new NumberInfo{ Number=506, EquationString="46 × 11" },
            new NumberInfo{ Number=376, EquationString="47 × 8" },
            new NumberInfo{ Number=572, EquationString="22 × 13" },
            new NumberInfo{ Number=297, EquationString="326 - 29" },
            new NumberInfo{ Number=299, EquationString="334 - 35" },
            new NumberInfo{ Number=344, EquationString="379 - 35" },
            new NumberInfo{ Number=480, EquationString="40 × 12" },
            new NumberInfo{ Number=490, EquationString="70 × 7" },
            new NumberInfo{ Number=444, EquationString="37 × 12" },
            new NumberInfo{ Number=336, EquationString="21 × 8" },
            new NumberInfo{ Number=720, EquationString="24 × 15" },
            new NumberInfo{ Number=378, EquationString="27 × 7" },
            new NumberInfo{ Number=264, EquationString="285 - 21" },
            new NumberInfo{ Number=314, EquationString="355 - 41" },
            new NumberInfo{ Number=504, EquationString="18 × 14" },
            new NumberInfo{ Number=390, EquationString="39 × 10" },
            new NumberInfo{ Number=464, EquationString="58 × 8" },
            new NumberInfo{ Number=343, EquationString="377 - 34" },
            new NumberInfo{ Number=676, EquationString="52 × 13" },
            new NumberInfo{ Number=380, EquationString="38 × 10" },
            new NumberInfo{ Number=612, EquationString="51 × 12" },
            new NumberInfo{ Number=594, EquationString="66 × 9" },
            new NumberInfo{ Number=225, EquationString="259 - 34" },
            new NumberInfo{ Number=590, EquationString="59 × 10" },
            new NumberInfo{ Number=244, EquationString="284 - 40" },
            new NumberInfo{ Number=288, EquationString="36 × 8" },
            new NumberInfo{ Number=380, EquationString="95 × 4" },
            new NumberInfo{ Number=362, EquationString="396 - 34" },
            new NumberInfo{ Number=658, EquationString="94 × 7" },
            new NumberInfo{ Number=238, EquationString="268 - 30" },
            new NumberInfo{ Number=585, EquationString="45 × 13" },
            new NumberInfo{ Number=679, EquationString="97 × 7" }
        };

        Level5Info.PlayerNumberInfo = level5Data;
        Level5Info.EnemyNumberInfo = new List<NumberInfo>(level5Data);
    }

    #endregion


    #region 原有逻辑（保持不变）
    /// <summary>
    /// 给玩家卡片设置式子和结果
    /// </summary>
    public void GetEquationString(Card card)
    {
        if (CurrentLeveInfo == null || CurrentLeveInfo.PlayerNumberInfo.Count == 0)
        {
            Debug.LogError("当前关卡无数据！请检查Level是否正确或数据是否初始化");
            return;
        }

        int index = GetIndex(CurrentLeveInfo.PlayerNumberInfo, AlreadyUsedIndexList_player);
        card.MyNumber.SetNumber(
            CurrentLeveInfo.PlayerNumberInfo[index].Number,
            CurrentLeveInfo.PlayerNumberInfo[index].EquationString
        );
    }

    /// <summary>
    /// 给敌人卡片设置式子和结果
    /// </summary>
    public void GetEnemyNumber(EnemyCard card)
    {
        if (CurrentLeveInfo == null || CurrentLeveInfo.EnemyNumberInfo.Count == 0)
        {
            Debug.LogError("当前关卡敌人无数据！请检查Level是否正确或数据是否初始化");
            return;
        }

        int index = GetIndex(CurrentLeveInfo.EnemyNumberInfo, AlreadyUsedIndexList_enemy);
        card.Number = CurrentLeveInfo.EnemyNumberInfo[index].Number;
        card.NumberText.text = CurrentLeveInfo.EnemyNumberInfo[index].EquationString;
    }

    /// <summary>
    /// 切换关卡数据
    /// </summary>
    public void ChangeLevelData(GameLevel level)
    {
        switch (level)
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
                CurrentLeveInfo = Level4Info;
                break;
            case GameLevel.Level5:
                CurrentLeveInfo = Level5Info;
                break;
        }

        // 切换关卡时清空已用索引，避免重复
        AlreadyUsedIndexList_player.Clear();
        AlreadyUsedIndexList_enemy.Clear();
    }

    /// <summary>
    /// 获取不重复的随机索引
    /// </summary>
    private int GetIndex(List<NumberInfo> list, List<int> usedIndices)
    {
        if (list.Count == 0)
        {
            Debug.LogError("数据列表为空！");
            return -1;
        }

        // 若所有索引都用过，清空重新开始
        if (usedIndices.Count >= list.Count)
            usedIndices.Clear();

        int index = Random.Range(0, list.Count);

        // 处理重复情况
        if (usedIndices.Contains(index))
        {
            // 按概率允许重复，否则重新获取
            if (Random.value < repeatProbability)
            {
                usedIndices.Remove(index);
                return index;
            }
            else
            {
                do
                {
                    index = Random.Range(0, list.Count);
                } while (usedIndices.Contains(index));
            }
        }

        usedIndices.Add(index);
        return index;
    }
    #endregion
}