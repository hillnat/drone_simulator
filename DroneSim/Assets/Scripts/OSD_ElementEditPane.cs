using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OSD_ElementEditPane : MonoBehaviour
{
    public TMP_Text nameText;
    public Toggle enabledToggle;
    public TMP_InputField posxInput;
    public TMP_InputField posyInput;
    public TMP_InputField scalexInput;
    public TMP_InputField scaleyInput;

    private void Awake()
    {
        if (nameText == null || enabledToggle== null || posxInput == null || posyInput == null || scalexInput == null || scaleyInput == null) { Debug.Log($"{gameObject.name} (OSD_ElementEditPane) was not setup correctly"); }
    }
}
