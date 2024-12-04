using UnityEngine;

public class ScrollTextureUV : MonoBehaviour
{
    public Material targetMaterial;
    private void Start()
    {
        if (targetMaterial == null) { Destroy(this.gameObject); return; }
        targetMaterial.mainTextureOffset = Vector2.zero;
    }
    void Update()
    {
        targetMaterial.mainTextureOffset += Vector2.one*Time.deltaTime*0.05f;
        if (targetMaterial.mainTextureOffset.magnitude > 99999) { targetMaterial.mainTextureOffset = Vector2.zero; }
    }
}
