using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetGoldArea : MonoBehaviour
{
    private static GetGoldArea instance;
    public static GetGoldArea Instance => instance;

    [SerializeField] private Transform GoldPos;//金币创建点

    //放出金币的区域
    [SerializeField] private GameObject Goldprefabs;//金币预制体

    private void Awake()
    {
        instance= this;
    }

    public void CreateGold(int Amount)
    {
        StartCoroutine(Create( Amount));
    }

    IEnumerator Create(int Amount)
    {
        for (int i = 0; i < Amount; i++)
        {
           var Obj= PoolManage.Instance.GetObj(Goldprefabs);//从对象池获取金币
            Obj.transform.position = GoldPos.position;//设置位置
           yield return new WaitForSeconds(0.2f);
           UI_ShowGold.Instance.RecycleGold(Obj, Goldprefabs);


        }
    }
}
