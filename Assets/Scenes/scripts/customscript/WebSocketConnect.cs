using System;
using UnityEngine;
using System.IO;
using NativeWebSocket;
using TMPro;
using Newtonsoft.Json;
using CurvedUI;
using Vuplex.WebView;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

//Reference: https://github.com/endel/NativeWebSocket

public class WebSocketConnect : MonoBehaviour
{
    public static WebSocket websocket;
    public string EditorAddress;
    public CanvasWebViewPrefab webViewPrefab;
    public Canvas canvas;
    public TextMeshProUGUI textMeshPro;
    public GameObject parent1;
    public GameObject parent2;
    public GameObject Gaze1;
    public GameObject Gaze2;

    
    public class UserInfo
    {
        public string type { get; set; }
        public string sCode { get; set; }
        public string gazeStatus { get; set; }
        public string userType { get; set; }
        public string taskType { get; set; }
        public string coord { get; set; }
        public string userName { get; set; }
        public string UID { get; set; }
        public string PID { get; set; }
        public string taskIndex { get; set; }
    }

    public class CalibInfo
    {
        public string type { get; set; }
        public string sCode { get; set; }
        public string DepthZ { get; set; }
        public string Angle { get; set; }
        public string Scale { get; set; }
    }


    public void CurvedARScreenToLargeDisplay(bool transferToParent2)
    {
        
        if (parent1 == null || parent2 == null)
        {
            Debug.LogError("Parent1 or Parent2 not found.");
            return;
        }

        GameObject userOne = parent1.transform.Find("UserT_One")?.gameObject;
        GameObject userTwo = parent1.transform.Find("UserT_Two")?.gameObject;

        if (userOne == null || userTwo == null)
        {
            userOne = parent2.transform.Find("UserT_One")?.gameObject;
            userTwo = parent2.transform.Find("UserT_Two")?.gameObject;
        }

        if (transferToParent2)
        {
            TransferChildren(parent1, parent2, userOne, userTwo);
            parent1.SetActive(false);
            ModifyScript(userOne, false);
            ModifyScript(userTwo, false);
            parent2.GetComponentInChildren<Image>().raycastTarget = true;
        }
        else
        {
            parent1.SetActive(true);
            TransferChildren(parent2, parent1, userOne, userTwo);
            ModifyScript(userOne, true);
            ModifyScript(userTwo, true);
            parent2.GetComponentInChildren<Image>().raycastTarget = false;
        }
    }

    void TransferChildren(GameObject fromParent, GameObject toParent, GameObject userOne, GameObject userTwo)
    {
        if (userOne.transform.parent == fromParent.transform)
        {
            userOne.transform.SetParent(toParent.transform);
        }
        if (userTwo.transform.parent == fromParent.transform)
        {
            userTwo.transform.SetParent(toParent.transform);
        }
    }

    void ModifyScript(GameObject obj, bool disableScript)
    {
        // Get script components on the current GameObject
        CurvedUIVertexEffect curvedUIVertexEffect = obj.GetComponent<CurvedUIVertexEffect>();

        // Disable or enable script components based on disableScript flag
        if (curvedUIVertexEffect != null)
        {
            curvedUIVertexEffect.enabled = disableScript;
        }
    }

