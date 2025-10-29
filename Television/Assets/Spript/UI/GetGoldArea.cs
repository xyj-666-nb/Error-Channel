using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetGoldArea : MonoBehaviour
{
    private static GetGoldArea instance;
    public static GetGoldArea Instance => instance;

    [SerializeField] private Transform GoldPos;//金币创建点
    [SerializeField] private Transform UseGoldPos;//使用金币点

    //放出金币的区域
    [SerializeField] private GameObject Goldprefabs;//金币预制体
    [SerializeField] private float spawnRadius = 0.5f; // 生成半径

    private void Awake()
    {
        instance = this;
    }

    public void CreateGold(int Amount)
    {
        StartCoroutine(Create(Amount));
    }

    public void UseGoldInAdvanceShop(int Amount)
    {
        StartCoroutine(UseGold(Amount));
    }

    IEnumerator Create(int Amount)
    {
        for (int i = 0; i < Amount; i++)
        {
            var Obj = PoolManage.Instance.GetObj(Goldprefabs);//从对象池获取金币

            // 在GoldPos周围的圆形区域内随机生成位置
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPosition = GoldPos.position + new Vector3(randomCircle.x, randomCircle.y, 0);
            Obj.transform.position = randomPosition;

            //在这里播放音乐
            MusicManager.Instance.PlayEffectMusic("Music/获得金币", false);
            yield return new WaitForSeconds(0.2f);
            UI_ShowGold.Instance.RecycleGold(Obj, Goldprefabs);
        }
    }

    IEnumerator UseGold(int Amount)
    {
        float WaitTime = 0;
        if (Amount <= 20)
            WaitTime = 0.2f;
        else if(Amount<100&& Amount > 20)
            WaitTime = 0.05f;
        else if(Amount >= 100)
        {
            WaitTime = 0.01f;
            Amount = 100;//最高播放100
        }

        for (int i = 0; i < Amount; i++)
        {
            var Obj = PoolManage.Instance.GetObj(Goldprefabs);//从对象池获取金币
            Obj.transform.position = UseGoldPos.position;//设置位置
            yield return new WaitForSeconds(WaitTime);
            UI_ShowGold.Instance.UseGoldInAdvanceShop(Obj, Goldprefabs);
        }
    }
}