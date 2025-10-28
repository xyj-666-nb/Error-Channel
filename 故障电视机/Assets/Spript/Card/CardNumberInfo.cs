using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NumberInfo
{
    public int Number; // 存储“结果”
    public string EquationString; // 存储“式子”
    public CardDesignMode CardMode;//卡牌的花色
}

[System.Serializable]
public class CardDesignPrefabs
{
    public CardDesignMode CardMode;//卡牌的花色
    public Sprite FrontSprite;
    public Sprite BackSprite;
}

[System.Serializable]
public class levelNumberInfo
{
    public List<NumberInfo> PlayerNumberInfo = new List<NumberInfo>();
    public List<NumberInfo> EnemyNumberInfo = new List<NumberInfo>();
}

// 花色枚举
public enum CardDesignMode
{
    RedHeart,//红心
    RedAngularShape,//红菱形
    Spades,//黑桃
    BlackPlumBlossom,//黑梅花
}
public class CardNumberInfo : MonoBehaviour
{
    private static CardNumberInfo instance;
    public static CardNumberInfo Instance => instance;

    [Header("牌组的照片预制体")]
    [Space(10)]
    public List<CardDesignPrefabs> CardDesignPrefabsList;//外部赋值

    [Header("游戏数据信息")]
    [Space(10)]
    [SerializeField] private levelNumberInfo Level1Info = new levelNumberInfo();
    [SerializeField] private levelNumberInfo Level2Info = new levelNumberInfo();
    [SerializeField] private levelNumberInfo Level3Info = new levelNumberInfo();
    [SerializeField] private levelNumberInfo Level4Info = new levelNumberInfo();
    [SerializeField] private levelNumberInfo Level5Info = new levelNumberInfo();

    [SerializeField] private levelNumberInfo CurrentLeveInfo;

