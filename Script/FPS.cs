using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour {
    //maximum frame to average over
    private int max_frame = 60;

    //frames states
    private static float last_calculated_FPS;
    private List<float> frame_time = new List<float>();

    //Start method from Unity -> used as an initialization
    void Start(){
        last_calculated_FPS = 0f;
        frame_time.Clear();
    }

    //Update method from Unity -> called once per frame 
    void Update(){
        AddFrame();
        last_calculated_FPS = CalculateFPS();
    }

    //all methods
    private void AddFrame(){
        frame_time.Add(Time.unscaledDeltaTime);
        if(frame_time.Count > max_frame){
            frame_time.RemoveAt(0);
        }
    }

    private float CalculateFPS(){
        float frames_total_time = 0f;

        foreach(float frame in frame_time){
            frames_total_time += frame;
        }

        return ((float)(frame_time.Count)) / frames_total_time;
    }

    public static float GetCurrentFPS(){
        return last_calculated_FPS;
    }
}