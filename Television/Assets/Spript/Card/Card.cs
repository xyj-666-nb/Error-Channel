using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

enum SortingLayerType
{
    Card,
    FrontCard,
}

public class Card : MonoBehaviour
{
    [SerializeField] private bool IsChoose = false;
    public bool IsAnimator = false;
    [SerializeField] private SpriteRenderer CardFrond;
    [SerializeField] private SpriteRenderer CardBack;
    private Vector3 originalPos;    // 初始位置
    private Vector3 originalScale;  // 初始缩放
    private Vector3 activatedPos;   // 激活状态的位置

    [SerializeField] private float MoveDistance = 2f;
    [SerializeField] private float ScaleSize = 1.2f;
    [SerializeField] private float AnimDuration = 0.7f; // 动画时长
    public static Card CurrentSelectedCard_1;//全局当前选中的卡牌
    public static Card CurrentSelectedCard_2;//全局选中的第二张牌
    public static int CurrentCanSelectCardNumber = 1;//默认可选择1张
    public static bool CanInitCar = true;
    public CardNumber MyNumber;
    [SerializeField] private Canvas MyCanvas;
    private bool IsPushed = false;//是否已经被打出

    private bool IsUp = false;//是否已经抬起
    private bool IsDrag = false;//是否正在拖动
    private Coroutine judgeClickCoroutine; // 协程引用，用于停止协程
    private Vector2 lastTouchPos; // 记录上一帧触摸位置，用于检测移动
    private bool isTouching = false; // 标记是否处于触摸中
    private Collider2D cardCollider; // 缓存碰撞体

    private bool IsFlip = false;
    public bool IsCanInteractive = true; // 控制是否允许交互


    // 翻牌按钮的引用
    private UnityEngine.UI.Button flipCardButton;
    public CardDesignMode MyDesignMode;

    // 获取未被打出的有效选中牌（双选打1张后用）
    private static Card GetValidUnpushedCard()
    {
        if (CurrentSelectedCard_1 != null && !CurrentSelectedCard_1.IsPushed)
            return CurrentSelectedCard_1;
        if (CurrentSelectedCard_2 != null && !CurrentSelectedCard_2.IsPushed)
            return CurrentSelectedCard_2;
        return null;
    }

    private void Awake()
    {
        // 启用增强触摸并注册事件（核心优化）
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerUp += OnFingerUp;
        Touch.onFingerMove += OnFingerMove;

        SetSortingLayer(SortingLayerType.Card);
        IsPushed = false;
        IsUp = false;
        IsDrag = false;
        isTouching = false;

        // 缓存碰撞体（避免反复获取）
        cardCollider = GetComponent<Collider2D>();
        if (cardCollider == null)
        {
            Debug.LogError("卡牌缺少Collider2D组件！请添加BoxCollider2D", this);
            enabled = false; // 缺少碰撞体直接禁用脚本
            return;
        }

        // 翻牌按钮空值保护
        if (UImanager.Instance != null)
        {
            var tvPanel = UImanager.Instance.GetPanel<televisionPanel>();
            if (tvPanel != null && tvPanel.controlDic.ContainsKey("FlipCardButton"))
            {
                flipCardButton = tvPanel.controlDic["FlipCardButton"].GetComponent<UnityEngine.UI.Button>();
            }
        }
    }

    private void Start()
    {
        IsAnimator = true; // 初始有动画，外部触发结束
        // 弱化PlayerManager依赖（使用默认值保底）
        if (PlayerManager.instance != null)
        {
            CurrentCanSelectCardNumber = PlayerManager.instance.CurrentMaxSelectCardMount;
        }
        else
        {
            CurrentCanSelectCardNumber = 1; // 默认值
            Debug.LogWarning("PlayerManager实例不存在，使用默认选择数量", this);
        }

        if (CardNumberInfo.Instance != null)
        {
            CardNumberInfo.Instance.SetCardInfo(this);
        }
    }

