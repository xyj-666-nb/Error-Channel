using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private static Main instance;
    public static Main Instance => instance;

    [SerializeField] private List<GameObject> NeedInitPrefabs = new List<GameObject>();
    public DialogueControl InitDia;


    private void Awake()
    {
        instance = this;

        // 先初始化所有的预制体
        foreach (var item in NeedInitPrefabs)
        {
            GameObject obj = Instantiate(item);
            DontDestroyOnLoad(obj);
        }
    }

    void Start()
    {
        Debug.Log("Main Start 开始执行");

        // 确保 InitDia 不为空
        if (InitDia == null)
        {
            Debug.LogError("InitDia 未赋值！请在 Inspector 中分配 DialogueControl 引用");
            return;
        }

        // 正确订阅事件 - 注意参数匹配
        InitDia.OnDialogueEnded += OnDialogueEnded;
        Debug.Log("已订阅对话结束事件");

        // 显示UI面板
        UImanager.Instance.ShowPanel<televisionPanel>();
        PlayerManager.instance.RefreshLevel();

        // 开始播放背景音乐
        MusicManager.Instance.SetSpecificBGMVolume("Music/背景音乐", 0.05f);
        MusicManager.Instance.PlayBKMusic("Music/背景音乐");

        // 延迟开始对话
        StartCoroutine(WaitTime());
    }

    // 正确的对话结束事件处理方法（带参数）
    private void OnDialogueEnded(int segmentIndex)
    {
        Debug.Log($"收到对话结束事件，段落索引: {segmentIndex}");

        if (segmentIndex == 0)
        {
            //找到canvas并把模式设置为world模式
            Canvas_FindMainCamera.instance.canvas.renderMode = RenderMode.WorldSpace;
            HandCardManger.Instance.InitCard(); //刷新牌组
        }
        else if(segmentIndex == 1)
        {
            Debug.Log("触发时间线");
            StartCoroutine(waitTime_1());
        }
        else if(segmentIndex == 2)//如果对话3结束
            TimeLineInstance1.Instance.OnDialogueEnd();//对话结束还原
        else if(segmentIndex == 3)
             TimeLineInstance1.Instance.OnDialogueEnd();//对话结束还原
        else if(segmentIndex == 4)
            TimeLineInstance1.Instance.OnDialogueEnd();//对话结束还原  
    }

    IEnumerator waitTime_1()
    {
        yield return new WaitForSeconds(1);
        //启动时间线
        TimeLineInstance1.Instance.timeline.Play();
    }

    IEnumerator WaitTime()
    {
        Debug.Log("等待1秒后开始对话");
        yield return new WaitForSeconds(1);

        if (InitDia != null)
        {
            // 调试：检查事件订阅状态
            InitDia.CheckEventSubscriptions();

            Debug.Log("开始对话段落 0");
            InitDia.StartDialogue(0);
        }
        else
        {
            Debug.LogError("InitDia 为 null，无法开始对话");
        }
    }

    void Update()
    {
        // 调试用：按空格键手动触发对话结束事件测试
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("手动测试对话结束事件");
            OnDialogueEnded(0);
        }
    }

    private void OnDestroy()
    {
        // 重要：取消事件订阅，防止内存泄漏
        if (InitDia != null)
        {
            InitDia.OnDialogueEnded -= OnDialogueEnded;
            Debug.Log("已取消对话结束事件订阅");
        }
    }
}