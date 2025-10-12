using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

enum SortingLayerType//显示层级
{
    Card ,
    FrontCard,
}

public class Card : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool IsChoose=false;//是否被选中,默认没有被选中
    [SerializeField] SpriteRenderer BackGround;//卡牌的背景
    [SerializeField] SpriteRenderer ContentImage;//卡牌的背景

    private void Awake()
    {
        SetSortingLayer(SortingLayerType.Card);//设置卡牌的显示层级
    }

    private void SetSortingLayer(SortingLayerType Type)
    {
        BackGround.sortingLayerName=Type.ToString();
        ContentImage.sortingLayerName= Type.ToString();
    }
    private void Update()
    {
        // 将鼠标屏幕位置转换为世界坐标（2D场景）
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 检测鼠标位置是否命中当前卡牌的碰撞体
        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos);

        if (hitCollider != null && hitCollider.gameObject == gameObject)
        {
            Debug.Log("鼠标正悬停在卡牌上（未点击）");
        }
        else
        {
            Debug.Log("鼠标不在卡牌上");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("鼠标进入");
        //鼠标进入
        SetSortingLayer(SortingLayerType.FrontCard);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //鼠标离开
        SetSortingLayer(SortingLayerType.Card);
    }
}
