
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BasePanel : MonoBehaviour
{
    protected Dictionary<string, UIBehaviour> controlDic = new Dictionary<string, UIBehaviour>();
    private static List<string> DefaultNameList = new List<string>()
    {
       "Image",
       "Text (TMP)",
       "RawImage",
       "BackGround",
       "Checkmark",
       "Label",
       "Text (Legacy)",
       "Arrow",
       "Placeholder",
       "Fill",
       "Handle",
       "Viewport",
       "Scrollbar Horizontal",
       "Scrollbar Vertical"
    };

    protected CanvasGroup canvasGroup;
    public float alhpaSpeed = 10; // 注意：建议修正拼写为 alphaSpeed（不影响功能，仅规范）
    protected bool isShow = false; // 淡入/淡出开关
    private UnityAction Hidecallback;

    public bool IsCanDestroy = true; // 是否可以销毁
    public bool IsUseRealTime = false; // 核心标志位：true=用真实时间（不受timeScale影响），false=用缩放时间


    public virtual void Awake()
    {
        // 优先查找交互类UI组件
        FindChildControl<Button>();
        FindChildControl<Toggle>();
        FindChildControl<Slider>();
        FindChildControl<Scrollbar>();
        FindChildControl<Dropdown>();
        FindChildControl<InputField>();

        // 查找显示类UI组件
        FindChildControl<Text>();
        FindChildControl<Image>();
        FindChildControl<TextMeshProUGUI>();

        // 初始化CanvasGroup（确保淡入淡出有载体）
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public virtual void Start()
    {
        // 可选：初始化时默认隐藏（根据需求调整）
        if (!isShow)
        {
            canvasGroup.alpha = 0;
        }
    }

    // 按钮点击事件（子类重写）
    public virtual void ClickButton(string controlName) { }
    // Slider值变化（子类重写）
    public virtual void SilderValueChange(string SilderName, float Value) { }
    // Toggle值变化（子类重写）
    public virtual void ToggleValueChange(string ToggleName, bool Value) { }
    // Scrollbar值变化（子类重写）
    public virtual void ScrollbarValueChange(string ScrollbarName, float Value) { }
    // Dropdown值变化（子类重写）
    public virtual void DropdownValueChange(string DropdownName, int Value) { }
    // InputField值变化（子类重写）
    public virtual void InputFieldValueChange(string InputFieldName, string Value) { }


    /// <summary>
    /// 显示面板（淡入）
    /// </summary>
    public virtual void ShowMe(bool IsNeedDefalutAnimator = true)
    {
        gameObject.SetActive(true); // 确保面板激活
        if (IsNeedDefalutAnimator)
        {
            gameObject.SetActive(true); // 确保面板激活
            canvasGroup.alpha = 0; // 初始透明度0
        }
        isShow = true; // 开启淡入开关
    }

    /// <summary>
    /// 隐藏面板（淡出）
    /// </summary>
    /// <param name="callback">淡出完成后的回调（如销毁、隐藏对象）</param>
    public virtual void HideMe(UnityAction callback)
    {
        canvasGroup.alpha = 1; // 初始透明度1
        isShow = false; // 开启淡出开关
        Hidecallback = callback; // 记录回调
    }


    /// <summary>
    /// 获取指定名称的UI控件
    /// </summary>
    public T GetControl<T>(string Name) where T : UIBehaviour
    {
        if (controlDic.TryGetValue(Name, out UIBehaviour control))
        {
            if (control is T targetControl)
            {
                return targetControl;
            }
            Debug.LogError($"【BasePanel】控件 {Name} 类型不匹配！期望 {typeof(T).Name}，实际 {control.GetType().Name}");
        }
        else
        {
            Debug.LogError($"【BasePanel】未找到控件 {Name}（面板：{gameObject.name}）");
        }
        return null;
    }


    /// <summary>
    /// 递归查找所有子物体的UI控件，并绑定事件
    /// </summary>
    private void FindChildControl<T>() where T : UIBehaviour
    {
        T[] controls = GetComponentsInChildren<T>(true); // true=包含未激活的子物体
        foreach (T control in controls)
        {
            string controlName = control.name;
            // 排除默认名称的控件，避免重复添加
            if (!controlDic.ContainsKey(controlName) && !DefaultNameList.Contains(controlName))
            {
                controlDic.Add(controlName, control);
                // 根据控件类型绑定对应的事件
                BindControlEvent(control, controlName);
            }
        }
    }

    /// <summary>
    /// 绑定UI控件的事件（解耦查找和绑定逻辑，更易维护）
    /// </summary>
    private void BindControlEvent<T>(T control, string controlName) where T : UIBehaviour
    {
        switch (control)
        {
            case Button btn:
                btn.onClick.AddListener(() => ClickButton(controlName));
                break;
            case Slider slider:
                slider.onValueChanged.AddListener((val) => SilderValueChange(controlName, val));
                break;
            case Toggle toggle:
                toggle.onValueChanged.AddListener((val) => ToggleValueChange(controlName, val));
                break;
            case Scrollbar scrollbar:
                scrollbar.onValueChanged.AddListener((val) => ScrollbarValueChange(controlName, val));
                break;
            case Dropdown dropdown:
                dropdown.onValueChanged.AddListener((val) => DropdownValueChange(controlName, val));
                break;
            case InputField input:
                input.onValueChanged.AddListener((val) => InputFieldValueChange(controlName, val));
                break;
                // 其他控件（如Text、Image）无需绑定事件，可忽略
        }
    }


    /// <summary>
    /// 核心：淡入淡出逻辑（根据IsUseRealTime选择时间变量）
    /// </summary>
    protected virtual void Update()
    {
        // 1. 动态选择时间：true=真实时间（不受暂停影响），false=缩放时间
        float deltaTime = IsUseRealTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // 2. 淡入逻辑（透明度从0→1）
        if (isShow && Mathf.Abs(canvasGroup.alpha - 1) > 0.01f) // 用差值判断，避免浮点精度问题
        {
            canvasGroup.alpha += alhpaSpeed * deltaTime;
            canvasGroup.alpha = Mathf.Min(canvasGroup.alpha, 1); // 防止超过1
        }
        // 3. 淡出逻辑（透明度从1→0）
        else if (!isShow && Mathf.Abs(canvasGroup.alpha - 0) > 0.01f)
        {
            canvasGroup.alpha -= alhpaSpeed * deltaTime;
            canvasGroup.alpha = Mathf.Max(canvasGroup.alpha, 0); // 防止低于0

            // 4. 淡出完成：调用回调（如销毁面板、隐藏对象）
            if (canvasGroup.alpha <= 0.01f)
            {
                Hidecallback?.Invoke();
                // 可选：如果不销毁，淡出后隐藏面板
                if (!IsCanDestroy)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 取消按钮选中状态（防止按钮残留选中效果）
    /// </summary>
    /// <param name="button"></param>
    public void DeselectButton(Button button)
    {
        if (button == null) return;

        var selectable = button.GetComponent<Selectable>();
        if (selectable == null) return;

        // 方法1：触发Deselect事件
        selectable.OnDeselect(new BaseEventData(EventSystem.current));

        // 方法2：清除EventSystem的选中
        if (EventSystem.current.currentSelectedGameObject == button.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // 方法3：重置按钮状态
        selectable.OnPointerExit(null);
    }
}
