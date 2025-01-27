using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class SettingsManager : MonoBehaviour
{
	public static SettingsManager instance;
	public PlayerSettings playerSettings;

	public Canvas settingsCanvas;    //Store all settings fields for settings default values
	public TMP_InputField SETTINGSUI_camAngleInputField;
	public TMP_InputField SETTINGSUI_nameInputField;
	public Slider SETTINGSUI_soundFXSlider;
	public Slider SETTINGSUI_qualitySlider;
	public Toggle SETTINGSUI_PostFXToggle;

	private GameObject osdElementEditPaneReference;
	public RectTransform osdElementEditPaneParent;
	#region Unity Callbacks
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}

		playerSettings = (PlayerSettings)Resources.Load("DefaultPlayerSettings");
		settingsCanvas.gameObject.SetActive(false);
		QualitySettings.SetQualityLevel(2, true);
		osdElementEditPaneReference = (GameObject)Resources.Load("OSD_ElementEditPane");
	}
	private void Start()
	{
		SetupOsdEditPanel();


        PopulateJoysticks();
        PopulateInputActions();

        joystickDropdown.onValueChanged.AddListener(OnJoystickSelected);
        axisOrButtonDropdown.onValueChanged.AddListener(OnAxisOrButtonSelected);
        inputActionDropdown.onValueChanged.AddListener(OnInputActionSelected);
        bindButton.onClick.AddListener(BindInput);

        StartListeningForJoystickInput();
    }
	#endregion
	public void ToggleUI()
	{
		settingsCanvas.gameObject.SetActive(!settingsCanvas.gameObject.activeInHierarchy);
	}
	public void SetDefaultValues()
	{
		SETTINGSUI_camAngleInputField.text = $"{playerSettings.cameraAngle}";
		SETTINGSUI_nameInputField.text = $"{GameManager.instance.localPlayer.name}";
		SETTINGSUI_soundFXSlider.value = playerSettings.soundFxVolume;
		SETTINGSUI_PostFXToggle.isOn = GameManager.instance.localPlayer.postProcessVolume.enabled;
		//SETTINGSUI_AngleIconsToggle.isOn = GameManager.instance.localPlayer.horizonLinesEnabled;
		SETTINGSUI_qualitySlider.value = QualitySettings.GetQualityLevel();
	}
	public void SetupOsdEditPanel()
	{
		for (int i = 0; i < osdElementEditPaneParent.childCount; i++)
		{
			Destroy(osdElementEditPaneParent.GetChild(0));
		}
		for (int i = 0; i < playerSettings.allOsdElemDatas.Length; i++)
		{
			OSD_ElementEditPane eep = Instantiate(osdElementEditPaneReference, Vector2.zero, Quaternion.identity, osdElementEditPaneParent).GetComponent<OSD_ElementEditPane>();
			((RectTransform)eep.transform).anchoredPosition = new Vector2(0, -80f * (i + 1));
			//set values
			eep.posxInput.text = $"{playerSettings.allOsdElemDatas[i].position.x}";
			eep.posyInput.text = $"{playerSettings.allOsdElemDatas[i].position.y}";
			eep.scalexInput.text = $"{playerSettings.allOsdElemDatas[i].scale.x}";
			eep.scaleyInput.text = $"{playerSettings.allOsdElemDatas[i].scale.y}";
			eep.enabledToggle.isOn = playerSettings.allOsdElemDatas[i].elementEnabled;
			eep.nameText.text = playerSettings.allOsdElemDatas[i].elementName;
			//setup ui components
			eep.posxInput.contentType = TMP_InputField.ContentType.DecimalNumber;
			eep.posyInput.contentType = TMP_InputField.ContentType.DecimalNumber;
			eep.scalexInput.contentType = TMP_InputField.ContentType.DecimalNumber;
			eep.scaleyInput.contentType = TMP_InputField.ContentType.DecimalNumber;
			//remove all listeners just in case
			eep.posxInput.onValueChanged.RemoveAllListeners();
			eep.posyInput.onValueChanged.RemoveAllListeners();
			eep.scalexInput.onValueChanged.RemoveAllListeners();
			eep.scaleyInput.onValueChanged.RemoveAllListeners();
			eep.enabledToggle.onValueChanged.RemoveAllListeners();
			//add listeners
			eep.posxInput.onValueChanged.AddListener(delegate { SaveOsdEditPanel(); });
			eep.posyInput.onValueChanged.AddListener(delegate { SaveOsdEditPanel(); });
			eep.scalexInput.onValueChanged.AddListener(delegate { SaveOsdEditPanel(); });
			eep.scaleyInput.onValueChanged.AddListener(delegate { SaveOsdEditPanel(); });
			eep.enabledToggle.onValueChanged.AddListener(delegate { SaveOsdEditPanel(); });
		}
	}
	public void SaveOsdEditPanel()
	{
		for (int i = 0; i < playerSettings.allOsdElemDatas.Length; i++)
		{
			OSD_ElementEditPane eep = osdElementEditPaneParent.GetChild(i).GetComponent<OSD_ElementEditPane>();
			if (float.TryParse(eep.posxInput.text, out float posx)) {
				playerSettings.allOsdElemDatas[i].position.x = posx;
			}
			if (float.TryParse(eep.posyInput.text, out float posy))	{
				playerSettings.allOsdElemDatas[i].position.y = posy;
			}
			if (float.TryParse(eep.scalexInput.text, out float scalex))	{
				playerSettings.allOsdElemDatas[i].scale.x = scalex;
			}
			if (float.TryParse(eep.scaleyInput.text, out float scaley))	{
				playerSettings.allOsdElemDatas[i].scale.y = scaley;
			}

			playerSettings.allOsdElemDatas[i].elementEnabled = eep.enabledToggle.isOn;
		}
		GameManager.instance.localPlayer.SetOsdElementsToOsdElementDatas();
	}
	#region UI Callbacks
	public void UICALLBACK_TogglePostFX()
	{
		GameManager.instance.localPlayer.postProcessVolume.enabled = !GameManager.instance.localPlayer.postProcessVolume.enabled;
		GameManager.instance.localPlayer.postProcessLayer.enabled = !GameManager.instance.localPlayer.postProcessLayer.enabled;
	}
	public void UICALLBACK_SoundFXVolume(float v)
	{
		playerSettings.soundFxVolume = v;
	}
	public void UICALLBACK_ToggleAngleIcons()
	{
		//GameManager.instance.localPlayer.horizonLinesEnabled = !GameManager.instance.localPlayer.horizonLinesEnabled;
	}
	public void UICALLBACK_ToggleCrosshair()
	{
		//GameManager.instance.localPlayer.horizonLinesEnabled = !GameManager.instance.localPlayer.horizonLinesEnabled;
	}
	public void UICALLBACK_ToggleAltitude()
	{
		//GameManager.instance.localPlayer.horizonLinesEnabled = !GameManager.instance.localPlayer.horizonLinesEnabled;
	}
	public void UICALLBACK_ToggleSpeed()
	{
		//GameManager.instance.localPlayer.horizonLinesEnabled = !GameManager.instance.localPlayer.horizonLinesEnabled;
	}
	public void UICALLBACK_ChangeCamAngle(string c)
	{
		float v;
		if (float.TryParse(c, out v))
		{
			playerSettings.cameraAngle = v;
			GameManager.instance.localPlayer.playerCamera.transform.localEulerAngles = new Vector3(playerSettings.cameraAngle, 0, 0);
		}
	}
	public void UICALLBACK_ChangeName(string v)
	{
		GameManager.instance.localPlayer.view.RPC("SetName", RpcTarget.AllBufferedViaServer, v);
	}
	public void UICALLBACK_ChangeQuality(float v)
	{
		QualitySettings.SetQualityLevel(Mathf.Clamp((int)v, 0, 3), true);
	}
	public void UICALLBACK_ReturnToMenu()
	{
		ChatManager.instance.view.RPC("RPC_AddChatMessage", RpcTarget.All, $"{GameManager.instance.localPlayer.name} is leaving");
		PhotonNetwork.LeaveRoom();
		SceneManager.LoadScene("Menu");
	}
    #endregion

    #region Keybinds
    [Header("UI Elements")]
    public TMP_Dropdown joystickDropdown;
    public TMP_Dropdown axisOrButtonDropdown;
    public TMP_Dropdown inputActionDropdown;
    public Button bindButton;
    public TMP_Text statusText;

    [Header("Input Action Settings")]
    public InputActionAsset inputActionAsset;

    private string selectedJoystick;
    private string selectedAxisOrButton;
    private InputAction selectedInputAction;

    private void PopulateJoysticks()
    {
        joystickDropdown.ClearOptions();
        var joysticks = Joystick.all.Select(j => j.name).ToList();

        if (joysticks.Count > 0)
        {
            joystickDropdown.AddOptions(joysticks);
            selectedJoystick = joysticks[0];
            PopulateAxisOrButtons();
        }
        else
        {
            joystickDropdown.AddOptions(new[] { "No Joysticks Connected" }.ToList());
            selectedJoystick = null;
        }
    }

    private void PopulateAxisOrButtons()
    {
        if (selectedJoystick == null) return;

        axisOrButtonDropdown.ClearOptions();
        var joystick = Joystick.all.FirstOrDefault(j => j.name == selectedJoystick);

        if (joystick != null)
        {
            var options = joystick.allControls.Select(c => c.name).ToList();
            axisOrButtonDropdown.AddOptions(options);
            selectedAxisOrButton = options[0];
        }
    }

    private void PopulateInputActions()
    {
        inputActionDropdown.ClearOptions();
        if (inputActionAsset == null) return;

        var actions = inputActionAsset.actionMaps
            .SelectMany(map => map.actions)
            .Select(action => action.name)
            .ToList();

        if (actions.Count > 0)
        {
            inputActionDropdown.AddOptions(actions);
            selectedInputAction = inputActionAsset.FindAction(actions[0]);
        }
    }

    private void OnJoystickSelected(int index)
    {
        selectedJoystick = joystickDropdown.options[index].text;
        PopulateAxisOrButtons();
    }

    private void OnAxisOrButtonSelected(int index)
    {
        selectedAxisOrButton = axisOrButtonDropdown.options[index].text;
    }

    private void OnInputActionSelected(int index)
    {
        var actionName = inputActionDropdown.options[index].text;
        selectedInputAction = inputActionAsset.FindAction(actionName);
    }

    private void BindInput()
    {
        if (selectedJoystick == null || string.IsNullOrEmpty(selectedAxisOrButton) || selectedInputAction == null)
        {
            statusText.text = "Please select a joystick, input, and action to bind.";
            return;
        }

        var joystick = Joystick.all.FirstOrDefault(j => j.name == selectedJoystick);
        if (joystick == null)
        {
            statusText.text = "Selected joystick is no longer connected.";
            return;
        }

        var control = joystick.allControls.FirstOrDefault(c => c.name == selectedAxisOrButton);
        if (control == null)
        {
            statusText.text = "Selected control is invalid.";
            return;
        }

        // Bind the control to the selected InputAction
        selectedInputAction.ApplyBindingOverride(new InputBinding
        {
            path = control.path
        });

        statusText.text = $"Bound {selectedAxisOrButton} from {selectedJoystick} to {selectedInputAction.name}";
    }

    private void StartListeningForJoystickInput()
    {
        if (Joystick.all.Count == 0)
        {
            statusText.text = "No joystick connected to listen to.";
            return;
        }

        var joystick = Joystick.all.FirstOrDefault(j => j.name == selectedJoystick);
        if (joystick == null)
        {
            statusText.text = "Selected joystick is not available.";
            return;
        }

        InputSystem.onEvent += (inputEvent, device) =>
        {
            if (device != joystick) return;

            foreach (var control in joystick.allControls)
            {
                if (control.IsPressed())
                {
                    selectedAxisOrButton = control.name;
                    //axisOrButtonDropdown.value = axisOrButtonDropdown.options.FindIndex(option => option.text == selectedAxisOrButton);
                    statusText.text = $"Detected input: {selectedAxisOrButton}";
                    break;
                }
            }
        };
    }
    #endregion
}
