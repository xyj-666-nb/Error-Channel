
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
    public float alhpaSpeed = 10; // ע�⣺��������ƴдΪ alphaSpeed����Ӱ�칦�ܣ����淶��
    protected bool isShow = false; // ����/��������
    private UnityAction Hidecallback;

    public bool IsCanDestroy = true; // �Ƿ��������
    public bool IsUseRealTime = false; // ���ı�־λ��true=����ʵʱ�䣨����timeScaleӰ�죩��false=������ʱ��


    public virtual void Awake()
    {
        // ���Ȳ��ҽ�����UI���
        FindChildControl<Button>();
        FindChildControl<Toggle>();
        FindChildControl<Slider>();
        FindChildControl<Scrollbar>();
        FindChildControl<Dropdown>();
        FindChildControl<InputField>();

        // ������ʾ��UI���
        FindChildControl<Text>();
        FindChildControl<Image>();
        FindChildControl<TextMeshProUGUI>();

        // ��ʼ��CanvasGroup��ȷ�����뵭�������壩
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public virtual void Start()
    {
        // ��ѡ����ʼ��ʱĬ�����أ��������������
        if (!isShow)
        {
            canvasGroup.alpha = 0;
        }
    }

    // ��ť����¼���������д��
    public virtual void ClickButton(string controlName) { }
    // Sliderֵ�仯��������д��
    public virtual void SilderValueChange(string SilderName, float Value) { }
    // Toggleֵ�仯��������д��
    public virtual void ToggleValueChange(string ToggleName, bool Value) { }
    // Scrollbarֵ�仯��������д��
    public virtual void ScrollbarValueChange(string ScrollbarName, float Value) { }
    // Dropdownֵ�仯��������д��
    public virtual void DropdownValueChange(string DropdownName, int Value) { }
    // InputFieldֵ�仯��������д��
    public virtual void InputFieldValueChange(string InputFieldName, string Value) { }


    /// <summary>
    /// ��ʾ��壨���룩
    /// </summary>
    public virtual void ShowMe(bool IsNeedDefalutAnimator = true)
    {
        gameObject.SetActive(true); // ȷ����弤��
        if (IsNeedDefalutAnimator)
        {
            gameObject.SetActive(true); // ȷ����弤��
            canvasGroup.alpha = 0; // ��ʼ͸����0
        }
        isShow = true; // �������뿪��
    }

    /// <summary>
    /// ������壨������
    /// </summary>
    /// <param name="callback">������ɺ�Ļص��������١����ض���</param>
    public virtual void HideMe(UnityAction callback)
    {
        canvasGroup.alpha = 1; // ��ʼ͸����1
        isShow = false; // ������������
        Hidecallback = callback; // ��¼�ص�
    }


    /// <summary>
    /// ��ȡָ�����Ƶ�UI�ؼ�
    /// </summary>
    public T GetControl<T>(string Name) where T : UIBehaviour
    {
        if (controlDic.TryGetValue(Name, out UIBehaviour control))
        {
            if (control is T targetControl)
            {
                return targetControl;
            }
            Debug.LogError($"��BasePanel���ؼ� {Name} ���Ͳ�ƥ�䣡���� {typeof(T).Name}��ʵ�� {control.GetType().Name}");
        }
        else
        {
            Debug.LogError($"��BasePanel��δ�ҵ��ؼ� {Name}����壺{gameObject.name}��");
        }
        return null;
    }


    /// <summary>
    /// �ݹ���������������UI�ؼ��������¼�
    /// </summary>
    private void FindChildControl<T>() where T : UIBehaviour
    {
        T[] controls = GetComponentsInChildren<T>(true); // true=����δ�����������
        foreach (T control in controls)
        {
            string controlName = control.name;
            // �ų�Ĭ�����ƵĿؼ��������ظ����
            if (!controlDic.ContainsKey(controlName) && !DefaultNameList.Contains(controlName))
            {
                controlDic.Add(controlName, control);
                // ���ݿؼ����Ͱ󶨶�Ӧ���¼�
                BindControlEvent(control, controlName);
            }
        }
    }

    /// <summary>
    /// ��UI�ؼ����¼���������ҺͰ��߼�������ά����
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
                // �����ؼ�����Text��Image��������¼����ɺ���
        }
    }


    /// <summary>
    /// ���ģ����뵭���߼�������IsUseRealTimeѡ��ʱ�������
    /// </summary>
    protected virtual void Update()
    {
        // 1. ��̬ѡ��ʱ�䣺true=��ʵʱ�䣨������ͣӰ�죩��false=����ʱ��
        float deltaTime = IsUseRealTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // 2. �����߼���͸���ȴ�0��1��
        if (isShow && Mathf.Abs(canvasGroup.alpha - 1) > 0.01f) // �ò�ֵ�жϣ����⸡�㾫������
        {
            canvasGroup.alpha += alhpaSpeed * deltaTime;
            canvasGroup.alpha = Mathf.Min(canvasGroup.alpha, 1); // ��ֹ����1
        }
        // 3. �����߼���͸���ȴ�1��0��
        else if (!isShow && Mathf.Abs(canvasGroup.alpha - 0) > 0.01f)
        {
            canvasGroup.alpha -= alhpaSpeed * deltaTime;
            canvasGroup.alpha = Mathf.Max(canvasGroup.alpha, 0); // ��ֹ����0

            // 4. ������ɣ����ûص�����������塢���ض���
            if (canvasGroup.alpha <= 0.01f)
            {
                Hidecallback?.Invoke();
                // ��ѡ����������٣��������������
                if (!IsCanDestroy)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// ȡ����ťѡ��״̬����ֹ��ť����ѡ��Ч����
    /// </summary>
    /// <param name="button"></param>
    public void DeselectButton(Button button)
    {
        if (button == null) return;

        var selectable = button.GetComponent<Selectable>();
        if (selectable == null) return;

        // ����1������Deselect�¼�
        selectable.OnDeselect(new BaseEventData(EventSystem.current));

        // ����2�����EventSystem��ѡ��
        if (EventSystem.current.currentSelectedGameObject == button.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // ����3�����ð�ť״̬
        selectable.OnPointerExit(null);
    }
}
