using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ShowGold : MonoBehaviour
{
    // ����ģʽ
    private static UI_ShowGold instance;
    public static UI_ShowGold Instance => instance;

    private TextMeshProUGUI goldText;
    private int targetGold; // Ŀ������ֵ
    private int currentGold; // ��ǰ��ʾ�Ľ����ֵ

    [SerializeField] Transform RecycleGoldPos; // ���ս��λ��

    [Header("��������")]
    [Tooltip("������ұ仯��ʱ�䣨�룩")]
    public float timePerGold = 0.2f;
    [Tooltip("�����������ߣ����ƹ���ƽ���ȣ�")]
    public Ease easeType = Ease.OutQuad;
    [Tooltip("����UpdateGold���ӳ�ִ�е�ʱ�䣨�룩")]
    public float delayTime = 0.5f; // �������ӳ�ʱ��

    private Tween goldTween;
    private Coroutine delayGoldCoroutine; // �洢��ǰ���ӳ�Э�̣�����ȡ����

    private void Awake()
    {
        instance = this;
        goldText = GetComponent<TextMeshProUGUI>();
        currentGold = 0;
        targetGold = 0;
    }

    public void UpdateGold(int gold)
    {
        if (!PlayerManager.instance.IsObtainShowGoldSkill)
        {
            // δ��ü��ܣ�������ʾδ֪����ֹ���ж������ӳ�
            goldText.text = "��ң�##?";
            targetGold = currentGold;
            goldTween?.Kill();
            if (delayGoldCoroutine != null)
                StopCoroutine(delayGoldCoroutine); // ȡ���ӳ�Э��
            return;
        }

        // Ŀ��ֵδ�仯������ִ��
        if (gold == targetGold)
        {
            // ȡ�����ܴ��ڵ��ӳ�Э�̣�����еĻ���
            if (delayGoldCoroutine != null)
                StopCoroutine(delayGoldCoroutine);
            return;
        }

        // ȡ��֮ǰ���ӳ�Э�̣�ȷ�������µ���Ϊ׼��
        if (delayGoldCoroutine != null)
            StopCoroutine(delayGoldCoroutine);

        // �����µ��ӳ�Э�̣��ȴ�0.5���ִ�и����߼�
        targetGold = gold; // �ȼ�¼Ŀ��ֵ���ӳ��ڼ���ܱ�����޸ģ������һ��Ϊ׼��
        delayGoldCoroutine = StartCoroutine(DelayUpdateGold());
    }

    // �ӳ�Э�̣��ȴ�delayTime��ִ�н�Ҹ��¶���
    private IEnumerator DelayUpdateGold()
    {
        yield return new WaitForSeconds(delayTime); // �ȴ�0.5��

        // �ȴ�������ִ�ж����߼�
        int delta = targetGold - currentGold;
        float totalDuration = Mathf.Abs(delta) * timePerGold;

        // ��ֹ��ǰ�������еĶ����������ͻ��
        goldTween?.Kill();

        // ������ҹ�������
        goldTween = DOTween.To(
                () => currentGold,
                value =>
                {
                    currentGold = value;
                    goldText.text = $"��ң�{currentGold}";
                },
                targetGold,
                totalDuration
            )
            .SetEase(easeType)
            .OnComplete(() =>
            {
                currentGold = targetGold;
                goldText.text = $"��ң�{currentGold}";
            });

        // Э�̽������������
        delayGoldCoroutine = null;
    }

    public void RecycleGold(GameObject Gold, GameObject Prefabs)
    {
        Gold.transform.DOMove(RecycleGoldPos.position, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                PoolManage.Instance.PushObj(Prefabs, Gold);
            });
    }
}