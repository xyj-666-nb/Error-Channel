using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class Main : MonoBehaviour
{
    private static Main instance;
    public static Main Instance => instance;

    [SerializeField] private List<GameObject> NeedInitPrefabs = new List<GameObject>();
    public DialogueControl InitDia;
    public CRTPostEffecter CRTPostEffecter;


    private void Awake()
    {
        instance = this;
        // 初始化所有预制体
        foreach (var item in NeedInitPrefabs)
        {
            GameObject obj = Instantiate(item);
        }
    }


    void Start()
    {
        // 强制去除渲染管线
        ForceRemoveRenderPipeline();

        CRTPostEffecter.enabled = false;//失活屏幕效果
        Debug.Log("Main Start 开始执行");

        // 确保 InitDia 不为空
        if (InitDia == null)
        {
            Debug.LogError("InitDia 未赋值！请在 Inspector 中分配 DialogueControl 引用");
            return;
        }

        // 订阅对话结束事件
        InitDia.OnDialogueEnded += OnDialogueEnded;
        Debug.Log("已订阅对话结束事件");

        // 显示UI面板
        UImanager.Instance.ShowPanel<televisionPanel>();
        PlayerManager.instance.RefreshLevel();

        // 开始播放背景音乐
        MusicManager.Instance.SetSpecificBGMVolume("Music/背景音乐", 0.09f);
        MusicManager.Instance.PlayBKMusic("Music/背景音乐");
        // 延迟开始对话
        StartCoroutine(WaitTime());
    }

    // 强制去除渲染管线
    private void ForceRemoveRenderPipeline()
    {
        Debug.Log("开始强制去除渲染管线");

        // 多次设置确保生效
        for (int i = 0; i < 3; i++)
        {
            GraphicsSettings.renderPipelineAsset = null;
            QualitySettings.renderPipeline = null;
        }

        // 清除可能的缓存
        ClearPipelineCache();

        Debug.Log("强制去除渲染管线完成");
    }

    private void ClearPipelineCache()
    {
        try
        {
            // 清除RenderPipelineManager的当前管线
            var pipelineField = typeof(RenderPipelineManager).GetField("s_CurrentPipeline",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (pipelineField != null)
                pipelineField.SetValue(null, null);

            // 清除可能的管线实例
            var instanceField = typeof(RenderPipelineManager).GetField("<currentPipeline>k__BackingField",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (instanceField != null)
                instanceField.SetValue(null, null);

            Debug.Log("成功清除渲染管线缓存");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"清除缓存时出错: {e.Message}");
        }
    }

    // 对话结束事件处理方法
    private void OnDialogueEnded(int segmentIndex)
    {
        Debug.Log($"收到对话结束事件，段落索引: {segmentIndex}");

        if (segmentIndex == 0)
        {
            // 找到canvas并设置为world模式
            Canvas_FindMainCamera.instance.canvas.renderMode = RenderMode.WorldSpace;
            HandCardManger.Instance.InitCard(); // 刷新牌组
        }
        else if (segmentIndex == 1)
        {
            Debug.Log("触发时间线");
            StartCoroutine(waitTime_1());
        }
        else if (segmentIndex == 2)// 对话3结束
            TimeLineInstance1.Instance.OnDialogueEnd();// 对话结束还原
        else if (segmentIndex == 3)
            TimeLineInstance1.Instance.OnDialogueEnd();// 对话结束还原
        else if (segmentIndex == 4)
            TimeLineInstance1.Instance.OnDialogueEnd();// 对话结束还原  
        else if (segmentIndex == 8)
        {
            // 开启屏幕损坏
            CRTPostEffecter.enabled = true;
            TimeLineInstance1.Instance.OnDialogueEnd();// 对话结束还原  
        }
        else if (segmentIndex == 10 || segmentIndex == 11)
        {
            // 切换回开始场景
            ChangeScence();
        }
        else if (segmentIndex == 5 || segmentIndex == 6 || segmentIndex == 7 || segmentIndex == 9)
        {
            CRTPostEffecter.enabled = true;
        }
    }

    public void ChangeScence()
    {
        StartCoroutine(AsyncLoadScene("GameStartScence"));
    }

    // 异步加载场景的协程
    private IEnumerator AsyncLoadScene(string sceneName)
    {
        // 开始异步加载场景
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        // 禁止加载完成后自动激活场景
        asyncOperation.allowSceneActivation = false;

        // 循环等待加载完成
        while (!asyncOperation.isDone)
        {
            // 异步加载的进度范围是0-1，0.9代表资源加载完成
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // 当进度达到100%时，允许激活场景
            if (progress >= 1.0f)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null; // 等待一帧
        }
    }

    IEnumerator waitTime_1()
    {
        yield return new WaitForSeconds(1);
        // 启动时间线
        TimeLineInstance1.Instance.timeline.Play();
    }

    IEnumerator WaitTime()
    {
        Debug.Log("等待1秒后开始对话");
        yield return new WaitForSeconds(1);

        if (InitDia != null)
        {
            Debug.Log("开始对话段落 0");
            InitDia.StartDialogue(0);
        }
        else
        {
            Debug.LogError("InitDia 为 null，无法开始对话");
        }
    }

    // 移除了Update中的调试按键逻辑

    private void OnDestroy()
    {
        // 取消事件订阅，防止内存泄漏
        if (InitDia != null)
        {
            InitDia.OnDialogueEnded -= OnDialogueEnded;
            Debug.Log("已取消对话结束事件订阅");
        }
    }
}