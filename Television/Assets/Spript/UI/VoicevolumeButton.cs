using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class VoicevolumeButton : MonoBehaviour
{
    private static VoicevolumeButton instance;
    public static VoicevolumeButton Instance => instance;
    private float LastValuue;

    [SerializeField] private Button me;
    [SerializeField] private float speed = 5;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private GameObject promptImageObj;
    private Camera mainCamera;
    private bool isDragging = false;

    private AudioSource audioSource;
    [SerializeField] private CircleCollider2D circleCollider;

    // 标记增强触摸是否已初始化
    private bool isEnhancedTouchInitialized = false;

    private void Awake()
    {
        instance = this;
        // 初始化摄像机（优先用手动拖入，其次找主摄像机，最后找场景中任意摄像机）
        mainCamera = Camera.main ?? FindObjectOfType<Camera>();

        if (circleCollider == null)
            circleCollider = GetComponent<CircleCollider2D>();

        SetButton(false);
    }

    private void OnEnable()
    {
        if (!isEnhancedTouchInitialized)
        {
            EnhancedTouchSupport.Enable();
            isEnhancedTouchInitialized = true;
        }
        // 重新获取摄像机（防止场景切换后摄像机失效）
        if (mainCamera == null)
            mainCamera = Camera.main ?? FindObjectOfType<Camera>();
    }

    private void OnDisable()
    {
        if (isEnhancedTouchInitialized)
        {
            EnhancedTouchSupport.Disable();
            isEnhancedTouchInitialized = false;
        }
        isDragging = false;
    }

    public void getaudio(AudioSource Sources)
    {
        audioSource = Sources;
    }

    public void SetButton(bool IsCan)
    {
        me.interactable = IsCan;
    }

    private void Update()
    {
        // 摄像机无效时不处理触摸
        if (mainCamera == null)
        {
            // 尝试重新获取摄像机
            mainCamera = Camera.main ?? FindObjectOfType<Camera>();
            return;
        }

        if (isEnhancedTouchInitialized)
        {
            HandleTouchInput();
        }

        if (PlayerManager.instance != null && PlayerManager.instance.FixVolume)
        {
            MusicManager.Instance.ChangeEffectVolume(GetNormalizedValue());
            MusicManager.Instance.ChangeMusicVolume(GetNormalizedValue());
        }
    }

    private void HandleTouchInput()
    {
        // 1. 检查是否有活跃触摸
        if (Touch.activeTouches.Count == 0)
            return;

        // 2. 获取第一个活跃触摸，捕获未初始化异常
        Touch touch = Touch.activeTouches[0];
        try
        {
            var tempPhase = touch.phase;
        }
        catch
        {
            return;
        }

        // 3. 处理触摸相位
        switch (touch.phase)
        {
            case UnityEngine.InputSystem.TouchPhase.Began:
                OnTouchBegan(touch);
                break;
            case UnityEngine.InputSystem.TouchPhase.Moved:
                OnTouchMoved(touch);
                break;
            case UnityEngine.InputSystem.TouchPhase.Ended:
            case UnityEngine.InputSystem.TouchPhase.Canceled:
                OnTouchEnded(touch);
                break;
        }
    }

    private void OnTouchBegan(Touch touch)
    {
        if (isDragging || mainCamera == null) return;

        // 过滤无效触摸位置（无限大/非数值/超出屏幕）
        Vector2 screenPos = touch.screenPosition;
        if (!IsValidScreenPos(screenPos))
            return;

        // 安全转换坐标（z值设为近裁剪面+0.1，确保在可视范围内）
        Vector3 touchWorldPos3D = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 0.1f)
        );
        touchWorldPos3D.z = 0; // 锁定z轴，匹配2D碰撞器检测
        Vector2 touchWorldPos = new Vector2(touchWorldPos3D.x, touchWorldPos3D.y);

        if (IsTouchOnButton(touchWorldPos))
        {
            isDragging = true;
            if (PlayerManager.instance != null && !PlayerManager.instance.FixVolume)
            {
                MusicManager.Instance.PlayEffectMusic("Music/Noise", false);
            }
        }
    }

    private void OnTouchMoved(Touch touch)
    {
        if (!isDragging || mainCamera == null) return;

        // 过滤无效触摸位置
        Vector2 screenPos = touch.screenPosition;
        if (!IsValidScreenPos(screenPos))
            return;

        // 安全转换坐标
        Vector3 touchWorldPos3D = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 0.1f)
        );
        touchWorldPos3D.z = 0;
        Vector2 touchWorldPos = new Vector2(touchWorldPos3D.x, touchWorldPos3D.y);

        RotateKnob(touchWorldPos);
        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            if (PlayerManager.instance != null && !PlayerManager.instance.FixVolume)
                promptText.text = "*#��"; // 建议替换为实际文本（如“调整音量”）
            else
                promptText.text = GetPrecentage();
        }
    }

    private void OnTouchEnded(Touch touch)
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        isDragging = false;
        if (PlayerManager.instance != null && !PlayerManager.instance.FixVolume && audioSource != null)
        {
            MusicManager.Instance.StopEffectMusic(audioSource);
        }
    }

    /// <summary>
    /// 检查屏幕坐标是否有效（非无限大、非数值、在屏幕范围内）
    /// </summary>
    private bool IsValidScreenPos(Vector2 screenPos)
    {
        // 排除无限大/非数值
        if (float.IsInfinity(screenPos.x) || float.IsInfinity(screenPos.y) ||
            float.IsNaN(screenPos.x) || float.IsNaN(screenPos.y))
        {
            Debug.LogWarning("检测到无效触摸坐标（无限大/非数值）");
            return false;
        }

        // 排除超出屏幕范围的坐标
        if (screenPos.x < 0 || screenPos.x > Screen.width ||
            screenPos.y < 0 || screenPos.y > Screen.height)
        {
            Debug.LogWarning($"触摸坐标超出屏幕范围：({screenPos.x}, {screenPos.y})");
            return false;
        }

        return true;
    }

    private bool IsTouchOnButton(Vector2 worldPosition)
    {
        return circleCollider != null && circleCollider.OverlapPoint(worldPosition);
    }

    private void RotateKnob(Vector2 touchPosition)
    {
        if (promptImageObj == null) return;

        Vector2 direction = touchPosition - (Vector2)promptImageObj.transform.position;

        if (direction != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            promptImageObj.transform.rotation = Quaternion.Lerp(
                promptImageObj.transform.rotation, targetRotation, speed * Time.deltaTime);
        }
    }

    public float GetCurrentAngle()
    {
        return promptImageObj != null ? promptImageObj.transform.eulerAngles.z : 0f;
    }

    public float GetNormalizedValue()
    {
        float angle = GetCurrentAngle();
        return 1f - (angle / 360f);
    }

    public string GetPrecentage()
    {
        return ((int)(GetNormalizedValue() * 100f)).ToString() + "%";
    }

    public void SetKnobAngle(float angle)
    {
        if (promptImageObj != null)
            promptImageObj.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}