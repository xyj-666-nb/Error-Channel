using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Collections;
using System.Diagnostics.Contracts;
using UnityEngine.Events;//引入dotween插件
public enum GameLevel
{
    Level1,
    Level2,
    Level3,
    Level4,
    Level5
}
public enum PushType { Touch,track }//1.是玩家触摸，2.是预设轨迹
public class HandCardManger : MonoBehaviour
{
    private static HandCardManger instance;
    public static HandCardManger Instance => instance;//单例模式

    [SerializeField]private int MaxHandCardCount = 4;//最大手牌数量
    public List<GameObject> HandCardList = new List<GameObject>();//手牌列表
    [SerializeField] private GameObject HandCarPrefabs;//手牌预制体
    [SerializeField] private Transform spawnPoint;//手牌生成点,也就是起始位置
    [SerializeField] private SplineContainer SplineContainer_getCard;//spline插件里面的容器，这里是卡牌的初始轨道
    [SerializeField]private SplineContainer SplineContainer_PushCard;//这里是打出卡牌的轨道

    [SerializeField]private Transform CardParent;//卡牌的父物体

    private bool IsStart = true;
   private void Awake()
    {
        instance = this;
    }

    public void AddCardMount()
    {
        MaxHandCardCount++;
    }

    public void InitCard()
    {
        // 如果已经在初始化过程中，直接返回
        if (IsInitializing) return;

        StartCoroutine(CreateInitCard());
    }

    private bool IsInitializing = false;

    IEnumerator CreateInitCard()
    {
        IsInitializing = true;

        AudioSource audioSource = null;
        //开启连续发牌
        MusicManager.Instance.PlayEffectMusic("Music/连续发牌", false, (resources) => { audioSource = resources; });
        yield return new WaitForSeconds(0.2f);

        if (EnemyCard.CurrentEnemyCard == null)//为空才创建敌人卡牌
            GetEnemyCard();

        var HandCardCount = HandCardList.Count;
        for (int i = 0; i < MaxHandCardCount - HandCardCount; i++)//直接补满手牌
        {
            CreatCard();//创建手牌
            yield return new WaitForSeconds(0.2f);
        }

        MusicManager.Instance.StopEffectMusic(audioSource);//暂停

        // 只在游戏开始时打开新手引导的对话，重新初始化时不触发
        if (IsStart && Main.Instance != null && Main.Instance.InitDia != null)
        {
            IsStart = false;
            Main.Instance.InitDia.StartDialogue(1);//开启对话1
        }

        IsInitializing = false;
    }

    public void CreatCard()
    {
        GameObject Card = PoolManage.Instance.GetObj(HandCarPrefabs);//通过对象池获取卡牌
        Card.transform.parent = CardParent;
        HandCardList.Add(Card);//添加进手牌列表
        UpdateCardPosition();//调用更新2
    }

    // 随机回收一张卡牌
    public void RandomRecycleCard()
    {
        int RandomIndex = Random.Range(0, HandCardList.Count);
        RecycleArea.Instance.RecycleCard(HandCardList[RandomIndex].GetComponent<Card>());
    }

    public void PushCard(PushType Type)
    {
        if (Card.CurrentSelectedCard_1 == null)
            return;

        // 缓存当前选中的卡牌
        Card pushedCard = Card.CurrentSelectedCard_1;

        switch (Type)
        {
            case PushType.Touch:
                break;
            case PushType.track:
                PushPlayerCard(pushedCard,0.3f, pushedCard);
                break;
        }

        pushedCard.Push();// 标记卡牌已打出
        HandCardList.Remove(pushedCard.gameObject);
        UpdateCardPosition();
        Card.CurrentSelectedCard_1 = null;
    }

    public void PushPlayerCard(Card card,float _totalDuration,Card PushCard)
    {
        Spline trackSpline = SplineContainer_PushCard.Spline; // 获取曲线
        BezierKnot knot = trackSpline[0]; // 获取第一个节点

        // 将对象的世界坐标转换为曲线所在对象的局部坐标
        Vector3 cardWorldPos = card.transform.position;
        Vector3 curveLocalPos = SplineContainer_PushCard.transform.InverseTransformPoint(cardWorldPos);

        knot.Position = curveLocalPos; // 赋值转换后的局部坐标
        trackSpline.SetKnot(0, knot); // 写回曲线

        StartCoroutine(MoveCard(card.transform, SplineContainer_PushCard, _totalDuration, RecycleCard, PushCard)); // 启动移动协程
    }

