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
    private GameObject controller;
    private Teleporter controller_tp;

    //room & expe attributes
    private GameObject room;
    private Rendering room_render;
    private Transform cards_area;
    private Experiment experiment;

    //input variables
    private string group { get; }
    private string participant;
    private string trial_nb;
    private string training { get; }
    private string collab_env { get; }
    private string move_mode { get; }
    private string task { get; }
    private string wall;
    private string card_to_tag;

    //trial's statements
    private bool current_trial_running { get; } = false;
    private bool trial_ended = false;
    private bool can_tag_card { get; } = true;
    private bool start_timer { get; } = false;

    //logs informations
    private int move_nb = 0;
    private int switch_wall_nb = 0;
    private int drag_wall_floor_nb = 0;
    private int roate_nb = 0;
    private float total_dist = 0;
    private float total_rotate = 0;
    private float trial_time;
    private float move_time = 0;

    //logs writing info
    private string path_log = "";
    private StreamWriter kine_writer;
    private float timer = 0;

    //Constructor
    public Trial(Experiment E_, string p_, string g_, string c_env, string trial, string train, string mm_, string t_, string w_, string ct_){
        SetParticipants(p_);

        controller = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)");
        controller_tp = controller.GetComponent<Teleporter>();
        room = GameObject.Find("/Salle");
        room_render = room.GetComponent<Rendering>();
        cards_area = room_render.card_area;

        expe = E_;
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
            card = experiment.cardList[int.Parse(card_to_tag)];
        }
    }

    //setters
    public void SetParticipants(string name){
        if(name=="ope"){
            operator_ = GameObject.Find("Network Operator(Clone)");
            operator_script = operator_.GetComponent<Network_Operator>();

            player = null;
        } else if(name==""){
            operator_ = GameObject.Find("Network Operator(Clone)");
            operator_script = operator_?.GetComponent<Network_Operator>();

            player = GameObject.Find("Network Player(Clone)");
            player_script = player?.GetComponent<Network_Player>();
        } else {
            player = GameObject.Find("Network Player(Clone)");
            player_script = player.GetComponent<Network_Player>();

            operator_ = null;
        }
    }

    //all other methods
    public void CheckConditions(){
        float dist = (controller_tp.center_btw_players - card.transform.position).magnitude;

        if(dist < 3){

        } 

        bool fst_cond = (card.transform.rotation.eulerAngles.y==0 
            && Math.Abs(controller_tp.center_btw_players.x - card.transform.position.x)<1
            && Math.Abs(controller_tp.center_btw_players.z - card.transform.position.z)<2.5f);
        bool snd_cond = (card.transform.rotation.eulerAngles.y!=0
            && Math.Abs(controller_tp.center_btw_players.x - card.transform.position.x)<2.5f
            && Math.Abs(controller_tp.center_btw_players.z - card.transform.position.z)<1);

        if (!trial_ended && fst_cond || snd_cond ){
            can_tag_card = true;
            if(player==null){
                card_area.GetComponent<Renderer>().material = operator_script.white;
            } else {
                card_area.GetComponent<Renderer>().material = player_script.white;
            }
        } else {
            can_tag_card = false;
            if(player==null){
                card_area.GetComponent<Renderer>().material = operator_script.None;
            } else {
                card_area.GetComponent<Renderer>().material = player_script.None;
            }
        }

        if(!trial_ended && can_tag_card && card.transform.GetChild(0).GetComponent<Renderer>().material!=init_card_material){
            EndTrial();
        }
    }

    public void StartTrial(){
        if(player==null){
            if(card.transform.GetChild(0).GetComponent<Renderer>().material == null){
                card.transform.GetChild(0).GetComponent<Renderer>().material = operator_script.None;
            }
        } else {
            card.transform.GetChild(0).GetComponent<Renderer>().material = player_script.None;
        }

        init_card_material = card.transform.GetChild(0).GetComponent<Renderer>().material;

        if(task=="search"){
            controller_tp.TpToOther();
            controller_tp.is_other_sync = false;
            controller_tp.move_mode = "sync";
        } else {
            controller_tp.is_other_sync = true;
            controller_tp.move_mode = move_mode;
        }

        start_timer = true;
    }

    public void EndTrial(){
        trial_time = Time.time - trial_time;
        card_area.gameObject.SetActive(false);

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