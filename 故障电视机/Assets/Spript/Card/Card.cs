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
    private Vector3 originalPos;    // ��ʼλ��
    private Vector3 originalScale;  // ��ʼ����
    private Vector3 activatedPos;   // ����״̬��λ��

    [SerializeField] private float MoveDistance = 2f;
    [SerializeField] private float ScaleSize = 1.2f;
    [SerializeField] private float AnimDuration = 0.7f; // ����ʱ��

    public static Card CurrentSelectedCard;//ȫ�ֵ�ǰѡ�еĿ���

    private bool IsPushed = false;//�Ƿ��Ѿ������

    private bool IsUp = false;//�Ƿ��Ѿ�̧��
    private bool IsDrag = false;//�Ƿ������϶�
    private Coroutine judgeClickCoroutine; // Э�����ã�����ֹͣЭ��
    private void Awake()
    {
        SetSortingLayer(SortingLayerType.Card);
        IsPushed = false;
        IsUp = false;
        IsDrag = false;
    }

    private void Start()
    {
        IsAnimator = true; // ��ʼ�ж������ⲿ��������
    }

    public void Push()
    {
        IsPushed = true;

        //���ı�ǩ
          this.gameObject.tag = "Default";
    }

    public void SetOriginalPos(Vector3 pos)//���ÿ��Ƶĳ�ʼλ��
    {
        originalPos = pos;
        transform.position = originalPos;
        originalScale = transform.localScale;
        activatedPos = originalPos - transform.up * MoveDistance; // ���㼤��״̬λ��
    }

    // ������Ⱦ�㼶
    private void SetSortingLayer(SortingLayerType type)
    {
        if (BackGround != null) BackGround.sortingLayerName = type.ToString();
        if (ContentImage != null) ContentImage.sortingLayerName = type.ToString();
    }

    // ��������˲�䣺�л�����/ȡ��״̬
    private void OnMouseDown()
    {
        Debug.Log("����");
        if (IsAnimator || IsPushed) return; // �����к���

        // ����״̬
        IsUp = false;
        IsDrag = false;

        // ����Ѿ����ж�Э�������У���ֹͣ
        if (judgeClickCoroutine != null)
        {
            StopCoroutine(judgeClickCoroutine);
        }

        if (CurrentSelectedCard != this)
        {
            // ����������ƣ���ȡ����һ�ţ������ڣ����ټ��ǰ
            if (CurrentSelectedCard != null)
                CurrentSelectedCard.SetDeactivate();
            SetActivate();
        }

        //����һ��Э��ȥ�ж��ǵ�������϶�
        judgeClickCoroutine = StartCoroutine(JudgeClick());
    }

    // ������ק��
    private void OnMouseDrag()
    {
        // ������������϶����˳�
        if (!IsDrag)
            return;

        //���ǽ�������Ļ����ת��Ϊ��������
        if (IsPushed || !IsChoose) return;//ֻ�б�ѡ�еĿ��Ʋſ����϶�
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // ����z�᲻��
        transform.position = mousePos;

    }
  
    IEnumerator JudgeClick()
    {
        float JudgeTime = 0.1f;
        float timer = 0f;

        while (timer < JudgeTime)
        {
            timer += Time.deltaTime;

            // ���������̧���ˣ�����Ϊ�ǵ��������Э��
            if (IsUp)
            {
                IsDrag = false;
                yield break;
            }

            yield return null;
        }

        // ����ʱ��û��̧�𣬾���Ϊ���϶�
        IsDrag = true;
        Debug.Log("��ʼ�϶�");
    }

    private void OnMouseUp()
    {
        // ����̧��״̬
        IsUp = true;

        // ֹͣ�ж�Э��
        if (judgeClickCoroutine != null)
        {
            StopCoroutine(judgeClickCoroutine);
            judgeClickCoroutine = null;
        }

        if (IsPushed || !IsChoose) return;

        // ������϶�״̬����λ������״̬��λ��
        if (IsDrag)
        {
            //�жϵ�ǰ��push�����Ƿ񼤻������ڼ���ͽ���Push
            if (AiCardArea.Instance.IsTrigger == true)
            {
                AiCardArea.Instance.PushCard(this);
                return;
            }

            //�����ʱ��Ļ�������򿪾ͽ��л���
            if (RecycleArea.Instance.IsTrigger)
            {
                RecycleArea.Instance.RecycleCard(this);
                return;
            }

            ReturnToActivatedPosition();
        }
        else
        {
            // ����ǵ��״̬������Ƿ�Ӧ��ȡ��ѡ��
            if (ShouldDeselect())
            {
                SetDeactivate();
            }
        }

        // �����϶�״̬
        IsDrag = false;
    }

    // ��������λ������״̬��λ��
    private void ReturnToActivatedPosition()
    {
        transform.DOKill();
        transform.DOMove(activatedPos, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnStart(() => IsAnimator = true)
            .OnComplete(() => IsAnimator = false);
    }

    // ����Ƿ�Ӧ��ȡ��ѡ��
    private bool ShouldDeselect()
    {
        // �����ק�����С����Ϊ�����ȡ��ѡ��
        // �����ק����ϴ󣬱���ѡ��״̬
        Vector3 currentPos = transform.position;
        float dragDistance = Vector3.Distance(currentPos, activatedPos);
        return dragDistance < 0.5f; // ���������ֵ
    }

    // ���ǰ����
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
                Debug.Log($"���Ƽ�����ɣ�λ��: {transform.position}");
            });
    }

    // ȡ�������ԭ��ԭʼλ�ã�
    public void SetDeactivate()
    {
        IsDrag = false;
        IsChoose = false;//ֱ������Ϊδѡ��״̬
        // ֻ�ڵ�ǰ������ȫ��ѡ��ʱ���������̬����
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
                Debug.Log($"����ȡ��������ɣ�λ��: {transform.position}");
            });
    }

    // ������ǿ��������Զ�ȡ������
    private void Update()
    {
        if (IsPushed)
            return;

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