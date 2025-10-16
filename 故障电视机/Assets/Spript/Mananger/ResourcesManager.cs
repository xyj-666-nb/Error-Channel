using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
///��Դ��Ϣ�࣬��Ҫ���������滻ԭ�� ����װ���������
/// </summary>
public abstract class BaseResinfo
{
    public int refCount = 0;//���ü���
}
    public class ResInfo<T> : BaseResinfo
{
    public T Asset;
    //��Ҫ�����첽������Դ�Ļص�����
    public UnityAction<T> CallBack;
    //���ڴ洢�첽����ʱ������Эͬ����ĺ����Ķ���
    public Coroutine Coroutine;

    //�����ü���Ϊ��ʱ�Ƿ���Ҫ�Ƴ�����Ϊ�е�ʱ��һ����Դ�ܴ�������������Ƴ��Ļ��ͻ���ֿ���
    public bool IsDel=false;
    /// <summary>
    /// �ı����ü���
    /// </summary>
    /// <param name="i">����1��-1</param>
    public void ChangeRefCount(int i)
    {
        refCount+=i;
        if(refCount<0)
        {
            refCount = 0;
            Debug.LogError("���ü���С�����ˣ�����������ж���Ƿ����");
        }
    }

}

public class ResourcesManager 
{
    private static ResourcesManager instance = new ResourcesManager();
    public static ResourcesManager Instance => instance;

    //���ڴ洢�����е���Դ�����Ѿ����ص���Դ
    private Dictionary<string, BaseResinfo> ResDic = new Dictionary<string, BaseResinfo>();

    /// <summary>
    /// ͬ������Resources�ļ����µ���Դ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> Info;
    
