using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorObject : MonoBehaviour
{
    public string objectName = "";
    public Transform meshParent;
    public int indexInMasterList = -1;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ReloadObject()
    {
        for (int i = 0; i < meshParent.childCount; i++)
        {
            Destroy(meshParent.GetChild(0).gameObject);
        }
        GameObject newObject = (GameObject)Resources.Load($"LevelEditor/Objects/{objectName}");
        if (newObject != null)
        {
            GameObject newGo = Instantiate(newObject, meshParent);
            newGo.transform.localPosition = Vector3.zero;
            newGo.transform.localRotation = Quaternion.identity;
            Debug.Log($"Loaded and spawned {objectName}");

        }
        else
        {
            Debug.Log($"Failed to load {objectName}");
        }
    }
}
