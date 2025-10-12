using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

enum SortingLayerType//��ʾ�㼶
{
    Card ,
    FrontCard,
}

public class Card : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool IsChoose=false;//�Ƿ�ѡ��,Ĭ��û�б�ѡ��
    [SerializeField] SpriteRenderer BackGround;//���Ƶı���
    [SerializeField] SpriteRenderer ContentImage;//���Ƶı���

    private void Awake()
    {
        SetSortingLayer(SortingLayerType.Card);//���ÿ��Ƶ���ʾ�㼶
    }

    private void SetSortingLayer(SortingLayerType Type)
    {
        BackGround.sortingLayerName=Type.ToString();
        ContentImage.sortingLayerName= Type.ToString();
    }
    private void Update()
    {
        // �������Ļλ��ת��Ϊ�������꣨2D������
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // ������λ���Ƿ����е�ǰ���Ƶ���ײ��
        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos);

        if (hitCollider != null && hitCollider.gameObject == gameObject)
        {
            Debug.Log("�������ͣ�ڿ����ϣ�δ�����");
        }
        else
        {
            Debug.Log("��겻�ڿ�����");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("������");
        //������
        SetSortingLayer(SortingLayerType.FrontCard);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //����뿪
        SetSortingLayer(SortingLayerType.Card);
    }
}
