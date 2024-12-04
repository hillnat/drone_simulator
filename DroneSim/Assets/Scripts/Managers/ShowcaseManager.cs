using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;

public class ShowcaseManager : MonoBehaviour
{
    private string[] dronePrefabNames = new string[3] { "Basic Drone", "Race Drone", "Tiny Whoop" };

    private int selectedDroneType//index of which drone to spawn
    {
        get { return _selectedDroneType; }
        set { _selectedDroneType = value; UpdateShowcase(); }
    }
    private int _selectedDroneType = 0;

    [SerializeField] private Animator SHOWCASE_cameraAnimator;
    [SerializeField] private TMP_Text SHOWCASE_selectedDroneNameText;
    public void UICALLBACK_SpawnDrone()
    {
        GameManager.instance.localPlayer.view.RPC("SetDroneType", RpcTarget.AllBufferedViaServer, selectedDroneType);
        GameManager.instance.levelCamera.enabled = false;
        gameObject.SetActive(false);
    }
    public void UICALLBACK_ChangeDroneIndex(int delta)
    {
        int newIndex = selectedDroneType + delta;
        if (newIndex >= dronePrefabNames.Length) { newIndex = 0; }
        if (newIndex < 0) { newIndex = dronePrefabNames.Length - 1; }
        selectedDroneType = newIndex;
    }
    void UpdateShowcase()
    {
        if (SHOWCASE_cameraAnimator != null) { SHOWCASE_cameraAnimator.SetInteger("showcaseIndex", _selectedDroneType); }
        SHOWCASE_selectedDroneNameText.text = dronePrefabNames[selectedDroneType];
    }
}
