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
        // 单例模式保护
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        InitializeCameraComponents();
    }

    /// <summary>
    /// 初始化相机组件
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
                Debug.LogWarning("CinemachineVirtualCamera 上没有找到 CinemachineBasicMultiChannelPerlin 组件!");
            }
        }
        else
        {
            Debug.LogError("场景中没有找到 CinemachineVirtualCamera!");
        }

        // 初始化列表
        CurrentCamerShakeList_Time = new List<CameraShakeInfo>();
        CurrentCamerShakeList = new List<float>();
    }

    private void Update()
    {
        Update_CamerShake();
    }

    // 震动系统变量
    private CinemachineBasicMultiChannelPerlin Noise;
    private float totalStrength = 0f;

    [Header("震动消失的速度")]
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

    // 用于存储当前的震动信息
    private List<CameraShakeInfo> CurrentCamerShakeList_Time;
    private List<float> CurrentCamerShakeList;

    /// <summary>
    /// 添加时间控制的震动
    /// </summary>
    public void AddCameraShake(float ShackStrength, float ShackeTime)
    {
        if (float.IsNaN(ShackStrength) || float.IsInfinity(ShackStrength) ||
            float.IsNaN(ShackeTime) || float.IsInfinity(ShackeTime))
        {
            Debug.LogWarning("AddCameraShake: 传入的震动参数包含非法值");
            return;
        }

        // 确保列表已初始化
        if (CurrentCamerShakeList_Time == null)
            CurrentCamerShakeList_Time = new List<CameraShakeInfo>();

        CurrentCamerShakeList_Time.Add(new CameraShakeInfo(
            Mathf.Clamp(ShackStrength, 0f, 10f),
            Mathf.Clamp(ShackeTime, 0.1f, 10f)
        ));
    }

    /// <summary>
    /// 添加手动控制的震动（需要手动停止）
    /// </summary>
    public void AddCameraShake(float ShackStrength)
    {
        if (float.IsNaN(ShackStrength) || float.IsInfinity(ShackStrength))
        {
            Debug.LogWarning("AddCameraShake: 传入的震动强度包含非法值");
            return;
        }

        // 确保列表已初始化
        if (CurrentCamerShakeList == null)
            CurrentCamerShakeList = new List<float>();

        CurrentCamerShakeList.Add(Mathf.Clamp(ShackStrength, 0f, 10f));
    }

    /// <summary>
    /// 放在update处理关于摄像机震动的相关功能
    /// </summary>
    private void Update_CamerShake()
    {
        // 检查关键组件是否存在
        if (Noise == null)
        {
            // 尝试重新初始化
            InitializeCameraComponents();
            if (Noise == null)
            {
                // 如果还是null，直接返回
                return;
            }
        }

        // 确保列表已初始化
        if (CurrentCamerShakeList_Time == null)
            CurrentCamerShakeList_Time = new List<CameraShakeInfo>();
        if (CurrentCamerShakeList == null)
            CurrentCamerShakeList = new List<float>();

        totalStrength = 0f;

        // 处理时间控制的震动
        for (int idx = CurrentCamerShakeList_Time.Count - 1; idx >= 0; idx--)
        {
            // 安全检查：确保索引有效
            if (idx < 0 || idx >= CurrentCamerShakeList_Time.Count)
                continue;

            var shake = CurrentCamerShakeList_Time[idx];

            // 安全检查：确保shake不为null
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
                // 震动时间结束后，慢慢减弱
                shake.ShackStrength = Mathf.MoveTowards(shake.ShackStrength, 0f, Time.deltaTime * ShakeFadeSpeed);
                totalStrength += shake.ShackStrength;

                // 如果已经减到0，可以移除
                if (shake.ShackStrength <= 0.01f)
                {
                    CurrentCamerShakeList_Time.RemoveAt(idx);
                }
            }
        }

        // 计算最终强度并应用
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
    /// 在编辑器模式下重新初始化组件（用于调试）
    /// </summary>
    [ContextMenu("重新初始化相机组件")]
    private void ReinitializeInEditor()
    {
        InitializeCameraComponents();
        Debug.Log("相机组件重新初始化完成");
    }
}