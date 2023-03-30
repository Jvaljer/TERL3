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
    private bool e_ = false;
    private bool w_ = false;
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

        if(move_mode != "Sync"){
            if(move_mode == "Tp"){
                if(click.GetStateDown(pose.inputSource)){
                    ClickTeleport();
                }
                if(wait && (Time.time - timer > 0.7f)){
                    long_click = true;
                    wait = false;
                }
                if(long_click){
                    tp_sync = true;
                    TryTeleport();
                    long_click = false;
                }

                if(click.GetStateUp(pose.inputSource)){
                    ClickStateUp();
                }
                if(moving){
                    prev_coord = intersect.transform.position;
                }

                if(has_pos==true){
                    MenuHandling();
                }

            } else if(move_mode == "Joy"){

            }
        }
    }
    

    //methods
    public void CheckForExpe(){
        if(experiment==null){
            experiment = GameObject.Find("/Room").GetComponent<Rendering>().experiment;
        }
    }

    public bool UpdatePointer(){
        Ray ray = new Ray(transform.position, transform.forward);

        if(Physics.Raycast(ray, out hit)){
            string hit_tag = hit.transform.tag;
            if(hit_tag!="" && hit_tag!=null){
                //must test all existing tags ??
                intersect.transform.position = hit.point;
                return true;
            }
        }
        return false;
    }

    public void TriggerStateDown(){
        string hit_tag = hit.transform.tag;
        if(hit_tag=="move_ctrl_TP" && move_mode!="Tp"){
            if(move_mode=="Sync"){
                photonView.RPC("ToggleOtherSync", Photon.Pun.RpcTarget.Others);
            }
            move_mode = "Tp";
        } else if(hit_tag=="move_ctrl_Joy" && move_mode!="Joy"){
            if(move_mode=="Sync"){
                photonView.RPC("ToggleOtherSync", Photon.Pun.RpcTarget.Others);
            }
            move_mode = "Joy";
        } else if(hit_tag=="move_ctrl_Drag" && move_mode!="Drag"){
            if(move_mode=="Sync"){
                photonView.RPC("ToggleOtherSync", Photon.Pun.RpcTarget.Others);
            }
            move_mode = "Drag";
        } else if(hit_tag=="move_ctrl_Sync" && move_mode!="Sync"){
            move_mode = "Sync";
            photonView.RPC("ToggleOtherSync", Photon.Pun.RpcTarget.Others);
            photonView.RPC("TpToOther", Photon.Pun.RpcTarget.AllBuffered);
        }
    }

    public void TriggerLastStateDown(){
        if(experiment!=null && !ctrl_right.GetComponent<DragDrop>().trial_start_cond && experiment.trial_running && experiment.current_trial.start_timer && move_mode=="Sync"){
            experiment.photonView.RPC("TrialStart", Photon.Pun.RpcTarget.AllBuffered);
        }
        move_timer = Time.time;
    }

    public void ClickStateDown(){
        //must implement 
        return;
    }

    public void ClickTeleport(){
        //must implement
        return;
    }

    public void TryTeleport(){
        if(teleporting){
            return;
        }

        Vector3 intersect_pos = intersect.transform.position;
        if(e_){
            if(tp_sync || is_other_synced){
                photonView.RPC("RotationRig", Photon.Pun.RpcTarget.All, "e");
            } else {
                cam_rig.RotateAround(cam.position, Vector3.up, 90);
                experiment.IncrementTotalRotation(90);
                experiment.IncrementRotationNb();
            }
            return;
        } else if(w_){
            if(tp_sync || is_other_synced){
                photonView.RPC("RotationRig", Photon.Pun.RpcTarget.All, "w");
            } else {
                cam_rig.RotateAround(cam.position, Vector3.up, -90);
                experiment.IncrementTotalRotation(90);
                experiment.IncrementRotationNb();
            }
            return;
        }
        Vector3 translate_vec;
        string hit_tag = hit.transform.tag;
        if(!has_pos){
            return;
        } else if(hit_tag=="Tp"){
            if(intersect_pos.x < -3.5f){ intersect_pos.x = -3.5f; }
            if(intersect_pos.x > 3.5f){ intersect_pos.x = -3.5f; }
            if(intersect_pos.z < -3.5f){ intersect_pos.z = -3.5f; }
            if(intersect_pos.z < -3.5f){ intersect_pos.z = -3.5f; }

            translate_vec = intersect_pos - cam.position;
            translate_vec.y = 0;

            StartCoroutine(MoveRig(translate_vec, null));

        } else if(hit_tag=="Wall" || hit_tag=="Card"){
            Transform parent_wall;

            Vector3 cam_look = new Vector3(cam.transforl.forward.x, 0, cam.transform.forward.z);
            hit_object = Physics.RaycastAll(cam_rig.transform.position, cam_rig.transform.forward, 100.0f);

            float x = -cam_rig.transform.position.x;
            float z = -cam_rig.transform.position.y;

            string hit_name = hit.transform.name;
            string hit_parent_name = hit.transform.parent.name;
            if(hit_name=="BackWall" || hit_parent_name=="BackWall"){
                translate_vec = new Vector3(intersect.transform.position.x - cam.position.x, 0, 3-cam.position.z);
                parent_wall = BackWall;

            } else if(hit_name=="RightWall" || hit_parent_name=="RightWall"){
                translate_vec = new Vector3(intersect.transform.position.x - cam.position.x, 0, 3-cam.position.z);
                parent_wall = RightWall;

            } else if(hit_name=="LeftWall" || hit_parent_name=="LeftWall"){
                translate_vec = new Vector3(intersect.transform.position.x - cam.position.x, 0, 3-cam.position.z);
                parent_wall = LeftWall;

            }
            StartCoroutine(MoveRig(translate_vec, parent_wall));

        } else if(hit_tag=="Player"){
            photonView.RPC("TpToOther", Photon.Pun.RpcTarget.AllBuffered);
        }
    }

    public void MenuHandling(){
        //might need to correct this
        string hit_name = hit.transform.name;
        if(name=="Synchro" || name=="Not Synchro"){
            menu.SetActive(false);
            photonView.RPC("TeleportMode", Photon.Pun.RpcTarget.All, tp_sync);
        } else if(name=="Synchro Tag" ||name=="Not Synchro Tag"){
            menu.SetActive(false);
            player = GameObject.Find("Network Player(Clone)");
            tag_sync = true;
            photonView.RPC("TagMode", Photon.Pun.RpcTarget.All, tag_sync);
        } else if(name=="Cancel"){
            menu.SetActive(false);
        }
    }

    public void UpdateCenter(){
        center_btw_players = (other_player_pos + cam.position) / 2f;
        center_btw_players.y = 0;
    }

    //Routine methods
    private IEnumerator MoveRigCasual(Vector3 translation, Transform wall){
        move_timer = Time.time;
        teleporting = true;

        SteamVR_Fade.Start(Color.black, fade_time, true);
        yield return new WaitForSeconds(fade_time);

        if(wal!=null){
            cam_rig.rotation = wall.rotation;
            if(experiment!=null && experiment.current_trial.current_trial_running){
                experiment.current_trial.IncrementTotalRotation(wall.rotation.eulerAngles.y - cam_rig.rotation.eulerAngles.y);
                if(wall.rotation.eulerAngles.y - cam_rig.rotation.eulerAngles.y!=0){
                    experiment.current_trial.IncrementRotationNb();
                }
            }
        }

        if(cam.position + translation.x <-3.5f){
            translation.x = -3.5f - cam.position.x;
        }
        if(cam.position.x + translation.x > 3.5f){
            translation.x = 3.5f - cam.position.x;
        }
        if(cam.position.z + translation.z < -3.5f){
            translation.z = -3.5f - cam.position.z;
        }
        if(cam.position.z + translation.z > 3.5f){
            translation.z = 3.5f - cam.position.z;
        }

        if(experiment!=null && experiment.current_trial.current_trial_running){
            experiment.current_trial.IncrementTotalDist(translation.magnitude);
        }

        SteamVR_Fade.Start(Color.clear, fade_time, true);
        if(tp_sync || is_other_synced){
            photonView.RPC("MoveRig", Photon.Pun.RpcTarget.Others, cam_rig.position, cam_rig.rotation.eulerAngles);
            tp_sync = false;
        }
        teleporting = false;

        if(experiment!=null && experiment.current_trial.current_trial_running){
            experiment.current_trial.IncrementMoveNb();
            experiment.current_trial.IncrementMoveTime(Time.time - move_timer);
            move_timer = Time.time;

            if(wall!=null){
                experiment.current_trial.IncrementWallSwitchNb();
            }
        }
    }

    private IEnumerator MoveRigForSyncTp(Vector3 pos, Vector3 rota){
        move_timer = Time.time;
        teleporting = true;

        SteamVR_Fade.Start(Color.black, fade_time, true);
        yield return new WaitForSeconds(fade_time);

        cam_rig.RotateAround(camp.position, Vector3.up, rota.y - cam_rig.rotation.eulerAngles.y);

        if(experiment!=null && experiment.current_trial.current_trial_running){
            experiment.current_trial.IncrementTotalRotation(rota.y - cam_rig.rotation.eulerAngles.y);
            experiment.current_trial.IncrementTotalDist((pos - cam_rig.position).magnitude);
        }
        cam_rig.position = pos;

        SteamVR_Fade.Start(Color.clear, fade_time, true);
        teleporting = false;

        if(experiment!=null && experiment.current_trial.current_trial_running){
            experiment.current_trial.IncrementMoveTime(Time.time - move_timer);
        }
        experiment.SetInfoLocation();
    }

    //Photon PunRPC methods
    [PunRPC]
    public void ReceiveOtherPos(Vector3 pos, Vector3 rota, Vector3 cam_rig_pos){
        other_player_cam_rig_pos = cam_rig_pos;
        other_player_pos = position;
        other_player_pos.y = 0;
        other_player_rotation = rotation;

        UpdateCenter();

        cube.transform.position = center_btw_players;
    }

    [PunRPC]
    public void TpToOther(){
        StartCoroutine(MoveRigForSyncTp(other_player_cam_rig_pos, other_player_rotation));
    }

    [PunRPC]
    public void RotationRig(string str){
        Transform cam_rig_bis = SteamVR_Render.Top().origin.parent;

        if(str=="e"){
            cube.transform.RotateAround(cube.transform.position, Vector3.up, 90);
            cam_rig_bis.RotateAround(cube.transform.position, Vector3.up, 90);

            if(experiment!=null){
                experiment.current_trial.IncrementTotalRotation(90);
                experiment.current_trial.IncrementRotationNb();
            }
        } else if(str=="w"){
            cube.transform.RotateAround(cube.transform.position, Vector3.up, -90);
            cam_rig_bis.RotateAround(cube.transform.position, Vector3.up, -90);

            if(experiment!=null){
                experiment.current_trial.IncrementTotalRotation(90);
                experiment.current_trial.IncrementRotationNb();
            }
        }
    }

    [PunRPC]
    public void MoveRig(Vector3 translation, Vector3 rota){
        StartCoroutine(MoveRigForSyncTp(translation, rota));
    }
}