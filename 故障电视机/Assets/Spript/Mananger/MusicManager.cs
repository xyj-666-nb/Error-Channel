using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MusicManager
{
    private static MusicManager _instance => new MusicManager();
    public static MusicManager Instance => _instance;


    //————————————————————背景音乐————————————————————
    private AudioSource BKAudioSource;
    private string CurrentAudioPath;
    private float CurrentAudioValue;
    private GameObject BKMusicObj;


    public void Init(string AudioSource)
    {
        CurrentAudioPath = AudioSource;
        ResourcesManager.Instance.LoadAsync<AudioSource>(AudioSource, GetAudioSource);
    }

    private void GetAudioSource(AudioSource audioSource)
    {
        if (BKAudioSource == null)
        {
            Debug.LogError("当前未检测到背景音乐");
            return;
        }

        BKAudioSource = audioSource;
        BKAudioSource.volume = CurrentAudioValue;
    }

    public void PlayerBKmusic(string AudioSourcel = null)
    {
        if (AudioSourcel == null && BKAudioSource == null)
        {
            Debug.LogError("当前未检测到背景音乐");
            return;
        }

        if (AudioSourcel != CurrentAudioPath)//传入了就用传入的
            Init(AudioSourcel);//初始化音乐
        //如果没有传入音乐进来就使用自己初始化的音乐

        if (BKMusicObj == null)//没有音乐物体就创建一个
        {
            GameObject Obj = new GameObject();//创建一个音乐播放物体
            Obj.name = "BkMusic";
            GameObject.DontDestroyOnLoad(Obj);//切换场景不销毁
            BKMusicObj = Obj;
        }
        BKAudioSource = BKMusicObj.AddComponent<AudioSource>();//添加音乐组件
        BKAudioSource.loop = true;//循环播放
        BKAudioSource.Play();//开始播放

    }

    public void PauseOrStartBkmusic(bool IsPause)
    {
        if (BKAudioSource == null)
        {
            Debug.LogError("当前未检测到背景音乐");
            return;
        }

        if (IsPause)
            BKAudioSource.Pause();
        else
            BKAudioSource.Play();
    }

    public void StopBkmusic()
    {
        if (BKAudioSource == null)
        {
            Debug.LogError("当前未检测到背景音乐");
            return;
        }

        BKAudioSource.Stop();

    }

    public void ChangeMusicValue(float Value)
    {
        CurrentAudioValue = Value;
        if (BKAudioSource != null)
            BKAudioSource.volume = Value;
    }



    //——————————————————————音效管理————————————————————

    private List<AudioSource> EffectMusicLis = new List<AudioSource>();
    [SerializeField] private GameObject EffectMusicObj;
    public void InitEffectMusic()
    {
        MonoMange.Instance.AddLister_Update(() => {
            //一直遍历音效列表
            for(int I= EffectMusicLis.Count-1;I>=0;I--)//从后往前遍历，防止移除列表时下标错误
            {
                if (!EffectMusicLis[I].isPlaying)
                {
                    EffectMusicLis[I].clip = null; 
                    PoolManage.Instance.PushObj(EffectMusicObj, EffectMusicLis[I].gameObject);//回收音效物体
                    EffectMusicLis.RemoveAt(I);//移除列表   
                }
            }
        });

        MonoMange.Instance.AddLister_OnDestory(() =>
        {
            ClearAllEffectMusic();
        });
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="Name">音效路径名字</param>
    /// <param name="IsLoop">是否循环</param>
    /// <param name="CallBack">加载完成的回调</param>
    public void PlayEffectMusic(string Name,bool IsLoop,UnityAction<AudioSource> CallBack)
    {
     
        //加载音效资源
        ResourcesManager.Instance.LoadAsync<AudioSource>(Name, (audioSource) =>
        {
            AudioSource audio = PoolManage.Instance.GetObj(EffectMusicObj).GetComponent<AudioSource>();
            audio.clip = audioSource.clip;
            audio.loop = IsLoop;
            audio.Play();
            audio.volume=CurrentAudioValue;
            if(!EffectMusicLis.Contains(audio))//不要做到重复添加
                EffectMusicLis.Add(audio);
            CallBack?.Invoke(audio);
        });
    }

    public void StopEffectMusic(AudioSource Source)
    {
        if(EffectMusicLis.Contains(Source))
        {
            Source.Stop();//停止播放
            EffectMusicLis.Remove(Source);//移除列表
            Source.clip = null;
            PoolManage.Instance.PushObj(EffectMusicObj, Source.gameObject);//回收音效物体
        }
    }

    public void PauseOrStartEffectMusic(bool IsPause)
    {
    }

    public void ChangeEffectMusicValue(float Value)
    {
        CurrentAudioValue = Value;
        for (int I = 0; I < EffectMusicLis.Count; I++)
        {
            EffectMusicLis[I].volume = Value;
        }
    }

    public void ClearAllEffectMusic()
    {
       foreach(var Music in EffectMusicLis)
        {
            Music.Stop();//停止播放
            Music.clip = null;
            PoolManage.Instance.PushObj(EffectMusicObj, Music.gameObject);//回收音效物体
        }
    }
}
