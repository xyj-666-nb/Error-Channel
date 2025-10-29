
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UImanager 
{
    private Transform canvasform;

    //UI管理器的作用就把所有的面板进行一个封装
    //我们只需要调用这一个类中的方法传入名字就可以获取所有的面板功能，非常方便

    //UI管理器只有一个所以要写成单例模式
    private static UImanager instance = new UImanager();//由于没有继承monbehavior所以可以new,并且必须去自己new
    public static UImanager Instance => instance;

    //我们这里需要一个容器用来存储面板（注意·这里的存储的面板是已经显示了的面板，不是全部的面板信息）
    //为什么要这样写呢，首先我们在showPanel的时候是在resources上去获取一个面板然后显示
    //那我们如何关闭这个面板呢，我们在关闭这个面板的时候需要这个面板的信息啊
    //所以需要一个空间来存储面板，获取，存入，提取信息，然后关闭


    //我们这里通过字典来存储这个面板，用名字来查找非常方便
    //里氏替换原则，通过父类来装载子类，最后用的时候as成想要的子类就ok
    private Dictionary<string, BasePanel> PanelDic = new Dictionary<string, BasePanel>();

    //写一个私有的构造函数去初始化组件
    private UImanager()
    {
        //一开始就从预设体中创建一个canvas并且不能让他随场景的改变而删除
        GameObject canvas = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("UI/Canvas"));
        //过场景不要删除
        canvasform = canvas.transform;
        GameObject.DontDestroyOnLoad(canvas);
    }

    //显示面板
    public T ShowPanel<T>() where T : BasePanel//加入一个约束就是只T必须要继承这个BasePanel
    {
        string panelName = typeof(T).Name;//获取T的类型并提取名字

        //寻找一下字典里有没有这个值，如果有就代表已经显示在界面上面了，这时候就不需要去再显示了
        if (PanelDic.ContainsKey(panelName))
        {
            //如果有就直接返回这个面板
            return PanelDic[panelName] as T;
        }
        GameObject panelObj = null;


        if (Resources.Load<GameObject>("UI/" + panelName).GetComponent<BasePanel>().IsCanDestroy)
        {
            //如果界面上没有面板那则需要代码去找到这个预设体去使用他
            panelObj = GameObject.Instantiate(Resources.Load<GameObject>("UI/" + panelName));
        }
        else
        {
            //如果这个面板是那种不能被删除的那我们就需要判断一下这个面板是否已经被创建过了
            //如果创建过了那就直接去找这个面板
            if (GameObject.Find(panelName))
                panelObj = GameObject.Find(panelName);
            else
                panelObj = GameObject.Instantiate(Resources.Load<GameObject>("UI/" + panelName));//如果没有创建过那就创建一个
        }


        //由于我们的的所有的面板都要创建再canvas里面这里我们需要设置父对象
        panelObj.transform.SetParent(canvasform, false);

        T panel = panelObj.GetComponent<T>();
        //然后再存到字典里面
        PanelDic.Add(panelName, panel);
        //最后调用这个面板的显示函数
        panel.ShowMe();
        return panel;
    }

    //隐藏面板

    //这里提供一个是否需要淡入淡出的bool值
    //这里不需要返回值
    public void HidePanel<T>(bool isFade = true, UnityAction CallBack = null) where T : BasePanel//加入一个约束就是只T必须要继承这个BasePanel
    {
        string panelName = typeof(T).Name;//获取T的类型并提取名字
        if (PanelDic.ContainsKey(panelName))
        {
            if (isFade)
            {
                //如果我们需要去有淡出这个效果那我们就使用hideMe这个函数
                //里面提供了当淡出完毕所执行的委托函数
                PanelDic[panelName].HideMe(() =>
                {
                    if (PanelDic[panelName].IsCanDestroy)//先判断是否能被删除
                        GameObject.Destroy(PanelDic[panelName].gameObject);
                    CallBack?.Invoke();//然后执行我们传入的回调函数
                    //然后记得还要删除字典里存的信息
                    PanelDic.Remove(panelName);
                });
            }
            else
            {
                //直接删除这个对象就欧克了
                //有的面板有特殊需求不能被删除
                GameObject.Destroy(PanelDic[panelName].gameObject);
                //然后记得还要删除字典里存的信息
                PanelDic.Remove(panelName);
            }
        }
    }
    //获取面板

    public T GetPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;//获取T的类型并提取名字
        if (PanelDic.ContainsKey(panelName))
        {
            return PanelDic[panelName] as T;
        }
        return null;
    }
}
