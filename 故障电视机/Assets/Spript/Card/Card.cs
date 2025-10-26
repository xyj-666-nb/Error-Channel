using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using UnityEngine.UI;
using UnityEngine.UIElements;

enum SortingLayerType
{
    Card,
    FrontCard,
}

public class Card : MonoBehaviour
{
    [SerializeField] private bool IsChoose = false;
    public bool IsAnimator = false;
    [SerializeField] private SpriteRenderer BackGround;
    private Vector3 originalPos;    // 初始位置
    private Vector3 originalScale;  // 初始缩放
    private Vector3 activatedPos;   // 激活状态的位置

    [SerializeField] private float MoveDistance = 2f;
    [SerializeField] private float ScaleSize = 1.2f;
    [SerializeField] private float AnimDuration = 0.7f; // 动画时长
    public static Card CurrentSelectedCard;//全局当前选中的卡牌
    public CardNumber MyNumber;
    [SerializeField] private Canvas MyCanvas;
    private bool IsPushed = false;//是否已经被打出

    private bool IsUp = false;//是否已经抬起
    private bool IsDrag = false;//是否正在拖动
    private Coroutine judgeClickCoroutine; // 协程引用，用于停止协程

    private bool IsFlip = false;

    // 翻牌按钮的引用（通过UI管理器获取，需确保televisionPanel已注册到UImanager）
    private UnityEngine.UI.Button flipCardButton;


    private void Awake()
    {
        SetSortingLayer(SortingLayerType.Card);
        IsPushed = false;
        IsUp = false;
        IsDrag = false;

        // 注意：需根据你的UImanager实现调整获取方式
        var tvPanel = UImanager.Instance.GetPanel<televisionPanel>();
        if (tvPanel != null)
        {
            flipCardButton = tvPanel.controlDic["FlipCardButton"].GetComponent<UnityEngine.UI.Button>();
        }
    }
    public void Flip()
    {
        IsFlip = !IsFlip;
        BackGround.transform.DOKill();

        Vector3 currentLocalRot = BackGround.transform.localEulerAngles;
        Vector3 targetLocalRot = new Vector3(
            currentLocalRot.x,  // 保持原X轴旋转
            IsFlip ? 0f : 180f, // 只修改Y轴
            currentLocalRot.z   // 保持原Z轴旋转
        );
        BackGround.transform.DORotate(targetLocalRot, 0.25f, (RotateMode)Space.Self);
        StartCoroutine(WaitTime());
        //设置翻牌音效
        MusicManager.Instance.PlayEffectMusic("Music/扑克翻牌", false);
    }

    IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(0.1f);
        MyNumber.SetActiveNumber(IsFlip);
    }

    private void Start()
    {
        IsAnimator = true; // 初始有动画，外部触发结束
        CardNumberInfo.Instance.GetEquationString(this);
    }

    // 在Card类的setLayer方法中，强制统一Z轴
    public void setLayer(int i)
    {
        int baseOrder = i;
        GetComponent<SpriteRenderer>().sortingOrder = baseOrder;
        MyCanvas.sortingOrder = baseOrder + 1;
        MyCanvas.overrideSorting = true;

        // 关键：统一Z轴坐标，消除Z轴对渲染顺序的影响
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    public void RefreshData()
    {
        Color color = MyNumber.EquationText.color;
        DOTween.Sequence().Append(MyNumber.EquationText.DOFade(0, 0.3f)).OnComplete(
        () => {
            CardNumberInfo.Instance.GetEquationString(this);
            MyNumber.EquationText.DOColor(color, 0.3f);
        });
    }

    public void Push()
    {
        IsPushed = true;
        this.gameObject.tag = "Default";

        if (HandCardManger.Instance.HandCardList.Count <= 1 + PlayerManager.instance.CurrentAdvanceCardAmount)
        {
            HandCardManger.Instance.InitCard();
        }
    }

    public void SetOriginalPos(Vector3 pos)
    {
        originalPos = pos;
        transform.position = originalPos;
        originalScale = transform.localScale;
        activatedPos = originalPos - transform.up * MoveDistance;
    }

    private void SetSortingLayer(SortingLayerType type)
    {
        if (BackGround != null) BackGround.sortingLayerName = type.ToString();
        MyCanvas.sortingLayerName = type.ToString();
    }

    private void OnMouseDown()
    {
        if (IsAnimator || IsPushed) return;

        IsUp = false;
        IsDrag = false;

        if (judgeClickCoroutine != null)
        {
            StopCoroutine(judgeClickCoroutine);
        }

        if (CurrentSelectedCard != this)
        {
            if (CurrentSelectedCard != null)
                CurrentSelectedCard.SetDeactivate();
            SetActivate();
        }

        judgeClickCoroutine = StartCoroutine(JudgeClick());
    }

    private void OnMouseDrag()
    {
        if (!IsDrag)
            return;

        if (IsPushed || !IsChoose) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;

        if (transform.localScale != originalScale)
            transform.DOScale(originalScale, AnimDuration);
    }

    IEnumerator JudgeClick()
    {
        float JudgeTime = 0.1f;
        float timer = 0f;

        while (timer < JudgeTime)
        {
            timer += Time.deltaTime;
            if (IsUp)
            {
                IsDrag = false;
                yield break;
            }
            yield return null;
        }

        IsDrag = true;
    }

    private void OnMouseUp()
    {
        IsUp = true;

        if (judgeClickCoroutine != null)
        {
            StopCoroutine(judgeClickCoroutine);
            judgeClickCoroutine = null;
        }

        if (IsPushed || !IsChoose) return;

        if (IsDrag)
        {
            if (AiCardArea.Instance.IsTrigger == true)
            {
                AiCardArea.Instance.PushCard(this);
                return;
            }

            if (RecycleArea.Instance.IsTrigger)
            {
                RecycleArea.Instance.RecycleCard(this);
                return;
            }

            ReturnToActivatedPosition();
        }
        else
        {
            if (ShouldDeselect())
            {
                SetDeactivate();
            }
        }

        IsDrag = false;
    }

    private void ReturnToActivatedPosition()
    {
        transform.DOKill();
        transform.DOScale(originalScale * ScaleSize, AnimDuration);
        transform.DOMove(activatedPos, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => IsAnimator = false);
    }

    private bool ShouldDeselect()
    {
        Vector3 currentPos = transform.position;
        float dragDistance = Vector3.Distance(currentPos, activatedPos);
        return dragDistance < 0.5f;
    }

    public void SetActivate()
    {
        CurrentSelectedCard = this;
        SetSortingLayer(SortingLayerType.FrontCard);

        transform.DOKill();
        DG.Tweening.Sequence seq = DOTween.Sequence()
            .Append(transform.DOMove(activatedPos, AnimDuration))
            .Join(transform.DOScale(originalScale * ScaleSize, AnimDuration))
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => {
                IsAnimator = false;
                IsChoose = true;
            });
    }

    public void SetDeactivate()
    {
        IsDrag = false;
        IsChoose = false;
        if (CurrentSelectedCard == this)
        {
            CurrentSelectedCard = null;
        }

        SetSortingLayer(SortingLayerType.Card);
        transform.DOKill();

        DG.Tweening.Sequence seq = DOTween.Sequence()
            .Append(transform.DOMove(originalPos, AnimDuration))
            .Join(transform.DOScale(originalScale, AnimDuration))
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => {
                IsAnimator = false;
            });
    }

    private void Update()
    {
        if (IsPushed)
            return;

        // 锁定BackGround的X/Z旋转
        if (BackGround != null)
        {
            Vector3 currentLocalRot = BackGround.transform.localEulerAngles;
            BackGround.transform.localEulerAngles = new Vector3(0f, currentLocalRot.y, 0f);
        }

        if (CurrentSelectedCard != null && !IsAnimator)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                CheckOutsideRelease(Input.GetTouch(0).position);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                CheckOutsideRelease(Input.mousePosition);
            }
        }
    }

    // 检测触摸/鼠标是否在卡牌外或翻牌按钮外结束
    private void CheckOutsideRelease(Vector2 screenPos)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool hitAnyCard = false;
        bool hitFlipButton = false; // 是否点击了翻牌按钮

        foreach (var result in results)
        {
            // 检测是否命中卡牌
            if (result.gameObject.GetComponentInChildren<Card>() != null)
            {
                hitAnyCard = true;
                break;
            }

            // 检测是否命中翻牌按钮（通过引用匹配）
            if (flipCardButton != null && result.gameObject == flipCardButton.gameObject)
            {
                hitFlipButton = true;
                break;
            }
        }

        // 只有当既未命中卡牌，也未命中翻牌按钮时，才取消激活
        if (!hitAnyCard && !hitFlipButton && CurrentSelectedCard != null)
        {
            CurrentSelectedCard.SetDeactivate();
        }
    }
}