    private List<int> AlreadyUsedIndexList_player = new List<int>();
    private List<int> AlreadyUsedIndexList_enemy = new List<int>();
    private float repeatProbability = 0.2f;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        InitAllLevelData();
    }

    private void Start()
    {
        ChangeLevelData(GameLevel.Level1);
    }


    #region 关卡数据初始化（核心修改：Level3/4各10条数据）
    private void InitAllLevelData()
    {
        InitLevel1Data();
        InitLevel2Data();
        InitLevel3Data();// 10条数据
        InitLevel4Data();// 10条数据
        InitLevel5Data();
    }

    // Level1数据：保持50条（用户未要求修改）
    private void InitLevel1Data()
    {
        int dataCount = 50;
        List<NumberInfo> playerData = new List<NumberInfo>();
        List<NumberInfo> enemyData = new List<NumberInfo>();

        for (int i = 0; i < dataCount; i++)
        {
            int randomNum = Random.Range(1, 21);
            playerData.Add(new NumberInfo
            {
                Number = randomNum,
                EquationString = randomNum.ToString(),
                CardMode = GetRandomDesignMode()
            });
        }

        for (int i = 0; i < dataCount; i++)
        {
            int randomNum = Random.Range(1, 31);
            enemyData.Add(new NumberInfo
            {
                Number = randomNum,
                EquationString = randomNum.ToString(),
                CardMode = GetRandomDesignMode()
            });
        }

        Level1Info.PlayerNumberInfo = playerData;
        Level1Info.EnemyNumberInfo = enemyData;
    }

    // Level2数据：保持50条（用户未要求修改）
    private void InitLevel2Data()
    {
        int dataCount = 50;
        List<NumberInfo> playerData = new List<NumberInfo>();
        List<NumberInfo> enemyData = new List<NumberInfo>();

        for (int i = 0; i < dataCount; i++)
        {
            int randomNum = Random.Range(60, 161);
            playerData.Add(new NumberInfo
            {
                Number = randomNum,
                EquationString = randomNum.ToString(),
                CardMode = GetRandomDesignMode()
            });
        }

        for (int i = 0; i < dataCount; i++)
        {
            int randomNum = Random.Range(50, 201);
            enemyData.Add(new NumberInfo
            {
                Number = randomNum,
                EquationString = randomNum.ToString(),
                CardMode = GetRandomDesignMode()
            });
        }

        Level2Info.PlayerNumberInfo = playerData;
        Level2Info.EnemyNumberInfo = enemyData;
    }

    /// <summary>
    /// Level3数据：
    /// - 玩家：[80, 200]随机10条，式子=结果，花色随机
    /// - 敌人：从提供的列表中随机挑10条，花色随机
    /// </summary>
    private void InitLevel3Data()
    {
        // 1. 玩家数据：10条[80,200]随机数
        int playerDataCount = 10;
        List<NumberInfo> playerData = new List<NumberInfo>();
        for (int i = 0; i < playerDataCount; i++)
        {
            int randomNum = Random.Range(80, 201); // [80,200]闭区间
            playerData.Add(new NumberInfo
            {
                Number = randomNum,
                EquationString = randomNum.ToString(),
                CardMode = GetRandomDesignMode()
            });
        }

        List<(string equation, int result)> level3EnemyRawData = new List<(string, int)>()
{
    ("156 + 89", 245), ("234 - 67", 167), ("178 + 95", 273), ("287 - 123", 164), ("145 + 78", 223),
    ("256 - 89", 167), ("192 + 108", 300), ("234 - 56", 178), ("167 + 89", 256), ("298 - 134", 164),
    ("123 + 156", 279), ("245 - 78", 167), ("189 + 67", 256), ("267 - 89", 178), ("134 + 123", 257),
    ("289 - 145", 144), ("156 + 134", 290), ("234 - 67", 167), ("178 + 89", 267), ("256 - 123", 133),
    ("145 + 134", 279), ("287 - 89", 198), ("192 + 78", 270), ("234 - 145", 89), ("167 + 123", 290),
    ("298 - 67", 231), ("123 + 178", 301), ("245 - 134", 111), ("189 + 56", 245), ("267 - 123", 144),
    ("134 + 145", 279), ("289 - 78", 211), ("156 + 123", 279), ("234 - 89", 145), ("178 + 134", 312),
    ("256 - 145", 111), ("145 + 89", 234), ("287 - 134", 153), ("192 + 123", 315), ("234 - 78", 156),
    ("167 + 145", 312), ("298 - 123", 175), ("123 + 134", 257), ("245 - 89", 156), ("189 + 134", 323),
    ("267 - 145", 122), ("134 + 78", 212), ("289 - 123", 166), ("156 + 145", 301), ("234 - 134", 100)
};
        List<NumberInfo> enemyData = new List<NumberInfo>();
        // 随机挑选10条（去重）
        HashSet<int> selectedIndices = new HashSet<int>();
        while (selectedIndices.Count < 10)
        {
            int randomIdx = Random.Range(0, level3EnemyRawData.Count);
            if (selectedIndices.Add(randomIdx)) // 确保不重复
            {
                var data = level3EnemyRawData[randomIdx];
                enemyData.Add(new NumberInfo
                {
                    Number = data.result,
                    EquationString = data.equation,
                    CardMode = GetRandomDesignMode()
                });
            }
        }

        Level3Info.PlayerNumberInfo = playerData;
        Level3Info.EnemyNumberInfo = enemyData;
    }

    /// <summary>
    /// Level4数据：
    /// - 玩家：[120, 280]随机10条，式子=结果，花色随机
    /// - 敌人：从提供的列表中随机挑10条，花色随机
    /// </summary>
    private void InitLevel4Data()
    {
        // 1. 玩家数据：10条[120,280]随机数
        int playerDataCount = 10;
        List<NumberInfo> playerData = new List<NumberInfo>();
        for (int i = 0; i < playerDataCount; i++)
        {
            int randomNum = Random.Range(120, 281); // [120,280]闭区间
            playerData.Add(new NumberInfo
            {
                Number = randomNum,
                EquationString = randomNum.ToString(),
                CardMode = GetRandomDesignMode()
            });
        }

        // 2. 敌人数据：从提供的列表中随机挑10条
        List<(string equation, int result)> level4EnemyRawData = new List<(string, int)>()
{
    ("127 + 186", 313), ("463 - 218", 245), ("83 × 3", 249), ("957 ÷ 3", 319), ("158 + 249", 407),
    ("492 - 167", 325), ("74 × 4", 296), ("1143 ÷ 3", 381), ("193 + 158", 351), ("478 - 234", 244),
    ("91 × 3", 273), ("1287 ÷ 4", 321), ("234 + 178", 412), ("521 - 289", 232), ("67 × 5", 335),
    ("1564 ÷ 5", 312), ("289 + 134", 423), ("498 - 256", 242), ("88 × 4", 352), ("1692 ÷ 6", 282),
    ("156 + 267", 423), ("473 - 197", 276), ("79 × 4", 316), ("1428 ÷ 4", 357), ("218 + 189", 407),
    ("512 - 278", 234), ("93 × 3", 279), ("1845 ÷ 5", 369), ("267 + 145", 412), ("489 - 213", 276),
    ("71 × 5", 355), ("1596 ÷ 6", 266), ("178 + 234", 412), ("501 - 234", 267), ("86 × 4", 344),
    ("1764 ÷ 7", 252), ("289 + 156", 445), ("523 - 289", 234), ("77 × 5", 385), ("1953 ÷ 7", 279),
    ("234 + 189", 423), ("478 - 245", 233), ("94 × 3", 282), ("2156 ÷ 8", 269), ("156 + 278", 434),
    ("492 - 197", 295), ("68 × 6", 408), ("1287 ÷ 3", 429), ("289 + 167", 456), ("512 - 234", 278)
};
        List<NumberInfo> enemyData = new List<NumberInfo>();
        // 随机挑选10条（去重）
        HashSet<int> selectedIndices = new HashSet<int>();
        while (selectedIndices.Count < 10)
        {
            int randomIdx = Random.Range(0, level4EnemyRawData.Count);
            if (selectedIndices.Add(randomIdx)) // 确保不重复
            {
                var data = level4EnemyRawData[randomIdx];
                enemyData.Add(new NumberInfo
                {
                    Number = data.result,
                    EquationString = data.equation,
                    CardMode = GetRandomDesignMode()
                });
            }
        }

        Level4Info.PlayerNumberInfo = playerData;
        Level4Info.EnemyNumberInfo = enemyData;
    }

    // Level5数据：保持原有39条
    private void InitLevel5Data()
    {
       // Level5数据：从提供的列表中均匀选取100条，覆盖全花色+全数值区间
        List<NumberInfo> level5Data = new List<NumberInfo>()
    {
        // 1-25条：覆盖150-180数值区间，全花色分布
        new NumberInfo{ Number=156, EquationString="156", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=167, EquationString="167", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=178, EquationString="178", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-189, EquationString="189", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=200, EquationString="200", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=150, EquationString="150", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=161, EquationString="161", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=172, EquationString="172", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-194, EquationString="194", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=205, EquationString="205", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=154, EquationString="154", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=165, EquationString="165", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=176, EquationString="176", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-209, EquationString="209", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=220, EquationString="220", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=158, EquationString="158", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-169, EquationString="169", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=180, EquationString="180", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=191, EquationString="191", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=202, EquationString="202", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=151, EquationString="151", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=162, EquationString="162", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-173, EquationString="173", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=184, EquationString="184", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=195, EquationString="195", CardMode=CardDesignMode.Spades },

        // 26-50条：覆盖181-220数值区间，全花色分布
        new NumberInfo{ Number=211, EquationString="211", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=222, EquationString="222", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=233, EquationString="233", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-244, EquationString="244", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=255, EquationString="255", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=216, EquationString="216", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=227, EquationString="227", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=238, EquationString="238", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-249, EquationString="249", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=260, EquationString="260", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=213, EquationString="213", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-224, EquationString="224", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=235, EquationString="235", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=246, EquationString="246", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=257, EquationString="257", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=206, EquationString="206", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=217, EquationString="217", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=228, EquationString="228", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-239, EquationString="239", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=250, EquationString="250", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=198, EquationString="198", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=209, EquationString="209", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=220, EquationString="220", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-231, EquationString="231", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=242, EquationString="242", CardMode=CardDesignMode.RedAngularShape },

        // 51-75条：覆盖221-260数值区间，全花色分布
        new NumberInfo{ Number=266, EquationString="266", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=277, EquationString="277", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=288, EquationString="288", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-299, EquationString="299", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=271, EquationString="271", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=282, EquationString="282", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=293, EquationString="293", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=261, EquationString="261", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=272, EquationString="272", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=283, EquationString="283", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-294, EquationString="294", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=268, EquationString="268", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=279, EquationString="279", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=290, EquationString="290", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-256, EquationString="256", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=267, EquationString="267", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=278, EquationString="278", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=289, EquationString="289", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=300, EquationString="300", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-160, EquationString="160", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=225, EquationString="225", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=236, EquationString="236", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=247, EquationString="247", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=258, EquationString="258", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-269, EquationString="269", CardMode=CardDesignMode.BlackPlumBlossom },

        // 76-100条：覆盖261-300数值区间，全花色分布
        new NumberInfo{ Number=280, EquationString="280", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=291, EquationString="291", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=152, EquationString="152", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=163, EquationString="163", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-174, EquationString="174", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=185, EquationString="185", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=196, EquationString="196", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=207, EquationString="207", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=218, EquationString="218", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-229, EquationString="229", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=240, EquationString="240", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=251, EquationString="251", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=262, EquationString="262", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=273, EquationString="273", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-284, EquationString="284", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=295, EquationString="295", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=153, EquationString="153", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=164, EquationString="164", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=175, EquationString="175", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-186, EquationString="186", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=197, EquationString="197", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=208, EquationString="208", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=219, EquationString="219", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=230, EquationString="230", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=-241, EquationString="241", CardMode=CardDesignMode.BlackPlumBlossom }
    };
        List<NumberInfo> level5EnemyData = new List<NumberInfo>()
    {
        // 一、Spades（黑桃·加法，20条）
        new NumberInfo{ Number=626, EquationString="127 + 186", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=814, EquationString="158 + 249", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=702, EquationString="193 + 158", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=824, EquationString="234 + 178", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=846, EquationString="289 + 134", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=846, EquationString="156 + 267", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=814, EquationString="218 + 189", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=824, EquationString="267 + 145", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=824, EquationString="178 + 234", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=890, EquationString="289 + 156", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=846, EquationString="234 + 189", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=868, EquationString="156 + 278", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=912, EquationString="289 + 167", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=890, EquationString="178 + 267", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=864, EquationString="234 + 198", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=912, EquationString="267 + 189", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=824, EquationString="156 + 256", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=934, EquationString="289 + 178", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=846, EquationString="234 + 189", CardMode=CardDesignMode.Spades },
        new NumberInfo{ Number=824, EquationString="178 + 234", CardMode=CardDesignMode.Spades },

        // 二、RedHeart（红心·减法，20条）
        new NumberInfo{ Number=245, EquationString="463 - 218", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=325, EquationString="492 - 167", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=244, EquationString="478 - 234", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=232, EquationString="521 - 289", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=242, EquationString="498 - 256", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=276, EquationString="473 - 197", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=234, EquationString="512 - 278", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=276, EquationString="489 - 213", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=267, EquationString="501 - 234", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=234, EquationString="523 - 289", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=233, EquationString="478 - 245", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=295, EquationString="492 - 197", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=278, EquationString="512 - 234", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=242, EquationString="498 - 256", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=255, EquationString="473 - 218", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=234, EquationString="501 - 267", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=265, EquationString="478 - 213", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=258, EquationString="492 - 234", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=245, EquationString="512 - 267", CardMode=CardDesignMode.RedHeart },
        new NumberInfo{ Number=255, EquationString="473 - 218", CardMode=CardDesignMode.RedHeart },

        // 三、BlackPlumBlossom（黑梅花·乘法，20条）
        new NumberInfo{ Number=249, EquationString="83 × 3", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=296, EquationString="74 × 4", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=273, EquationString="91 × 3", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=335, EquationString="67 × 5", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=352, EquationString="88 × 4", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=316, EquationString="79 × 4", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=279, EquationString="93 × 3", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=355, EquationString="71 × 5", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=344, EquationString="86 × 4", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=385, EquationString="77 × 5", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=282, EquationString="94 × 3", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=408, EquationString="68 × 6", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=328, EquationString="82 × 4", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=285, EquationString="95 × 3", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=365, EquationString="73 × 5", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=356, EquationString="89 × 4", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=380, EquationString="76 × 5", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=364, EquationString="91 × 4", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=336, EquationString="84 × 4", CardMode=CardDesignMode.BlackPlumBlossom },
        new NumberInfo{ Number=432, EquationString="72 × 6", CardMode=CardDesignMode.BlackPlumBlossom },

        // 四、RedAngularShape（红菱形·除法，20条）
        new NumberInfo{ Number=319, EquationString="957 ÷ 3", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=381, EquationString="1143 ÷ 3", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=321, EquationString="1287 ÷ 4", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=312, EquationString="1564 ÷ 5", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=282, EquationString="1692 ÷ 6", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=357, EquationString="1428 ÷ 4", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=369, EquationString="1845 ÷ 5", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=266, EquationString="1596 ÷ 6", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=252, EquationString="1764 ÷ 7", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=279, EquationString="1953 ÷ 7", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=269, EquationString="2156 ÷ 8", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=429, EquationString="1287 ÷ 3", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=294, EquationString="1764 ÷ 6", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=293, EquationString="2052 ÷ 7", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=319, EquationString="1596 ÷ 5", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=307, EquationString="1845 ÷ 6", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=256, EquationString="2052 ÷ 8", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=239, EquationString="2156 ÷ 9", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=263, EquationString="1845 ÷ 7", CardMode=CardDesignMode.RedAngularShape },
        new NumberInfo{ Number=244, EquationString="1953 ÷ 8", CardMode=CardDesignMode.RedAngularShape }
    };

        Level5Info.PlayerNumberInfo = level5Data;
        Level5Info.EnemyNumberInfo = level5EnemyData;
    }


    // 随机花色生成（前4级通用）
    private CardDesignMode GetRandomDesignMode()
    {
        CardDesignMode[] allModes = (CardDesignMode[])System.Enum.GetValues(typeof(CardDesignMode));
        return allModes[Random.Range(0, allModes.Length)];
    }
    #endregion


    #region 业务逻辑（未修改）
    /// <summary>
    /// 给玩家卡片设置数据
    /// </summary>
    public void SetCardInfo(Card card)
    {
        if (CurrentLeveInfo == null || CurrentLeveInfo.PlayerNumberInfo.Count == 0)
        {
            Debug.LogError("当前关卡玩家数据为空！请检查Level配置");
            return;
        }

        int index = GetIndex(CurrentLeveInfo.PlayerNumberInfo, AlreadyUsedIndexList_player);
        card.MyNumber.SetNumber(
            CurrentLeveInfo.PlayerNumberInfo[index].Number,
            CurrentLeveInfo.PlayerNumberInfo[index].EquationString
        );
        card.MyDesignMode = CurrentLeveInfo.PlayerNumberInfo[index].CardMode;
        foreach (var designPrefab in CardDesignPrefabsList)
        {
            if (designPrefab.CardMode == card.MyDesignMode)
            {
                card.SetCardSprite(designPrefab.FrontSprite, designPrefab.BackSprite);
                break;
            }
        }

        // 红花色黑字，黑花色蓝字
        if (card.MyDesignMode == CardDesignMode.RedHeart || card.MyDesignMode == CardDesignMode.RedAngularShape)
            card.MyNumber.EquationText.color = Color.black;
        else
            card.MyNumber.EquationText.color = Color.blue;
    }

    /// <summary>
    /// 给敌人卡片设置数据
    /// </summary>
    public void GetEnemyNumber(EnemyCard card)
    {
        if (CurrentLeveInfo == null || CurrentLeveInfo.EnemyNumberInfo.Count == 0)
        {
            Debug.LogError("当前关卡敌人数据为空！请检查Level配置");
            return;
        }
        if (card.NumberText == null)
        {
            Debug.LogError("EnemyCard的NumberText未赋值！");
            return;
        }

        int index = GetIndex(CurrentLeveInfo.EnemyNumberInfo, AlreadyUsedIndexList_enemy);
        card.Number = CurrentLeveInfo.EnemyNumberInfo[index].Number;
        card.NumberText.text = CurrentLeveInfo.EnemyNumberInfo[index].EquationString;
        card.MyMode = CurrentLeveInfo.EnemyNumberInfo[index].CardMode;
        foreach (var designPrefab in CardDesignPrefabsList)
        {
            if (designPrefab.CardMode == card.MyMode)
            {
                card.SetSprite(designPrefab.FrontSprite, designPrefab.BackSprite);
                return;
            }
        }
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
            default:
                CurrentLeveInfo = Level1Info;
                Debug.LogWarning($"未知关卡{level}，默认使用Level1数据");
                break;
        }

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
            Debug.LogError("数据列表为空，无法获取索引！");
            return -1;
        }

        if (usedIndices.Count >= list.Count)
            usedIndices.Clear();

        int randomIndex = Random.Range(0, list.Count);
        if (usedIndices.Contains(randomIndex))
        {
            if (Random.value < repeatProbability)
            {
                usedIndices.Remove(randomIndex);
                usedIndices.Add(randomIndex);
                return randomIndex;
            }
            else
            {
                do
                {
                    randomIndex = Random.Range(0, list.Count);
                } while (usedIndices.Contains(randomIndex));
            }
        }

        usedIndices.Add(randomIndex);
        return randomIndex;
    }
    #endregion
}