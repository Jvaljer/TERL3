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
    private GameObject operator;
    private Network_Operator operator_script;
    private GameObject player;
    private Network_Player player_script;
    private GameObject controller;
    private Teleporter controller_tp;

    //room & expe attributes
    private GameObject room;
    private rendering room_render;
    private Transform cards_area;
    private Experiment experiment;

    //input variables
    private string group;
    private string participant;
    private string trial_nb;
    private string training;
    private string collab_env;
    private string move_mode;
    private string task;
    private string wall;
    private string card_to_tag;

    //trial's statements
    private bool current_trial_running = false;
    private bool triel_ended = false;
    private bool can_tag_card = true;
    private bool start_timer = false;

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
            operator = GameObject.Find("Network Operator(Clone)");
            operator_script = operator.GetComponent<Network_Operator>();
        } else if(name==""){
            operator = GameObject.Find("Network Operator(Clone)");
            operator_script = operator?.GetComponent<Network_Operator>();

            player = GameObject.Find("Network Player(Clone)");
            player_script = player?.GetComponent<Network_Player>();
        } else {
            player = GameObject.Find("Network Player(Clone)");
            player_script = player.GetComponent<Network_Player>();
        }
    }
}