using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Experiment {
    
    //random thread definition
    static class ThreadSafeRandom {
        [ThreadStatic] private static System.Random local;

        public static System.Random ThisThreadsRandom {
            get { 
                return local ?? (local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); 
            }
        }
    }

    //expe global variables
    private string participant;
    private string group;
    private int trial_start = 1;
    private int trial_nb = 0;
    public bool expe_running = false;
    public bool trial_running = false;

    //participant attributes
    private GameObject player;
    private NetworkPlayer player_script;
    private GameObject controller;
    public Teleporter ctrl_tp;
    
    //room script
    private Rendering room_render;

    //specific logs variables
    private readonly string expe_description_file;
    private List<Trial> trials;
    public List<GameObject> card_list;
    public Trial current_trial;
    private StreamWriter writer;
    private StreamWriter kine_writer;


    //Constructor
    public Experiment(string prt, string grp, int start_nb, List<GameObject> c_list, bool with_ope) {
        

        //variables instantiation 
        expe_running = true;
        group = grp;
        participant = prt;
        trial_nb = start_nb;
        card_list = c_list;

        //getting the player's attributes
        player = GameObject.Find("Network Player(Clone)");
        player_script = player.GetComponent<NetworkPlayer>();
        controller = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)");
        ctrl_tp = controller.GetComponent<Teleporter>();

        //getting the operator's room attribute depending on who's the operator (master client)
        int master_id = PhotonNetwork.MasterClient.ActorNumber;
        GameObject master = PhotonView.Find(master_id).gameObject;
        room_render = SetRoomRender(master, with_ope);

        //initialization of Log files for this expe
        string date = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string path = "Assets/Resources/logs/class-" + group + "-" + participant + "-" + date + ".csv";
        writer = new StreamWriter(path, false);
        writer.WriteLine(
            "Group;Participant;CollabEnvironment;trialNb;training;MoveMode;Task;Wall;CardToTag;" +
            "nbMove;nbMoveWall;nbDragWallFloor;distTotal;nbRotate;rotateTotal;" +
            "trialTime;moveTime"
        );
        writer.Flush();

        path = "Assets/Resources/logs/class-" + participant + "-" + date + ".txt";
        kine_writer = new StreamWriter(path, false);
        expe_description_file = SetDescriptionFile(with_ope);

        TextAsset data_txt = (TextAsset) Resources.Load(expe_description_file);
        string txt = data_txt.text;
        List<string> lines = new List<string>(txt.Split('\n'));
        trials = new List<Trial>();

        foreach(string line in lines){
            List<string> values = new List<string>(line.Split(';'));
            if(values[0]=="pause" && trials.Count!=0 && trials[trials.Count-1].group==group){
                trials.Add(new Trial(this, values[0], "", "", "", "", "", "", "", ""));
            } else if(values[0]==group && values[1]==participant){
                trials.Add(new Trial(this, values[0], values[1], values[2], values[3],
                                    values[4], values[5], values[6], values[7], values[8]));
                trials[trials.Count-1].path_log = path;
                trials[trials.Count-1].kine_writer = kine_writer;
            }
        }
        current_trial = trials[trial_nb];
        kine_writer.WriteLine(current_trial.group + " " + current_trial.participant + " kine action");
        kine_writer.Flush();
    }

    //all setters
    public Rendering SetRoomRender(GameObject GO, bool b_){
        if(!b_){
            return (GO.GetComponent<NetworkPlayer>().room_render);
        } else {
            return (GO.GetComponent<NetworkOperator>().room_render);
        }
    }

    public string SetDescriptionFile(bool b_){
        if(b_){
            return "Experiments/textDataFile";
        } else {
            return "Experiments/initialTrialFile";
        }
    }

    //all other methods
    public void SetInfoLocation(){
        ctrl_tp.menu.transform.position = ctrl_tp.cam.position + 1f*ctrl_tp.cam.forward;
        ctrl_tp.menu.transform.rotation = ctrl_tp.cam.rotation;
        ctrl_tp.menu.transform.RotateAround(ctrl_tp.menu.transform.position, Vector3.up, ctrl_tp.cam.rotation.eulerAngles.y - ctrl_tp.menu.transform.rotation.eulerAngles.y);
    }

    public void Write(){
        writer.WriteLine(
            // "factor"
            trials[trial_nb].group + ";" + trials[trial_nb].participant + ";" + trials[trial_nb].collab_env + ";" + trials[trial_nb].trial_nb + ";" + trials[trial_nb].training + ";" + trials[trial_nb].move_mode + ";" + trials[trial_nb].task + ";" + trials[trial_nb].wall + ";" + trials[trial_nb].card_to_tag + ";"
            // measure
            + trials[trial_nb].move_nb + ";" + trials[trial_nb].switch_wall_nb + ";" + trials[trial_nb].drag_wall_floor_nb  + ";" + trials[trial_nb].total_dist + ";" + trials[trial_nb].roate_nb + ";" + trials[trial_nb].total_rotate + ";"
            + trials[trial_nb].trial_time + ";" + trials[trial_nb].move_time
            );
        writer.Flush();
    }

    public void Finish(){
        Write();
        ctrl_tp.menu.transform.Find("textInfo").gameObject.SetActive(true);
        ctrl_tp.menu.transform.Find("textInfo").GetComponent<TextMesh>().text = "Fin de l'expérience";
        trial_running = false;
        expe_running = false;
        writer.Close();
        kineWriter.Close();
    }

    public void IncrementTrialNb(){
        trial_nb += 1;
        if (trial_nb - 2 >= 0 && theTrials[trial_nb - 2].group != "#pause"){
            trials[trial_nb - 2].card.transform.GetChild(1).gameObject.SetActive(false);
            trials[trial_nb - 2].card.transform.GetChild(1).GetComponent<Renderer>().material = player_script.lightRed;
            trials[trial_nb - 2].card.transform.GetChild(0).GetComponent<Renderer>().material = player_script.none;
        }
    }

    public void End(){
        Write();
        ctrl_tp.menu.transform.Find("text_info").gameObject.SetActive(true);
        ctrl_tp.menu.transform.Find("text_info").GetComponent<TextMesh>().text = "End of the experiment";
        trial_running = false;
        expe_running = false;
        writer.Close();
        kine_writer.Close();
    }

    public void NextTrial(){
        if(!trial_running){
            trial_running = true;
            SetInfoLocation();
            ctrl_tp.menu.SetActive(true);

            if(trials[trial_nb].task == "Search"){
                if(trials[trial_nb].training == "1"){
                    ctrl_tp.menu.transform.Find("move_mode_text").GetComponent<TextMesh>().text = "Search \n Training";
                } else {
                    ctrl_tp.menu.transform.Find("move_mode_text").GetComponent<TextMesh>().text = "Search";
                }
                ctrl_tp.menu.transform.Find("text_info").GetComponent<TextMesh>().text = "You are the one synchronized \n click to start the trial \n spot the card and tell the other";
            } else {
                if(trials[trial_nb].training == "1"){
                    ctrl_tp.menu.transform.Find("move_mode_text").GetComponent<TextMesh>().text = "Navigate " + trials[trial_nb].moveMode + "\n Training";
                } else {
                    ctrl_tp.menu.transform.Find("move_mode_text").GetComponent<TextMesh>().text = "Navigate " + trials[trial_nb].moveMode;
                }
                ctrl_tp.menu.transform.Find("text_info").GetComponent<TextMesh>().text = "You are the one moving \n wait for the other to start \n let the other tell you where to go";
            }

            trials[trial_nb].StartTrial();
            current_trial = trials[trial_nb];

        } else if(trial_nb == trials.Count - 1){
            Write();
            trials[trial_nb - 1].card.transform.GetChild(1).gameObject.SetActive(false);
            trials[trial_nb].card.transform.GetChild(1).gameObject.SetActive(false);
            Finish();
        } else {
            Write();
            IncrementTrialNb();
            if(trials[trial_nb].group == "#pause"){
                trial_running = false;
                ctrl_tp.photonView.RPC("ResetPosition", Photon.Pun.RpcTarget.AllBuffered);
                ctrl_tp.menu.transform.Find("text_info").GetComponent<TextMesh>();
                writer.WriteLine("#pause;");
                IncrementTrialNb();
                ctrl_tp.menu.transform.Find("move_mode_text").GetComponent<TextMesh>().text = "Next move " + trials[trial_nb].moveMode;
                SetInfoLocation();
                ctrl_tp.menu.SetActive(true);
            } else {
                SetInfoLocation();
                if(trials[trial_nb].task == "Search"){
                    if(trials[trial_nb].training == "1"){
                        ctrl_tp.menu.transform.Find("move_mode_text").GetComponent<TextMesh>().text = "Search \n Training";
                    } else {
                        ctrl_tp.menu.transform.Find("move_mode_text").GetComponent<TextMesh>().text = "Search";
                    }
                    ctrl_tp.menu.transform.Find("text_info").GetComponent<TextMesh>().text = "You are the one synchronized \n click to start the trial \n spot the card and tell the other";
                } else {
                    if(trials[trial_nb].training == "1"){
                        ctrl_tp.menu.transform.Find("move_mode_text").GetComponent<TextMesh>().text = "Navigate " + trials[trial_nb].moveMode + "\n Training";
                    } else {
                        ctrl_tp.menu.transform.Find("move_mode_text").GetComponent<TextMesh>().text = "Navigate " + trials[trial_nb].moveMode;
                    }
                    ctrl_tp.menu.transform.Find("text_info").GetComponent<TextMesh>().text = "You are the one moving \n wait for the other to start \n let the other tell you where to go";
                }
                ctrl_tp.menu.SetActive(true);
                trials[trial_nb].StartTrial();
                current_trial = trials[trial_nb];
            }
        }
    }

    public IEnumerator TrialStarted(){
        current_trial.StartTimer();
        ctrl_tp.menu.transform.Find("text_info").GetComponent<TextMesh>().text = "Trial Started";
        yield return new WaitForSeconds(5);
        ctrl_tp.menu.SetActive(false);
    }
}