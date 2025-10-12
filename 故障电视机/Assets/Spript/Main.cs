using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    //程序的主要入口
   [SerializeField] private List<GameObject> NeedInitPrefabs = new List<GameObject>();//需要初始化的预制体列表

    private void Awake()
    {
        //先初始化所有的预制体
        foreach (var item in NeedInitPrefabs)
        {
            GameObject obj = Instantiate(item);
            DontDestroyOnLoad(obj);//切换场景不销毁
        }
    }

    void Start()
    {
        //呼唤ui，目前只有电视机这一个面板所以就先呼出这个面板
        UImanager.Instance.ShowPanel<televisionPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
