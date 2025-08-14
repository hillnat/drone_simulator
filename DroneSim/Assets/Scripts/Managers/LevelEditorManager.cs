using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelEditorManager : MonoBehaviour
{
    public List<LevelEditorObject> objects = new List<LevelEditorObject>();
    public GameObject baseObjectPrefab;
    public TMP_Dropdown objectOptionsDropdown;
    public TMP_Dropdown currentObjectsDropdown;
    public int currentSelectedObjectIndex;
    public float cameraMoveSpeed = 0.2f;
    public float cameraSensitivity = 0.5f;
    private float mouselookX = 0;
    private float mouselookY = 0;
    public TMP_InputField positionX_InspectorInputField;
    public TMP_InputField positionY_InspectorInputField;
    public TMP_InputField positionZ_InspectorInputField;
    public TMP_InputField rotationX_InspectorInputField;
    public TMP_InputField rotationY_InspectorInputField;
    public TMP_InputField rotationZ_InspectorInputField;
    public TMP_InputField scaleX_InspectorInputField;
    public TMP_InputField scaleY_InspectorInputField;
    public TMP_InputField scaleZ_InspectorInputField;
    public TMP_Text cameraSpeedText;
    Coroutine fadeCameraSpeedTextCoroutine;
    void Start()
    {
        objectOptionsDropdown.ClearOptions();
        Object[] allMeshes = Resources.LoadAll("LevelEditor/Objects");
        for (int i = 0; i < allMeshes.Length; i++)
        {
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(allMeshes[i].name);
            objectOptionsDropdown.options.Add(newOption);
        }
        Debug.Log($"Found {allMeshes.Length} Level Editor Objects");
        RefreshCurrentObjectsDropdown();


        RefreshCameraSpeedText();

        //Debug.Log(Application.persistentDataPath);
        SetInspectorToSelectedObject();
    }

    void Update()
    {
        if (InputManager.instance.mouse1 && 
            Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit))
        {
            LevelEditorObject leo = hit.transform.gameObject.GetComponent<LevelEditorObject>();
            if (leo != null)
            {
                currentSelectedObjectIndex = leo.indexInMasterList;
            }
        }
        if (InputManager.instance.scrollDelta != Vector2.zero)
        {
            if (InputManager.instance.scrollDelta.y > 0)
            {
                cameraMoveSpeed = cameraMoveSpeed * 1.5f;
            }
            else
            {
                cameraMoveSpeed = cameraMoveSpeed / 1.5f;
            }
            RefreshCameraSpeedText();
        }
        if (InputManager.instance.mouse2Hold && InputManager.instance.mouseDelta != Vector2.zero)
        {
            mouselookX += -InputManager.instance.mouseDelta.y * cameraSensitivity;
            mouselookX = Mathf.Clamp(mouselookX, -90, 90);
            mouselookY += InputManager.instance.mouseDelta.x * cameraSensitivity;
            if (mouselookY >= 360f) { mouselookY -= 360f; }
            else if (mouselookY <= -360f) { mouselookY += 360f; }
            Camera.main.transform.eulerAngles = new Vector3(mouselookX, mouselookY, 0);
        }
        if (InputManager.instance.directionalInputs != Vector3.zero)
        {
            float y = InputManager.instance.directionalInputs.y * Time.deltaTime * cameraMoveSpeed;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y + y, Camera.main.transform.position.z);
            Camera.main.transform.Translate(Vector3.right * Time.deltaTime * cameraMoveSpeed * InputManager.instance.directionalInputs.x);
            Camera.main.transform.Translate(Vector3.forward * Time.deltaTime * cameraMoveSpeed * InputManager.instance.directionalInputs.z);
        }
    }
    
    private LevelEditorObject? GetSelectedObject()
    {
        if(objects.Count == 0){return null;}
        return objects[currentSelectedObjectIndex];
    }
    public void ClearObjects()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            Destroy(objects[0]);
            objects.RemoveAt(0);
        }
        objects.Clear();
        RefreshCurrentObjectsDropdown();
    }
    public void CreateObject()
    {
        LevelEditorObject newObject = Instantiate(baseObjectPrefab, Vector3.zero, Quaternion.identity).GetComponent<LevelEditorObject>();

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 50)) { newObject.transform.position = hit.transform.position; Debug.Log("a"); }
        else { newObject.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 5); Debug.Log("b"); }

        newObject.indexInMasterList = objects.Count;
        objects.Add(newObject);
        newObject.objectName = objectOptionsDropdown.options[objectOptionsDropdown.value].text;
        newObject.ReloadObject();
        RefreshAllObjectIndex();
        currentSelectedObjectIndex = newObject.indexInMasterList;
        RefreshCurrentObjectsDropdown();
        SetInspectorToSelectedObject();
    }
    public void ReloadAll()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].ReloadObject();
            objects[i].indexInMasterList = i;
        }
        RefreshAllObjectIndex();
        RefreshCurrentObjectsDropdown();
    }
    public void RefreshAllObjectIndex()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].indexInMasterList = i;
        }
    }
    private void RefreshCurrentObjectsDropdown()
    {
        currentObjectsDropdown.ClearOptions();
        for (int i = 0; i < objects.Count; i++)
        {
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(objects[i].objectName);
            currentObjectsDropdown.options.Add(newOption);
        }
        if (currentSelectedObjectIndex != -1)//If we have an object selected already, set the dropdown to that value
        {
            currentObjectsDropdown.value = currentSelectedObjectIndex;
        }
    }
    public void SetCurrentItemToDropdown()//Called when current objects list is changed
    {
        currentSelectedObjectIndex = currentObjectsDropdown.value;
    }
    public void SetSelectedObjectTransformToInspectorValues()
    {
        LevelEditorObject selectedObject = GetSelectedObject();
        if (selectedObject != null)
        {
            if (float.TryParse(positionX_InspectorInputField.text, out float valPX)) {
                selectedObject.transform.position = new Vector3(valPX, selectedObject.transform.position.y, selectedObject.transform.position.z);
            }
            if (float.TryParse(positionY_InspectorInputField.text, out float valPY))
            {
                selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, valPY, selectedObject.transform.position.z);
            }
            if (float.TryParse(positionZ_InspectorInputField.text, out float valPZ))
            {
                selectedObject.transform.position = new Vector3(selectedObject.transform.position.z, selectedObject.transform.position.y, valPZ);
            }

            if (float.TryParse(rotationX_InspectorInputField.text, out float valRX))
            {
                selectedObject.transform.eulerAngles = new Vector3(valRX, selectedObject.transform.eulerAngles.y, selectedObject.transform.eulerAngles.z);
            }
            if (float.TryParse(rotationY_InspectorInputField.text, out float valRY))
            {
                selectedObject.transform.eulerAngles = new Vector3(selectedObject.transform.eulerAngles.x, valRY, selectedObject.transform.eulerAngles.z);
            }
            if (float.TryParse(rotationZ_InspectorInputField.text, out float valRZ))
            {
                selectedObject.transform.eulerAngles = new Vector3(selectedObject.transform.eulerAngles.z, selectedObject.transform.eulerAngles.y, valRZ);
            }

            if (float.TryParse(scaleX_InspectorInputField.text, out float valSX))
            {
                selectedObject.transform.localScale = new Vector3(valSX, selectedObject.transform.localScale.y, selectedObject.transform.localScale.z);
            }
            if (float.TryParse(scaleY_InspectorInputField.text, out float valSY))
            {
                selectedObject.transform.localScale = new Vector3(selectedObject.transform.localScale.x, valSY, selectedObject.transform.localScale.z);
            }
            if (float.TryParse(scaleZ_InspectorInputField.text, out float valSZ))
            {
                selectedObject.transform.localScale = new Vector3(selectedObject.transform.localScale.x, selectedObject.transform.localScale.y, valSZ);
            }
        }
    }
    public void SetInspectorToSelectedObject()
    {
        LevelEditorObject selectedObject = GetSelectedObject();

        positionX_InspectorInputField.text = "POS X";
        positionY_InspectorInputField.text = "POS Y";
        positionZ_InspectorInputField.text = "POS Z";
        rotationX_InspectorInputField.text = "ROT X";
        rotationY_InspectorInputField.text = "ROT Y";
        rotationZ_InspectorInputField.text = "ROT Z";
        scaleX_InspectorInputField.text = "SCALE X";
        scaleY_InspectorInputField.text = "SCALE Y";
        scaleZ_InspectorInputField.text = "SCALE Z";

        if (selectedObject != null)
        {
            positionX_InspectorInputField.text = $"{selectedObject.transform.position.x}";
            positionY_InspectorInputField.text = $"{selectedObject.transform.position.y}";
            positionZ_InspectorInputField.text = $"{selectedObject.transform.position.z}";
            rotationX_InspectorInputField.text = $"{selectedObject.transform.eulerAngles.x}";
            rotationY_InspectorInputField.text = $"{selectedObject.transform.eulerAngles.y}";
            rotationZ_InspectorInputField.text = $"{selectedObject.transform.eulerAngles.z}";
            scaleX_InspectorInputField.text = $"{selectedObject.transform.localScale.x}";
            scaleY_InspectorInputField.text = $"{selectedObject.transform.localScale.y}";
            scaleZ_InspectorInputField.text = $"{selectedObject.transform.localScale.z}";
        }
    }
    public void RefreshCameraSpeedText()
    {
        cameraSpeedText.text = $"{cameraMoveSpeed:F2}";
        if (fadeCameraSpeedTextCoroutine != null) { StopCoroutine(fadeCameraSpeedTextCoroutine); }
        fadeCameraSpeedTextCoroutine = StartCoroutine(IE_FadeCameraSpeedText());
    }
    private IEnumerator IE_FadeCameraSpeedText()
    {
        for (int i = 0; i < 100; i++)
        {
            float t = Mathf.InverseLerp(0, 100, i);//get i between 0 and 1
            cameraSpeedText.color = Color.Lerp(Color.white, Color.clear, t);
            yield return new WaitForSeconds(0.015f);
        }
    }
    public void SaveLevelToJson()
    {
        
    }
}
