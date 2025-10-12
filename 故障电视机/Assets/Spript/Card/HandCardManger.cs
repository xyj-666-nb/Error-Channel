using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;//����dotween���

public class HandCardManger : MonoBehaviour
{
    [SerializeField]private int MaxHandCardCount = 10;//�����������
    [SerializeField] private List<GameObject> HandCardList = new List<GameObject>();//�����б�
    [SerializeField] private GameObject HandCarPrefabs;//����Ԥ����
    [SerializeField] private Transform spawnPoint;//�������ɵ�,Ҳ������ʼλ��
    [SerializeField] private SplineContainer SplineContainer;//spline������������
    [SerializeField]private Transform CardParent;//���Ƶĸ�����

    private void Update()
    {
        //���в���
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            CreatCard();
        }
    }

    public void CreatCard()
    {
        if (HandCardList.Count > MaxHandCardCount)
            return;
        GameObject Card = PoolManage.Instance.GetObj(HandCarPrefabs);//ͨ������ػ�ȡ����
        Card.transform.parent = CardParent;
        HandCardList.Add(Card);//��ӽ������б�
        UpdateCardPosition();//���ø���2
    }

    private void UpdateCardPosition()//���¿��Ƶ�λ��
    {
        if (HandCardList.Count <= 0)
            return;//�����������С�ڵ���0��ֱ�ӷ���
        //����ÿ�ſ���֮������߱���λ��
        float step = 1f / (MaxHandCardCount + 1);//����ÿ�ſ���֮��ļ���������������������С�ļ��
        float FirstCardPos= 0.5f - step * (HandCardList.Count - 1) / 2f;
        //��������д��Ϊ�����㣬1.���Ǳ�֤���Ƶ�λ�þ��У�2.ֻҪ������������2�ţ���ʵ��һ�ŵ�λ�þ����㣬��Ϊʲô������Ϊ���أ�����Ҫ���ǵ�ֻ��һ�ſ��Ƶ����
        //Ϊʲô���ǵ���step�أ���Ϊstep�ǿ���֮��ļ�����������step����ô��ֻ�����ſ��Ƶ�ʱ�򣬵�һ�ſ��Ƶ�λ�þ���step��Ҳ����1�������Ͳ����У��������һ�ſ��Ƶ�λ�þͻᳬ����Χ

        //�������п��Ƶ�λ��
        for (int i = 0; i < HandCardList.Count;i++)
        {
            float StartPos = FirstCardPos + step * i;//����ÿ�ſ��Ƶ�λ��
            Vector3 localSplinePos = SplineContainer.Spline.EvaluatePosition(StartPos); // 1. �Ȼ�ȡ����������ֲ��ռ��λ��
            // 2. ͨ�� SplineContainer �� transform�����ֲ�λ��ת��Ϊ����λ��
            Vector3 worldSplinePos = SplineContainer.transform.TransformPoint(localSplinePos);

            UnityEngine.Vector3 forWard = SplineContainer.Spline.EvaluateTangent(StartPos);//��ȡ�����������ϵ����߷���
            UnityEngine.Vector3 Up = SplineContainer.Spline.EvaluateUpVector(StartPos);//��ȡ�����������ϵ��Ϸ���
            UnityEngine.Quaternion rot = UnityEngine.Quaternion.LookRotation(Up,UnityEngine.Vector3.Cross(Up,forWard).normalized);//���㿨�Ƶ���ת
            //���ò��Dotween���ƶ���Ƭ
            HandCardList[i].transform.DOMove(worldSplinePos, 0.5f);//�ƶ�λ��
            HandCardList[i].transform.DORotateQuaternion(rot, 0.5f);//��ת
        }



    }
}
