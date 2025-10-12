using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    //�������Ҫ���
   [SerializeField] private List<GameObject> NeedInitPrefabs = new List<GameObject>();//��Ҫ��ʼ����Ԥ�����б�

    private void Awake()
    {
        //�ȳ�ʼ�����е�Ԥ����
        foreach (var item in NeedInitPrefabs)
        {
            GameObject obj = Instantiate(item);
            DontDestroyOnLoad(obj);//�л�����������
        }
    }

    void Start()
    {
        //����ui��Ŀǰֻ�е��ӻ���һ��������Ծ��Ⱥ���������
        UImanager.Instance.ShowPanel<televisionPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
