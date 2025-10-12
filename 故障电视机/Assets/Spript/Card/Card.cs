using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

enum SortingLayerType
{
    Card,
    FrontCard,
}

public class Card : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private bool IsChoose = false;
    public bool IsAnimator = false;
    [SerializeField] private SpriteRenderer BackGround;
    [SerializeField] private SpriteRenderer ContentImage;
    private Vector3 originalPos;    // ��ʼλ��
    private Vector3 originalScale;  // ��ʼ����

    [SerializeField] private float MoveDistance = 2f;
    [SerializeField] private float ScaleSize = 1.2f;
    [SerializeField] private float AnimDuration = 0.7f; // ����ʱ��

    public static Card CurrentSelectedCard;//ȫ�ֵ�ǰѡ�еĿ���

    private void Awake()
    {
        SetSortingLayer(SortingLayerType.Card);
    }

    private void Start()
    {
        IsAnimator = true; // ��ʼ�ж������ⲿ��������
    }

    public void SetOriginalPos(Vector3 pos)//���ÿ��Ƶĳ�ʼλ��
    {
        originalPos = pos;
        transform.position = originalPos;
        originalScale = transform.localScale;
    }

    // ������Ⱦ�㼶
    private void SetSortingLayer(SortingLayerType type)
    {
        if (BackGround != null) BackGround.sortingLayerName = type.ToString();
        if (ContentImage != null) ContentImage.sortingLayerName = type.ToString();
    }

    // ��������˲�䣺�л�����/ȡ��״̬
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("����");
        if (IsAnimator) return; // �����к���

        if (CurrentSelectedCard == this)
            SetDeactivate();
        else
        {
            // ����������ƣ���ȡ����һ�ţ������ڣ����ټ��ǰ
            if (CurrentSelectedCard != null)
                   CurrentSelectedCard.SetDeactivate();
            SetActivate();
        }
    }

    // ���ǰ����
    public void SetActivate()
    {
        CurrentSelectedCard = this;
        IsChoose = true;
        SetSortingLayer(SortingLayerType.FrontCard);

        transform.DOKill();
        Sequence seq = DOTween.Sequence()
            .Append(transform.DOMove(originalPos - transform.up * MoveDistance, AnimDuration)) // ���Ƹ�-Ϊ+
            .Join(transform.DOScale(originalScale * ScaleSize, AnimDuration))
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => IsAnimator = false);
    }

    // ȡ�������ԭ��
    public void SetDeactivate()
    {
        IsChoose = false;
        // ֻ�ڵ�ǰ������ȫ��ѡ��ʱ���������̬����
        if (CurrentSelectedCard == this)
        {
            CurrentSelectedCard = null;
        }

        SetSortingLayer(SortingLayerType.Card);
        transform.DOKill();

        Sequence seq = DOTween.Sequence()
            .Append(transform.DOMove(originalPos, AnimDuration))
            .Join(transform.DOScale(originalScale, AnimDuration))
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => IsAnimator = false);
    }

    // ������ǿ��������Զ�ȡ������
    private void Update()
    {
        // ������ѡ�п���ʱ���
        if (CurrentSelectedCard != null && !IsAnimator)
        {
            // �ƶ��ˣ����������Ҳ��ڿ�����
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                CheckOutsideRelease(Input.GetTouch(0).position);
            }
            // PC�ˣ�������̧���Ҳ��ڿ�����
            else if (Input.GetMouseButtonUp(0))
            {
                CheckOutsideRelease(Input.mousePosition);
            }
        }
    }

    // ��ⴥ��/����Ƿ��ڿ��������
    private void CheckOutsideRelease(Vector2 screenPos)
    {
        // ���߼�⵱ǰλ���Ƿ����п���
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // ��δ�����κο��ƣ���ȡ����ǰѡ��
        bool hitAnyCard = false;
        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<Card>() != null)
            {
                hitAnyCard = true;
                break;
            }
        }
        if (!hitAnyCard && CurrentSelectedCard != null)
        {
            CurrentSelectedCard.SetDeactivate();
        }
    }
}