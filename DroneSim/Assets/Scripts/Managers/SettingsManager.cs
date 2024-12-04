using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SettingsManager : MonoBehaviour
{
	public static SettingsManager instance;
	public PlayerSettings playerSettings;

    public Canvas settingsCanvas;    //Store all settings fields for settings default values
    public TMP_InputField SETTINGSUI_camOffsetYInputField;
    public TMP_InputField SETTINGSUI_camOffsetZInputField;
    public TMP_InputField SETTINGSUI_camAngleInputField;
    public TMP_InputField SETTINGSUI_nameInputField;
    public Slider SETTINGSUI_soundFXSlider;
    public Slider SETTINGSUI_qualitySlider;
    public Toggle SETTINGSUI_PostFXToggle;
    public Toggle SETTINGSUI_AngleIconsToggle;
    public Toggle SETTINGSUI_PhoneVrModeToggle;

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
    }

    public void ToggleUI()
    {
        settingsCanvas.gameObject.SetActive(!settingsCanvas.gameObject.activeInHierarchy);
    }
	public void SetDefaultValues()
	{
		if (GameManager.instance.localPlayer != null && GameManager.instance.localPlayer.drone != null)
		{
			//Note setting input field value like this still invokes any callbacks on the input field
			SETTINGSUI_camOffsetYInputField.text = $"{GameManager.instance.localPlayer.drone.droneStats.cameraOffset.y}";
			SETTINGSUI_camOffsetZInputField.text = $"{GameManager.instance.localPlayer.drone.droneStats.cameraOffset.z}";
		}

		SETTINGSUI_camAngleInputField.text = $"{playerSettings.cameraAngle}";
		SETTINGSUI_nameInputField.text = $"{GameManager.instance.localPlayer.name}";
		SETTINGSUI_soundFXSlider.value = playerSettings.soundFxVolume;
		SETTINGSUI_PostFXToggle.isOn = GameManager.instance.localPlayer.postProcessVolume.enabled;
		SETTINGSUI_AngleIconsToggle.isOn = GameManager.instance.localPlayer.angleIconsEnabled;
        SETTINGSUI_PhoneVrModeToggle.isOn = GameManager.instance.localPlayer.vrEnabled;
		SETTINGSUI_qualitySlider.value = QualitySettings.GetQualityLevel();
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
        GameManager.instance.localPlayer.angleIconsEnabled = !GameManager.instance.localPlayer.angleIconsEnabled;
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
    public void UICALLBACK_ChangeCamOffsetY(string c)
    {
        float v;
        if (float.TryParse(c, out v))
        {
            GameManager.instance.localPlayer.drone.droneStats.cameraOffset.y = v;
            GameManager.instance.localPlayer.playerCamera.transform.localPosition = GameManager.instance.localPlayer.drone.droneStats.cameraOffset;
        }
    }
    public void UICALLBACK_ChangeCamOffsetZ(string c)
    {
        float v;
        if (float.TryParse(c, out v))
        {
            GameManager.instance.localPlayer.drone.droneStats.cameraOffset.z = v;
            GameManager.instance.localPlayer.playerCamera.transform.localPosition = GameManager.instance.localPlayer.drone.droneStats.cameraOffset;
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
    public void UICALLBACK_ToggleVrMode(bool v)
    {
        if (v) { GameManager.instance.localPlayer.EnableVr(); }
        else { GameManager.instance.localPlayer.DisableVr(); }
    }
    public void UICALLBACK_ReturnToMenu()
    {
        ChatManager.instance.view.RPC("RPC_AddChatMessage", RpcTarget.All, $"{GameManager.instance.localPlayer.name} is leaving");
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Menu");
    }
    #endregion
}
