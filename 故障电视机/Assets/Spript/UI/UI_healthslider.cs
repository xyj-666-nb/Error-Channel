using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_healthslider : MonoBehaviour
{
    //ȫ�־�һ��Ѫ�����뵥��
    public static UI_healthslider instance;//����
    public UI_healthslider Instance => instance;//Ѫ��������

    [SerializeField] private Slider MyhealthBar;//�ҵ�Ѫ��
    [SerializeField] private TextMeshProUGUI HealthText;//Ѫ���ı�
    [SerializeField]private Image HealthImage;//Ѫ��ͼƬ

    private bool IsNeedUpdate = false;//�Ƿ���Ҫ����Ѫ��
    private float TargetValue;//Ŀ��Ѫ��
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
            HealthImage.DOColor(Color.green , 0.2f); ;//��Ѫ����
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
            HealthImage.DOColor(Color.red, 0.2f); ;//���
        }

    }
}
