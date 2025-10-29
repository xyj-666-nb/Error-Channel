using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_healthslider : MonoBehaviour
{
    public static UI_healthslider instance;
    public UI_healthslider Instance => instance;

    [SerializeField] private Slider MyhealthBar;
    [SerializeField] private TextMeshProUGUI HealthText;
    [SerializeField] private Image HealthImage;

    private bool IsNeedUpdate = false; // 正常状态更新标记
    private bool IsMessyUpdating = false; // 乱码状态更新标记
    private float TargetValue; // 正常状态目标值
    private float MessyTargetValue; // 乱码状态目标值

    [SerializeField] private float MessyCodeUpdateTime = 0.4f; // 乱码文本刷新间隔
    [SerializeField] private float TypeSpeed = 0.1f; // 打字速度
    [SerializeField] private float messyCodeHealthTime = 0.5f; // 乱码血条变化间隔
    private float lastTime_messyCodeHealth = 0;

    private char[] messyChars = new char[] { '%', '#', '&', '?', '*' };
    private bool IsTyping = false;


    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        // 初始化血条为实际血量
        float initialValue = (float)PlayerManager.instance.CurrentHealth / PlayerManager.instance.MaxHealth;
        MyhealthBar.value = initialValue;
        TargetValue = initialValue;
        MessyTargetValue = initialValue;
        // 初始文本显示（只调用一次，不触发循环）
        UpdateHealthText();
    }

    // 正常状态：更新血条（仅处理数值和颜色，不碰文本）
    public void UpdateHeathBar()
    {
        if (!PlayerManager.instance.IsFixShowPlayerHealth)
            return;

        IsNeedUpdate = true;
        // 计算目标值（用当前最新的血量）
        TargetValue = (float)PlayerManager.instance.CurrentHealth / PlayerManager.instance.MaxHealth;

        // 颜色反馈：回血绿，掉血红
        HealthImage.DOColor(MyhealthBar.value < TargetValue ? Color.green : Color.red, 0.2f);
    }

    // 乱码状态：更新血条（仅处理随机数值和颜色，不碰文本）
    private void UpdateMessyHealth()
    {
        if (PlayerManager.instance.IsFixShowPlayerHealth)
            return;

        IsMessyUpdating = true;
        MessyTargetValue = Random.Range(0f, 1f); // 随机目标值

        // 颜色反馈：上升绿，下降红
        HealthImage.DOColor(MyhealthBar.value < MessyTargetValue ? Color.green : Color.red, 0.2f);
    }

    // 仅负责文本显示（正常/乱码），不调用任何血条更新方法
    private void UpdateHealthText()
    {
        if (PlayerManager.instance.IsFixShowPlayerHealth)
        {
            // 正常状态：显示实际血量
            HealthText.text = $"{PlayerManager.instance.CurrentHealth}/{PlayerManager.instance.MaxHealth}";
        }
        else
        {
            // 乱码状态：启动打字动画
            if (!IsTyping)
                StartCoroutine(TypeMessyCodeCoroutine());
        }
    }

    // 乱码打字动画（仅处理文本，不影响血条更新逻辑）
    private IEnumerator TypeMessyCodeCoroutine()
    {
        IsTyping = true;
        string messyCode = GenerateMessyCode();
        HealthText.text = "";

        for (int i = 0; i < messyCode.Length; i++)
        {
            // 若中途切换到正常状态，立即终止
            if (PlayerManager.instance.IsFixShowPlayerHealth)
            {
                IsTyping = false;
                UpdateHealthText(); // 切换后同步显示正常文本
                yield break;
            }

            HealthText.text += messyCode[i];
            yield return DOTween.To(() => 0f, x => { }, 1f, TypeSpeed).WaitForCompletion();
        }

        // 打字完成后等待刷新间隔，再循环
        yield return DOTween.To(() => 0f, x => { }, 1f, MessyCodeUpdateTime).WaitForCompletion();

        IsTyping = false;
        if (!PlayerManager.instance.IsFixShowPlayerHealth)
            UpdateHealthText(); // 继续下一轮乱码
    }

    // 生成随机乱码
    private string GenerateMessyCode()
    {
        int length = Random.Range(1, 6);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < length; i++)
        {
            char randomChar = messyChars[Random.Range(0, messyChars.Length)];
            sb.Append(randomChar);
        }
        return sb.ToString();
    }

    // 终止打字动画（状态切换时用）
    private void StopTypingAnimation()
    {
        if (IsTyping)
        {
            DOTween.KillAll(false);
            IsTyping = false;
        }
    }

    private void Update()
    {
        // 状态切换时强制同步文本（避免状态变化后文本未更新）
        if (PlayerManager.instance.IsFixShowPlayerHealth)
        {
            // 从乱码切回正常时，终止打字并更新文本
            if (IsTyping)
            {
                StopTypingAnimation();
                UpdateHealthText();
            }
        }
        else
        {
            // 从正常切回乱码时，启动文本动画
            if (!IsTyping && HealthText.text.Contains("/"))
            {
                UpdateHealthText();
            }
        }

        // 乱码状态：定时更新血条（仅血条，不碰文本）
        if (!PlayerManager.instance.IsFixShowPlayerHealth)
        {
            if (Time.time >= lastTime_messyCodeHealth + messyCodeHealthTime)
            {
                lastTime_messyCodeHealth = Time.time;
                UpdateMessyHealth();
            }
        }

        // 正常状态：血条平滑过渡
        if (PlayerManager.instance.IsFixShowPlayerHealth && IsNeedUpdate)
        {
            if (Mathf.Abs(MyhealthBar.value - TargetValue) > 0.01f)
            {
                MyhealthBar.value = Mathf.Lerp(MyhealthBar.value, TargetValue, 0.05f);
            }
            else
            {
                MyhealthBar.value = TargetValue;
                IsNeedUpdate = false;
                HealthImage.DOColor(Color.red, 0.2f); // 恢复默认红色
            }
        }
        // 乱码状态：血条平滑过渡
        else if (!PlayerManager.instance.IsFixShowPlayerHealth && IsMessyUpdating)
        {
            if (Mathf.Abs(MyhealthBar.value - MessyTargetValue) > 0.01f)
            {
                MyhealthBar.value = Mathf.Lerp(MyhealthBar.value, MessyTargetValue, 0.05f);
            }
            else
            {
                MyhealthBar.value = MessyTargetValue;
                IsMessyUpdating = false;
                HealthImage.DOColor(Color.red, 0.2f); // 恢复默认红色
            }
        }
    }
}