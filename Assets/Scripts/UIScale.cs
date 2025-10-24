using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScale : MonoBehaviour
{
    [SerializeField] private float scaleValue = 1f;
    [SerializeField] private float UHDScale = 2;

    void Start()
    {
        if (Screen.width > 1920)
        {
            scaleValue = UHDScale;
        }

        this.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
    }
}
