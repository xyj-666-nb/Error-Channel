using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池中的抽屉对象
/// </summary>
public class PoolDate
{
    private Stack<GameObject> DataStack; // 存储抽屉中的对象
    private GameObject RootObj; // 对象池根节点（方便管理）

    public PoolDate(GameObject root, string name)
    {
        DataStack = new Stack<GameObject>();
        RootObj = new GameObject(name + "_Pool");
        RootObj.transform.SetParent(root.transform);
    }

    public int Count => DataStack.Count;

    // 从池子里取出对象
    public GameObject Pop()
    {
        if (DataStack.Count == 0) return null; // 池空返回null

        GameObject obj = DataStack.Pop();
        obj.SetActive(true);
        obj.transform.SetParent(null); // 暂时解除父节点（后续赋值给Content）
        return obj;
    }

    // 回收对象到池子
    public void Pushobj(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(RootObj.transform);
        DataStack.Push(obj);
    }
}

/// <summary>
/// 标准单例对象池管理器
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
                // 若实例不存在，自动创建对象池管理器
                GameObject poolObj = new GameObject("PoolManage");
                instance = poolObj.AddComponent<PoolManage>();
                DontDestroyOnLoad(poolObj); // 跨场景不销毁
            }
            return instance;
        }
    }

    private Dictionary<string, PoolDate> objPoolDic; // 预制体名 → 对象池
    private GameObject PoolRoot; // 所有对象池的根节点

    private void Awake()
    {
        // 单例防重复（场景中手动拖放的实例）
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
    /// 从对象池获取对象（优先复用，无则实例化）
    /// </summary>
    public GameObject GetObj(GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        string prefabName = prefab.name;
        GameObject obj = null;

        // 1. 池子里有闲置对象，直接复用
        if (objPoolDic.ContainsKey(prefabName) && objPoolDic[prefabName].Count > 0)
        {
            obj = objPoolDic[prefabName].Pop();
        }
        // 2. 池子没有，实例化新对象
        else
        {
            obj = Instantiate(prefab);
            obj.name = prefabName; // 去掉"(Clone)"后缀，方便匹配
        }

        return obj;
    }

    /// <summary>
    /// 回收对象到池
    /// </summary>
    public void PushObj(GameObject prefab, GameObject obj)
    {
        if (prefab == null || obj == null)
        {
            Debug.LogError("PushObj：预制体或对象为空！");
            return;
        }

        string prefabName = prefab.name;
        // 1. 没有对应池子，创建新池子
        if (!objPoolDic.ContainsKey(prefabName))
        {
            objPoolDic.Add(prefabName, new PoolDate(PoolRoot, prefabName));
        }

        // 2. 回收对象（解除父节点，避免被Content销毁影响）
        if (obj.transform.parent != null)
        {
            obj.transform.SetParent(null);
        }
        objPoolDic[prefabName].Pushobj(obj);
    }

    /// <summary>
    /// 清空指定预制体的对象池
    /// </summary>
    public void ClearPool(GameObject prefab)
    {
        if (prefab == null || !objPoolDic.ContainsKey(prefab.name)) return;

        objPoolDic.Remove(prefab.name);
        Debug.Log($"[对象池] 清空 {prefab.name} 池");
    }
}