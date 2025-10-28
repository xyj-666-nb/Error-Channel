using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueText
{
    public string speakerName;
    [TextArea(1, 3)]
    public string content;
}

[System.Serializable]
public class DialogueSegment
{
    [Header("对话段落名称")]
    public string segmentName;

    [Header("对话内容")]
    public List<DialogueText> dialogueTexts;

    [Header("触发设置")]
    public bool autoStart = false;

    [Header("对话结束事件")]
    public UnityEvent onDialogueEnd;
}

public class DialogueControl : MonoBehaviour
{
    [Header("对话段落列表")]
    public List<DialogueSegment> dialogueSegments = new List<DialogueSegment>();

    [Header("调试设置")]
    [SerializeField] private bool enableDebug = true;

    // 状态变量
    private int currentSegmentIndex = -1;
    private int currentTextIndex = 0;
    private bool isDialogueActive = false;
    private DialogueState currentState = DialogueState.Inactive;

    // 组件引用
    private DialoguePanel currentPanel;

    // 对话状态枚举
    private enum DialogueState
    {
        Inactive,           // 对话未激活
        PanelAnimating,     // 面板动画播放中
        TextAnimating,      // 文本打字效果中
        WaitingForInput,    // 等待用户输入
        Complete            // 对话完成
    }

    private void Start()
    {
        // 检查是否有需要自动开始的对话段落
        for (int i = 0; i < dialogueSegments.Count; i++)
        {
            if (dialogueSegments[i].autoStart)
            {
                StartDialogue(i);
                break;
            }
        }
    }

    private void Update()
    {
        // 只有在等待输入状态时才检测输入
        if (currentState == DialogueState.WaitingForInput)
        {
            if (Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.E))
            {
                LogDebug("检测到输入，继续下一句对话");
                ProceedToNextDialogue();
            }
        }

        // 检测跳过当前打字效果
        if (currentState == DialogueState.TextAnimating &&
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            if (currentPanel != null)
            {
                currentPanel.SkipTypingEffect();
            }
        }

