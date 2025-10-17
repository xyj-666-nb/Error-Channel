using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Collections;
using System.Diagnostics.Contracts;//����dotween���

public enum PushType { Touch,track }//1.����Ҵ�����2.��Ԥ��켣


public class HandCardManger : MonoBehaviour
{
    private static HandCardManger instance;
    public static HandCardManger Instance => instance;//����ģʽ

    [SerializeField]private int MaxHandCardCount = 10;//�����������
    public List<GameObject> HandCardList = new List<GameObject>();//�����б�
    [SerializeField] private GameObject HandCarPrefabs;//����Ԥ����
    [SerializeField] private Transform spawnPoint;//�������ɵ�,Ҳ������ʼλ��
    [SerializeField] private SplineContainer SplineContainer_getCard;//spline�������������������ǿ��Ƶĳ�ʼ���
    [SerializeField]private SplineContainer SplineContainer_PushCard;//�����Ǵ�����ƵĹ��

    [SerializeField]private Transform CardParent;//���Ƶĸ�����

    private void Awake()
    {
        instance = this;
    }

    private void OnGUI()//����
    {
        // ֻ�Ӵ�ť��С����200����60���ɸ�����Ҫ������ֵ��
        if (GUILayout.Button("����������",
            GUILayout.Width(200),  // ��ť���
            GUILayout.Height(60))) // ��ť�߶�
        {
            GetEnemyCard();
        }
    }

    public void CreatCard()
    {
        if (HandCardList.Count > MaxHandCardCount)
        {
            UImanager.Instance.GetPanel<televisionPanel>().SetGetCardButtonState(false);//������������������������������ð�ť
            return;
        }
        GameObject Card = PoolManage.Instance.GetObj(HandCarPrefabs);//ͨ������ػ�ȡ����
        Card.transform.parent = CardParent;
        HandCardList.Add(Card);//��ӽ������б�
        UpdateCardPosition();//���ø���2
    }

    public void PushCard(PushType Type)
    {
        if (Card.CurrentSelectedCard == null)
            return;

        // ���浱ǰѡ�еĿ���
        Card pushedCard = Card.CurrentSelectedCard;

        switch (Type)
        {
            case PushType.Touch:
                break;
            case PushType.track:
                PushPlayerCard(pushedCard,0.3f);
                break;
        }

        pushedCard.Push();// ��ǿ����Ѵ��
        HandCardList.Remove(pushedCard.gameObject);
        UpdateCardPosition();
        Card.CurrentSelectedCard = null;
    }

    public void PushPlayerCard(Card card,float _totalDuration)
    {
        Spline trackSpline = SplineContainer_PushCard.Spline; // ��ȡ����
        BezierKnot knot = trackSpline[0]; // ��ȡ��һ���ڵ�

        // ���������������ת��Ϊ�������ڶ���ľֲ�����
        Vector3 cardWorldPos = card.transform.position;
        Vector3 curveLocalPos = SplineContainer_PushCard.transform.InverseTransformPoint(cardWorldPos);

        knot.Position = curveLocalPos; // ��ֵת����ľֲ�����
        trackSpline.SetKnot(0, knot); // д������

        StartCoroutine(MoveCard(card.transform, SplineContainer_PushCard, _totalDuration)); // �����ƶ�Э��
    }

    //���������������ƶ���ͨ��Э��
    IEnumerator MoveCard(Transform _transform, SplineContainer SplineTrack,float _totalDuration)
    {
        // ��ֹ���������в�����DOTween����
        _transform.DOKill();

        Transform splineContainerTrans = SplineTrack.transform;
        Spline spline = SplineTrack.Spline;

        float totalDuration = _totalDuration; // ���ƶ�ʱ��
        float elapsedTime = 0f;   // ������ʱ��

        while (elapsedTime < totalDuration)
        {
            // ���㵱ǰ���� t
            float t = elapsedTime / totalDuration;
            // ���������ϵ�λ��
            Vector3 localPos = spline.EvaluatePosition(t);
            Vector3 worldPos = splineContainerTrans.TransformPoint(localPos);

            // ֱ������λ��
            _transform.position = worldPos;
            yield return null;
            // �ۼ�ʱ��
            //����һ�����ٵ�Ч��
            var AddTime = Time.deltaTime * (1 + (elapsedTime / totalDuration));//ʱ��Խ�������Խ�죬���100%
            elapsedTime += AddTime;
            if( elapsedTime > totalDuration)
            {
                elapsedTime = totalDuration;//��ֹ����ʱ��
            }
        }

        Vector3 finalPos = splineContainerTrans.TransformPoint(spline.EvaluatePosition(1f));
        _transform.position = finalPos;
    }

    public void UpdateCardPosition()//���¿��Ƶ�λ��
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
            Vector3 localSplinePos = SplineContainer_getCard.Spline.EvaluatePosition(StartPos); // 1. �Ȼ�ȡ����������ֲ��ռ��λ��
            // 2. ͨ�� SplineContainer �� transform�����ֲ�λ��ת��Ϊ����λ��
            Vector3 worldSplinePos = SplineContainer_getCard.transform.TransformPoint(localSplinePos);

            UnityEngine.Vector3 forWard = SplineContainer_getCard.Spline.EvaluateTangent(StartPos);//��ȡ�����������ϵ����߷���
            UnityEngine.Vector3 Up = SplineContainer_getCard.Spline.EvaluateUpVector(StartPos);//��ȡ�����������ϵ��Ϸ���
            UnityEngine.Quaternion rot = UnityEngine.Quaternion.LookRotation(Up,UnityEngine.Vector3.Cross(Up,forWard).normalized);//���㿨�Ƶ���ת
            //���ò��Dotween���ƶ���Ƭ
            HandCardList[i].transform.DOMove(worldSplinePos, 0.5f);//�ƶ�λ��

            Card card = HandCardList[i].GetComponent<Card>();
            HandCardList[i].transform.DORotateQuaternion(rot, 0.5f);//��ת
            //��Э��
            StartCoroutine(WaitTime(card, worldSplinePos));//���߿�Ƭ�������

        }

    }


    IEnumerator WaitTime( Card Card,Vector3 Pos)
    {
        yield return new WaitForSeconds(0.5f);
        Card.IsAnimator = false;//�����������
        Card.SetOriginalPos(Pos);//���ÿ��Ƶĳ�ʼλ��
    }

    //�������������������������������顪������������������
    [Space(10)]
    [SerializeField] private SplineContainer SplineContainer_PushEnemyCard;//�����ǵ��˿��ƵĵĹ��
    public GameObject EnemyCard;//���˿���Ԥ����

    public void GetEnemyCard()//�õ�һ�ŵ�����
    {
       GameObject EnemyCardObj= PoolManage.Instance.GetObj(EnemyCard);//ͨ������ػ�ȡ����
        //���ø�����
        EnemyCardObj.transform.parent = CardParent;
        EnemyCardObj.transform.position = SplineContainer_PushEnemyCard.Spline[0].Position;//���ó�ʼλ��
        StartCoroutine(MoveCard(EnemyCardObj.transform, SplineContainer_PushEnemyCard, 0.3f)); // �����ƶ�Э��
    }
}
