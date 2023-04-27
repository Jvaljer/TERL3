using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Valve.VR;

public class Teleporter : MonoBehaviourPun {
    //teleporter's menu
    public GameObject menu { get; private set; }

    //player attributes
    private GameObject player;
    private Transform cam_rig;
    public Transform cam;
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
    public Vector3 center_btw_players { get; } = new Vector3(0, 0, 0);

    //room's wall
    private Transform BackWall;
    private Transform RightWall;
    private Transform LeftWall;

    //Raycast atributes
    private GameObject pointer;
    private bool has_pos = false;
    RaycastHit hit;
    RaycastHit init_hit;
    RaycastHit[] hit_object;

    //SteamVR inputs
    private SteamVR_Action_Boolean click;
    private SteamVR_Action_Boolean trigger = SteamVR_Input.GetBooleanAction("InteractUI");

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
    public string move_mode = "Drag"; //other are "Tp" "Joy" "Sync"
    public bool is_other_synced = false;
    public bool tag_sync = true;

    //self atttributes
    public PhotonView photon_view;
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
        photon_view.RPC("ReceiveOtherPos", Photon.Pun.RpcTarget.Others, cam.position, cam_rig.rotation.eulerAngles, cam_rig.position);

        if(trigger.GetStateDown(pose.inputSource) && has_pos){
            TriggerStateDown();
        }

        if(trigger.GetLastStateDown(pose.inputSource)){
            TriggerLastStateDown();
        }

        position = SteamVR_Actions.default_Pos.GetAxis(SteamVR_Input_Sources.Any);
        if(click.GetStateDown(pose.inputSource)){
            ClickPress();
        }

