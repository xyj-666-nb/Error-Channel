using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartPanel : MonoBehaviour
{
    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button ExitGameButton;
    [SerializeField] private Button introduceButton;
    public PlayableDirector timeline; // 赋值你的Timeline
    // 可选：添加进度条UI（如果需要显示加载进度）
    [SerializeField] private Slider loadProgressSlider;
    [SerializeField] private Text progressText;

    private void Start()
    {
        StartGameButton.onClick.AddListener(() => { timeline.Play(); });//启动时间线 
        ExitGameButton.onClick.AddListener(() => { Application.Quit(); });//退出游戏（修正原introduceButton的错误绑定）
        introduceButton.onClick.AddListener(ShowIntroduction); // 假设介绍按钮用于显示游戏说明
    }

    // 启动异步加载场景的协程
    public  void  ChangeScence()
    {
        StartCoroutine(AsyncLoadScene("SampleScene"));
    }

    // 异步加载场景的协程
    private  IEnumerator AsyncLoadScene(string sceneName)
    {
        // 开始异步加载场景，第二个参数为加载模式（Single表示关闭当前场景加载新场景）
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        // 禁止加载完成后自动激活场景（便于处理进度显示）
        asyncOperation.allowSceneActivation = false;

        // 循环等待加载完成
        while (!asyncOperation.isDone)
        {
            // 异步加载的进度范围是0-1，但实际到0.9时就代表资源加载完成
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // 更新进度条（如果有）
            if (loadProgressSlider != null)
                loadProgressSlider.value = progress;
            if (progressText != null)
                progressText.text = $"{(int)(progress * 100)}%";

            // 当进度达到100%时，允许激活场景
            if (progress >= 1.0f)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null; // 等待一帧
        }
    }

    public void StopTimeLine()
    {
        timeline.Pause();
    }

    // 游戏介绍（示例方法，可根据实际需求实现）
    private void ShowIntroduction()
    {
        Debug.Log("显示游戏介绍");
        // 这里可以实现打开介绍面板等逻辑
    }
}