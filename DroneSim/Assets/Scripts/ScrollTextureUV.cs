using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTextureUV : MonoBehaviour
{
    public Material targetMaterial;

    void Update()
    {
        targetMaterial.mainTextureOffset += Vector2.one*Time.deltaTime*0.05f;
        if (targetMaterial.mainTextureOffset.magnitude > 99999) { targetMaterial.mainTextureOffset = Vector2.zero; }
    }
}
