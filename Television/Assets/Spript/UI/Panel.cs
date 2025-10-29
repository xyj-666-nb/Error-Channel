using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    [SerializeField] private Button Button;
    void Start()
    {
        Button.onClick.AddListener(() => { this.gameObject.SetActive(false); });
    }

  
}