        if (!ResDic.ContainsKey(resName))
        {
            //ֱ��ͬ�����ز������ֵ��м�¼ 
            T Res = Resources.Load<T>(path);
            Info= new ResInfo<T>();
            Info.Asset = Res; //�����ص���Դ��ֵ����Դ��Ϣ����
            ResDic.Add(resName, Info); //����Դ��Ϣ������ӵ��ֵ���
            Info.ChangeRefCount(1);
            return Info.Asset; //���ؼ��ص���Դ
        }
        else
        {
            //��ʱ����������Դ�Ѿ����ֵ��д�����
            Info = ResDic[resName] as ResInfo<T>;
            Info.ChangeRefCount(1);
            if (Info.Asset==null)
            {
                //ֹͣ�첽���أ�ֱ�Ӳ���ͬ�����صķ�ʽ
                MonoMange.Instance.StopCoroutine(Info.Coroutine);
                T Res = Resources.Load<T>(path);
                Info.Asset = Res; //�����ص���Դ��ֵ����Դ��Ϣ����
                Info.CallBack?.Invoke(Res); //���ûص�����
                Info.Coroutine = null; //���Эͬ��������
                Info.CallBack = null; //����ص���������
                return Res;
            }
            else
            {
                return Info.Asset; //�����Դ�Ѿ��������,��ֱ�ӷ��������Դ
            }
        }
    }
   
    /// <summary>
    /// �첽������Դ�ķ���
    /// </summary>
    /// <typeparam name="T">��Դ������</typeparam>
    /// <param name="Path">��resources���µ��ļ�·��</param>
    /// <param name="CallBack">���ؽ�����Ļص�������ֻ�е�������ϲŻ�����������</param>
    public void LoadAsync<T>(string Path,UnityAction<T> CallBack)where T :UnityEngine. Object
    {
        //������Դ��Ψһid,����·����_��Դ����ƴ�Ӷ��ɵ�
        string resName = Path + "_" + typeof(T).Name;
        ResInfo<T> info;
        if (!ResDic.ContainsKey(resName))
        {
            //����һ����Դ��Ϣ����
            info = new ResInfo<T>();
            info.ChangeRefCount(1);
            ResDic.Add(resName, info);//����Դ��Ϣ������ӵ��ֵ���
            //��¼ί�к���һ���������ʹ��
            info.CallBack += CallBack;
            //����Э�̽����첽����,���Ҽ�¼���Э�̳���
            info.Coroutine= MonoMange.Instance.StartCoroutine(ReallyLoadAsync<T>(Path));
        }
        else
        {
            //����ֵ����Ѿ����������Դ��Ϣ����,��ֱ�ӻ�ȡ
            info = ResDic[resName] as ResInfo<T>;
            info.ChangeRefCount(1);
            //�����Դ��û�м�����
            if (info.Asset==null)
                info.CallBack += CallBack; //��¼�ص�����
            else
                CallBack?.Invoke(info.Asset); //�����Դ�Ѿ��������,��ֱ�ӵ��ûص�����

        }
        //ͨ��Эͬ��������첽��Դ 
    }
    private IEnumerator ReallyLoadAsync<T>(string Path) where T : UnityEngine.Object
    {
        ResourceRequest rq=Resources.LoadAsync<T>(Path);
        yield return rq;
        //��Դ���ؽ���,����Դ���ݸ��ص�����
        string resName = Path + "_" + typeof(T).Name;
        if (ResDic.ContainsKey(resName))
        {
            ResInfo<T> resInfo = ResDic[resName] as ResInfo<T>;
            resInfo.Asset = rq.asset as T; //�����ص���Դ��ֵ����Դ��Ϣ����
            if(resInfo.refCount == 0)
            {
                UnloadAsset<T>(Path); //������Ϊ��Ҫɾ������ֱ��ж����Դ
            }
            else
            {
                resInfo.CallBack?.Invoke(resInfo.Asset); //���ûص�����
                                                         //������Ϻ���Щ���ÿ�����գ���ֹ�����ڴ�й©
                resInfo.Coroutine = null; //���Эͬ��������
                resInfo.CallBack = null; //����ص���������
            }
        }   
    }

    /// <summary>
    /// �첽������Դ�ķ���
    /// </summary>
    /// <param name="Path">��resources���µ��ļ�·��</param>
    /// <param name="CallBack">���ؽ�����Ļص�������ֻ�е�������ϲŻ�����������</param>
    [Obsolete("ע����û���ʹ�÷�����������Դ�����һ��Ҫ��type���м��ؾ�һ�����ܶ������Դ���ü���")]
    public void LoadAsync(string Path,Type type, UnityAction<UnityEngine.Object> CallBack)
    {
        //������Դ��Ψһid,����·����_��Դ����ƴ�Ӷ��ɵ�
        string resName = Path + "_" + type.Name;
        ResInfo < UnityEngine.Object > info;
        if (!ResDic.ContainsKey(resName))
        {
            //����һ����Դ��Ϣ����
            info = new ResInfo<UnityEngine.Object>();
            ResDic.Add(resName, info);//����Դ��Ϣ������ӵ��ֵ���
            //��¼ί�к���һ���������ʹ��
            info.CallBack += CallBack;
            //����Э�̽����첽����,���Ҽ�¼���Э�̳���
            info.Coroutine = MonoMange.Instance.StartCoroutine(ReallyLoadAsync(Path, type));
        }
        else
        {
            //����ֵ����Ѿ����������Դ��Ϣ����,��ֱ�ӻ�ȡ
            info = ResDic[resName] as ResInfo<UnityEngine.Object>;
            //�����Դ��û�м�����
            if (info.Asset == null)
                info.CallBack += CallBack; //��¼�ص�����
            else
                CallBack?.Invoke(info.Asset); //�����Դ�Ѿ��������,��ֱ�ӵ��ûص�����

        }
        //ͨ��Эͬ��������첽��Դ 
    }
    private IEnumerator ReallyLoadAsync(string Path, Type type)
    {
        ResourceRequest rq = Resources.LoadAsync(Path,type);
        yield return rq;
        //��Դ���ؽ���,����Դ���ݸ��ص�����
        string resName = Path + "_" + type.Name;
        if (ResDic.ContainsKey(resName))
        {
            ResInfo<UnityEngine.Object> resInfo = ResDic[resName] as ResInfo<UnityEngine.Object>;
            resInfo.Asset = rq.asset; //�����ص���Դ��ֵ����Դ��Ϣ����

            resInfo.CallBack?.Invoke(resInfo.Asset); //���ûص�����
            //������Ϻ���Щ���ÿ�����գ���ֹ�����ڴ�й©
            if (resInfo.refCount==0)
            {
                UnloadAsset(Path,type,resInfo.IsDel,null,true); //������Ϊ��Ҫɾ������ֱ��ж����Դ
            }
            else
            {
                resInfo.Coroutine = null; //���Эͬ��������
                resInfo.CallBack = null; //����ص���������
            }
        }
    }

    /// <summary>
    /// ж��Resources�ļ����µ���Դ
    /// </summary>
    /// <param name="asset"></param>
    public void UnloadAsset<T>(string Path, bool IsDel=false, UnityAction<T> CallBack=null, bool isSub = false)
    {
        string resName = Path + "_" + typeof(T).Name;
        if (ResDic.ContainsKey(resName))
        {
            ResInfo<T> resInfo = ResDic[resName] as ResInfo<T>;
               resInfo.ChangeRefCount(-1);
            resInfo.IsDel = IsDel;
            if (resInfo.Asset!=null&& resInfo.refCount == 0 && resInfo.IsDel)
            {
                ResDic.Remove(resName); //���ֵ����Ƴ������Դ��Ϣ����
                Resources.UnloadAsset(resInfo.Asset as UnityEngine.Object); //ж����Դ
            }
            else if(resInfo.Asset != null)//����ָ������Դ���ڼ�����
            {
                if(CallBack!=null)
                    resInfo.CallBack -= CallBack;
               // resInfo.IsDel = true; //���Ϊ��Ҫɾ��
            }
        }
    }
    public void UnloadAsset(string Path,Type type, bool IsDel = false, UnityAction<UnityEngine.Object> CallBack=null, bool isSub = false)
    {
        string resName = Path + "_" + type.Name;
        if (ResDic.ContainsKey(resName))
        {
            ResInfo<UnityEngine.Object> resInfo = ResDic[resName] as ResInfo<UnityEngine.Object>;
            if (isSub)
                resInfo.ChangeRefCount(-1);
            resInfo.IsDel = IsDel;
            if (resInfo.Asset != null&& resInfo.refCount == 0 && resInfo.IsDel)
            {
                ResDic.Remove(resName); //���ֵ����Ƴ������Դ��Ϣ����
                Resources.UnloadAsset(resInfo.Asset ); //ж����Դ
            }
            else if(resInfo.Asset != null)//����ָ������Դ���ڼ�����
            {
                if (CallBack != null)
                    resInfo.CallBack -= CallBack;
               // resInfo.IsDel = true; //���Ϊ��Ҫɾ��
            }
        }
       
    }
    /// <summary>
    /// �첽ж��δʹ�õ���Դ
    /// </summary>
    /// <param name="CallBack"></param>
    /// <returns></returns>
    public void UnloadUnusedAssets(UnityAction CallBack)
    {
       MonoMange.Instance.StartCoroutine(UnloadUnusedAssetsCoroutine( CallBack));
    }
   
    private IEnumerator UnloadUnusedAssetsCoroutine(UnityAction CallBack)
    {
        //�������������Ƴ�����û��ʹ����Դǰ��Ӧ�ð������Լ���¼����Щ���ü���Ϊ�㲢��û�б��Ƴ�����Դ�Ƴ���
        List<string> list = new List<string>();
        foreach (string path in ResDic.Keys)
        {
            if (ResDic[path].refCount == 0)
                list.Add(path);
        }
        foreach (string path in list)
        {
            ResDic.Remove(path);
        }
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        CallBack();
      
    }
    public void ClearDic(UnityAction CallBack)
    {
        MonoMange.Instance.StartCoroutine(ReallyClearDic(CallBack));
    }

    private IEnumerator ReallyClearDic(UnityAction CallBack)
    {
        ResDic.Clear();
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        CallBack();
    }
}
