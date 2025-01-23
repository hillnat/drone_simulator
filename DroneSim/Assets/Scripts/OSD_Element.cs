using UnityEngine;

public class OSD_Element : MonoBehaviour
{
    public string elementName;

    private void Awake()
    {
        gameObject.name= elementName;
    }
}
