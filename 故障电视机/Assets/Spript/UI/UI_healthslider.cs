using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_healthslider : MonoBehaviour
{
    //全局就一个血条给与单例
    public static UI_healthslider instance;//单例
    public UI_healthslider Instance => instance;//血条控制器

    [SerializeField] private Slider MyhealthBar;//我的血条
    [SerializeField] private TextMeshProUGUI HealthText;//血量文本
    [SerializeField]private Image HealthImage;//血条图片

    private bool IsNeedUpdate = false;//是否需要更新血条
    private float TargetValue;//目标血量
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    private void Start()
    {
        MyhealthBar.value = PlayerManager.instance.CurrentHealth;
        HealthText.text = PlayerManager.instance.CurrentHealth + "/" + PlayerManager.instance.MaxHealth;
    }

    public void UpdateHeathBar(float Value, float MaxValue)
    {
        IsNeedUpdate = true;
        TargetValue = Value / MaxValue;
        if (MyhealthBar.value < TargetValue)
            HealthImage.DOColor(Color.green , 0.2f); ;//回血变绿
        HealthText.text = PlayerManager.instance.CurrentHealth + "/" + PlayerManager.instance.MaxHealth;
    }

    private void Update()
    {
        if(!IsNeedUpdate)
            return;

        if(MyhealthBar.value!= TargetValue)
            MyhealthBar.value=Mathf.Lerp(MyhealthBar.value, TargetValue, 0.05f);
        else
        {
            IsNeedUpdate = false;
            HealthImage.DOColor(Color.red, 0.2f); ;//变红
        }

    }
}