        if(move_mode != "Sync"){
            if(move_mode == "Tp"){
                if(click.GetStateDown(pose.inputSource)){
                    ClickTeleportDown();
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
                    ClickTeleportUp();
                }

                if(moving){
                    prev_coord = pointer.transform.position;
                }

                if(has_pos==true){
                    MenuHandling();
                }

            } else if(move_mode == "Joy"){
                OldFingerCheck();

                if(click.GetState(pose.inputSource)){
                    ClickJoy();
                }

                old_finger.x = position.x;
                old_finger.y = position.y;

            } else if(move_mode == "Drag"){
                if(click.GetStateUp(pose.inputSource) && experiment != null && experiment.current_trial.current_trial_running){
                    ClickDragUp();
                }

                if(click.GetStateDown(pose.inputSource)){
                    move_timer = Time.time;
                    initial_drag_direction = (hit.point - new Vector3(ctrl_right.position.x, 0, ctrl_right.position.z)).normalized;
                }

                if(click.GetState(pose.inputSource)){
                    ClickDrag();
                }
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
                pointer.transform.position = hit.point;
                return true;
            }
        }
        return false;
    }

    public void TriggerStateDown(){
        string hit_tag = hit.transform.tag;
        if(hit_tag=="move_ctrl_TP" && move_mode!="Tp"){
            if(move_mode=="Sync"){
                photon_view.RPC("ToggleOtherSync", Photon.Pun.RpcTarget.Others);
            }
            move_mode = "Tp";
        } else if(hit_tag=="move_ctrl_Joy" && move_mode!="Joy"){
            if(move_mode=="Sync"){
                photon_view.RPC("ToggleOtherSync", Photon.Pun.RpcTarget.Others);
            }
            move_mode = "Joy";
        } else if(hit_tag=="move_ctrl_Drag" && move_mode!="Drag"){
            if(move_mode=="Sync"){
                photon_view.RPC("ToggleOtherSync", Photon.Pun.RpcTarget.Others);
            }
            move_mode = "Drag";
        } else if(hit_tag=="move_ctrl_Sync" && move_mode!="Sync"){
            move_mode = "Sync";
            photon_view.RPC("ToggleOtherSync", Photon.Pun.RpcTarget.Others);
            photon_view.RPC("TpToOther", Photon.Pun.RpcTarget.AllBuffered);
        }
    }

    public void TriggerLastStateDown(){
        if(experiment!=null && !ctrl_right.GetComponent<DragDrop>().trial_start_cond && experiment.trial_running && experiment.current_trial.start_timer && move_mode=="Sync"){
            experiment.photonView.RPC("TrialStart", Photon.Pun.RpcTarget.AllBuffered);
        }
        move_timer = Time.time;
    }

    public void ClickPress(){
        old_ctrl_rotation = ctrl_right.transform.rotation.eulerAngles;
        old_ctrl_forward = ctrl_right.forward;
        init_hit = hit;
    }

    public void ClickTeleportDown(){
        if(position.x < -0.5f){
            w_ = true;
            TryTeleport();
        } else if(position.x > 0.5f){
            e_ = true;
            TryTeleport();
        } else {
            nb_click++;
            click_coord = pointer.transform.position;
            prev_coord = pointer.transform.position;
        }
    }

    public void ClickTeleportUp(){
        moving = false;
        long_click = false;
        e_ = false;
        w_ = false;

        if(wait){
            TryTeleport();
        }

        wait = false;
        long_click = false;
    }

    public void ClickJoy(){
        Quaternion rota = Quaternion.Euler(ctrl_right.rotation.eulerAngles);
        Matrix4x4 mat = Matrix4x4.Rotate(rota);
        Vector3 translate_vec = new Vector3(0,0,0);

        if(position.x < -0.5f){
            float joy_rota = -joystick_rotation * Vector3.Cross(Vector3.up, ctrl_right.forward).magnitude;
            if(is_other_synced){
                photon_view.RPC("MoveRigTransform", Photon.PunRpcTarget.Others, translate_vec, joy_rota);
                cam_rig.RotateAround(cam_rig.position, Vector3.up, joy_rota);
            } else {
                cam_rig.RotateAround(cam_rig.position, Vector3.up, joy_rota);
            }

            if(experiment != null && experiment.current_trial.current_trial_running){
                experiment.current_trial.IncrementTotalRotation(joy_rota* -1f);
            }
        }

        if(position.y > 0.5f){
            translate_vec = mat.MultiplyPoint3x4(plus_Z * joy_rota * 1/FPS.GetCurrentFPS());

            Vector3 cam_angle = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z);
            Vector3 ctrl_angle = new Vector3(ctrl_right.transform.forward.x, 0, ctrl_right.transform.forward.z);

            double cross_product = Vector3.Cross(cam_angle, ctrl_angle).y;

            if(cross_product > 0){
                cam_rotator.RotateAround(cam.position, Vector3.up, 0.25f);
            } else if(cross_product < 0){
                cam_rotator.RotateAround(cam.position, Vector3.up, -0.25f);
            }
        }

        if(position.y < -0.5f){
            translate_vec = mat.MultiplyPoint3x4(minus_Z * joy_rota * 1/FPS.GetCurrentFPS());
        }

        if(position.x > 0.5f){
            if(is_other_synced){
                photon_view.RPC("MoveRigTransform", Photon.PunRpcTarget.Others, translate_vec, joy_rota * -1f);
                cam_rig.RotateAround(cam_rig.position, Vector3.up, joy_rota * -1f);
            } else {
                cam_rig.RotateAround(cam.position, Vector3.up, joy_rota * -1f);
            }

            if(experiment != null && experiment.current_trial.current_trial_running){
                experiment.current_trial.IncrementTotalDist(joy_rota * -1f);
            }
        }

        translate_vec.y = 0;
        Translate(3.5f);
        cam_rig.position += translate_vec;

        if(experiment != null && experiment.current_trial.current_trial_running){
            experiment.current_trial.IncrementTotalDist(translate_vec.magnitude);
        }
        if(is_other_synced){
            photon_view.RPC("MoveRigTransform", Photon.Pun.RpcTarget.Others, translateVect, 0f);
        }
    }

    public void ClickDragUp(){
        if(hit.transform != null){
            if(hit.transform.tag == "Tp"){
                if(init_hit.transform != null && (init_hit.transform.tag == "Wall" || init_hit.transform.tag == "Card")){
                    experiment.current_trial.IncrementDragWallFloorNb();
                }
            }

            if(hit.transform.tag == "Wall" || hit.transform.tag == "Card"){
                experiment.current_trial.IncrementMoveNb();
                experiment.current_trial.IncrementWallSwitchNb();
            }

            experiment.current_trial.IncrementMoveTime(Time.time - move_timer);
            move_timer = Time.time;
        } else {
            experiment.current_trial.IncrementRotationNb();
            move_timer = Time.time;
        }
    }

    public void ClickDrag(){
        Vector3 translate_vec = new Vector3(0, 0, 0);

        if(has_pos && hit.transform.tag == "TP"){
            float old_angle = Vector3.SignedAngle(Vector3.down, old_ctrl_forward, Vector3.Cross(Vector3.down, initialDragDirection).normalized);
            float current_angle = Vector3.SignedAngle(Vector3.down, ctrl_right.forward, Vector3.Cross(Vector3.down, initial_drag_direction).normalized);
        
            float a = Mathf.Tan(old_angle * Mathf.PI/180);
            float b = Mathf.Tan(current_angle * Mathf.PI/180);

            translate_vec = initial_drag_direction;
            translate_vec *= (a_b) * ctrl_right.position.y;
            translate_vec.y = 0;

            Translate(3.5f);

            cam_rig.position += translate_vec;

            if(experiment != null && experiment.current_trial.current_trial_running){
                experiment.current_trial.IncrementTotalDist(translate_vec.magnitude);
            }        

            if(is_other_synced){
                photon_view.RPC("MoveRigTransform", Photon.Pun.RpcTarget.Others, translate_vec, 0f);
            }
        } else if(has_pos && (hit.transform.tag=="Wall" || hit.transform.parent.tag=="Wall")){
            Transform wall;
            float a, b, wall_dist;
            float a_tan = Mathf.Tan( (old_ctrl_rotation.y - wall.rotation.eulerAngles.y) * Mathf.PI/180);
            float b_tan = Mathf.Tan( (ctrl_right.rotation.eulerAngles.y - wall.rotation.eulerAngles.y) * Mathf.PI/180);

            if(hit.transform.name=="BackWall" || hit.transform.parent.name=="BackWall"){
                wall = BackWall;
                wall_dist = Mathf.Abs(wall.position.z - ctrl_right.position.z);

                a = a_tan;
                b = b_tan;

                translate_vec.x = 1.0f;
            } else if(hit.transform.name=="RightWall" || hit.transform.parent.name=="RightWall"){
                wall = RightWall;
                wall_dist = Mathf.Abs(wall.position.x - ctrl_right.position.x);

                a = - a_tan;
                b = - b_tan;

                translate_vec.z = 1.0f;
            } else if(hit.transform.name=="LeftWall" || hit.transform.parent.name=="LeftWall"){
                wall = LeftWall;
                wall_dist = Mathf.Abs(wall.position.x - ctrl_right.position.x);

                a = a_tan;
                b = b_tan;

                translate_vec.z = 1.0f;
            }

            translate_vec *= (a-b) * wall_dist;
            Translate(3.5f);
            cam_rig.position += translate_vec;

            if(experiment != null && experiment.current_trial.current_trial_running){
                experiment.current_trial.IncrementTotalDist(translate_vec.magnitude);
            }

            if(is_other_synced){
                photon_view.RPC("MoveRigTransform", Photon.Pun.RpcTarget.Others, translate_vec, 0f);
            }
        } else {
            float angle = old_ctrl_rotation.y - ctrl_right.transform.rotation.eulerAngles.y;

            if(is_other_synced){
                photon_view.RPC("MoveRigTransform", Photon.Pun.RpcTarget.Others, translate_vec, angle);
                cam_rig.RotateAround(cam_rig.position, Vector3.up, angle);
            } else {
                cam_rig.RotateAround(cam.position, Vector3.up, angle);
            }

            if(experiment != null && experiment.current_trial.current_trial_running){
                experiment.IncrementTotalRotation(angle);
            }
        }

        old_ctrl_forward = ctrl_right.forward;
        old_ctrl_rotation = ctrl_right.transform.rotation.eulerAngles;
    }

    public void TryTeleport(){
        if(teleporting){
            return;
        }

        Vector3 pointer_pos = pointer.transform.position;
        if(e_){
            if(tp_sync || is_other_synced){
                photon_view.RPC("RotationRig", Photon.Pun.RpcTarget.All, "e");
            } else {
                cam_rig.RotateAround(cam.position, Vector3.up, 90);
                experiment.IncrementTotalRotation(90);
                experiment.IncrementRotationNb();
            }
            return;
        } else if(w_){
            if(tp_sync || is_other_synced){
                photon_view.RPC("RotationRig", Photon.Pun.RpcTarget.All, "w");
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
            if(pointer_pos.x < -3.5f){ pointer_pos.x = -3.5f; }
            if(pointer_pos.x > 3.5f){ pointer_pos.x = -3.5f; }
            if(pointer_pos.z < -3.5f){ pointer_pos.z = -3.5f; }
            if(pointer_pos.z < -3.5f){ pointer_pos.z = -3.5f; }

            translate_vec = pointer_pos - cam.position;
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
                translate_vec = new Vector3(pointer.transform.position.x - cam.position.x, 0, 3-cam.position.z);
                parent_wall = BackWall;

            } else if(hit_name=="RightWall" || hit_parent_name=="RightWall"){
                translate_vec = new Vector3(pointer.transform.position.x - cam.position.x, 0, 3-cam.position.z);
                parent_wall = RightWall;

            } else if(hit_name=="LeftWall" || hit_parent_name=="LeftWall"){
                translate_vec = new Vector3(pointer.transform.position.x - cam.position.x, 0, 3-cam.position.z);
                parent_wall = LeftWall;

            }
            StartCoroutine(MoveRig(translate_vec, parent_wall));

        } else if(hit_tag=="Player"){
            photon_view.RPC("TpToOther", Photon.Pun.RpcTarget.AllBuffered);
        }
    }

    public void MenuHandling(){
        //might need to correct this
        string hit_name = hit.transform.name;
        if(name=="Synchro" || name=="Not Synchro"){
            menu.SetActive(false);
            photon_view.RPC("TeleportMode", Photon.Pun.RpcTarget.All, tp_sync);
        } else if(name=="Synchro Tag" ||name=="Not Synchro Tag"){
            menu.SetActive(false);
            player = GameObject.Find("Network Player(Clone)");
            tag_sync = true;
            photon_view.RPC("TagMode", Photon.Pun.RpcTarget.All, tag_sync);
        } else if(name=="Cancel"){
            menu.SetActive(false);
        }
    }

    public void UpdateCenter(){
        center_btw_players = (other_player_pos + cam.position) / 2f;
        center_btw_players.y = 0;
    }

    //Routine methods
    private IEnumerator MoveRigRoutine(Vector3 translation, Transform wall){
        move_timer = Time.time;
        teleporting = true;

        SteamVR_Fade.Start(Color.black, fade_time, true);
        yield return new WaitForSeconds(fade_time);

        if(wall!=null){
            cam_rig.rotation = wall.rotation;
            if(experiment!=null && experiment.current_trial.current_trial_running){
                experiment.current_trial.IncrementTotalRotation(wall.rotation.eulerAngles.y - cam_rig.rotation.eulerAngles.y);
                if(wall.rotation.eulerAngles.y - cam_rig.rotation.eulerAngles.y!=0){
                    experiment.current_trial.IncrementRotationNb();
                }
            }
        }

        Translate(3.5f);

        if(experiment!=null && experiment.current_trial.current_trial_running){
            experiment.current_trial.IncrementTotalDist(translation.magnitude);
        }

        SteamVR_Fade.Start(Color.clear, fade_time, true);
        if(tp_sync || is_other_synced){
            photon_view.RPC("MoveRig", Photon.Pun.RpcTarget.Others, cam_rig.position, cam_rig.rotation.eulerAngles);
            tp_sync = false;
        }
        teleporting = false;

        if(experiment!=null && experiment.current_trial.current_trial_running){
            experiment.current_trial.IncrementMoveNb();
            experiment.current_trial.IncrementMoveTime(Time.time - move_timer);
            move_timer = Time.time;

            if(wall!=null){
                experiment.current_trial.IncrementDragWallFloorNb();
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

    public void OldFingerCheck(){
        if(click.GetLastStateDown(pose.inputSource)){
            old_finger.x = position.x;
            old_finger.y = position.y;
        }

        if( (old_finger.x > 0.5f && (position.x < 0.5f || click.GetStateUp(pose.inputSource)))
        || (old_finger.x < -0.5f && (position.x > -0.5f || click.GetStateUp(pose.inputSource))) 
        && experiment.current_trial.current_trial_running) {

            experiment.current_trial.IncrementRotationNb();
        }

        if( (old_finger.y > 0.5f && (position.y < 0.5f || click.GetStateUp(pose.inputSource)))
        || (old_finger.y < -0.5f && (position.y > -0.5f || click.GetStateUp(pose.inputSource))) 
        && experiment.current_trial.current_trial_running) {

            experiment.current_trial.IncrementMoveNb();
            experiment.current_trial.IncrementMoveTime(Time.time - move_timer);
            move_timer = Time.time;
        }

        if( (old_finger.y < 0.5f && position.y > 0.5f) || (old_finger.y > -0.5f && position.y < -0.5f) ){
            move_timer = Time.time;
        }
    }

    public void Translate(float n){
        if(cam.position.x + translate.x < -n){
            translat.x = -n - cam.position.x;
        }
        if(cam.position.x + translate.x > n){
            translat.x = n - cam.position.x;
        }
        if(cam.position.z + translate.z < -n){
            translat.z = -n - cam.position.z;
        }
        if(cam.position.z + translate.x > n){
            translat.x = n - cam.position.z;
        }
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

    [PunRPC]
    public void MoveRigTransform(Vector3 translate, float rota){
        Translate(4f);

        cam_rig.position += translate;
        cam_rig.RotateAround(other_player_cam_rig_pos, Vector3.up, rota);
        experiment.current_trial.IncrementTotalRotation(joystick_rotation);
        experiment.current_trial.IncrementTotalDist(translate.magnitude);
    }
}