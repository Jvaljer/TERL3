using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spawner : MonoBehaviourPunCallbacks {
    //entities prefabs
    private GameObject spawned_player_prefab;
    private GameObject spawned_operator_prefab;

    //operator predicate
    private bool with_ope = true;

    //override methods
    public override void OnJoinedRoom(){
        base.OnJoinedRoom();
        if(PhotonNetwork.LocalPlayer.IsMasterClient){
            if(with_ope){
                spawned_operator_prefab = PhotonNetwork.Instantiate("Operator", new Vector3(0,0,0), transform.rotation);
            } else {
                spawned_player_prefab = PhotonNetwork.Instantiate("Player", new Vector3(0,0,0), transform.rotation);
            }
        } else {
            spawned_player_prefab = PhotonNetwork.Instantiate("Player", new Vector3(0,0,0), transform.rotation);
        }
    }

    public override void OnLeftRoom(){
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawned_player_prefab);
        PhotonNetwork.Destroy(spawned_operator_prefab);
    }
}