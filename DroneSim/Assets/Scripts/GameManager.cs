using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    public Camera levelCamera;
    public PlayerController localPlayer;

    #region Unity Callbacks
    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Setup");
        }
        PhotonNetwork.SendRate = 25;
        PhotonNetwork.SerializationRate = 50;
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        GameStats.spawn = transform.position;
        localPlayer = PhotonNetwork.Instantiate("player", GameStats.spawn, GameStats.spawnRotation).GetComponent<PlayerController>();
        SettingsManager.instance.SetDefaultValues();
    }
    #endregion
    
}
