using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MonoMange : MonoBehaviour
{
    private static MonoMange instance ;
    public static MonoMange Instance => instance;


    //����౾�����Ǽ̳���MonoBehaviour�ģ����п���ֱ�ӵ���StartCoroutine����Э�̺���
    public UnityAction UpdateAction;
    private UnityAction FixedUpdateAction;
    private UnityAction LateUpdateAction;
    private UnityAction OndestoryAction;

    private void Start()
    {
    }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("MonoMange��ʼ�����");
    }

    /// <summary>
    /// ��update��������Ӻ���
    /// </summary>
    /// <param name="UpdateAction"></param>
    public void AddLister_Update(UnityAction UpdateAction)
    {
        Debug.Log("���Update����");
        this.UpdateAction += UpdateAction;
    }

    /// <summary>
    /// ��update�������Ƴ�����
    /// </summary>
    /// <param name="UpdateAction"></param>
    public void RemoveLiater_Update(UnityAction UpdateAction)
    {
        this.UpdateAction -= UpdateAction;
    }

    /// <summary>
    /// ��LateUpdate��������Ӻ���
    /// </summary>
    /// <param name="FixedUpdateAction"></param>
    public void AddLister_FixedUpdate(UnityAction FixedUpdateAction)
    {
        this.FixedUpdateAction += FixedUpdateAction;
    }
    /// <summary>
    /// ��LateUpdate�������Ƴ�����
    /// </summary>
    /// <param name="FixedUpdateAction"></param>
    public void RemoveLiater_FixedUpdate(UnityAction FixedUpdateAction)
    {
        this.FixedUpdateAction -= FixedUpdateAction;
    }

    /// <summary>
    /// ��LateUpdate��������Ӻ���
    /// </summary>
    /// <param name="LateUpdateAction"></param>
    public void AddLister_LateUpdate(UnityAction LateUpdateAction)
    {
        this.LateUpdateAction += LateUpdateAction;
    }

    public void AddLister_OnDestory(UnityAction _OndestoryAction)
    {
        OndestoryAction += _OndestoryAction;
    }
    /// <summary>
    /// ��LateUpdate�������Ƴ�����
    /// </summary>
    /// <param name="LateUpdateAction"></param>
    public void RemoveLister_LateUpdate(UnityAction LateUpdateAction)
    {
        this.LateUpdateAction -= LateUpdateAction;
    }

    public void RemoveLister_OnDestory(UnityAction _OndestoryAction)
    {
        OndestoryAction-= _OndestoryAction;
    }

    private void Update()
    {
        UpdateAction?.Invoke();
    }

    private void FixedUpdate()
    {
        FixedUpdateAction?.Invoke();
    }
    private void LateUpdate()
    {
        LateUpdateAction?.Invoke();
    }

    //��û�м̳�MonoBehaviour�����ṩ��ʼ��Ԥ����ķ���
    public GameObject InitPrefab(string Name)
    {
        GameObject prefab = Resources.Load<GameObject>(Name);
        if (prefab == null)
        {
            Debug.LogError($"δ�ҵ�Ԥ����{Name}������·���Ƿ���ȷ");
            return null;
        }
        GameObject instance = Instantiate(prefab);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        return instance;
    }

    public GameObject iniPrefab(GameObject obj)
    {
        return Instantiate(obj);
    }

    private void OnDestroy()
    {
        OndestoryAction?.Invoke();
    }
}
