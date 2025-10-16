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
    [SerializeField] private TextMeshProUGUI promptText; // ��ʾ�ı�
    [SerializeField] private GameObject promptImageObj; // ��ʾͼƬ
    private CircleCollider2D collider;// ��ť����ײ��
    private Camera mainCamera;
    private bool isDragging = false;

    private bool IsCheck;
    private void Awake()
    {
        IsCheck = false;
        // ��ȡ�����
        mainCamera = Camera.main;

        // ���û���ҵ�����������Բ���
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
        if(!IsCheck)//���û�п������Ͳ��ü����

        // ֻҪһ���������㴥����
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

        // ��鴥���Ƿ��ڰ�ť��Χ��
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
        // ʹ����ײ�����
        if (collider != null)
        {
            return collider.OverlapPoint(worldPosition);
        }

        return false;
    }

    private void RotateKnob(Vector2 touchPosition)
    {
        if (promptImageObj == null) return;

        // ������ť���ĵ�������ķ���
        Vector2 direction = touchPosition - (Vector2)promptImageObj.transform.position;

        if (direction != Vector2.zero)
        {
            // ����Ŀ��Ƕ�
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;//ת��Ϊ�Ƕ�

            // ƽ����ת��Ŀ��Ƕ�
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);//ֻתz��
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

    // ��ȡ��ǰ��ת�Ƕȣ�0-360�ȣ�
    public float GetCurrentAngle()
    {
        if (promptImageObj == null) 
            return 0f;
        return promptImageObj.transform.eulerAngles.z;
    }

    // ��ȡ��׼��ֵ
    public float GetNormalizedValue()
    {
        return GetCurrentAngle() / 360f;
    }

    public string GetPrecentage()
    {
        return ((int)(GetNormalizedValue() * 100f)).ToString() +"%";
    }

    // ������ť�Ƕ�
    public void SetKnobAngle(float angle)
    {
        if (promptImageObj == null) return;
        promptImageObj.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}