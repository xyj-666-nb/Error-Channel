using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MonoMange : MonoBehaviour
{
    private static MonoMange instance ;
    public static MonoMange Instance => instance;


    //这个类本来就是继承自MonoBehaviour的，所有可以直接调用StartCoroutine这种协程函数
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
        Debug.Log("MonoMange初始化完成");
    }

    /// <summary>
    /// 在update函数中添加函数
    /// </summary>
    /// <param name="UpdateAction"></param>
    public void AddLister_Update(UnityAction UpdateAction)
    {
        Debug.Log("添加Update监听");
        this.UpdateAction += UpdateAction;
    }

    /// <summary>
    /// 在update函数中移除函数
    /// </summary>
    /// <param name="UpdateAction"></param>
    public void RemoveLiater_Update(UnityAction UpdateAction)
    {
        this.UpdateAction -= UpdateAction;
    }

    /// <summary>
    /// 在LateUpdate函数中添加函数
    /// </summary>
    /// <param name="FixedUpdateAction"></param>
    public void AddLister_FixedUpdate(UnityAction FixedUpdateAction)
    {
        this.FixedUpdateAction += FixedUpdateAction;
    }
    /// <summary>
    /// 在LateUpdate函数中移除函数
    /// </summary>
    /// <param name="FixedUpdateAction"></param>
    public void RemoveLiater_FixedUpdate(UnityAction FixedUpdateAction)
    {
        this.FixedUpdateAction -= FixedUpdateAction;
    }

    /// <summary>
    /// 在LateUpdate函数中添加函数
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
    /// 在LateUpdate函数中移除函数
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

    //给没有继承MonoBehaviour的类提供初始化预制体的方法
    public GameObject InitPrefab(string Name)
    {
        GameObject prefab = Resources.Load<GameObject>(Name);
        if (prefab == null)
        {
            Debug.LogError($"未找到预制体{Name}，请检查路径是否正确");
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
