using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorControl : MonoBehaviour
{
    //¶¯»­¿ØÖÆ
    private void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void  SetAnimatorStart()
    {
        this.gameObject.SetActive(true);
    }

    public void SetAnimatorEnd()
    {
        this.gameObject.SetActive(false);
    }
}