    async void Start()
    {
        CurvedUISettings mySettings;
        mySettings = GetComponentInParent<CurvedUISettings>();

        string path = Path.Combine(Application.persistentDataPath, "settings.txt");
        
        if (!File.Exists(path))
        {
            StreamWriter sw = File.CreateText(path);
            sw.WriteLine(PerspectARConfig.strDefaultServerIP);
            sw.WriteLine(PerspectARConfig.sURL);
            sw.WriteLine(PerspectARConfig.positionStep);
            sw.WriteLine(PerspectARConfig.rotationStep);
            sw.WriteLine(PerspectARConfig.gazeLerp);
            sw.WriteLine("0");
            sw.WriteLine("y");
            sw.Close();
        }
        string[] fLines = File.ReadAllLines(path);

        //Connect to the Server
        if (fLines[0].Length > 0)
        {
            websocket = new WebSocket(fLines[0]);
        }
        else
        {
            websocket = new WebSocket(PerspectARConfig.strDefaultURL);
        }

        if(webViewPrefab.isActiveAndEnabled)
        {
            await webViewPrefab.WaitUntilInitialized();

            //Open the Website
            if (fLines[1].Length > 0)
            {
                webViewPrefab.WebView.LoadUrl(fLines[1]);
                float.TryParse(fLines[2], out PerspectARConfig.positionStep);
                float.TryParse(fLines[3], out PerspectARConfig.rotationStep);
                float.TryParse(fLines[4], out PerspectARConfig.gazeLerp);
            }
            else
            {
                webViewPrefab.WebView.LoadUrl(PerspectARConfig.sURL);
            }
        }
        

        websocket.OnOpen += () =>
        {
            // Create your JSON object
            var data = new { type = "system", userName = PerspectARConfig.strUserName };//When smartphone user connected, we need to connect the corresponding HL user
            // Serialize the JSON object to a string
            string json = JsonConvert.SerializeObject(data);
            websocket.SendText(json);

        };
        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };
        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };
        websocket.OnMessage += (bytes) =>
        {
            string receivedData = System.Text.Encoding.UTF8.GetString(bytes);

            // Parse the JSON string into a JObject
            var receivedJObject = JObject.Parse(receivedData);

            string type = receivedJObject["type"]?.ToString();

            switch (type)
            {
                case "smessage":

                    var receivedJson = receivedJObject.ToObject<UserInfo>();

                    //Initilize Setting
                    if (receivedJson.sCode == "config")
                    {
                        PerspectARConfig.UID = receivedJson.UID;
                        PerspectARConfig.PID = receivedJson.PID;
                        PerspectARConfig.userType = receivedJson.userType;
                        PerspectARConfig.userID = receivedJson.userName;

                        if (receivedJson.gazeStatus == "enablemygaze")//Enable Gaze Indicators
                        {
                            PerspectARConfig.bEyeGaze = true;
                            Debug.Log("enablemygaze");
                        }
                        else if (receivedJson.gazeStatus == "disablemygaze")//Disable Gaze Indicators
                        {
                            PerspectARConfig.bEyeGaze = false;
                            Debug.Log("disablemygaze");
                        }
                    }


                    if (receivedJson.sCode == "disablecanvas")//Disable virtual screen
                    {
                        Debug.Log("Disable");
                        //canvas.enabled = false;
                        PerspectARConfig.bDynamicAdjustment = false;
                    }
                    else if (receivedJson.sCode == "enablecanvas")//Enable virtual screen
                    {
                        Debug.Log("Enable");
                        //canvas.enabled = true;
                        PerspectARConfig.bDynamicAdjustment = true;
                    }
                    
                    else if (receivedJson.sCode == "cTask")//Get Current Task and set the log files name accordiangly
                    {
                        Debug.Log("Current Active Task:" + receivedJson.taskType);
                        if(receivedJson.taskType != "none")
                        {
                            PerspectARConfig.filename = receivedJson.taskType + "_" + receivedJson.taskIndex + ".csv";
                        }
                        else
                        {
                            PerspectARConfig.filename = "none.csv";
                        }

                    }
                    else if (receivedJson.sCode == "gazedataunityFirst")//Show the Second user the gaze of First user
                    {

                        if (!Gaze1.activeSelf)
                        {
                            Gaze1.gameObject.SetActive(true);
                            Gaze2.gameObject.SetActive(false);
                        }

                        String receivedCoord = receivedJson.coord;

                        // Remove parentheses and split the string by commas
                        string[] components = receivedCoord.Replace("(", "").Replace(")", "").Split(',');

                        if (components.Length == 2)
                        {
                            // Parse the individual components
                            float x = float.Parse(components[0]);
                            float y = float.Parse(components[1]);
 
                            if (PerspectARConfig.gazeLerp > 0.5)
                            {
                                Gaze1.transform.localPosition = Vector2.Lerp(Gaze1.transform.localPosition, new Vector2(x, y), PerspectARConfig.gazeLerp * Time.deltaTime);
                            }
                            else
                            {
                                Gaze1.transform.localPosition = new Vector2(x, y);
                            }
                        }
                        else
                        {
                            Debug.LogError("Invalid Vector3 string format: " + receivedCoord);
                        }

                    }
                    else if (receivedJson.sCode == "gazedataunitySecond")//Show the First user the gaze of Second user
                    {

                        if (!Gaze2.gameObject.activeSelf)
                        {
                            Gaze1.gameObject.SetActive(false);
                            Gaze2.gameObject.SetActive(true);
                        }


                        String receivedCoord = receivedJson.coord;

                        // Remove parentheses and split the string by commas
                        string[] components = receivedCoord.Replace("(", "").Replace(")", "").Split(',');

                        if (components.Length == 2)
                        {
                            // Parse the individual components
                            float x = float.Parse(components[0]);
                            float y = float.Parse(components[1]);

                            if (PerspectARConfig.gazeLerp > 0.5)
                            {
                                Gaze2.transform.localPosition = Vector2.Lerp(Gaze2.transform.localPosition, new Vector2(x, y), PerspectARConfig.gazeLerp * Time.deltaTime);
                            }
                            else
                            {
                                Gaze2.transform.localPosition = new Vector2(x, y);
                            }
                        }
                        else
                        {
                            Debug.LogError("Invalid Vector3 string format: " + receivedCoord);
                        }

                    }

                    else if (receivedJson.userType == PerspectARConfig.strUserNameTemp1)//Large Display Disable
                    {
                        //CurvedUISettings.dynamicadjustment = false;
                        //canvas.enabled = false;
                        //Debug.Log(receivedJson.userType);
                        PerspectARConfig.strHitObjectName = "Large Display";
                        CurvedARScreenToLargeDisplay(true);
                        PerspectARConfig.bDynamicAdjustment = false;

                    }
                    else if (receivedJson.userType == PerspectARConfig.strUserNameTemp2)//AR Enable
                    {
                        //CurvedUISettings.dynamicadjustment = true;
                        //canvas.enabled = true;
                        PerspectARConfig.strHitObjectName = "CurvedARScreen";
                        CurvedARScreenToLargeDisplay(false);
                        PerspectARConfig.bDynamicAdjustment = true;
                    }

                    break;
                case "calibInfoType":
                    var receivedJsonCalib = receivedJObject.ToObject<CalibInfo>();

                    if (receivedJsonCalib.sCode == "scalib")
                    {
                        mySettings.Angle = int.Parse(receivedJsonCalib.Angle);
                        GameObject objToHide1 = GameObject.Find("CurvedARScreen");
                        GameObject objToHide2 = GameObject.Find("Transparent");

                        Vector3 newScaleDepthZ = objToHide1.transform.localScale;
                        newScaleDepthZ.z = float.Parse(receivedJsonCalib.DepthZ);
                        objToHide1.transform.localScale = newScaleDepthZ;

                        Vector3 newScaleTransparent = objToHide2.transform.localScale;
                        newScaleTransparent.x = float.Parse(receivedJsonCalib.Scale);
                        objToHide2.transform.localScale = newScaleTransparent;

                    }


                    break;
                default:
                    Debug.LogWarning("Unknown type or type missing in received JSON");
                    break;
            }

        };
        // waiting for messages
        await websocket.Connect();
    }

    
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif

    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}