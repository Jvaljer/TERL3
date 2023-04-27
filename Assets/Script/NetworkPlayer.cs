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
    public Material blue;
    public Material green;
    public Material white;
    public Material red;
    public Material light_red;
    public Material none;

    //camera tracker
    private GameObject cam_rig;
    private GameObject headset;
    private GameObject ctrl_right;
    private GameObject ctrl_left;

    //room & corresponding script
    private GameObject room;
    public Rendering room_render;
    private Experiment experiment;

    //related operator & corresponding script
    private bool is_operator;
    private GameObject ope;
    private NetworkOperator ope_script;

    //other self attributes
    private bool pun_view = true;
    private Transform ray;
    private Transform pos;

    //controller's attributes
    private RaycastHit hit;
    public string ray_name;
    private string ray_tag_name;
    private SteamVR_Behaviour_Pose pose;
    private SteamVR_Action_Boolean trigger = SteamVR_Input.GetBooleanAction("InteractUI");

    //synchro attributes
    private bool sync_tag = true;
    private bool is_other_synced;
    private string move_mode = "Drag"; //possible others are : "TP" "Joy" "Sync"

    //Unity Start method, used as an initializer
    void Start(){
        if(GameObject.Find("/Network Operator(Clone)")!=null){
            //if we get an operator then we are instantiating the referred operator as the existing one
            ope = GameObject.Find("/Network Operator(Clone)");
            ope_script = ope.GetComponent<NetworkOperator>();
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

        if(experiment==null && room_render.experiment!=null){
            //whilst we are inside an experiment then the ability to switch the move mode is disabled
            experiment = room_render.experiment;
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
                if(hit.transform.tag=="Color tag" && (sync_tag || photonView.IsMine)){
                    //then we change the color of the ray
                    UpdateRayColor();
                } else if(photonView.IsMine){
                    //then we change the move mode of THIS player
                    UpdatePalette(hit.transform.tag);
                    photonView.RPC("ChangeMoveMode", Photon.Pun.RpcTarget.All, move_mode);
                }
            }
        }
    }

    //other methods
    private void OperatorActions(){
        if(Input.GetKeyDown(KeyCode.Space)){
            room_render.SpacePressed();
        }
        if(Input.GetKeyDown(KeyCode.E)){
            room_render.EPressed();
        }
        if(Input.GetKeyDown(KeyCode.T)){
            room_render.TPressed();
        }
        if(Input.GetKeyDown(KeyCode.D)){
            room_render.DPressed();
        }
    }

    private void MapPosition(){
        //positionning the left hand
        palette.position = ctrl_left.transform.position;
        palette.rotation = ctrl_left.transform.rotation;

        //right hand
        right_hand.position = ctrl_right.transform.position;
        right_hand.rotation = ctrl_right.transform.rotation;

        //the ray
        ray.position = ctrl_right.transform.position;
        ray.rotation = ctrl_right.transform.rotation;

        //the head
        head.position = headset.transform.position;
        head.rotation = headset.transform.rotation;

        //the body
        chest.position = headset.transform.position;

        //the circle & position
        pos.position = new Vector3(headset.transform.position.x, 0, headset.transform.position.z);
        circle.rotation = new Quaternion(0, headset.transform.rotation.y, 0, headset.transform.rotation.w);
    }

    private void UpdateRayColor(){
        ray_name = hit.transform.GetComponent<Renderer>().material.name;
        photonView.RPC("ChangeRayColor", Photon.Pun.RpcTarget.All, ray_name);
        ctrl_right.GetComponent<PhotonView>().RPC("RayColor", Photon.Pun.RpcTarget.All, ray_name);
    }   

    private void UpdatePalette(string ht_){
        switch (ht_){
            case "MoveCtrl_TP":
                palette.Find("GameObjectMoveJoy/CubeJoy").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveDrag/CubeDrag").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveSync/CubeSync").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveTP/CubeTP").GetComponent<Renderer>().material = green;
                move_mode = "TP";
                break;

            case "MoveCtrl_Joy":
                palette.Find("GameObjectMoveJoy/CubeJoy").GetComponent<Renderer>().material = green;
                palette.Find("GameObjectMoveDrag/CubeDrag").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveSync/CubeSync").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveTP/CubeTP").GetComponent<Renderer>().material = red;
                move_mode = "Joy";
                break;

            case "MoveCtrl_Drag":
                palette.Find("GameObjectMoveJoy/CubeJoy").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveDrag/CubeDrag").GetComponent<Renderer>().material = green;
                palette.Find("GameObjectMoveSync/CubeSync").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveTP/CubeTP").GetComponent<Renderer>().material = red;
                move_mode = "Drag";
                break;

            case "MoveCtrl_Sync":
                palette.Find("GameObjectMoveJoy/CubeJoy").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveDrag/CubeDrag").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveSync/CubeSync").GetComponent<Renderer>().material = green;
                palette.Find("GameObjectMoveTP/CubeTP").GetComponent<Renderer>().material = red;
                move_mode = "Sync";
                break;

            default:
                break;
        }
    }

    //PunRPC methods
    [PunRPC]
    public void ChangeMoveMode(string mm_){
        switch (mm_){
            case "TP":
                palette.Find("GameObjectMoveJoy/CubeJoy").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveDrag/CubeDrag").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveSync/CubeSync").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveTP/CubeTP").GetComponent<Renderer>().material = green;
                break;

            case "Joy":
                palette.Find("GameObjectMoveJoy/CubeJoy").GetComponent<Renderer>().material = green;
                palette.Find("GameObjectMoveDrag/CubeDrag").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveSync/CubeSync").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveTP/CubeTP").GetComponent<Renderer>().material = red;
                break;

            case "Drag":
                palette.Find("GameObjectMoveJoy/CubeJoy").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveDrag/CubeDrag").GetComponent<Renderer>().material = green;
                palette.Find("GameObjectMoveSync/CubeSync").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveTP/CubeTP").GetComponent<Renderer>().material = red;
                break;

            case "Sync":
                palette.Find("GameObjectMoveJoy/CubeJoy").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveDrag/CubeDrag").GetComponent<Renderer>().material = red;
                palette.Find("GameObjectMoveSync/CubeSync").GetComponent<Renderer>().material = green;
                palette.Find("GameObjectMoveTP/CubeTP").GetComponent<Renderer>().material = red;
                break;

            default:
                break;
        }
        move_mode = mm_;
    }

    [PunRPC]
    public void ChangeRayColor(string color){
        Material material;
        switch (color){
            case "blue (Instance)":
                material = blue;
                break;

            case "green (Instance)":
                material = green;
                break;

            case "red (Instance)":
                material = red;
                break;

            case "white (Instance)":
                material = white;
                break;

            default:
                material = none;
                break;
        }
        ray_cast.GetComponent<Renderer>().material = material;
    }

    [PunRPC]
    public void RemoveTag(int ob_id){
        if(PhotonView.Find(ob_id).gameObject.tag!="Card"){
            return;
        }
        PhotonView.Find(ob_id).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = none;
    }

    [PunRPC]
    public void ChangeTag(int ob_id){
        if(PhotonView.Find(ob_id).gameObject.tag!="Card"){
            return;
        }

        ray_tag_name = ray_cast.GetComponent<Renderer>().material.name;
        Material mat;

        if(experiment!=null && experiment.current_trial.can_tag_card){
            switch (ray_tag_name){
                case "green (Instance)":
                    mat = green;
                    break;
                case "blue (Instance)":
                    mat = blue;
                    break;
                case "red (Instance)":
                    mat = red;
                    break;
                case "white (Instance)":
                    mat = white;
                    break;
                default:
                    mat = none;
                    break;
            }

            PhotonView.Find(ob_id).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = mat;
        }
    }
}