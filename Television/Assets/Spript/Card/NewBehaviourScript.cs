using UnityEngine;
using UnityEngine.InputSystem;

public class TouchTest : MonoBehaviour
{
    private void Update()
    {
        // 检测是否有触摸设备
        if (Touchscreen.current == null)
            return;

        // 获取主触摸信息
        var primaryTouch = Touchscreen.current.primaryTouch;
        if (primaryTouch == null)
            return;

        // 检测触摸开始
        if (primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            Vector2 touchPos = primaryTouch.position.ReadValue();
            Debug.Log($"触摸开始：{touchPos}");
        }

        // 检测触摸结束
        if (primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
            primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            Vector2 touchPos = primaryTouch.position.ReadValue();
            Debug.Log($"触摸结束：{touchPos}");
        }
    }
}