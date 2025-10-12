using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Collections;//引入dotween插件

public class HandCardManger : MonoBehaviour
{
    private static HandCardManger instance;
    public static HandCardManger Instance => instance;//单例模式

    [SerializeField]private int MaxHandCardCount = 10;//最大手牌数量
    [SerializeField] private List<GameObject> HandCardList = new List<GameObject>();//手牌列表
    [SerializeField] private GameObject HandCarPrefabs;//手牌预制体
    [SerializeField] private Transform spawnPoint;//手牌生成点,也就是起始位置
    [SerializeField] private SplineContainer SplineContainer;//spline插件里面的容器
    [SerializeField]private Transform CardParent;//卡牌的父物体

    private void Awake()
    {
        instance = this;
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

    private void UpdateCardPosition()//更新卡牌的位置
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
            Vector3 localSplinePos = SplineContainer.Spline.EvaluatePosition(StartPos); // 1. 先获取曲线在自身局部空间的位置
            // 2. 通过 SplineContainer 的 transform，将局部位置转换为世界位置
            Vector3 worldSplinePos = SplineContainer.transform.TransformPoint(localSplinePos);

            UnityEngine.Vector3 forWard = SplineContainer.Spline.EvaluateTangent(StartPos);//获取卡牌在曲线上的切线方向
            UnityEngine.Vector3 Up = SplineContainer.Spline.EvaluateUpVector(StartPos);//获取卡牌在曲线上的上方向
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
}
