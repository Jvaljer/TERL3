using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Valve.VR;
using Photon.Pun;

public class DragDrop : MonoBehaviourPun {
    //intersection ray/object attributes
    private GameObject pointer;
    private bool has_pos = false;
    private RaycastHit hit;

    //trigger boolean
    private SteamVR_Action_Boolean trigger = SteamVR.Input.GetBooleanAction("InteractUI");

    //pose
    private SteamVR_Behaviour_Pose pose;

    //machine state
    private bool moving = false;
    private bool wait = false;
    private bool long_click = false;
    private bool trial_start_cstr = false;
    private float timer = 0;

    //click attributes
    private Vector3 click_coord;
    private Vector3 click_forward;

    //object to move
    private Gameobject object;

    //undoed objects
    private List<GameObject> undo_objects;

    //room attributes & experiment
    private GameObject room;
    private Rendering room_render;
    private Transform BackWall;
    private Transform LeftWall;
    private Transform RightWall;
    private Experiment experiment;

    //card texture
    private Texture texture;

    //ray attributes
    private string ray_name;
    private string wall_name;

    //referred player & attributes
    private GameObject player;
    private Teleporter teleporter;

    //selected cards attributes
    private GameObject empty_card_to_move;
    private bool from_group = false;
    private Vector3 empty_local_scale;
    private string move_mode = "Drag";

    //class enabled predicate
    private bool enabled = true;

    //Unity Awake method, called before any 'Start' method, used here as an initializer
    void Awake(){
        pose = GetComponent<SteamVR_Behaviour_Pose>();
        room = GameObject.Find("/Room");
        room_render = room.GetComponent<Rendering>();
        teleporter = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)").GetComponent<Teleporter>();
    }

    //Unity Update method, called once per frame
    void Update(){
        if(experiment==null && room_render.experiment){
            experiment = GameObject.Find("/Room").GetComponent<Rendering>().experiment;
            enabled = false;
        }

        if(enabled){
            has_pos = UpdatePointer();

            if(trigger.GetStateUp(pose.inputSource)){
                if(wait && object!=null){
                    //just a click -> tagging object
                    player = GameObject.Find("Network Player(Clone)");
                    player.GetComponent<PhotonView>().RPC("ChangeTag", Photon.Pun.RpcTarget.AllBuffered, hit.transform.gameObject.GetComponent<PhotonView>().ViewID);
                }

                if(empty_card_to_move!=null){
                    EmptyUpdate();
                }

                moving = false;
                object = null;
                wait = false;
                long_click = false;
                timer = 0;
            }

            if(trigger.GetStateDown(pose.inputSource) && has_pos){
                HitCheck(hit.transform.tag);

                click_coord = hit.transform.position;
                click_forward = transform.forward;
                wait = true;
                timer = Time.time;
            }

            if(object!=null && wait && Vector3.Angle(click_forward, transform.forward) > 2){
                moving = true;
                wait = false;
            }

            has_pos = UpdatePointer();

            if(object!=null && has_pos && hit.transform.tag=="trash"){
                TrashClickDestroy();
            }

            if(undo_objects!=null && has_pos && hit.transform.tag=="trash" && trigger.GetStateDown(pose.inputSource)){
                TrashClickUndo();
            }

            if(wait && (Time.time - timer > 1.5f)){
                long_click = true;
                wait = false;
            }

            has_pos = UpdatePointer();

            if(long_click && has_pos && ()){
                MoveCardWithTag();
            }

            if(trigger.GetState(pose.inputSource)){
                MoveCard();
            }
        }
    }

    //other methods
    private bool UpdatePointer(){
        //must implement
        return false;
    }

    private void EmptyUpdate(){
        //must implement
        return;
    }

    private void HitCheck(string tag){
        //must implement
        return;
    }

    private void TrashClickDestroy(){
        //must implement
        return;
    }

    private void TrashClickUndo(){
        //must implement
        return;
    }

    private void MoveCardWithTag(){
        //must implement
        return;
    }

    private void MoveCard(){
        //must implement
        return;
    }

    //PunRPC methods
}