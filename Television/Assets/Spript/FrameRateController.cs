using UnityEngine;

public class FrameRateController : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 120; // 目标帧率
    private static FrameRateController instance; // 单例实例

    private void Awake()
    {
        // 单例模式：确保全局只有一个该对象
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 关键：切换场景时不销毁该对象
            ApplyFrameRateSettings(); // 应用帧率设置
        }
        else
        {
            Destroy(gameObject); // 重复创建时销毁多余对象
        }
    }

    // 封装帧率设置逻辑，方便后续重新调整
    public void ApplyFrameRateSettings()
    {
        QualitySettings.vSyncCount = 0; // 关闭垂直同步
        Application.targetFrameRate = targetFrameRate; // 设置目标帧率
    }

    // 可选：提供动态修改帧率的方法（如根据场景需求切换）
    public void SetNewFrameRate(int newRate)
    {
        targetFrameRate = newRate;
        ApplyFrameRateSettings();
    }
}