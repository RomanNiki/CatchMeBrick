using Networking;
using UnityEngine;
using Zenject;

public class ConnectionMenu : MonoBehaviour
{
    private GameNetPortal _gameNetPortal;
    private ClientGameNetPortal _clientGameNetPortal;

    [Inject]
    public void Constructor(GameNetPortal gameNetPortal, ClientGameNetPortal clientGameNetPortal)
    {
        _gameNetPortal = gameNetPortal;
        _clientGameNetPortal = clientGameNetPortal;
    }

    public void StartHost()
    {
        _gameNetPortal.StartHost("127.0.0.1", 7777);
    }

    public void StartClient()
    {
        _clientGameNetPortal.StartClient("127.0.0.1", 7777);
    }
}