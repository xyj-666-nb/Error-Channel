using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetGoldArea : MonoBehaviour
{
    private static GetGoldArea instance;
    public static GetGoldArea Instance => instance;

    [SerializeField] private Transform GoldPos;//��Ҵ�����

    //�ų���ҵ�����
    [SerializeField] private GameObject Goldprefabs;//���Ԥ����

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
           var Obj= PoolManage.Instance.GetObj(Goldprefabs);//�Ӷ���ػ�ȡ���
            Obj.transform.position = GoldPos.position;//����λ��
           yield return new WaitForSeconds(0.2f);
           UI_ShowGold.Instance.RecycleGold(Obj, Goldprefabs);


        }
    }
}
