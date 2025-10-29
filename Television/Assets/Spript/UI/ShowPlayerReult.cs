using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class ShowPlayerReult : MonoBehaviour
{
    private static ShowPlayerReult instance;
    public static ShowPlayerReult Instance => instance;
    [SerializeField] TextMeshProUGUI Number;
    //显示玩家的结果（确保4位数，不足补零）
    [SerializeField]private TextMeshProUGUI StrengthenResult;

    private void Awake()
    {
        instance = this;
        StrengthenResult.gameObject.SetActive(false);
    }

    public void SetResult(int result)
    {
        transform.DOKill();
        string formattedResult = result.ToString("D4");
        Number.DOFade(1, 0.5f).OnComplete(() => {
            Number.text = formattedResult;
            if (PlayerManager.instance.CurrentPlayerStrengthenReult > 0)
            {
                // 关键修复：激活前先重置透明度为1，确保能看到
                StrengthenResult.alpha = 1f;
                StrengthenResult.text = "结果强化加成！+" + PlayerManager.instance.CurrentPlayerStrengthenReult.ToString();
                StrengthenResult.gameObject.SetActive(true);
            }
            StartCoroutine(WaitTime());
        });
    }

    IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(0.5f);
        Number.DOFade(0, 0.5f).OnComplete(() => {
            Number.text = "0000";
            Number.DOFade(1, 0.5f);
            // 优化：停止文本现有动画，再执行淡出
            StrengthenResult.DOKill();
            StrengthenResult.DOFade(0, 1f).OnComplete(() => {
                StrengthenResult.gameObject.SetActive(false);
            });
        });
    }
}