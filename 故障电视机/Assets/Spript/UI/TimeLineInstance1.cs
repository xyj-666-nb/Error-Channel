using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimeLineInstance1 : MonoBehaviour
{
    private static TimeLineInstance1 instance;
    public static TimeLineInstance1 Instance=> instance;

    [Header("Timeline引用")]
    public PlayableDirector timeline; // 赋值你的Timeline

    private void Awake()
    {
        instance= this;
    }

    public void OnDialogTrigger_1()
    {
        timeline.Pause(); // 暂停Timeline
        //开启第三段对话
        Main.Instance.InitDia.StartDialogue(2);//开启第二段的对话
    }

    public void OnDialogTrigger_2()
    {
        timeline.Pause(); // 暂停Timeline
        //开启第三段对话
        Debug.Log("触发第4段对话");
        Main.Instance.InitDia.StartDialogue(3);//开启第四段的对话
    }

    /// <summary>
    /// 对话结束后，继续播放Timeline
    /// </summary>
    public void OnDialogueEnd()
    {
        timeline.Play(); // 继续播放Timeline剩余部分（还原摄像机）
    }

    public void OnDialogTrigger_3()
    {
        timeline.Pause(); // 暂停Timeline
        //开启第三段对话
        Debug.Log("触发第5段对话");
        Main.Instance.InitDia.StartDialogue(4);//开启第五段的对话
    }


}
