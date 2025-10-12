using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������еĳ������
/// </summary>
public class PoolDate
{
    private Stack<GameObject> DataStack; // �洢�����еĶ���
    private GameObject RootObj; // ����ظ��ڵ㣨�������

    public PoolDate(GameObject root, string name)
    {
        DataStack = new Stack<GameObject>();
        RootObj = new GameObject(name + "_Pool");
        RootObj.transform.SetParent(root.transform);
    }

    public int Count => DataStack.Count;

    // �ӳ�����ȡ������
    public GameObject Pop()
    {
        if (DataStack.Count == 0) return null; // �ؿշ���null

        GameObject obj = DataStack.Pop();
        obj.SetActive(true);
        obj.transform.SetParent(null); // ��ʱ������ڵ㣨������ֵ��Content��
        return obj;
    }

    // ���ն��󵽳���
    public void Pushobj(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(RootObj.transform);
        DataStack.Push(obj);
    }
}

/// <summary>
/// ��׼��������ع�����
/// </summary>
public class PoolManage : MonoBehaviour
{
    private static PoolManage instance;
    public static PoolManage Instance
    {
        get
        {
            if (instance == null)
            {
                // ��ʵ�������ڣ��Զ���������ع�����
                GameObject poolObj = new GameObject("PoolManage");
                instance = poolObj.AddComponent<PoolManage>();
                DontDestroyOnLoad(poolObj); // �糡��������
            }
            return instance;
        }
    }

    private Dictionary<string, PoolDate> objPoolDic; // Ԥ������ �� �����
    private GameObject PoolRoot; // ���ж���صĸ��ڵ�

    private void Awake()
    {
        // �������ظ����������ֶ��Ϸŵ�ʵ����
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        objPoolDic = new Dictionary<string, PoolDate>();
        PoolRoot = new GameObject("PoolRoot");
        DontDestroyOnLoad(PoolRoot);
    }

    /// <summary>
    /// �Ӷ���ػ�ȡ�������ȸ��ã�����ʵ������
    /// </summary>
    public GameObject GetObj(GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        string prefabName = prefab.name;
        GameObject obj = null;

        // 1. �����������ö���ֱ�Ӹ���
        if (objPoolDic.ContainsKey(prefabName) && objPoolDic[prefabName].Count > 0)
        {
            obj = objPoolDic[prefabName].Pop();
        }
        // 2. ����û�У�ʵ�����¶���
        else
        {
            obj = Instantiate(prefab);
            obj.name = prefabName; // ȥ��"(Clone)"��׺������ƥ��
        }

        return obj;
    }

    /// <summary>
    /// ���ն��󵽳�
    /// </summary>
    public void PushObj(GameObject prefab, GameObject obj)
    {
        if (prefab == null || obj == null)
        {
            Debug.LogError("PushObj��Ԥ��������Ϊ�գ�");
            return;
        }

        string prefabName = prefab.name;
        // 1. û�ж�Ӧ���ӣ������³���
        if (!objPoolDic.ContainsKey(prefabName))
        {
            objPoolDic.Add(prefabName, new PoolDate(PoolRoot, prefabName));
        }

        // 2. ���ն��󣨽�����ڵ㣬���ⱻContent����Ӱ�죩
        if (obj.transform.parent != null)
        {
            obj.transform.SetParent(null);
        }
        objPoolDic[prefabName].Pushobj(obj);
    }

    /// <summary>
    /// ���ָ��Ԥ����Ķ����
    /// </summary>
    public void ClearPool(GameObject prefab)
    {
        if (prefab == null || !objPoolDic.ContainsKey(prefab.name)) return;

        objPoolDic.Remove(prefab.name);
        Debug.Log($"[�����] ��� {prefab.name} ��");
    }
}