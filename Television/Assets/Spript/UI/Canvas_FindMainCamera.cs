using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canvas_FindMainCamera : MonoBehaviour
{
    private static Canvas_FindMainCamera Instance;
    public static Canvas_FindMainCamera instance=> Instance;

    public Canvas canvas;
    private void Awake()
    {
        Instance = this;
        canvas =GetComponent<Canvas>();
        canvas.worldCamera =Camera.main;
    }

    private void Update()
    {
     
    }
}
