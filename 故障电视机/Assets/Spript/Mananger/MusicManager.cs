using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MusicManager
{
    private static MusicManager _instance => new MusicManager();
    public static MusicManager Instance => _instance;


    //�����������������������������������������������֡���������������������������������������
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
            Debug.LogError("��ǰδ��⵽��������");
            return;
        }

        BKAudioSource = audioSource;
        BKAudioSource.volume = CurrentAudioValue;
    }

    public void PlayerBKmusic(string AudioSourcel = null)
    {
        if (AudioSourcel == null && BKAudioSource == null)
        {
            Debug.LogError("��ǰδ��⵽��������");
            return;
        }

        if (AudioSourcel != CurrentAudioPath)//�����˾��ô����
            Init(AudioSourcel);//��ʼ������
        //���û�д������ֽ�����ʹ���Լ���ʼ��������

        if (BKMusicObj == null)//û����������ʹ���һ��
        {
            GameObject Obj = new GameObject();//����һ�����ֲ�������
            Obj.name = "BkMusic";
            GameObject.DontDestroyOnLoad(Obj);//�л�����������
            BKMusicObj = Obj;
        }
        BKAudioSource = BKMusicObj.AddComponent<AudioSource>();//����������
        BKAudioSource.loop = true;//ѭ������
        BKAudioSource.Play();//��ʼ����

    }

    public void PauseOrStartBkmusic(bool IsPause)
    {
        if (BKAudioSource == null)
        {
            Debug.LogError("��ǰδ��⵽��������");
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
            Debug.LogError("��ǰδ��⵽��������");
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



    //����������������������������������������������Ч������������������������������������������

    private List<AudioSource> EffectMusicLis = new List<AudioSource>();
    [SerializeField] private GameObject EffectMusicObj;
    public void InitEffectMusic()
    {
        MonoMange.Instance.AddLister_Update(() => {
            //һֱ������Ч�б�
            for(int I= EffectMusicLis.Count-1;I>=0;I--)//�Ӻ���ǰ��������ֹ�Ƴ��б�ʱ�±����
            {
                if (!EffectMusicLis[I].isPlaying)
                {
                    EffectMusicLis[I].clip = null; 
                    PoolManage.Instance.PushObj(EffectMusicObj, EffectMusicLis[I].gameObject);//������Ч����
                    EffectMusicLis.RemoveAt(I);//�Ƴ��б�   
                }
            }
        });

        MonoMange.Instance.AddLister_OnDestory(() =>
        {
            ClearAllEffectMusic();
        });
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    /// <param name="Name">��Ч·������</param>
    /// <param name="IsLoop">�Ƿ�ѭ��</param>
    /// <param name="CallBack">������ɵĻص�</param>
    public void PlayEffectMusic(string Name,bool IsLoop,UnityAction<AudioSource> CallBack)
    {
     
        //������Ч��Դ
        ResourcesManager.Instance.LoadAsync<AudioSource>(Name, (audioSource) =>
        {
            AudioSource audio = PoolManage.Instance.GetObj(EffectMusicObj).GetComponent<AudioSource>();
            audio.clip = audioSource.clip;
            audio.loop = IsLoop;
            audio.Play();
            audio.volume=CurrentAudioValue;
            if(!EffectMusicLis.Contains(audio))//��Ҫ�����ظ����
                EffectMusicLis.Add(audio);
            CallBack?.Invoke(audio);
        });
    }

    public void StopEffectMusic(AudioSource Source)
    {
        if(EffectMusicLis.Contains(Source))
        {
            Source.Stop();//ֹͣ����
            EffectMusicLis.Remove(Source);//�Ƴ��б�
            Source.clip = null;
            PoolManage.Instance.PushObj(EffectMusicObj, Source.gameObject);//������Ч����
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
            Music.Stop();//ֹͣ����
            Music.clip = null;
            PoolManage.Instance.PushObj(EffectMusicObj, Music.gameObject);//������Ч����
        }
    }
}
