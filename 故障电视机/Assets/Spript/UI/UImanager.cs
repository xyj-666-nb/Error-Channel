
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UImanager 
{
    private Transform canvasform;

    //UI�����������þͰ����е�������һ����װ
    //����ֻ��Ҫ������һ�����еķ����������־Ϳ��Ի�ȡ���е���幦�ܣ��ǳ�����

    //UI������ֻ��һ������Ҫд�ɵ���ģʽ
    private static UImanager instance = new UImanager();//����û�м̳�monbehavior���Կ���new,���ұ���ȥ�Լ�new
    public static UImanager Instance => instance;

    //����������Ҫһ�����������洢��壨ע�⡤����Ĵ洢��������Ѿ���ʾ�˵���壬����ȫ���������Ϣ��
    //ΪʲôҪ����д�أ�����������showPanel��ʱ������resources��ȥ��ȡһ�����Ȼ����ʾ
    //��������ιر��������أ������ڹر��������ʱ����Ҫ���������Ϣ��
    //������Ҫһ���ռ����洢��壬��ȡ�����룬��ȡ��Ϣ��Ȼ��ر�


    //��������ͨ���ֵ����洢�����壬�����������ҷǳ�����
    //�����滻ԭ��ͨ��������װ�����࣬����õ�ʱ��as����Ҫ�������ok
    private Dictionary<string, BasePanel> PanelDic = new Dictionary<string, BasePanel>();

    //дһ��˽�еĹ��캯��ȥ��ʼ�����
    private UImanager()
    {
        //һ��ʼ�ʹ�Ԥ�����д���һ��canvas���Ҳ��������泡���ĸı��ɾ��
        GameObject canvas = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("UI/Canvas"));
        //��������Ҫɾ��
        canvasform = canvas.transform;
        GameObject.DontDestroyOnLoad(canvas);
    }

    //��ʾ���
    public T ShowPanel<T>() where T : BasePanel//����һ��Լ������ֻT����Ҫ�̳����BasePanel
    {
        string panelName = typeof(T).Name;//��ȡT�����Ͳ���ȡ����

        //Ѱ��һ���ֵ�����û�����ֵ������оʹ����Ѿ���ʾ�ڽ��������ˣ���ʱ��Ͳ���Ҫȥ����ʾ��
        if (PanelDic.ContainsKey(panelName))
        {
            //����о�ֱ�ӷ���������
            return PanelDic[panelName] as T;
        }
        GameObject panelObj = null;


        if (Resources.Load<GameObject>("UI/" + panelName).GetComponent<BasePanel>().IsCanDestroy)
        {
            //���������û�����������Ҫ����ȥ�ҵ����Ԥ����ȥʹ����
            panelObj = GameObject.Instantiate(Resources.Load<GameObject>("UI/" + panelName));
        }
        else
        {
            //��������������ֲ��ܱ�ɾ���������Ǿ���Ҫ�ж�һ���������Ƿ��Ѿ�����������
            //������������Ǿ�ֱ��ȥ��������
            if (GameObject.Find(panelName))
                panelObj = GameObject.Find(panelName);
            else
                panelObj = GameObject.Instantiate(Resources.Load<GameObject>("UI/" + panelName));//���û�д������Ǿʹ���һ��
        }


        //�������ǵĵ����е���嶼Ҫ������canvas��������������Ҫ���ø�����
        panelObj.transform.SetParent(canvasform, false);

        T panel = panelObj.GetComponent<T>();
        //Ȼ���ٴ浽�ֵ�����
        PanelDic.Add(panelName, panel);
        //���������������ʾ����
        panel.ShowMe();
        return panel;
    }

    //�������

    //�����ṩһ���Ƿ���Ҫ���뵭����boolֵ
    //���ﲻ��Ҫ����ֵ
    public void HidePanel<T>(bool isFade = true, UnityAction CallBack = null) where T : BasePanel//����һ��Լ������ֻT����Ҫ�̳����BasePanel
    {
        string panelName = typeof(T).Name;//��ȡT�����Ͳ���ȡ����
        if (PanelDic.ContainsKey(panelName))
        {
            if (isFade)
            {
                //���������Ҫȥ�е������Ч�������Ǿ�ʹ��hideMe�������
                //�����ṩ�˵����������ִ�е�ί�к���
                PanelDic[panelName].HideMe(() =>
                {
                    if (PanelDic[panelName].IsCanDestroy)//���ж��Ƿ��ܱ�ɾ��
                        GameObject.Destroy(PanelDic[panelName].gameObject);
                    CallBack?.Invoke();//Ȼ��ִ�����Ǵ���Ļص�����
                    //Ȼ��ǵû�Ҫɾ���ֵ�������Ϣ
                    PanelDic.Remove(panelName);
                });
            }
            else
            {
                //ֱ��ɾ����������ŷ����
                //�е���������������ܱ�ɾ��
                GameObject.Destroy(PanelDic[panelName].gameObject);
                //Ȼ��ǵû�Ҫɾ���ֵ�������Ϣ
                PanelDic.Remove(panelName);
            }
        }
    }
    //��ȡ���

    public T GetPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;//��ȡT�����Ͳ���ȡ����
        if (PanelDic.ContainsKey(panelName))
        {
            return PanelDic[panelName] as T;
        }
        return null;
    }
}
