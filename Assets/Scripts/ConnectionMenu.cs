using Misc;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionMenu : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkManager.Singleton.OnServerStarted += ServerStarted;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnServerStarted -= ServerStarted;
    }

    private static void ServerStarted()
    {
        using (new Load("Logging you in..."))
        {
            NetworkManager.Singleton.SceneManager.LoadScene(Scenes.Lobby, LoadSceneMode.Single);
        }
    }
    
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
