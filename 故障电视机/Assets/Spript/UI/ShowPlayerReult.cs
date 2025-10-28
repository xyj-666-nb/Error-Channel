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
        // 核心修改：使用"D4"格式确保4位数，不足自动补零
        string formattedResult = result.ToString("D4");
        Number.DOFade(1, 0.5f).OnComplete(() => {
            Number.text = formattedResult;
            if (PlayerManager.instance.CurrentPlayerStrengthenReult>0)//如果解锁结果强化！
                StrengthenResult.text = "结果强化加成！+" + PlayerManager.instance.CurrentPlayerStrengthenReult.ToString();
                StrengthenResult.gameObject.SetActive(true);
            StartCoroutine(WaitTime());
        });
    }

    IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(0.5f);
        Number.DOFade(0, 0.5f).OnComplete(() => {
            Number.text = "0000"; // 保持4位零的初始状态
            Number.DOFade(1, 0.5f);
            StrengthenResult.DOFade(0, 1f).OnComplete(() => { StrengthenResult.gameObject.SetActive(false); });
        });
    }
}