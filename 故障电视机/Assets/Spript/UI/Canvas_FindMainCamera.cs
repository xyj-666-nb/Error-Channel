using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canvas_FindMainCamera : MonoBehaviour
{
    private Canvas canvas;
    private void Awake()
    {
        canvas =GetComponent<Canvas>();
        canvas.worldCamera =Camera.main;
    }
}