    // 手指按下事件（替代原Update中的触摸开始检测）
    private void OnFingerDown(Finger finger)
    {
        if (!IsCanInteractive || IsAnimator || IsPushed || !enabled)
            return;

        Vector2 touchPos = finger.currentTouch.screenPosition;
        if (!IsValidTouchPosition(touchPos))
            return;


        if (!IsTouchHitCard(touchPos))
            return;

        // 执行触摸开始逻辑
        IsUp = false;
        IsDrag = false;
        isTouching = true;
        lastTouchPos = touchPos;

        if (judgeClickCoroutine != null)
            StopCoroutine(judgeClickCoroutine);

        // 选择逻辑（保留原有逻辑但增加空值保护）
        int maxSelect = PlayerManager.instance != null ? PlayerManager.instance.CurrentMaxSelectCardMount : 1;
        if (maxSelect == 1)
        {
            SingleSelectLogic();
        }
        else if (maxSelect == 2)
        {
            DoubleSelectLogic();
        }

        judgeClickCoroutine = StartCoroutine(JudgeClick());
    }

    void OnEnable()
    {
        if (!EnhancedTouchSupport.enabled)
            EnhancedTouchSupport.Enable();

        TouchSimulation.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerMove += OnFingerMove;
        Touch.onFingerUp += OnFingerUp;
    }

    void OnDisable()
    {
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerMove -= OnFingerMove;
        Touch.onFingerUp -= OnFingerUp;
    }

