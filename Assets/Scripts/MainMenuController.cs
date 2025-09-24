using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField createRoomName;
    [SerializeField] TMP_InputField joinRoomName;
    [SerializeField] GameObject newGamePanel;
    [SerializeField] GameObject joinGamePanel;
    [SerializeField] GameObject mainMenuPanel;
    
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();  // Connect to Photon
    }
    
    public void NewGame()
    {
        mainMenuPanel.SetActive(false);
        newGamePanel.SetActive(true);
    }
    
    public void JoinGame()
    {
        mainMenuPanel.SetActive(false);
        joinGamePanel.SetActive(true);
    }
    
    public void MainMenu()
    {
        mainMenuPanel.SetActive(true);
        newGamePanel.SetActive(false);
        joinGamePanel.SetActive(false);
    }
    

    public void OnCreateRoom()
    {
        PhotonNetwork.CreateRoom(createRoomName.text);
    }
    
    public void OnJoinRoom()
    {
        PhotonNetwork.JoinRoom(createRoomName.text);
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("GameScene");
    }
}
