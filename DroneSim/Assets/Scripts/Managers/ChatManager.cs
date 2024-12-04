using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public PhotonView view;
    public static ChatManager instance;
    private float lastMessageTime = 0;
    private bool inputOpen = false;
    [SerializeField]private Canvas chatCanvas;
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private TMP_InputField chatInput;
    private List<string> chatList = new List<string>();
    private int holdChatOpenTime = 5;
    private float time = 0;
    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(this.gameObject); }
        view=GetComponent<PhotonView>();
        time = holdChatOpenTime+1;
        chatText.text = "";
        chatInput.text = "";
        chatCanvas.enabled = true;
        inputOpen = false;
    }

    private void Update()
    {
        time += Time.deltaTime;
        if(!inputOpen) {
            if (time > lastMessageTime + holdChatOpenTime && chatCanvas.gameObject.activeInHierarchy) { chatCanvas.gameObject.SetActive(false); }
            else if (time < lastMessageTime + holdChatOpenTime && !chatCanvas.gameObject.activeInHierarchy) { chatCanvas.gameObject.SetActive(true); }
        }
        else if(!chatCanvas.gameObject.activeInHierarchy) { chatCanvas.gameObject.SetActive(true); }
        if (inputOpen) {
            chatInput.Select();
            chatInput.ActivateInputField();
        }
        if (inputOpen && !chatInput.gameObject.activeInHierarchy) { chatInput.gameObject.SetActive(true);}
        else if (!inputOpen && chatInput.gameObject.activeInHierarchy) { chatInput.gameObject.SetActive(false);}
    }
    void RewriteGameChat()
    {
        while (chatList.Count > 7)//get rid of overflow
        {
            chatList.RemoveAt(0);
        }
        chatText.text = "";
        for (int i = 0; i < chatList.Count; i++)//rewrite truncated version
        {
            chatText.text += chatList[i];
        }
        lastMessageTime = time;
    }
    [PunRPC]
    public void RPC_AddChatMessage(string message)//must call as buffered via server
    {
        chatList.Add($"{message}\n");
        RewriteGameChat();
    }
    public void OpenChat()
    {
        inputOpen = true;
    }
    public void UICALLBACK_AddChatMessage(string v)
    {
        if (v == "") { return; }
        view.RPC("RPC_AddChatMessage", RpcTarget.All, $"{GameManager.instance.localPlayer.name} : {v}");
        chatInput.text = "";
        inputOpen = false;
    }
}
