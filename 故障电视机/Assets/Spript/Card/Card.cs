using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;

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
    [SerializeField] private SpriteRenderer ContentImage;
    private Vector3 originalPos;    // 初始位置
    private Vector3 originalScale;  // 初始缩放
    private Vector3 activatedPos;   // 激活状态的位置

    [SerializeField] private float MoveDistance = 2f;
    [SerializeField] private float ScaleSize = 1.2f;
    [SerializeField] private float AnimDuration = 0.7f; // 动画时长

    public static Card CurrentSelectedCard;//全局当前选中的卡牌

    private bool IsPushed = false;//是否已经被打出

    private bool IsUp = false;//是否已经抬起
    private bool IsDrag = false;//是否正在拖动
    private Coroutine judgeClickCoroutine; // 协程引用，用于停止协程
    private void Awake()
    {
        SetSortingLayer(SortingLayerType.Card);
        IsPushed = false;
        IsUp = false;
        IsDrag = false;
    }

    private void Start()
    {
        IsAnimator = true; // 初始有动画，外部触发结束
    }

    public void Push()
    {
        IsPushed = true;

        //更改标签
          this.gameObject.tag = "Default";
    }

    public void SetOriginalPos(Vector3 pos)//设置卡牌的初始位置
    {
        originalPos = pos;
        transform.position = originalPos;
        originalScale = transform.localScale;
        activatedPos = originalPos - transform.up * MoveDistance; // 计算激活状态位置
    }

    // 设置渲染层级
    private void SetSortingLayer(SortingLayerType type)
    {
        if (BackGround != null) BackGround.sortingLayerName = type.ToString();
        if (ContentImage != null) ContentImage.sortingLayerName = type.ToString();
    }

    // 触摸按下瞬间：切换激活/取消状态
    private void OnMouseDown()
    {
        Debug.Log("按下");
        if (IsAnimator || IsPushed) return; // 动画中忽略

        // 重置状态
        IsUp = false;
        IsDrag = false;

        // 如果已经有判断协程在运行，先停止
        if (judgeClickCoroutine != null)
        {
            StopCoroutine(judgeClickCoroutine);
        }

        if (CurrentSelectedCard != this)
        {
            // 点击其他卡牌：先取消上一张（若存在），再激活当前
            if (CurrentSelectedCard != null)
                CurrentSelectedCard.SetDeactivate();
            SetActivate();
        }

        //开启一个协程去判断是点击还是拖动
        judgeClickCoroutine = StartCoroutine(JudgeClick());
    }

    // 触摸拖拽中
    private void OnMouseDrag()
    {
        // 如果不被允许拖动就退出
        if (!IsDrag)
            return;

        //我们将鼠标的屏幕坐标转化为世界坐标
        if (IsPushed || !IsChoose) return;//只有被选中的卡牌才可以拖动
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // 保持z轴不变
        transform.position = mousePos;

    }
  
    IEnumerator JudgeClick()
    {
        float JudgeTime = 0.1f;
        float timer = 0f;

        while (timer < JudgeTime)
        {
            timer += Time.deltaTime;

            // 如果过程中抬起了，就认为是点击，结束协程
            if (IsUp)
            {
                IsDrag = false;
                yield break;
            }

            yield return null;
        }

        // 超过时间没有抬起，就认为是拖动
        IsDrag = true;
        Debug.Log("开始拖动");
    }

    private void OnMouseUp()
    {
        // 设置抬起状态
        IsUp = true;

        // 停止判断协程
        if (judgeClickCoroutine != null)
        {
            StopCoroutine(judgeClickCoroutine);
            judgeClickCoroutine = null;
        }

        if (IsPushed || !IsChoose) return;

        // 如果是拖动状态，归位到激活状态的位置
        if (IsDrag)
        {
            //判断当前的push区域是否激活，如果存在激活就进行Push
            if (AiCardArea.Instance.IsTrigger == true)
            {
                AiCardArea.Instance.PushCard(this);
                return;
            }

            //如果这时候的回收区域打开就进行回收
            if (RecycleArea.Instance.IsTrigger)
            {
                RecycleArea.Instance.RecycleCard(this);
                return;
            }

            ReturnToActivatedPosition();
        }
        else
        {
            // 如果是点击状态，检查是否应该取消选中
            if (ShouldDeselect())
            {
                SetDeactivate();
            }
        }

        // 重置拖动状态
        IsDrag = false;
    }

    // 新增：归位到激活状态的位置
    private void ReturnToActivatedPosition()
    {
        transform.DOKill();
        transform.DOMove(activatedPos, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => IsAnimator = false);
    }

    // 检查是否应该取消选中
    private bool ShouldDeselect()
    {
        // 如果拖拽距离很小，视为点击，取消选中
        // 如果拖拽距离较大，保持选中状态
        Vector3 currentPos = transform.position;
        float dragDistance = Vector3.Distance(currentPos, activatedPos);
        return dragDistance < 0.5f; // 调整这个阈值
    }

    // 激活当前卡牌
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
                Debug.Log($"卡牌激活完成，位置: {transform.position}");
            });
    }

    // 取消激活（还原到原始位置）
    public void SetDeactivate()
    {
        IsDrag = false;
        IsChoose = false;//直接设置为未选中状态
        // 只在当前卡牌是全局选中时，才清除静态变量
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
                Debug.Log($"卡牌取消激活完成，位置: {transform.position}");
            });
    }

    // 检测点击非卡牌区域，自动取消激活
    private void Update()
    {
        if (IsPushed)
            return;

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