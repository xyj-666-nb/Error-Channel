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
    public static int CurrentCanSelectCardNumber;//当前玩家还能选择的牌数
    public static bool CanInitCar = true;
    public CardNumber MyNumber;
    [SerializeField] private Canvas MyCanvas;
    private bool IsPushed = false;//是否已经被打出

    private bool IsUp = false;//是否已经抬起
    private bool IsDrag = false;//是否正在拖动
    private Coroutine judgeClickCoroutine; // 协程引用，用于停止协程
    private Vector3 lastMousePos; // 记录上一帧鼠标位置，用于快速检测移动

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
        SetSortingLayer(SortingLayerType.Card);
        IsPushed = false;
        IsUp = false;
        IsDrag = false;

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
        // 初始化可选择牌数（与最大选择数同步）
        if (PlayerManager.instance != null)
        {
            CurrentCanSelectCardNumber = PlayerManager.instance.CurrentMaxSelectCardMount;
        }
        // CardNumberInfo空值保护
        if (CardNumberInfo.Instance != null)
        {
            CardNumberInfo.Instance.SetCardInfo(this);
        }
    }

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
        // 音效空值保护
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayEffectMusic("Music/扑克翻牌", false);
        }
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

    // 在Card类的setLayer方法中，强制统一Z轴
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

        // 关键：统一Z轴坐标，消除Z轴对渲染顺序的影响
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

  

    public void Push()
    {
        IsPushed = true;
        this.gameObject.tag = "Default";

        // 清理当前牌的选中状态（配合双选出牌逻辑）
        if (this == CurrentSelectedCard_1)
        {
            CurrentSelectedCard_1 = null;
        }
        else if (this == CurrentSelectedCard_2)
        {
            CurrentSelectedCard_2 = null;
        }

        // 初始化牌逻辑（空值保护）
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

    #region 核心：双选+可选择数限制逻辑（添加交互控制）
    private void OnMouseDown()
    {
        // 新增：如果不允许交互，直接返回
        if (!IsCanInteractive) return;

        // 忽略UI遮挡（避免按钮/面板挡住卡牌点击）
        if (EventSystem.current.IsPointerOverGameObject()) return;
        // 基础拦截：动画中/已打出/PlayerManager未初始化
        if (IsAnimator || IsPushed || PlayerManager.instance == null) return;

        IsUp = false;
        IsDrag = false;
        lastMousePos = Input.mousePosition; // 记录初始鼠标位置

        // 停止之前的判断协程
        if (judgeClickCoroutine != null)
        {
            StopCoroutine(judgeClickCoroutine);
        }

        // 单选逻辑
        if (PlayerManager.instance.CurrentMaxSelectCardMount == 1)
        {
            if (CurrentSelectedCard_1 != this)
            {
                if (CurrentSelectedCard_1 != null)
                    CurrentSelectedCard_1.SetDeactivate();
                SetActivate();
            }
            else
            {
                SetDeactivate(); // 点击已选中的牌取消
            }
        }
        // 双选逻辑
        else if (PlayerManager.instance.CurrentMaxSelectCardMount == 2)
        {
            // 1. 可选择数为0：仅允许取消已选中的牌
            if (CurrentCanSelectCardNumber <= 0)
            {
                if (CurrentSelectedCard_1 == this) CurrentSelectedCard_1.SetDeactivate();
                if (CurrentSelectedCard_2 == this) CurrentSelectedCard_2.SetDeactivate();
                return;
            }

            // 2. 点击已选中的牌：取消并恢复可选择数
            if (CurrentSelectedCard_1 == this || CurrentSelectedCard_2 == this)
            {
                SetDeactivate();
                return;
            }

            // 3. 可选择数为1（打第一张牌后）：只能替换未被打出的有效牌
            if (CurrentCanSelectCardNumber == 1)
            {
                Card validCard = GetValidUnpushedCard();
                // 有有效牌：取消有效牌，激活当前牌（替换）
                if (validCard != null)
                {
                    validCard.SetDeactivate();
                }
                // 无有效牌：直接激活（极端情况防护）
                SetActivate();
                return;
            }

            // 4. 可选择数为2（正常双选）：选第1/2张
            if (CurrentSelectedCard_1 == null)
            {
                SetActivate(); // 选第1张
            }
            else if (CurrentSelectedCard_2 == null)
            {
                SetActivate(); // 选第2张
            }
            // 5. 意外情况（两张都满）：取消第1张，选当前牌
            else
            {
                CurrentSelectedCard_1.SetDeactivate();
                SetActivate();
            }
        }

        // 启动点击/拖动判断协程
        judgeClickCoroutine = StartCoroutine(JudgeClick());
    }
    #endregion

    #region 拖动判定协程
    IEnumerator JudgeClick()
    {
        float JudgeTime = 0.05f; // 缩短至0.05秒，减少等待感
        float timer = 0f;

        while (timer < JudgeTime)
        {
            timer += Time.deltaTime;
            if (IsUp) // 鼠标抬起=点击，非拖动
            {
                IsDrag = false;
                yield break;
            }

            // 检测到鼠标移动时，立即判定为拖动
            if (Input.GetMouseButton(0) && Input.mousePosition != lastMousePos)
            {
                IsDrag = true;
                yield break;
            }
            lastMousePos = Input.mousePosition; // 更新鼠标位置

            yield return null;
        }

        // 超时未抬起/未移动，也判定为拖动
        IsDrag = true;
    }
    #endregion

    private void OnMouseDrag()
    {
        // 新增：如果不允许交互，直接返回
        if (!IsCanInteractive) return;

        if (!IsDrag || IsPushed || !IsChoose || Camera.main == null) return;

        // 拖动逻辑：Z轴固定为0
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;

        // 拖动时恢复原缩放
        if (transform.localScale != originalScale)
            transform.DOScale(originalScale, AnimDuration);
    }

    private void OnMouseUp()
    {
        // 新增：如果不允许交互，直接返回
        if (!IsCanInteractive) return;

        IsUp = true;

        // 停止协程
        if (judgeClickCoroutine != null)
        {
            StopCoroutine(judgeClickCoroutine);
            judgeClickCoroutine = null;
        }

        if (IsPushed || !IsChoose) return;

        // 拖动结束：打出手牌/回收
        if (IsDrag)
        {
            bool isHandled = false;
            // 打给AI区域（空值保护）
            if (AiCardArea.Instance != null && AiCardArea.Instance.IsTrigger)
            {
                AiCardArea.Instance.PushCard(this);
                isHandled = true;
            }
            // 回收区域（空值保护）
            else if (RecycleArea.Instance != null && RecycleArea.Instance.IsTrigger)
            {
                RecycleArea.Instance.RecycleCard(this);
                isHandled = true;
            }

            // 未处理：回到激活位置
            if (!isHandled)
            {
                ReturnToActivatedPosition();
            }
        }
        // 点击结束：取消选中（距离过近）
        else
        {
            if (ShouldDeselect())
            {
                SetDeactivate();
            }
        }

        IsDrag = false;
    }

    #region 回到激活位置动画
    private void ReturnToActivatedPosition()
    {
        transform.DOKill();
        // 回到激活位置+恢复缩放
        transform.DOScale(originalScale * ScaleSize, AnimDuration);
        transform.DOMove(activatedPos, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => {
                IsAnimator = false; // 动画一结束就允许拖动，无延迟
                IsChoose = true;
            });
    }
    #endregion

    private bool ShouldDeselect()
    {
        // 点击位置与激活位置距离<0.5=取消
        float dragDistance = Vector3.Distance(transform.position, activatedPos);
        return dragDistance < 0.5f;
    }

    #region 激活逻辑
    public void SetActivate()
    {
        if (PlayerManager.instance == null) return;

        // 单选激活
        if (PlayerManager.instance.CurrentMaxSelectCardMount == 1)
        {
            CurrentSelectedCard_1 = this;
            CurrentCanSelectCardNumber = 0; // 选满后可选择数为0
        }
        // 双选激活
        else if (PlayerManager.instance.CurrentMaxSelectCardMount == 2)
        {
            // 可选择数为1（打第一张后）：赋值到未被打出的有效位置
            if (CurrentCanSelectCardNumber == 1)
            {
                Card validCard = GetValidUnpushedCard();
                if (validCard != null)
                {
                    // 替换有效牌的位置（第一张/第二张）
                    if (validCard == CurrentSelectedCard_1)
                        CurrentSelectedCard_1 = this;
                    else
                        CurrentSelectedCard_2 = this;
                }
                else
                {
                    // 无有效牌：默认赋值到第一张
                    CurrentSelectedCard_1 = this;
                }
                CurrentCanSelectCardNumber = 0; // 选满后可选择数为0
            }
            // 可选择数为2（正常双选）：分配第1/2张
            else
            {
                if (CurrentSelectedCard_1 == null)
                {
                    CurrentSelectedCard_1 = this;
                    CurrentCanSelectCardNumber = 1; // 还能选1张
                }
                else if (CurrentSelectedCard_2 == null)
                {
                    CurrentSelectedCard_2 = this;
                    CurrentCanSelectCardNumber = 0; // 选满
                }
            }
        }

        // 激活状态：排序层+动画
        SetSortingLayer(SortingLayerType.FrontCard);
        transform.DOKill();
        DG.Tweening.Sequence seq = DOTween.Sequence()
            .Append(transform.DOMove(activatedPos, AnimDuration))
            .Join(transform.DOScale(originalScale * ScaleSize, AnimDuration))
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => {
                IsAnimator = false; // 激活动画结束立即允许拖动
                IsChoose = true;
            });
    }
    #endregion

    #region 取消激活逻辑
    public void SetDeactivate()
    {
        IsDrag = false;
        IsChoose = false;
        bool wasSelected1 = (CurrentSelectedCard_1 == this);
        bool wasSelected2 = (CurrentSelectedCard_2 == this);

        // 清空选中状态
        if (wasSelected1) CurrentSelectedCard_1 = null;
        if (wasSelected2) CurrentSelectedCard_2 = null;

        // 双选模式：恢复可选择数
        if (PlayerManager.instance != null && PlayerManager.instance.CurrentMaxSelectCardMount == 2)
        {
            if (wasSelected1 || wasSelected2)
            {
                // 恢复规则：可选择数 = 最大数 - 剩余未被打出的选中牌数
                int remainingValid = (CurrentSelectedCard_1 != null && !CurrentSelectedCard_1.IsPushed) ? 1 : 0;
                remainingValid += (CurrentSelectedCard_2 != null && !CurrentSelectedCard_2.IsPushed) ? 1 : 0;
                CurrentCanSelectCardNumber = PlayerManager.instance.CurrentMaxSelectCardMount - remainingValid;
                // 防护：不超过最大数、不小于0
                CurrentCanSelectCardNumber = Mathf.Clamp(CurrentCanSelectCardNumber, 0, PlayerManager.instance.CurrentMaxSelectCardMount);
            }
        }
        // 单选模式：恢复可选择数为1
        else if (PlayerManager.instance != null && PlayerManager.instance.CurrentMaxSelectCardMount == 1 && wasSelected1)
        {
            CurrentCanSelectCardNumber = 1;
        }

        // 取消状态：排序层+动画
        SetSortingLayer(SortingLayerType.Card);
        transform.DOKill();
        DG.Tweening.Sequence seq = DOTween.Sequence()
            .Append(transform.DOMove(originalPos, AnimDuration))
            .Join(transform.DOScale(originalScale, AnimDuration))
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => {
                IsAnimator = false; // 取消动画结束立即允许拖动
            });
    }
    #endregion

    private void Update()
    {
        // 新增：如果不允许交互，直接返回（不处理空白区域取消等逻辑）
        if (!IsCanInteractive) return;

        if (IsPushed || PlayerManager.instance == null) return;

        // 锁定卡牌X/Z轴旋转
        if (CardFrond != null)
        {
            Vector3 currentLocalRot = CardFrond.transform.localEulerAngles;
            CardFrond.transform.localEulerAngles = new Vector3(0f, currentLocalRot.y, 0f);
        }

        // 检测空白区域取消选中（任意一张选中就触发）
        if ((CurrentSelectedCard_1 != null || CurrentSelectedCard_2 != null) && !IsAnimator)
        {
            // 触摸端
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                CheckOutsideRelease(Input.GetTouch(0).position);
            }
            // 鼠标端
            else if (Input.GetMouseButtonUp(0))
            {
                CheckOutsideRelease(Input.mousePosition);
            }
        }
    }

    #region 空白区域取消：同时取消两张牌
    private void CheckOutsideRelease(Vector2 screenPos)
    {
        if (EventSystem.current == null) return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool hitAnyCard = false;
        bool hitFlipButton = false;

        // 检测是否点击卡牌/翻牌按钮
        foreach (var result in results)
        {
            if (result.gameObject.GetComponentInChildren<Card>() != null)
            {
                hitAnyCard = true;
                break;
            }
            if (flipCardButton != null && result.gameObject == flipCardButton.gameObject)
            {
                hitFlipButton = true;
                break;
            }
        }

        // 未点击有效区域：取消所有选中牌
        if (!hitAnyCard && !hitFlipButton)
        {
            if (CurrentSelectedCard_1 != null) CurrentSelectedCard_1.SetDeactivate();
            if (CurrentSelectedCard_2 != null) CurrentSelectedCard_2.SetDeactivate();
        }
    }
    #endregion
}