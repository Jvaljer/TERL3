using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Operating_Menu : MonoBehaviour {

    //Must Manage to make it fits the screen's size...


    private Player_Spawner spawner;
    private NetworkManager manager;

    private GameObject OperatorSetting;
    private GameObject With;
    private GameObject Without;

    /*//awake could be nice ?
    void Awake(){
        //same as Start() ? 
    }

    */
    // Start is called before the first frame update
    void Start(){
        Debug.Log("Operating_Menu is starting");
        spawner = GameObject.Find("NetworkManager").GetComponent<Player_Spawner>();
        manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        OperatorSetting = GameObject.Find("Operator Setting");
        With = GameObject.Find("With");
        Without = GameObject.Find("Without");

        OperatorSetting.gameObject.SetActive(true);

        spawner.withOperator = true;
    }

    // Update is called once per frame
    void Update(){
        //must implement the Current Menu setter & associated Activations
    }

    //OperatorSettings 'OnCLick' methods
    public void SwitchOperatorState(){
        spawner.withOperator = !spawner.withOperator;
        if(spawner.withOperator){
            With.SetActive(true);
            Without.SetActive(false);
        } else {
            With.SetActive(false);
            Without.SetActive(true);
        }
    }

    public void Validation(){
        manager.Connect();
        OperatorSetting.SetActive(false);
    }
    
}
