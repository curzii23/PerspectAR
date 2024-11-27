using Microsoft.MixedReality.Toolkit;
using System;
using UnityEngine;

public class CameraLogger : MonoBehaviour
{
    

    public GameObject parentAnchor;
    private float elapsedTime = 0;

 
    private string previousFileName = "";
    void addHeader(string filename)
    {
        if (filename != previousFileName)
        {
            Logger.AddHeaderRequest(filename, "UserType", "PID", "UID",
            "Date", "Hour", "Minute", "Second", "Milisecond", "Angle",
            "DisX", "DisY", "DisZ",//Display Object Origin
            "PosX", "PosY", "PosZ", //User Position
            "RotX", "RotY", "RotZ", "RotW", //Head Rotation
            "GazeX", "GazeY", "GazeZ", "GazeDirX", "GazeDirY", "GazeDirZ",//Gaze Origin and Direction
            "HitPosX", "HitPosY", "HitPosZ",
            "LocalPosX", "LocalPosY");//Hit Pose on the Flat Canvas

            previousFileName = filename;
        }
    }

    void Update()
    {
        elapsedTime += Time.fixedTime;
        if (elapsedTime >= PerspectARConfig.RecordFrequency)
        {
            elapsedTime = 0;

            try
            {
                if (PerspectARConfig.filename != "log.csv" && PerspectARConfig.filename != "none.csv"
                    && PerspectARConfig.filename != "Break1.csv" && PerspectARConfig.filename != "Break2.csv")
                //if(filename == "log.csv")//If only record the data from the unity program
                {
                    //Debug.Log("Inside:" + PerspectARConfig.filename);
                    addHeader(PerspectARConfig.filename);

                    //Initializing default variables
                    DateTime now = DateTime.Now;
                    string date = now.ToString("yyyy-MM-dd");
                    Vector3 displayOrigin = parentAnchor.transform.position;
                    Vector3 pos = parentAnchor.transform.InverseTransformPoint(Camera.main.transform.position);
                    Quaternion rot = transform.rotation;
                    Vector3 gazeOrigin = Vector3.zero;
                    Vector3 gazeDir = Vector3.zero;
                    Vector3 gazeHitPos = Vector3.zero;
                    
                    gazeOrigin = parentAnchor.transform.InverseTransformPoint(CoreServices.InputSystem.EyeGazeProvider.GazeOrigin);//Origin at the center of the display
                    gazeDir = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;


                    if (CoreServices.InputSystem.EyeGazeProvider.GazeTarget != null)
                    {
                        if (CoreServices.InputSystem.EyeGazeProvider.GazeTarget.name == PerspectARConfig.strHitObjectName)//check if the gaze is hitting the CurvedARScreen
                        {
                            gazeHitPos = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
                        }
                    }
                    
                    Logger.WriteRequest(PerspectARConfig.filename,
                        PerspectARConfig.userType, PerspectARConfig.PID, PerspectARConfig.UID,//3
                        date, now.Hour, now.Minute, now.Second, now.Millisecond, PerspectARConfig.izAngle,//6
                        displayOrigin.x, displayOrigin.y, displayOrigin.z,//3
                        pos.x, pos.y, pos.z,//3
                        rot.x, rot.y, rot.z, rot.w,//4
                        gazeOrigin.x, gazeOrigin.y, gazeOrigin.z, gazeDir.x, gazeDir.y, gazeDir.z,//6
                        gazeHitPos.x, gazeHitPos.y, gazeHitPos.z,//3
                        MyGaze.vecGlobalPosition.x, MyGaze.vecGlobalPosition.y);//3
                }

            }
            catch
            {
                Debug.Log("Bugged out!");
            }
        }
    }
}
 
 