    private void RecycleCard(Card PushCard)
    {
        AiCardArea.Instance.PushCard(PushCard);
    }

    //让物体沿着曲线移动的通用协程
    IEnumerator MoveCard(Transform _transform, SplineContainer SplineTrack,float _totalDuration,UnityAction<Card> CallBack=null,Card PushCard=null)
    {
        // 终止卡牌上所有残留的DOTween动画
        _transform.DOKill();

        Transform splineContainerTrans = SplineTrack.transform;
        Spline spline = SplineTrack.Spline;

        float totalDuration = _totalDuration; // 总移动时间
        float elapsedTime = 0f;   // 已流逝时间

        while (elapsedTime < totalDuration)
        {
            // 计算当前进度 t
            float t = elapsedTime / totalDuration;
            // 计算曲线上的位置
            Vector3 localPos = spline.EvaluatePosition(t);
            Vector3 worldPos = splineContainerTrans.TransformPoint(localPos);

            // 直接设置位置
            _transform.position = worldPos;
            yield return null;
            // 累加时间
            //给出一个加速的效果
            var AddTime = Time.deltaTime * (1 + (elapsedTime / totalDuration));//时间越往后加速越快，最大100%
            elapsedTime += AddTime;
            if( elapsedTime > totalDuration)
            {
                elapsedTime = totalDuration;//防止超出时间
            }
        }

        Vector3 finalPos = splineContainerTrans.TransformPoint(spline.EvaluatePosition(1f));
        _transform.position = finalPos;
        CallBack?.Invoke(PushCard);
    }

    public void UpdateCardPosition()
    {
        if (HandCardList.Count <= 0)
            return;

        if (Card.CurrentSelectedCard_1 != null)//如果当前有选中的牌就直接归位
            Card.CurrentSelectedCard_1.SetDeactivate();


        // 计算每张卡牌在曲线上的位置（保持原有逻辑）
        float step = 1f / (MaxHandCardCount + 1);
        float firstCardPos = 0.5f - step * (HandCardList.Count - 1) / 2f;

        // 遍历所有手牌，按列表顺序设置层级（i从0开始，依次递增）
        for (int i = 0; i < HandCardList.Count; i++)
        {
            float startPos = firstCardPos + step * i;

            // 核心：传递当前索引i作为层级基础值（确保后一张牌i更大，层级更高）
            HandCardList[i].GetComponent<Card>().setLayer(i);

            // （保持原有位置和旋转逻辑不变）
            Vector3 localSplinePos = SplineContainer_getCard.Spline.EvaluatePosition(startPos);
            Vector3 worldSplinePos = SplineContainer_getCard.transform.TransformPoint(localSplinePos);
            Vector3 forward = SplineContainer_getCard.Spline.EvaluateTangent(startPos);
            Vector3 up = SplineContainer_getCard.Spline.EvaluateUpVector(startPos);
            Quaternion rot = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

            HandCardList[i].transform.DOMove(worldSplinePos, 0.5f);
            HandCardList[i].transform.DORotateQuaternion(rot, 0.5f);

            Card card = HandCardList[i].GetComponent<Card>();
            StartCoroutine(WaitTime(card, worldSplinePos));
        }
    }

    IEnumerator WaitTime( Card Card,Vector3 Pos)
    {
        yield return new WaitForSeconds(0.5f);
        Card.IsAnimator = false;//动画播放完毕
        Card.SetOriginalPos(Pos);//设置卡牌的初始位置
        //Card.Flip();//再次翻转
    }

    //——————————给出敌人牌组——————————
    [Space(10)]
    [SerializeField] private SplineContainer SplineContainer_PushEnemyCard;//这里是敌人卡牌的的轨道
    public GameObject enemyCard;//敌人卡牌预制体

    public void GetEnemyCard()//得到一张敌人牌
    {
       GameObject EnemyCardObj= PoolManage.Instance.GetObj(enemyCard);//通过对象池获取卡牌
        //设置父对象
        EnemyCardObj.transform.parent = CardParent;
        EnemyCardObj.transform.position = SplineContainer_PushEnemyCard.Spline[0].Position;//设置初始位置
        StartCoroutine(MoveCard(EnemyCardObj.transform, SplineContainer_PushEnemyCard, 0.3f)); // 启动移动协程


        //重新初始化换牌
        PassPromptText.Instance.InitPassText();
    }
}
