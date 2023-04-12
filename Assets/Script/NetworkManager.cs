using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks {

    //Unity Awake method, called before any other ones
    void Awake(){
        //Syncing the scenes
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    //Unity Start method, used here to connect the machine to the server & room
    void Start(){
        Connect();
    }

    //override methods 
    public override void OnConnectedToMaster(){
        base.OnConnectedToMaster();

        //setting the room's parameters
        RoomOptions room_options = new RoomOptions {
            MaxPlayers = 3,
            IsVisible = true,
            IsOpen = true
        };

        //creating the room
        PhotonNetwork.JoinOrCreateRoom("Room", room_options,TypedLobby.Default);
    }

    public override void OnJoinedRoom(){
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player new_player){
        base.OnPlayerEnteredRoom(new_player);
    }

    //connect method
    private void Connect(){
        PhotonNetwork.ConnectUsingSettings();
    }
}