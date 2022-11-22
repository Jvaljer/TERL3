using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trial2
{

    // input
    public string group;
    public string participant;
    public string training;

    public string cardSet;
    public string collabEnvironememn;

    // measures
    // public float size;
    // public int tct;
    // public float mux, muy, muz;

        //tag 
    public int nbTag = 0;
    public int nbChangeTag = 0;

        //tp
    public int nbSyncTp  = 0;
    public int nbAsyncTP = 0;
       
    public int nbSyncTpWall = 0;
    public int nbAsyncTpWall = 0;

    public int nbSyncTpGround = 0;
    public int nbAsyncTpGround = 0;

    public int nbSyncTpRotateLeft = 0;
    public int nbSyncTpRotateRight = 0;

    public int nbAsyncTpRotateLeft = 0;
    public int nbAsyncTpRotateRight = 0;


    //drag
    public int nbSyncDragWall = 0;
    public int nbAsyncDragWall = 0;

    public int nbSyncDragGround = 0;
    public int nbAsyncDragGround = 0;

        //joy
    public int nbSyncJoyForward = 0;
    public int nbAsyncJoyForward = 0;

    public int nbSyncJoyBackward = 0;
    public int nbAsyncJoyBackward = 0;

        //card
    public int nbDragCard = 0;
    public int nbGroupCardTP = 0;

    public int nbDestroyCard = 0;
    public int nbUndoCard = 0;

    public string pathLog = "";
    public StreamWriter kineWriter;
    private readonly float timer = 0;


    public Trial2(
        string g_, string p_, string train,
        string cardS, string colabEnv
        )
    {
        group = g_; participant = p_;  training = train;
        cardSet = cardS; collabEnvironememn = colabEnv;
        timer = Time.time;
    }
    public string StringToLog()
    {
        string str = group + ";" + participant + ";" + training + ";" + cardSet + ";" + collabEnvironememn;

        return str;
    }


    // Tag 
    public void incNbTag(string nameR)
    {
        nbTag = nbTag + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Tag" + " ; color : " + nameR);
        kineWriter.Flush();
    }

    public void incNbChangeTag(string nameR)
    {
        nbChangeTag = nbChangeTag + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Change Tag" + " ; color : " + nameR);
        kineWriter.Flush();
    }

    //TP 
    public void incNbSyncTp()
    {
        nbSyncTp = nbSyncTp + 1;
    }

    public void incNbAsyncTP()
    {
        nbAsyncTP = nbAsyncTP + 1;
    }

    public void incNbSyncTpWall(Vector3 translateVector)
    {
        nbSyncTpWall = nbSyncTpWall + 1;
        kineWriter.WriteLine(Time.time - timer +";" + " Sync TP Wall" + " ;  translateVector : " + translateVector);
        kineWriter.Flush();
    }
    public void incNbAsyncTpWall(Vector3 translateVector)
    {
        nbAsyncTpWall = nbAsyncTpWall + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Async TP Wall" + " ; translateVector : " + translateVector);
        kineWriter.Flush();
    }
    public void incNbSyncTpGround(Vector3 translateVector)
    {
        nbSyncTpGround = nbSyncTpGround + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Sync Tp Ground" + " ; translateVector : " + translateVector);
        kineWriter.Flush();
    }
    public void incNbAsyncTpGround(Vector3 translateVector)
    {
        nbAsyncTpGround = nbAsyncTpGround + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Async Tp Ground" + " ; translateVector : " + translateVector);
        kineWriter.Flush();
    }

    public void incNbSyncTpRotateLeft()
    {
        nbSyncTpRotateLeft += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Sync Tp Rotate Left");
        kineWriter.Flush();
    }
    public void incNbSyncTpRotateRight()
    {
        nbSyncTpRotateRight += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Sync Tp Rotate Right");
        kineWriter.Flush();
    }

    public void incNbAsyncTpRotateLeft()
    {
        nbAsyncTpRotateLeft += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Async Tp Rotate Left");
        kineWriter.Flush();
    }

    public void incNbAsyncTpRotateRight()
    {
        nbAsyncTpRotateRight += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Async Tp Rotate Right");
        kineWriter.Flush();
    }

    //Drag
    public void incNbSyncDragWall(Vector3 translateVector)
    {
        nbSyncDragWall += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Sync Drag Wall" + " ;  translateVector : " + translateVector);
        kineWriter.Flush();
    }
    public void incNbAsyncDragWall(Vector3 translateVector)
    {
        nbAsyncDragWall += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Async Drag Wall" + " ; translateVector : " + translateVector);
        kineWriter.Flush();
    }
    public void incNbSyncDragGround(Vector3 translateVector)
    {
        nbSyncDragGround += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Sync Drag Ground" + " ; translateVector : " + translateVector);
        kineWriter.Flush();
    }
    public void incNbAsyncDragGround(Vector3 translateVector)
    {
        nbAsyncDragGround += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Async Drag Ground" + " ; translateVector : " + translateVector);
        kineWriter.Flush();
    }

    //Joystick
    public void incNbSyncJoyForward(Vector3 translateVector)
    {
        nbSyncJoyForward += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Sync Joystick forward" + " ;  translateVector : " + translateVector);
        kineWriter.Flush();
    }
    public void incNbAsyncJoyForward(Vector3 translateVector)
    {
        nbAsyncJoyForward += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Async Joystick forward" + " ; translateVector : " + translateVector);
        kineWriter.Flush();
    }
    public void incNbSyncJoyBackward(Vector3 translateVector)
    {
        nbSyncJoyBackward += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Sync Joystick backward" + " ; translateVector : " + translateVector);
        kineWriter.Flush();
    }
    public void incNbAsyncJoyBackward(Vector3 translateVector)
    {
        nbAsyncJoyBackward += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Async Joystick backward" + " ; translateVector : " + translateVector);
        kineWriter.Flush();
    }

    //card
    public void incNbDragCard()
    {
        nbDragCard = nbDragCard + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Drag card ");
        kineWriter.Flush();
    }
    public void incNbGroupCardTP(string namewall)
    {
        nbGroupCardTP = nbGroupCardTP + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " GroupCardTP" + " ; wall : " + namewall);
        kineWriter.Flush();
    }

    public void incNbDestroyCard()
    {
        nbDestroyCard = nbDestroyCard + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Destroy card ");
        kineWriter.Flush();
    }
    public void incNbUndoCard()
    {
        nbUndoCard = nbUndoCard + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Undo destroy ");
        kineWriter.Flush();
    }
}