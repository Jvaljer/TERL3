using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Valve.VR;

public class Teleporter : MonoBehaviour {
    //teleporter's menu
    public GameObject menu { get; private set; }

    //player attributes
    private GameObject player;
    private Transform cam_rig;
    private Transform cam;
    private Transform cam_rotator;
    private Transform ctrl_rotator;
    private Transform ctrl_right;
    private Transform ctrl_left;
    private float init_cam_rotation_Y = 0;

    //player's visual attribute
    private GameObject cube;
    private GameObject cube_player;

    //other player attributes
    private Vector3 other_player_pos = new Vector3(0,0,0);
    private Vector3 other_player_rotation = new Vector3(0, 0, 0);
    private Vector3 other_player_cam_rig_pos = new Vector3(0, 0, 0);
    public Vector3 center_btw_players { get; private set; } = new Vector3(0, 0, 0);

    //room's wall
    private Transform BackWall;
    private Transform RightWall;
    private Transform LeftWall;

    //Raycast atributes
    private GameObject intersect;
    private bool has_pos = false;
    RaycastHit hit;
    RaycastHit[] hit_object;

    //SteamVR inputs
    private SteamVR_Action_Boolean click;
    private SteamCR_Action_Boolean trigger = SteamVR_Input.GetBoolean("InteractUI");

    //SteamVR Pose
    private SteamVR_Behaviour_Pose pose = null;

    //teleport parameters
    private bool teleporting = false;
    private readonly float fade_time = 0.5f;
    private bool tp_sync = false;
    private string tp_mode = "Not Synchro";
    private readonly float desired_dist = 1f;

    //machine & inputs statements
    private bool wait = false;
    private bool moving = false;
    private bool long_click = false;
    private readonly bool dub_click = false;
    private Vector3 click_coord;
    private Vector3 prev_coord;
    private Vector3 click_forward;
    private Vector3 old_ctrl_rotation;
    private Vector3 old_ctrl_forward;
    private Vector3 initial_drag_direction;

    private Vector2 old_finger = new Vector2(0,0);
    private const float move_speed = 4f;

    private Vector3 plus_Z = new Vector3(0f, 0f, move_speed);
    private Vector3 minus_Z = new Vector3(0f, 0f, -move_speed);

    private const float joystick_rotation = 0.5f;
    private float move_timer;
    private float timer = 0f;

    private int nb_click = 0;

    //move mode attributes
    public string move_mode { get; private set; } = "drag";
    private bool is_other_synced = false;
    private bool tag_sync = true;

    //self atttributes
    private PhotonView photon_view;
    private Vector2 position;
    private Experiment experiment;

    //Unity's Awake method 
    void Awake(){
        pose = GetComponent<SteamVR_Behaviour_Pose>();
        photon_view = GetComponent<PhotonView>();
        menu.SetActive(false);
    }

    //Unity's Update method called once per frame
    void Update(){
        CheckForExpe();

        has_pos = UpdatePointer();
        photonView.RPC("ReceiveOtherPos", Photon.Pun.RpcTarget.Others, cam.position, cam_rig.rotation.eulerAngles, cam_rig.position);

        if(trigger.GetStateDown(pos.inputSource) && has_pos){
            TriggerStateDown();
        }

        if(trigger.GetLastStateDown(pose.inputSource)){
            TriggerLastStateDown();
        }

        position = SteamVR_Actions.default_Pos.GetAxis(SteamVR_Input_Sources.Any);
        if(click.GetStateDown(pose.inputSource)){
            ClickStateDown();
        }
    }

    //methods
    public void CheckForExpe(){
        if(experiment==null){
            experiment = GameObject.Find("/Room").GetComponent<Rendering>().experiment;
        }
    }

    public bool UpdatePointer(){
        //must implement
        return true;
    }

    public void TriggerStateDown(){
        //must implement
        return;
    }

    public void TriggerLastStateDown(){
        //must implement
        return;
    }

    public void ClickStateDown(){
        //must implement 
        return;
    }

    //Photon PunRPC methods
    [PunRPC]
    public void ReceiveOtherPos(Vector3 pos, Vector3 rota, Vector3 cam_rig_pos){
        //must implement
        return true;
    }
}