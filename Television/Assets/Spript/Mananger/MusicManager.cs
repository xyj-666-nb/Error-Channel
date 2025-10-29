using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MusicManager : MonoBehaviour
{
    private static MusicManager _instance;
    public static MusicManager Instance => _instance;

    //————————————————————背景音乐————————————————————
    private AudioSource BKAudioSource;
    private string CurrentAudioPath; // 当前播放的背景音乐路径（用于匹配特定音量）
    private float bgmGlobalVolume = 0.5f; // 背景音乐全局音量
    // 新增：存储特定背景音乐的独立音量（键：音乐路径/名称，值：0-1的音量）
    private Dictionary<string, float> specificBGMVolumes = new Dictionary<string, float>();
    private GameObject BKMusicObj;

    //——————————————————————音效管理————————————————————
    [SerializeField] private List<AudioSource> EffectMusicLis = new List<AudioSource>();
    [SerializeField] private GameObject EffectMusicPrefab;

    private float effectGlobalVolume = 0.8f; // 音效全局音量
    private Dictionary<string, float> specificEffectVolumes = new Dictionary<string, float>(); // 特定音效音量


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeBackgroundMusic();
    }

    private void Start()
    {
        InitializeEffectSystem();
    }

    /// <summary>
    /// 初始化背景音乐系统
    /// </summary>
    private void InitializeBackgroundMusic()
    {
        if (BKMusicObj == null)
        {
            BKMusicObj = new GameObject("BackgroundMusic");
            BKMusicObj.transform.SetParent(transform);
            BKAudioSource = BKMusicObj.AddComponent<AudioSource>();
            BKAudioSource.loop = true;
            BKAudioSource.volume = bgmGlobalVolume; // 初始使用全局音量
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayBKMusic(string audioPath = null)
    {
        if (string.IsNullOrEmpty(audioPath))
        {
            if (BKAudioSource.clip != null)
            {
                BKAudioSource.Play();
            }
            else
            {
                Debug.LogWarning("没有设置背景音乐剪辑");
            }
            return;
        }

        if (audioPath == CurrentAudioPath && BKAudioSource.isPlaying)
            return;

        CurrentAudioPath = audioPath; // 记录当前音乐路径

        ResourcesManager.Instance.LoadAsync<AudioClip>(audioPath, (audioClip) =>
        {
            if (audioClip != null && BKAudioSource != null)
            {
                BKAudioSource.clip = audioClip;
                // 核心：计算背景音乐最终音量
                float finalVolume = specificBGMVolumes.TryGetValue(audioPath, out float vol) ? vol : bgmGlobalVolume;
                BKAudioSource.volume = finalVolume;
                BKAudioSource.Play();
            }
            else
            {
                Debug.LogError($"无法加载背景音乐: {audioPath}");
            }
        });
    }

    public void PauseOrStartBKMusic(bool isPause)
    {
        // 原有逻辑不变
        if (BKAudioSource == null)
        {
            Debug.LogWarning("背景音乐源未初始化");
            return;
        }

        if (isPause)
            BKAudioSource.Pause();
        else
            BKAudioSource.Play();
    }

    public void StopBKMusic()
    {
        // 原有逻辑不变
        if (BKAudioSource != null)
        {
            BKAudioSource.Stop();
            BKAudioSource.clip = null;
            CurrentAudioPath = null;
        }
    }

    /// <summary>
    /// 改变背景音乐全局音量
    /// </summary>
    public void ChangeMusicVolume(float value)
    {
        bgmGlobalVolume = Mathf.Clamp01(value);

        // 添加调试信息
        Debug.Log($"Changing BGM volume to: {bgmGlobalVolume}, Current Audio: {CurrentAudioPath}");

        if (BKAudioSource != null && !string.IsNullOrEmpty(CurrentAudioPath))
        {
            // 关键修改：总是重新计算最终音量
            float finalVolume = specificBGMVolumes.TryGetValue(CurrentAudioPath, out float specificVol)
                ? specificVol * bgmGlobalVolume  // 如果特定音量存在，将其与全局音量结合
                : bgmGlobalVolume;               // 否则使用全局音量

            BKAudioSource.volume = finalVolume;
            Debug.Log($"Applied BGM volume: {finalVolume}");
        }
        else
        {
            Debug.LogWarning("BKAudioSource is null or no current audio path");
        }
    }

    // 新增：单独设置某个背景音乐的音量
    /// <param name="bgmName">背景音乐的路径/名称（和PlayBKMusic传入的audioPath一致）</param>
    /// <param name="volume">0-1的音量值</param>
    public void SetSpecificBGMVolume(string bgmName, float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (specificBGMVolumes.ContainsKey(bgmName))
        {
            specificBGMVolumes[bgmName] = volume;
        }
        else
        {
            specificBGMVolumes.Add(bgmName, volume);
        }

        // 实时更新正在播放的该背景音乐音量
        if (BKAudioSource != null && CurrentAudioPath == bgmName)
        {
            BKAudioSource.volume = volume;
        }
    }

    //——————————————————————音效管理————————————————————

    private void InitializeEffectSystem()
    {
        if (EffectMusicPrefab == null)
        {
            Debug.LogError("音效预制体未设置!");
            return;
        }

        MonoMange.Instance.AddLister_Update(CleanupFinishedEffects);

        MonoMange.Instance.AddLister_OnDestory(() =>
        {
            ClearAllEffectMusic();
        });
    }

    private void CleanupFinishedEffects()
    {
        for (int i = EffectMusicLis.Count - 1; i >= 0; i--)
        {
            var audioSource = EffectMusicLis[i];
            if (audioSource != null && !audioSource.isPlaying && audioSource.clip != null)
            {
                if (!audioSource.loop)
                {
                    ReturnEffectToPool(audioSource);
                    EffectMusicLis.RemoveAt(i);
                }
            }
            else if (audioSource == null)
            {
                EffectMusicLis.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlayEffectMusic(string name, bool isLoop = false, UnityAction<AudioSource> callback = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("音效名称为空");
            return;
        }

        ResourcesManager.Instance.LoadAsync<AudioClip>(name, (audioClip) =>
        {
            if (audioClip == null)
            {
                Debug.LogError($"无法加载音效: {name}");
                callback?.Invoke(null);
                return;
            }

            GameObject effectObj = PoolManage.Instance.GetObj(EffectMusicPrefab);
            if (effectObj == null)
            {
                Debug.LogError("无法从对象池获取音效对象");
                callback?.Invoke(null);
                return;
            }

            AudioSource audioSource = effectObj.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = effectObj.AddComponent<AudioSource>();
            }

            // 优先使用特定音效音量，否则用全局音效音量
            float finalVolume = specificEffectVolumes.TryGetValue(name, out float vol) ? vol : effectGlobalVolume;

            audioSource.clip = audioClip;
            audioSource.loop = isLoop;
            audioSource.volume = finalVolume;
            audioSource.Play();

            if (!EffectMusicLis.Contains(audioSource))
            {
                EffectMusicLis.Add(audioSource);
            }

            callback?.Invoke(audioSource);
        });
    }

    public void StopEffectMusic(AudioSource source)
    {
        if (source != null && EffectMusicLis.Contains(source))
        {
            source.Stop();
            ReturnEffectToPool(source);
            EffectMusicLis.Remove(source);
        }
    }

    public void PauseOrStartAllEffects(bool isPause)
    {
        foreach (var audioSource in EffectMusicLis)
        {
            if (audioSource != null)
            {
                if (isPause)
                    audioSource.Pause();
                else
                    audioSource.UnPause();
            }
        }
    }

    /// <summary>
    /// 改变音效全局音量
    /// </summary>
    public void ChangeEffectVolume(float value)
    {
        effectGlobalVolume = Mathf.Clamp01(value);

        Debug.Log($"Changing Effect volume to: {effectGlobalVolume}, Active effects: {EffectMusicLis.Count}");

        foreach (var audioSource in EffectMusicLis)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                string clipName = audioSource.clip.name;
                // 关键修改：总是重新计算最终音量
                float finalVolume = specificEffectVolumes.TryGetValue(clipName, out float specificVol)
                    ? specificVol * effectGlobalVolume  // 结合特定音量和全局音量
                    : effectGlobalVolume;               // 使用全局音量

                audioSource.volume = finalVolume;
                Debug.Log($"Applied effect volume to {clipName}: {finalVolume}");
            }
        }
    }

    // 音效特定音量设置（与音乐逻辑对称）
    public void SetSpecificEffectVolume(string effectName, float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (specificEffectVolumes.ContainsKey(effectName))
        {
            specificEffectVolumes[effectName] = volume;
        }
        else
        {
            specificEffectVolumes.Add(effectName, volume);
        }

        foreach (var source in EffectMusicLis)
        {
            if (source != null && source.clip != null && source.clip.name == effectName)
            {
                source.volume = volume;
            }
        }
    }

    public void ClearAllEffectMusic()
    {
        foreach (var audioSource in EffectMusicLis)
        {
            if (audioSource != null)
            {
                ReturnEffectToPool(audioSource);
            }
        }
        EffectMusicLis.Clear();
    }

    private void ReturnEffectToPool(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            PoolManage.Instance.PushObj(EffectMusicPrefab, audioSource.gameObject);
        }
    }

    // 实用方法
    public bool IsBackgroundMusicPlaying => BKAudioSource != null && BKAudioSource.isPlaying;
    public float BackgroundMusicGlobalVolume => bgmGlobalVolume; // 全局背景音乐音量
    public float EffectGlobalVolume => effectGlobalVolume; // 全局音效音量
    public int ActiveEffectCount => EffectMusicLis.Count;
}