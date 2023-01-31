using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Operating_Menu : MonoBehaviour {
    private GameObject title;

    private GameObject Options; //Canvas which will be shown throughout the program

    private GameObject Canvas_Operator; //all components of "Canvas with Operator"
    private GameObject Canvas_Options; //all components of "Canvas Option"
    private GameObject Canvas_Demo; //all components of "Canvas Demo"
    private GameObject Canvas_Expe_1; //all components of "Canvas Expe 1"
    private GameObject Canvas_Expe_2; //all components of "Canvas Expe 2"

    private GameObject Current_Canvas; //actual canva we wanna show when needed


    private Player_Spawner spawner;
    private NetworkManager manager;

    // Start is called before the first frame update
    void Start(){
        spawner = GameObject.Find("NetworkManager").GetComponent<Player_Spawner>();
        manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        
        Canvas_Operator = GameObject.Find("Canvas with Operator");
        Canvas_Options = GameObject.Find("Canvas Option");
        Canvas_Demo = GameObject.Find("Canvas Demo");
        Canvas_Expe_1 = GameObject.Find("Canvas Expe 1");
        Canvas_Expe_2 = GameObject.Find("Canvas Expe 2");

        Current_Canvas = Canvas_Operator;
        Current_Canvas.gameObject.SetActive(true);

        Canvas_Options.gameObject.SetActive(false);
        Canvas_Demo.gameObject.SetActive(false);
        Canvas_Expe_1.gameObject.SetActive(false);
        Canvas_Expe_2.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update(){
        //here we just wanna make sure which Canvas (i.e. Menu) we wanna set as Active.
    }

    //Buttons functions
    public void OpeOption(){
        Debug.Log("Button -> OpeOption");
        //here we wanna make the PlayerSpawner 'WithOpe' variable switch from True to False to True ... on each click
        // & print the result on the menu (true & false setActive(...))

        spawner.withOperator = !spawner.withOperator;
        return;
    }

    public void OpeChoosed(){
        Debug.Log("Button -> OpeChoosed");
        //here we wanna go on with the 'ok' button with the chosen parameter 'WithOpe'

        manager.Connect();
        return;
    }

    public void StartDemo(){
        Debug.Log("Button -> StartDemo");
        //simply wanna launch the demo -> giving the Demo Menu to operator (I guess)
        return;
    }

    public void StartExpe(){
        Debug.Log("Button -> StartExpe");
        //here wanna start the expe (check if in a demo first -> if so then stop it, if not so simply initiates a demo)
        // careful with the startExpe method of 'rendering'...
        return;
    }

    public void CreateCards(){
        Debug.Log("Button -> CreateCards");
        //here we wanna create the cards for the demo version (maybe wanna make a second CreateCards method for the demo ?)
        return;
    }

    public void DeleteCards(){
        Debug.Log("Button -> DeleteCards");
        //here we wanna delete the cards of the demo version (maybe wanna make a second DeleteCards method for the demo ?)
        return;
    }

    public void End(){
        Debug.Log("Button -> End");
        //here we wanna end the expe -> make everything stop as the expe is the finality of the program ? 
        return;
    }

    public void LaunchExpe(){
        Debug.Log("Button -> LaunchExpe");
        //just wanna launch the expe -> use for the first time 'NextTrial' to start the first one (as the 2nd space press actually)
        return;
    }

    public void LaunchNextTrial(){
        Debug.Log("Button -> LaunchNextTrial");
        //here we're simply gonna launch the 'nextTrial' method as the first one has already been created
        return;
    }

    public void TpToOther(){
        Debug.Log("Button -> TpToOther");
        //here we wanna implement a new feature -> TP both player at the beginning point but without going on nextTrial
        //not a priority tho...
        return;
    }
}
