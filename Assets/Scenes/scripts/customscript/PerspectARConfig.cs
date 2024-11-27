using System;
using UnityEngine;

public class PerspectARConfig : MonoBehaviour
{

    public static bool bCalib = false;
    public static int pCompenAngle = 90;

    //User Defined Settings
    
    public static String strUserName = "VID:HLVFirst";//unity virtual user
    public static String strUserNameTemp1 = "ULDFirst";//large display
    public static String strUserNameTemp2 = "UARFirst";//ar user
    public static String strDefaultServerIP = "ws://192.168.4.51:3000";//Server Address
    public static String sURL = "http://192.168.4.51/perspectAR_webapp/hl_user_one.html";
    public static String strDefaultURL = "wss://none";


    /*
    public static String strUserName = "VID:HLVSecond";
    public static String strUserNameTemp1 = "ULDSecond";
    public static String strUserNameTemp2 = "UARSecond";
    public static String strDefaultServerIP = "ws://192.168.4.51:3000";
    public static String sURL = "http://192.168.4.51/perspectAR_webapp/hl_user_two.html";
    public static String strDefaultURL = "wss://none";
    */


    //PerspectAR Introploation Configuration
    public static bool bEnableInter = false;
    public static float fInter = 2.0f;

    //Virtual Screen
    public static int iHalfWidth = 2880;
    public static int iWidth = 5760;
    public static int izAngle = 1; //z Angle dynamically adjsuted by the user's z-axis movements

    //Virtual Screen Work Area (WorkSpace)
    public static float minDistance = 0.5f;
    public static float maxDistance = 1.65f;
    public static bool bDynamicAdjustment = true;//Dynamic adjustment of the Virtual Screen based on the user's position

    //Gaze Indicators
    public static bool bEyeGaze = false;
    public static String strHitObjectName = "CurvedARScreen";

    //Transparent Window
    public static float fMinScale = 0.2f;
    public static Vector3 fTransparentWindowDepth;

    //Logger
    public static string userID = "none";
    public static float RecordFrequency = 1;
    public static string filename = "log.csv";
    public static string UID = "none";
    public static string PID = "none";
    public static string strQuestionIndex = "none";
    public static string userType = "none";

    //Manual Adjustments of a virtual screen
    public static float positionStep = 0.025f; // Constant step for position
    public static float rotationStep = 0.5f; // Constant step for rotation
    public static float gazeLerp = 10.0f;

}
