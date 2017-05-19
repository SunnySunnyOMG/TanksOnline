using UnityEngine;
using System.Collections;
using Prototype.NetworkLobby;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook
{

    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();//get the script in lobbyPlayer gameobject
        TankLoad tankLoad = gamePlayer.GetComponent<TankLoad>();

        tankLoad.color = lobby.playerColor;
        tankLoad.playerName = lobby.playerName;
    }
}
