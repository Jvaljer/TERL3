using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperatorMenu : MonoBehaviour
{
    public static bool Paused = false;

    // Update is called once per frame
    void Update() {
        //Nothing we wanna do here 
    }
    
    public void Resume(){
        //operatorMenuUI.SetActive(false);
        Debug.Log("Resume");
        Time.timeScale = 1f;
        Paused = false;
    }

    public void Pause(){
        //operatorMenuUI.SetActive(true);
        Debug.Log("Pause");
        Time.timeScale = 0f;
        Paused = true;
    }

    public void Button(){
        Debug.Log("pressing button");
    }
}
