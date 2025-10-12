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
    private Vector3 originalPos;    // 初始位置
    private Vector3 originalScale;  // 初始缩放

    [SerializeField] private float MoveDistance = 2f;
    [SerializeField] private float ScaleSize = 1.2f;
    [SerializeField] private float AnimDuration = 0.7f; // 动画时长

    public static Card CurrentSelectedCard;//全局当前选中的卡牌

    private void Awake()
    {
        SetSortingLayer(SortingLayerType.Card);
    }

    private void Start()
    {
        IsAnimator = true; // 初始有动画，外部触发结束
    }

    public void SetOriginalPos(Vector3 pos)//设置卡牌的初始位置
    {
        originalPos = pos;
        transform.position = originalPos;
        originalScale = transform.localScale;
    }

    // 设置渲染层级
    private void SetSortingLayer(SortingLayerType type)
    {
        if (BackGround != null) BackGround.sortingLayerName = type.ToString();
        if (ContentImage != null) ContentImage.sortingLayerName = type.ToString();
    }

    // 触摸按下瞬间：切换激活/取消状态
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("按下");
        if (IsAnimator) return; // 动画中忽略

        if (CurrentSelectedCard == this)
            SetDeactivate();
        else
        {
            // 点击其他卡牌：先取消上一张（若存在），再激活当前
            if (CurrentSelectedCard != null)
                   CurrentSelectedCard.SetDeactivate();
            SetActivate();
        }
    }

    // 激活当前卡牌
    public void SetActivate()
    {
        CurrentSelectedCard = this;
        IsChoose = true;
        SetSortingLayer(SortingLayerType.FrontCard);

        transform.DOKill();
        Sequence seq = DOTween.Sequence()
            .Append(transform.DOMove(originalPos - transform.up * MoveDistance, AnimDuration)) // 上移改-为+
            .Join(transform.DOScale(originalScale * ScaleSize, AnimDuration))
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => IsAnimator = false);
    }

    // 取消激活（还原）
    public void SetDeactivate()
    {
        IsChoose = false;
        // 只在当前卡牌是全局选中时，才清除静态变量
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

    // 检测点击非卡牌区域，自动取消激活
    private void Update()
    {
        // 仅在有选中卡牌时检测
        if (CurrentSelectedCard != null && !IsAnimator)
        {
            // 移动端：触摸结束且不在卡牌上
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                CheckOutsideRelease(Input.GetTouch(0).position);
            }
            // PC端：鼠标左键抬起且不在卡牌上
            else if (Input.GetMouseButtonUp(0))
            {
                CheckOutsideRelease(Input.mousePosition);
            }
        }
    }

    // 检测触摸/鼠标是否在卡牌外结束
    private void CheckOutsideRelease(Vector2 screenPos)
    {
        // 射线检测当前位置是否命中卡牌
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // 若未命中任何卡牌，则取消当前选中
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