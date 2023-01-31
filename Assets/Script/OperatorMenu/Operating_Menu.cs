using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Operating_Menu : MonoBehaviour {

    private Player_Spawner spawner;
    private NetworkManager manager;

    private GameObject OperatorSetting;
    private GameObject StartingMenu;

    /*//awake could be nice ?
    void Awake(){
        //same as Start() ? 
    }
    */
    // Start is called before the first frame update
    void Start(){
        spawner = GameObject.Find("NetworkManager").GetComponent<Player_Spawner>();
        manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        OperatorSetting = GameObject.Find("Operator Setting");
        StartingMenu = GameObject.Find("Starting Menu");

        OperatorSetting.gameObject.SetActive(true);
        StartingMenu.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update(){
        //must implement the Current Menu setter & associated Activations
    }

    //OperatorSettings 'OnCLick' methods
    public void SwitchOperatorState(){
        spawner.withOperator = !spawner.withOperator;
        if(spawner.withOperator){
            GameObject.Find("With").SetActive(true);
            GameObject.Find("Without").SetActive(false);
        } else {
            GameObject.Find("With").SetActive(false);
            GameObject.Find("Without").SetActive(true);
        }
    }

    public void Validation(){
        manager.Connect();
        OperatorSetting.SetActive(false);
    }

    //StartingMenu 'OnClick' methods
    public void StartDemo(){
        //must implement
        //  -> shall do the same as if pressing D in the current model
        StartingMenu.gameObject.SetActive(false);
    }

    public void StartExpe(){
        //must implement 
        //  -> shall do the same as if pressing SPACE in the current model
        StartingMenu.gameObject.SetActive(false);
    }
}
