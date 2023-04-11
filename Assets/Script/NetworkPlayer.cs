using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviourPun {
    //avatar's attributes
    private Transform head;
    private Transform left_hand;
    private Transform right_hand;
    private Transform chest;
    public Transform head_sphere;
    public Transform left_hand_sphere;
    public Transform right_hand_sphere;
    public Transform palette;
    public Transform ray_cast;
    public Transform circle;

    //Materials (tag colors)
    private Material blue;
    private Material green;
    private Material white;
    private Material red;
    private Material light_red;
    private Material none;

    //camera tracker
    private GameObject cam_rig;
    private GameObject headset;
    private GameObject ctrl_right;
    private GameObject ctrl_left;

    //room & corresponding script
    private GameObject room;
    private Rendering room_render;
    private Experiment experiment;

    //related operator & corresponding script
    private bool is_operator;
    private GameObject operator;
    private NetWorkOperator ope_script;

    //other self attributes
    private boolean pun_view = true;
    private Transform ray;
    private Transform pos;

    //controller's attributes
    private RaycastHit hit;
    private string ray_name;
    private string ray_tag;
    private SteamVR_Behaviour_Pose pose;
    private SteamVR_Action_Boolean trigger = SteamVR_Input.GetBooleanAction("InteractUI");

    //synchro attributes
    private bool sync_tag = true;
    private bool is_other_synced;
    private string move_mode = "drag"; //possible others are : "TP" "joy" "sync"

    //Unity Start method, used as an initializer
    void Start(){
        if(GameObject.Find("/Network Operator(Clone)")!=null){
            //if we get an operator then we are instantiating the referred operator as the existing one
            operator = GameObject.Find("/Network Operator(Clone)");
            ope_script = operator.GetComponent<NetworkOperator>();
            is_operator = false;
        } else {
            //and if we don't get any operator then this player is direclty referring to the room
            room = GameObject.Find("/Room");
            room_render = room.GetComponent<Rendering>();
            //if the instantiated player is the first one to join & this is well its view then he become the operator
            is_operator = (photonView.IsMine && PhotonNetwork.LocalPlayer.ActorNumber==1);
        }

        //getting the VR attributes
        cam_rig = GameObject.Find("/[CameraRig]");
        headset = GameObject.Find("Camera (eye)");
        ctrl_right = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)");
        ctrl_left = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (left)");

        //getting the right controller's pose
        pose = ctrl_right?.GetComponent<SteamVR_Behaviour_Pose>();

        left_hand.gameObject.SetActive(true);
        palette.gameObject.SetActive(true);

        //unshowing my avatar for myself
        bool pv_is_mine = photonView.IsMine;
        left_hand.gameObject.SetActive(!pv_is_mine);
        right_hand.gameObject.SetActive(!pv_is_mine);
        head.gameObject.SetActive(!pv_is_mine);
        chest.gameObject.SetActive(!pv_is_mine);

        ray_name = ray_cast.GetComponent<Renderer>().material.name.ToString();
    }


    //Unity Update method, called once per frame
    void Update(){
        if(is_operator){
            OperatorActions();
        }

        if(experiment==null && room_render.expe!=null){
            //whilst we are inside an experiment then the ability to switch the move mode is disabled
            expe = room_render.expe;
            palette.gameObject.SetActive(false);
        }

        //checking if the other either I or the other player is synced
        bool? sync_tag_test = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)")?.GetComponent<Teleporter>().tag_sync;
        bool? is_other_synced_test = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)")?.GetComponent<Teleporter>().is_other_synced;

        sync_tag = sync_tag_test == null ? false : (bool)sync_tag_test;
        is_other_synced = is_other_synced_test == null ? false : (bool)is_other_synced_test;

        if(photonView.IsMine){
            if(pun_view){
                pun_view = false;
            }
            MapPosition();
        }

        //simulating new ray
        Ray ray = new Ray(ctrl_right.transform.position, ctrl_right.transform.forward);

        //and now checking its conditions & situations
        if(Physics.Raycast(ray, out hit)){
            //checking to change either color of raycast or move_mode
            if(trigger.GetStateDown(pose.inputSource)){
                if(hit.transform.tag=="Color tag"){
                    //then we change the color of the ray
                    UpdateRayColor();
                } else if(photonView.IsMine){
                    //then we change the move mode of THIS player
                    UpdatePalette(hit.transform.tag);
                    photonView.RPC("ChangeMoveMode", Photon.Pun.RpcTarget.all, move_mode);
                }
            }
        }
    }

    //other methods
    private void MapPosition(){
        //must implement
        return;
    }

    private void UpdateRay(){
        //must implement
        return;
    }   

    private void UpdatePalette(string ht_){
        //must implement
        return;
    }

    //PunRPC methods
    [PunRPC]
    private void ChangeMoveMode(string mm_){
        //must implement
        return;
    }
}