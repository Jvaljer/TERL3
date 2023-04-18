using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Photon.Pun;
using Photon.Realtime;

public class Trial {
    //card attributes
    private GameObject card;
    private Material init_card_material;

    //participants attributes
    private GameObject operator_;
    private NetworkOperator operator_script;
    private GameObject player;
    private NetworkPlayer player_script;
    private GameObject ctrl;
    private Teleporter ctrl_tp;

    //room & expe attributes
    private GameObject room;
    private Rendering room_render;
    private Transform cards_area;
    private Experiment experiment;

    //input variables
    public string group;
    public string participant;
    private string trial_nb;
    public string training;
    public string collab_env;
    public string move_mode;
    public string task;
    private string wall;
    private string card_to_tag;

    //trial's statements
    public bool current_trial_running= false;
    private bool trial_ended = false;
    public bool can_tag_card = true;
    public bool start_timer = false;

    //logs informations
    public int move_nb = 0;
    public int switch_wall_nb = 0;
    public int drag_wall_floor_nb = 0;
    public int roate_nb = 0;
    public float total_dist = 0;
    public float total_rotate = 0;
    public float trial_time;
    public float move_time = 0;

    //logs writing info
    public string path_log = "";
    public StreamWriter kine_writer;
    private float timer = 0;

    //Constructor
    public Trial(Experiment E_, string p_, string g_, string c_env, string trial, string train, string mm_, string t_, string w_, string ct_){
        SetParticipants(p_);

        ctrl = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)");
        ctrl_tp = ctrl.GetComponent<Teleporter>();
        room = GameObject.Find("/Salle");
        room_render = room.GetComponent<Rendering>();
        cards_area = room_render.card_area;

        experiment = E_;
        group = g_;
        participant = p_;
        collab_env = c_env;
        trial_nb = trial;
        training = train;
        move_mode = mm_;
        task = t_;
        wall = w_;
        card_to_tag = ct_;
        timer = Time.time;

        if(card_to_tag!=""){
            card = experiment.card_list[int.Parse(card_to_tag)];
        }
    }

    //setters
    public void SetParticipants(string name){
        if(name=="ope"){
            operator_ = GameObject.Find("Network Operator(Clone)");
            operator_script = operator_.GetComponent<NetworkOperator>();

            player = null;
        } else if(name==""){
            operator_ = GameObject.Find("Network Operator(Clone)");
            operator_script = operator_?.GetComponent<NetworkOperator>();

            player = GameObject.Find("Network Player(Clone)");
            player_script = player?.GetComponent<NetworkPlayer>();
        } else {
            player = GameObject.Find("Network Player(Clone)");
            player_script = player.GetComponent<NetworkPlayer>();

            operator_ = null;
        }
    }

    //all other methods
    public void CheckConditions(){
        float dist = (ctrl_tp.center_btw_players - card.transform.position).magnitude;

        if(dist < 3){

        } 

        bool fst_cond = (card.transform.rotation.eulerAngles.y==0 
            && Math.Abs(ctrl_tp.center_btw_players.x - card.transform.position.x)<1
            && Math.Abs(ctrl_tp.center_btw_players.z - card.transform.position.z)<2.5f);
        bool snd_cond = (card.transform.rotation.eulerAngles.y!=0
            && Math.Abs(ctrl_tp.center_btw_players.x - card.transform.position.x)<2.5f
            && Math.Abs(ctrl_tp.center_btw_players.z - card.transform.position.z)<1);

        if (!trial_ended && fst_cond || snd_cond ){
            can_tag_card = true;
            if(player==null){
                cards_area.GetComponent<Renderer>().material = operator_script.white;
            } else {
                cards_area.GetComponent<Renderer>().material = player_script.white;
            }
        } else {
            can_tag_card = false;
            if(player==null){
                cards_area.GetComponent<Renderer>().material = operator_script.none;
            } else {
                cards_area.GetComponent<Renderer>().material = player_script.none;
            }
        }

        if(!trial_ended && can_tag_card && card.transform.GetChild(0).GetComponent<Renderer>().material!=init_card_material){
            EndTrial();
        }
    }

    public void StartTrial(){
        if(player==null){
            if(card.transform.GetChild(0).GetComponent<Renderer>().material == null){
                card.transform.GetChild(0).GetComponent<Renderer>().material = operator_script.none;
            }
        } else {
            card.transform.GetChild(0).GetComponent<Renderer>().material = player_script.none;
        }

        init_card_material = card.transform.GetChild(0).GetComponent<Renderer>().material;

        if(task=="search"){
            ctrl_tp.TpToOther();
            ctrl_tp.is_other_synced = false;
            ctrl_tp.move_mode = "sync";
        } else {
            ctrl_tp.is_other_synced = true;
            ctrl_tp.move_mode = move_mode;
        }

        start_timer = true;
    }

    public void EndTrial(){
        trial_time = Time.time - trial_time;
        cards_area.gameObject.SetActive(false);

        if(player==null){
            card.transform.GetChild(1).GetComponent<Renderer>().material = operator_script.Green;
        } else {
            card.transform.GetChild(1).GetComponent<Renderer>().material = player_script.Green;
        }

        trial_ended = true;
        current_trial_running = false;
        room_render.photonView.RPC("NextTrial", Photon.Pun.RpcTarget.AllBuffered);
    }

    //Logs Incrementation methods
    public void IncrementTotalRotation(float angle){
        total_rotate += Mathf.Abs(angle);
        kine_writer.WriteLine(Time.time - timer + "; Rotate " + angle);
        kine_writer.Flush();
    }

    public void IncrementTotalDist(float dist){
        total_dist += dist;
        kine_writer.WriteLine(Time.time - timer + "; Move " + dist);
        kine_writer.Flush();
    }

    public void IncrementMoveTime(float t_){
        move_time += t_;
    }

    public void IncrementRotationNb(){
        roate_nb += 1;
        kine_writer.WriteLine(Time.time - timer + ";Rotate");
        kine_writer.Flush();
    }

    public void IncrementMoveNb(){
        move_nb += 1;
        kine_writer.WriteLine(Time.time - timer + "; Move");
        kine_writer.Flush();
    }

    public void IncrementDragWallFloorNb(){
        drag_wall_floor_nb += 1;
        kine_writer.WriteLine(Time.time - timer + "; DragWallFloor");
        kine_writer.Flush();
    }

    public void IncrementWallSwitchNb(){
        switch_wall_nb += 1;
        kine_writer.WriteLine(Time.time - timer + "; WallSwitch");
        kine_writer.Flush();
    }

    public void StartTimer(){
        trial_time = Time.time;
        if(task=="Search"){
            card.transform.GetChild(1).gameObject.SetActive(true);
        }
        current_trial_running = true;
        start_timer = false;
    }
}