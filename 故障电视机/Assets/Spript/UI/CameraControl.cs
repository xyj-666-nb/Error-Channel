using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour
{
    private static CameraControl _instance;
    public static CameraControl Instance => _instance;

    public CinemachineVirtualCamera virtualCamera;

    private void Awake()
    {
        // ����ģʽ����
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        InitializeCameraComponents();
    }

    /// <summary>
    /// ��ʼ��������
    /// </summary>
    private void InitializeCameraComponents()
    {
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }

        if (virtualCamera != null)
        {
            Noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            if (Noise == null)
            {
                Debug.LogWarning("CinemachineVirtualCamera ��û���ҵ� CinemachineBasicMultiChannelPerlin ���!");
            }
        }
        else
        {
            Debug.LogError("������û���ҵ� CinemachineVirtualCamera!");
        }

        // ��ʼ���б�
        CurrentCamerShakeList_Time = new List<CameraShakeInfo>();
        CurrentCamerShakeList = new List<float>();
    }

    private void Update()
    {
        Update_CamerShake();
    }

    // ��ϵͳ����
    private CinemachineBasicMultiChannelPerlin Noise;
    private float totalStrength = 0f;

    [Header("����ʧ���ٶ�")]
    [SerializeField] private float ShakeFadeSpeed = 5f;

    [System.Serializable]
    public class CameraShakeInfo
    {
        public CameraShakeInfo(float _ShackStrength, float _ShackeTime)
        {
            ShackStrength = _ShackStrength;
            ShackeTime = _ShackeTime;
        }

        public float ShackStrength;
        public float ShackeTime;
    }

    // ���ڴ洢��ǰ������Ϣ
    private List<CameraShakeInfo> CurrentCamerShakeList_Time;
    private List<float> CurrentCamerShakeList;

    /// <summary>
    /// ���ʱ����Ƶ���
    /// </summary>
    public void AddCameraShake(float ShackStrength, float ShackeTime)
    {
        if (float.IsNaN(ShackStrength) || float.IsInfinity(ShackStrength) ||
            float.IsNaN(ShackeTime) || float.IsInfinity(ShackeTime))
        {
            Debug.LogWarning("AddCameraShake: ������𶯲��������Ƿ�ֵ");
            return;
        }

        // ȷ���б��ѳ�ʼ��
        if (CurrentCamerShakeList_Time == null)
            CurrentCamerShakeList_Time = new List<CameraShakeInfo>();

        CurrentCamerShakeList_Time.Add(new CameraShakeInfo(
            Mathf.Clamp(ShackStrength, 0f, 10f),
            Mathf.Clamp(ShackeTime, 0.1f, 10f)
        ));
    }

    /// <summary>
    /// ����ֶ����Ƶ��𶯣���Ҫ�ֶ�ֹͣ��
    /// </summary>
    public void AddCameraShake(float ShackStrength)
    {
        if (float.IsNaN(ShackStrength) || float.IsInfinity(ShackStrength))
        {
            Debug.LogWarning("AddCameraShake: �������ǿ�Ȱ����Ƿ�ֵ");
            return;
        }

        // ȷ���б��ѳ�ʼ��
        if (CurrentCamerShakeList == null)
            CurrentCamerShakeList = new List<float>();

        CurrentCamerShakeList.Add(Mathf.Clamp(ShackStrength, 0f, 10f));
    }

    /// <summary>
    /// ����update�������������𶯵���ع���
    /// </summary>
    private void Update_CamerShake()
    {
        // ���ؼ�����Ƿ����
        if (Noise == null)
        {
            // �������³�ʼ��
            InitializeCameraComponents();
            if (Noise == null)
            {
                // �������null��ֱ�ӷ���
                return;
            }
        }

        // ȷ���б��ѳ�ʼ��
        if (CurrentCamerShakeList_Time == null)
            CurrentCamerShakeList_Time = new List<CameraShakeInfo>();
        if (CurrentCamerShakeList == null)
            CurrentCamerShakeList = new List<float>();

        totalStrength = 0f;

        // ����ʱ����Ƶ���
        for (int idx = CurrentCamerShakeList_Time.Count - 1; idx >= 0; idx--)
        {
            // ��ȫ��飺ȷ��������Ч
            if (idx < 0 || idx >= CurrentCamerShakeList_Time.Count)
                continue;

            var shake = CurrentCamerShakeList_Time[idx];

            // ��ȫ��飺ȷ��shake��Ϊnull
            if (shake == null)
            {
                CurrentCamerShakeList_Time.RemoveAt(idx);
                continue;
            }

            shake.ShackeTime -= Time.deltaTime;

            if (shake.ShackeTime > 0)
            {
                totalStrength += shake.ShackStrength;
            }
            else
            {
                // ��ʱ���������������
                shake.ShackStrength = Mathf.MoveTowards(shake.ShackStrength, 0f, Time.deltaTime * ShakeFadeSpeed);
                totalStrength += shake.ShackStrength;

                // ����Ѿ�����0�������Ƴ�
                if (shake.ShackStrength <= 0.01f)
                {
                    CurrentCamerShakeList_Time.RemoveAt(idx);
                }
            }
        }

        // ��������ǿ�Ȳ�Ӧ��
        float finalStrength = Mathf.Clamp(totalStrength + GetCamneraShake(), 0f, 10f);
        Noise.m_AmplitudeGain = finalStrength;
    }

    public void StopCameraShake(float ShackStrength)
    {
        if (CurrentCamerShakeList == null) return;

        CurrentCamerShakeList.RemoveAll(strength => Mathf.Approximately(strength, ShackStrength));
    }

    public void StopAllCameraShake()
    {
        if (CurrentCamerShakeList != null)
            CurrentCamerShakeList.Clear();
    }

    public void ResetAllShake()
    {
        if (CurrentCamerShakeList_Time != null)
            CurrentCamerShakeList_Time.Clear();
        if (CurrentCamerShakeList != null)
            CurrentCamerShakeList.Clear();

        totalStrength = 0f;

        if (Noise != null)
        {
            Noise.m_AmplitudeGain = 0f;
        }
    }

    private float GetCamneraShake()
    {
        if (CurrentCamerShakeList == null) return 0f;

        float total = 0f;
        foreach (var shake in CurrentCamerShakeList)
        {
            total += shake;
        }
        return Mathf.Clamp(total, 0f, 10f);
    }

    /// <summary>
    /// �ڱ༭��ģʽ�����³�ʼ����������ڵ��ԣ�
    /// </summary>
    [ContextMenu("���³�ʼ��������")]
    private void ReinitializeInEditor()
    {
        InitializeCameraComponents();
        Debug.Log("���������³�ʼ�����");
    }
}