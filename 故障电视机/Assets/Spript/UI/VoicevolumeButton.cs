using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class VoicevolumeButton : MonoBehaviour
{
    [SerializeField] private Button me;
    [SerializeField] private float speed = 5;
    [SerializeField] private TextMeshProUGUI promptText; // 提示文本
    [SerializeField] private GameObject promptImageObj; // 提示图片
    private CircleCollider2D collider;// 按钮的碰撞器
    private Camera mainCamera;
    private bool isDragging = false;

    private bool IsCheck;
    private void Awake()
    {
        IsCheck = false;
        // 获取主相机
        mainCamera = Camera.main;

        // 如果没有找到主相机，尝试查找
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        collider=GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        me.onClick.AddListener(() => { IsCheck = true; });
    }

    private void Update()
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if(!IsCheck)//如果没有开启检测就不用检测点击

        // 只要一个个触碰点触发了
        if (Touch.fingers[0].isActive)
        {
            Touch touch = Touch.fingers[0].currentTouch;
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
    }

    private void OnTouchBegan(Touch touch)
    {
        if (isDragging) return;

        // 检查触摸是否在按钮范围内
        Vector2 touchWorldPos = mainCamera.ScreenToWorldPoint(touch.screenPosition);
        if (IsTouchOnButton(touchWorldPos))
        {
            isDragging = true;
        }
    }

    private void OnTouchMoved(Touch touch)
    {
        if (!isDragging ) return;

        Vector2 touchWorldPos = mainCamera.ScreenToWorldPoint(touch.screenPosition);
        RotateKnob(touchWorldPos);
        promptText.gameObject.SetActive(true);
        promptText.text = GetPrecentage();
    }

    private void OnTouchEnded(Touch touch)
    {
        promptText.gameObject.SetActive(false);
        isDragging = false;
    }

    private bool IsTouchOnButton(Vector2 worldPosition)
    {
        // 使用碰撞器检测
        if (collider != null)
        {
            return collider.OverlapPoint(worldPosition);
        }

        return false;
    }

    private void RotateKnob(Vector2 touchPosition)
    {
        if (promptImageObj == null) return;

        // 计算旋钮中心到触摸点的方向
        Vector2 direction = touchPosition - (Vector2)promptImageObj.transform.position;

        if (direction != Vector2.zero)
        {
            // 计算目标角度
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;//转化为角度

            // 平滑旋转到目标角度
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);//只转z轴
            promptImageObj.transform.rotation = Quaternion.Lerp(
            promptImageObj.transform.rotation, targetRotation, speed * Time.deltaTime);
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        isDragging = false;
        IsCheck=false;
    }

    // 获取当前旋转角度（0-360度）
    public float GetCurrentAngle()
    {
        if (promptImageObj == null) 
            return 0f;
        return promptImageObj.transform.eulerAngles.z;
    }

    // 获取标准化值
    public float GetNormalizedValue()
    {
        return GetCurrentAngle() / 360f;
    }

    public string GetPrecentage()
    {
        return ((int)(GetNormalizedValue() * 100f)).ToString() +"%";
    }

    // 设置旋钮角度
    public void SetKnobAngle(float angle)
    {
        if (promptImageObj == null) return;
        promptImageObj.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}