    // 手指移动事件
    private void OnFingerMove(Finger finger)
    {
        if (!IsCanInteractive || !isTouching || IsPushed || !IsChoose || Camera.main == null)
            return;

        Vector2 touchPos = finger.currentTouch.screenPosition;
        if (!IsValidTouchPosition(touchPos))
            return;

        // 拖动逻辑
        if (IsDrag)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, Camera.main.nearClipPlane));
            worldPos.z = 0;
            transform.position = worldPos;

            if (transform.localScale != originalScale)
                transform.DOScale(originalScale, AnimDuration);
        }
        // 检测是否触发拖动
        else if (Vector2.Distance(touchPos, lastTouchPos) > 5f)
        {
            IsDrag = true;
        }

        lastTouchPos = touchPos;
    }

    // 手指抬起事件
    private void OnFingerUp(Finger finger)
    {
        if (!IsCanInteractive || !isTouching || !enabled)
            return;

        Vector2 touchPos = finger.currentTouch.screenPosition;
        IsUp = true;
        isTouching = false;

        if (judgeClickCoroutine != null)
        {
            StopCoroutine(judgeClickCoroutine);
            judgeClickCoroutine = null;
        }

        if (IsPushed || !IsChoose)
            return;

        // 处理抬起逻辑
        if (IsDrag)
        {
            bool isHandled = false;
            if (AiCardArea.Instance != null && AiCardArea.Instance.IsTrigger)
            {
                AiCardArea.Instance.PushCard(this);
                isHandled = true;
            }
            else if (RecycleArea.Instance != null && RecycleArea.Instance.IsTrigger)
            {
                RecycleArea.Instance.RecycleCard(this);
                isHandled = true;
            }

            if (!isHandled)
            {
                ReturnToActivatedPosition();
            }
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

    #region 选择逻辑简化
    private void SingleSelectLogic()
    {
        if (CurrentSelectedCard_1 != this)
        {
            if (CurrentSelectedCard_1 != null)
                CurrentSelectedCard_1.SetDeactivate();
            SetActivate();
        }
        else
        {
            SetDeactivate();
        }
    }

    private void DoubleSelectLogic()
    {
        if (CurrentCanSelectCardNumber <= 0)
        {
            if (CurrentSelectedCard_1 == this) CurrentSelectedCard_1.SetDeactivate();
            if (CurrentSelectedCard_2 == this) CurrentSelectedCard_2.SetDeactivate();
            return;
        }

        if (CurrentSelectedCard_1 == this || CurrentSelectedCard_2 == this)
        {
            SetDeactivate();
            return;
        }

        if (CurrentCanSelectCardNumber == 1)
        {
            Card validCard = GetValidUnpushedCard();
            if (validCard != null)
                validCard.SetDeactivate();
            SetActivate();
            return;
        }

        if (CurrentSelectedCard_1 == null)
        {
            SetActivate();
        }
        else if (CurrentSelectedCard_2 == null)
        {
            SetActivate();
        }
        else
        {
            CurrentSelectedCard_1.SetDeactivate();
            SetActivate();
        }
    }
    #endregion

    #region 核心检测方法优化
    // 触摸命中检测（使用OverlapPoint更可靠）
    private bool IsTouchHitCard(Vector2 screenPos)
    {
        if (cardCollider == null || Camera.main == null)
            return false;

        // 将屏幕坐标转换为世界坐标，z 深度根据摄像机与卡牌距离动态计算
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(
                screenPos.x,
                screenPos.y,
                Mathf.Abs(Camera.main.transform.position.z - transform.position.z)
            )
        );

        // OverlapPoint 仅检测2D碰撞器是否包含该点
        return cardCollider.OverlapPoint(worldPos);
    }

    // 触摸坐标验证（严格过滤无效值）
    private bool IsValidTouchPosition(Vector2 pos)
    {
        // 过滤无穷大/NaN
        if (float.IsInfinity(pos.x) || float.IsInfinity(pos.y) ||
            float.IsNaN(pos.x) || float.IsNaN(pos.y))
        {
            return false;
        }

        // 过滤屏幕外坐标
        if (Camera.main == null) return false;
        Rect cameraRect = Camera.main.pixelRect;
        return pos.x >= cameraRect.xMin - 50 && pos.x <= cameraRect.xMax + 50 &&
               pos.y >= cameraRect.yMin - 50 && pos.y <= cameraRect.yMax + 50;
    }
    #endregion

    public void Flip()
    {
        IsFlip = !IsFlip;
        CardFrond.transform.DOKill();

        Vector3 currentLocalRot = CardFrond.transform.localEulerAngles;
        Vector3 targetLocalRot = new Vector3(
            currentLocalRot.x,  // 保持原X轴旋转
            IsFlip ? 0f : 180f, // 只修改Y轴
            currentLocalRot.z   // 保持原Z轴旋转
        );
        CardFrond.transform.DORotate(targetLocalRot, 0.25f, (RotateMode)Space.Self);
        StartCoroutine(WaitTime());
        //设置翻牌音效
        MusicManager.Instance.PlayEffectMusic("Music/扑克翻牌", false);
    }

    public void SetCardSprite(Sprite Front, Sprite Back)
    {
        if (CardFrond != null) CardFrond.sprite = Front;
        if (CardBack != null) CardBack.sprite = Back;
    }

    IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(0.1f);
        if (MyNumber != null)
        {
            MyNumber.SetActiveNumber(IsFlip);
        }
    }

    public void setLayer(int i)
    {
        int baseOrder = i;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = baseOrder;
        if (MyCanvas != null)
        {
            MyCanvas.sortingOrder = baseOrder + 1;
            MyCanvas.overrideSorting = true;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    public void Push()
    {
        IsPushed = true;
        gameObject.tag = "Default";

        if (this == CurrentSelectedCard_1)
            CurrentSelectedCard_1 = null;
        else if (this == CurrentSelectedCard_2)
            CurrentSelectedCard_2 = null;

        if (HandCardManger.Instance != null && PlayerManager.instance != null)
        {
            int minCount = 1 + PlayerManager.instance.CurrentAdvanceCardAmount;
            if (HandCardManger.Instance.HandCardList.Count <= minCount)
            {
                if (PlayerManager.instance.CurrentMaxSelectCardMount == 1)
                {
                    HandCardManger.Instance.InitCard();
                }
                else if (PlayerManager.instance.CurrentMaxSelectCardMount == 2 && CanInitCar)
                {
                    HandCardManger.Instance.InitCard();
                }
            }
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
        if (CardFrond != null) CardFrond.sortingLayerName = type.ToString();
        if (MyCanvas != null) MyCanvas.sortingLayerName = type.ToString();
    }

    IEnumerator JudgeClick()
    {
        float JudgeTime = 0.05f;
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

    private void ReturnToActivatedPosition()
    {
        transform.DOKill();
        transform.DOScale(originalScale * ScaleSize, AnimDuration);
        transform.DOMove(activatedPos, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => {
                IsAnimator = false;
                IsChoose = true;
            });
    }

    private bool ShouldDeselect()
    {
        float dragDistance = Vector3.Distance(transform.position, activatedPos);
        return dragDistance < 0.5f;
    }

    public void SetActivate()
    {
        int maxSelect = PlayerManager.instance != null ? PlayerManager.instance.CurrentMaxSelectCardMount : 1;

        if (maxSelect == 1)
        {
            CurrentSelectedCard_1 = this;
            CurrentCanSelectCardNumber = 0;
        }
        else if (maxSelect == 2)
        {
            if (CurrentCanSelectCardNumber == 1)
            {
                Card validCard = GetValidUnpushedCard();
                if (validCard != null)
                {
                    if (validCard == CurrentSelectedCard_1)
                        CurrentSelectedCard_1 = this;
                    else
                        CurrentSelectedCard_2 = this;
                }
                else
                {
                    CurrentSelectedCard_1 = this;
                }
                CurrentCanSelectCardNumber = 0;
            }
            else
            {
                if (CurrentSelectedCard_1 == null)
                {
                    CurrentSelectedCard_1 = this;
                    CurrentCanSelectCardNumber = 1;
                }
                else if (CurrentSelectedCard_2 == null)
                {
                    CurrentSelectedCard_2 = this;
                    CurrentCanSelectCardNumber = 0;
                }
            }
        }

        SetSortingLayer(SortingLayerType.FrontCard);
        transform.DOKill();
        DOTween.Sequence()
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
        bool wasSelected1 = (CurrentSelectedCard_1 == this);
        bool wasSelected2 = (CurrentSelectedCard_2 == this);

        if (wasSelected1) CurrentSelectedCard_1 = null;
        if (wasSelected2) CurrentSelectedCard_2 = null;

        int maxSelect = PlayerManager.instance != null ? PlayerManager.instance.CurrentMaxSelectCardMount : 1;
        if (maxSelect == 2)
        {
            if (wasSelected1 || wasSelected2)
            {
                int remainingValid = (CurrentSelectedCard_1 != null && !CurrentSelectedCard_1.IsPushed) ? 1 : 0;
                remainingValid += (CurrentSelectedCard_2 != null && !CurrentSelectedCard_2.IsPushed) ? 1 : 0;
                CurrentCanSelectCardNumber = maxSelect - remainingValid;
                CurrentCanSelectCardNumber = Mathf.Clamp(CurrentCanSelectCardNumber, 0, maxSelect);
            }
        }
        else if (maxSelect == 1 && wasSelected1)
        {
            CurrentCanSelectCardNumber = 1;
        }

        SetSortingLayer(SortingLayerType.Card);
        transform.DOKill();
        DOTween.Sequence()
            .Append(transform.DOMove(originalPos, AnimDuration))
            .Join(transform.DOScale(originalScale, AnimDuration))
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => IsAnimator = false);
    }
    private void OnDestroy()
    {
        // 注销触摸事件
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerUp -= OnFingerUp;
        Touch.onFingerMove -= OnFingerMove;
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        // 锁定BackGround的X/Z旋转
        if (CardFrond != null)
        {
            Vector3 currentLocalRot = CardFrond.transform.localEulerAngles;
            CardFrond.transform.localEulerAngles = new Vector3(0f, currentLocalRot.y, 0f);
        }
        MyNumber.EquationText.color = (MyDesignMode == CardDesignMode.RedHeart || MyDesignMode == CardDesignMode.RedAngularShape) ? Color.black : new Color(255 / 255f, 124 / 255f, 62 / 255f);
    }
}