        // 测试用触发键
        if (!isDialogueActive && Input.GetKeyDown(KeyCode.T))
        {
            // 默认触发第一段对话
            if (dialogueSegments.Count > 0)
            {
                StartDialogue(0);
            }
        }
    }

    /// <summary>
    /// 开始指定段落的对话
    /// </summary>
    /// <param name="segmentIndex">段落索引</param>
    public void StartDialogue(int segmentIndex)
    {
        if (isDialogueActive ||
            segmentIndex < 0 ||
            segmentIndex >= dialogueSegments.Count ||
            dialogueSegments[segmentIndex].dialogueTexts == null ||
            dialogueSegments[segmentIndex].dialogueTexts.Count == 0)
        {
            LogDebug($"无法开始对话段落 {segmentIndex}，状态: {isDialogueActive}, 段落数量: {dialogueSegments.Count}");
            return;
        }

        LogDebug($"开始对话段落: {segmentIndex} - {dialogueSegments[segmentIndex].segmentName}");

        isDialogueActive = true;
        currentSegmentIndex = segmentIndex;
        currentTextIndex = 0;
        currentState = DialogueState.PanelAnimating;

        UImanager.Instance.ShowPanel<DialoguePanel>();
        currentPanel = UImanager.Instance.GetPanel<DialoguePanel>();

        if (currentPanel != null)
        {
            currentPanel.AnimaEndCallBack = OnDialoguePanelReady;
            //currentPanel.ShowAnimator();
        }
        else
        {
            LogError("无法获取对话面板！");
            EndDialogue();
        }
    }

    /// <summary>
    /// 通过名称开始对话段落
    /// </summary>
    /// <param name="segmentName">段落名称</param>
    public void StartDialogue(string segmentName)
    {
        int segmentIndex = FindDialogueSegmentIndex(segmentName);
        if (segmentIndex != -1)
        {
            StartDialogue(segmentIndex);
        }
        else
        {
            LogError($"未找到名为 '{segmentName}' 的对话段落");
        }
    }

    /// <summary>
    /// 查找对话段落的索引
    /// </summary>
    private int FindDialogueSegmentIndex(string segmentName)
    {
        for (int i = 0; i < dialogueSegments.Count; i++)
        {
            if (dialogueSegments[i].segmentName == segmentName)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 对话面板动画完成回调
    /// </summary>
    private void OnDialoguePanelReady()
    {
        LogDebug("对话面板准备就绪");
        currentState = DialogueState.TextAnimating;
        ShowCurrentDialogue();
    }

    /// <summary>
    /// 显示当前对话
    /// </summary>
    private void ShowCurrentDialogue()
    {
        if (currentSegmentIndex == -1 ||
            currentTextIndex >= dialogueSegments[currentSegmentIndex].dialogueTexts.Count)
        {
            EndDialogue();
            return;
        }

        if (currentPanel == null)
        {
            LogError("对话面板丢失！");
            EndDialogue();
            return;
        }

        var currentDialogue = dialogueSegments[currentSegmentIndex].dialogueTexts[currentTextIndex];
        LogDebug($"显示对话 {currentTextIndex + 1}/{dialogueSegments[currentSegmentIndex].dialogueTexts.Count}");

        currentPanel.SetDialogue(
            currentDialogue.speakerName,
            currentDialogue.content,
            OnCurrentDialogueFinished
        );
    }

    /// <summary>
    /// 当前对话显示完成回调
    /// </summary>
    private void OnCurrentDialogueFinished()
    {
        LogDebug($"对话 {currentTextIndex + 1} 显示完成，等待用户输入");
        currentState = DialogueState.WaitingForInput;
    }

    /// <summary>
    /// 继续到下一句对话
    /// </summary>
    private void ProceedToNextDialogue()
    {
        // 确保在正确状态
        if (currentState != DialogueState.WaitingForInput)
        {
            LogWarning($"尝试继续对话但状态不正确，当前状态: {currentState}");
            return;
        }

        currentTextIndex++;

        if (currentTextIndex < dialogueSegments[currentSegmentIndex].dialogueTexts.Count)
        {
            currentState = DialogueState.TextAnimating;
            ShowCurrentDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    /// <summary>
    /// 结束对话
    /// </summary>
    private void EndDialogue()
    {
        Debug.Log($"对话段落结束: {currentSegmentIndex}");

        // 触发当前段落的结束事件
        if (currentSegmentIndex != -1)
        {
            dialogueSegments[currentSegmentIndex].onDialogueEnd?.Invoke();
        }

        // 保存当前段落索引，因为后面会重置
        int endedSegmentIndex = currentSegmentIndex;

        // 重置状态
        isDialogueActive = false;
        currentSegmentIndex = -1;
        currentTextIndex = 0;
        currentState = DialogueState.Inactive;

        if (currentPanel != null)
        {
            UImanager.Instance.HidePanel<DialoguePanel>();
            currentPanel = null;
        }

        // 触发对话结束事件（传递结束的段落索引）
        Debug.Log($"触发 OnDialogueEnded 事件，索引: {endedSegmentIndex}");
        OnDialogueEnded?.Invoke(endedSegmentIndex);
    }
    /// <summary>
    /// 检查事件订阅状态（用于调试）
    /// </summary>
    public void CheckEventSubscriptions()
    {
        if (OnDialogueEnded == null)
        {
            Debug.Log("OnDialogueEnded 事件没有订阅者");
        }
        else
        {
            int subscriberCount = OnDialogueEnded.GetInvocationList().Length;
            Debug.Log($"OnDialogueEnded 事件有 {subscriberCount} 个订阅者");
        }
    }

    // 对话结束事件（带段落索引）
    public UnityAction<int> OnDialogueEnded;

    /// <summary>
    /// 外部调用开始对话（默认第一段）
    /// </summary>
    public void TriggerDialogue()
    {
        if (dialogueSegments.Count > 0)
        {
            StartDialogue(0);
        }
    }

    /// <summary>
    /// 强制结束对话
    /// </summary>
    public void EndDialogueImmediately()
    {
        EndDialogue();
    }

    /// <summary>
    /// 检查指定段落是否可用
    /// </summary>
    public bool IsDialogueSegmentAvailable(int segmentIndex)
    {
        return segmentIndex >= 0 &&
               segmentIndex < dialogueSegments.Count &&
               dialogueSegments[segmentIndex].dialogueTexts != null &&
               dialogueSegments[segmentIndex].dialogueTexts.Count > 0;
    }

    /// <summary>
    /// 检查指定段落是否可用
    /// </summary>
    public bool IsDialogueSegmentAvailable(string segmentName)
    {
        int index = FindDialogueSegmentIndex(segmentName);
        return IsDialogueSegmentAvailable(index);
    }

    /// <summary>
    /// 获取当前对话状态（用于调试）
    /// </summary>
    public string GetCurrentState()
    {
        return currentState.ToString();
    }

    /// <summary>
    /// 获取当前正在播放的段落索引
    /// </summary>
    public int GetCurrentSegmentIndex()
    {
        return currentSegmentIndex;
    }

    /// <summary>
    /// 获取对话段落数量
    /// </summary>
    public int GetSegmentCount()
    {
        return dialogueSegments.Count;
    }

    /// <summary>
    /// 调试日志
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebug)
        {
            Debug.Log($"[DialogueSystem] {message}");
        }
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[DialogueSystem] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[DialogueSystem] {message}");
    }

    private void OnDestroy()
    {
        OnDialogueEnded = null;

        // 清理所有UnityEvent
        foreach (var segment in dialogueSegments)
        {
            segment.onDialogueEnd?.RemoveAllListeners();
        }
    }
}