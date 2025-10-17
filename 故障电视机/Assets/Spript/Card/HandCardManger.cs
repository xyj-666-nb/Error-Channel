using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Collections;
using System.Diagnostics.Contracts;//引入dotween插件

public enum PushType { Touch,track }//1.是玩家触摸，2.是预设轨迹


public class HandCardManger : MonoBehaviour
{
    private static HandCardManger instance;
    public static HandCardManger Instance => instance;//单例模式

    [SerializeField]private int MaxHandCardCount = 10;//最大手牌数量
    public List<GameObject> HandCardList = new List<GameObject>();//手牌列表
    [SerializeField] private GameObject HandCarPrefabs;//手牌预制体
    [SerializeField] private Transform spawnPoint;//手牌生成点,也就是起始位置
    [SerializeField] private SplineContainer SplineContainer_getCard;//spline插件里面的容器，这里是卡牌的初始轨道
    [SerializeField]private SplineContainer SplineContainer_PushCard;//这里是打出卡牌的轨道

    [SerializeField]private Transform CardParent;//卡牌的父物体

    private void Awake()
    {
        instance = this;
    }

    private void OnGUI()//测试
    {
        // 只加大按钮大小：宽200，高60（可根据需要调整数值）
        if (GUILayout.Button("创建敌人牌",
            GUILayout.Width(200),  // 按钮宽度
            GUILayout.Height(60))) // 按钮高度
        {
            GetEnemyCard();
        }
    }

    public void CreatCard()
    {
        if (HandCardList.Count > MaxHandCardCount)
        {
            UImanager.Instance.GetPanel<televisionPanel>().SetGetCardButtonState(false);//如果手牌数量大于最大手牌数量则禁用按钮
            return;
        }
        GameObject Card = PoolManage.Instance.GetObj(HandCarPrefabs);//通过对象池获取卡牌
        Card.transform.parent = CardParent;
        HandCardList.Add(Card);//添加进手牌列表
        UpdateCardPosition();//调用更新2
    }

    public void PushCard(PushType Type)
    {
        if (Card.CurrentSelectedCard == null)
            return;

        // 缓存当前选中的卡牌
        Card pushedCard = Card.CurrentSelectedCard;

        switch (Type)
        {
            case PushType.Touch:
                break;
            case PushType.track:
                PushPlayerCard(pushedCard,0.3f);
                break;
        }

        pushedCard.Push();// 标记卡牌已打出
        HandCardList.Remove(pushedCard.gameObject);
        UpdateCardPosition();
        Card.CurrentSelectedCard = null;
    }

    public void PushPlayerCard(Card card,float _totalDuration)
    {
        Spline trackSpline = SplineContainer_PushCard.Spline; // 获取曲线
        BezierKnot knot = trackSpline[0]; // 获取第一个节点

        // 将对象的世界坐标转换为曲线所在对象的局部坐标
        Vector3 cardWorldPos = card.transform.position;
        Vector3 curveLocalPos = SplineContainer_PushCard.transform.InverseTransformPoint(cardWorldPos);

        knot.Position = curveLocalPos; // 赋值转换后的局部坐标
        trackSpline.SetKnot(0, knot); // 写回曲线

        StartCoroutine(MoveCard(card.transform, SplineContainer_PushCard, _totalDuration)); // 启动移动协程
    }

    //让物体沿着曲线移动的通用协程
    IEnumerator MoveCard(Transform _transform, SplineContainer SplineTrack,float _totalDuration)
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
    }

    public void UpdateCardPosition()//更新卡牌的位置
    {
        if (HandCardList.Count <= 0)
            return;//如果手牌数量小于等于0，直接返回
        //计算每张卡牌之间的曲线比例位置
        float step = 1f / (MaxHandCardCount + 1);//计算每张卡牌之间的间隔比例，这里用算的是最小的间距
        float FirstCardPos= 0.5f - step * (HandCardList.Count - 1) / 2f;
        //首先这样写是为了两点，1.就是保证卡牌的位置居中，2.只要卡牌数量大于2张，其实第一张的位置就是零，那为什么不设置为零呢，这里要考虑到只有一张卡牌的情况
        //为什么不是等于step呢，因为step是卡牌之间的间隔，如果等于step，那么当只有两张卡牌的时候，第一张卡牌的位置就是step，也就是1，这样就不居中，并且最后一张卡牌的位置就会超出范围

        //处理所有卡牌的位置
        for (int i = 0; i < HandCardList.Count;i++)
        {
            float StartPos = FirstCardPos + step * i;//计算每张卡牌的位置
            Vector3 localSplinePos = SplineContainer_getCard.Spline.EvaluatePosition(StartPos); // 1. 先获取曲线在自身局部空间的位置
            // 2. 通过 SplineContainer 的 transform，将局部位置转换为世界位置
            Vector3 worldSplinePos = SplineContainer_getCard.transform.TransformPoint(localSplinePos);

            UnityEngine.Vector3 forWard = SplineContainer_getCard.Spline.EvaluateTangent(StartPos);//获取卡牌在曲线上的切线方向
            UnityEngine.Vector3 Up = SplineContainer_getCard.Spline.EvaluateUpVector(StartPos);//获取卡牌在曲线上的上方向
            UnityEngine.Quaternion rot = UnityEngine.Quaternion.LookRotation(Up,UnityEngine.Vector3.Cross(Up,forWard).normalized);//计算卡牌的旋转
            //利用插件Dotween来移动卡片
            HandCardList[i].transform.DOMove(worldSplinePos, 0.5f);//移动位置

            Card card = HandCardList[i].GetComponent<Card>();
            HandCardList[i].transform.DORotateQuaternion(rot, 0.5f);//旋转
            //打开协程
            StartCoroutine(WaitTime(card, worldSplinePos));//告诉卡片播放完毕

        }

    }


    IEnumerator WaitTime( Card Card,Vector3 Pos)
    {
        yield return new WaitForSeconds(0.5f);
        Card.IsAnimator = false;//动画播放完毕
        Card.SetOriginalPos(Pos);//设置卡牌的初始位置
    }

    //——————————给出敌人牌组——————————
    [Space(10)]
    [SerializeField] private SplineContainer SplineContainer_PushEnemyCard;//这里是敌人卡牌的的轨道
    public GameObject EnemyCard;//敌人卡牌预制体

    public void GetEnemyCard()//得到一张敌人牌
    {
       GameObject EnemyCardObj= PoolManage.Instance.GetObj(EnemyCard);//通过对象池获取卡牌
        //设置父对象
        EnemyCardObj.transform.parent = CardParent;
        EnemyCardObj.transform.position = SplineContainer_PushEnemyCard.Spline[0].Position;//设置初始位置
        StartCoroutine(MoveCard(EnemyCardObj.transform, SplineContainer_PushEnemyCard, 0.3f)); // 启动移动协程
    }
}
