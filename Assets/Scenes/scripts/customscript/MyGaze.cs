using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using CurvedUI;
using Newtonsoft.Json;
using TMPro;
using System.Linq;

public class MyGaze : MonoBehaviour
{


    public GameObject btnIndicator;
    public GameObject btnIndicatorSecond;

    public Canvas canvasLargeDisplay;
    public GameObject objCurvedARScreen;


    public TextMeshProUGUI textMeshPro;

    Vector3 userPose;
    Vector3 puserPose;

    public static Vector2 vecGlobalPosition = Vector2.zero;

    private void Update()
    {
        if (CurvedUIVertexEffect.m_curvedVertsAngles.Count > 0)
        {
            userPose = canvasLargeDisplay.transform.InverseTransformPoint(Camera.main.transform.position);

            if (userPose.x >= -PerspectARConfig.iHalfWidth && userPose.x <= PerspectARConfig.iHalfWidth)
            {

                puserPose = userPose;
            }
            else
            {
                userPose.x = puserPose.x;
            }

            if (CoreServices.InputSystem.EyeGazeProvider.GazeTarget != null)
            {
                if (CoreServices.InputSystem.EyeGazeProvider.GazeTarget.name == PerspectARConfig.strHitObjectName)
                {
         
                    Vector2 pointOnCanvas = new Vector2(0, 0);

                    // Get the world position of the eye gaze hit
                    Vector3 hitWorldPosition = CoreServices.InputSystem.EyeGazeProvider.HitPosition;

                    if (PerspectARConfig.bDynamicAdjustment)
                    {
                        Vector3 localHitPoint = objCurvedARScreen.GetComponent<CurvedUISettings>().GetComponent<Canvas>().transform.worldToLocalMatrix.MultiplyPoint3x4(hitWorldPosition);

                        Vector3 newMidPoint = new Vector3(userPose.x, 0, -objCurvedARScreen.GetComponent<CurvedUISettings>().GetCyllinderRadiusInCanvasSpace());

                        Vector3 directionFromCyllinderCenter = (localHitPoint - newMidPoint).normalized;

                        float referenceAngle = objCurvedARScreen.GetComponent<CurvedUISettings>().Angle;

                        //angle between middle of the projected canvas and hit point direction
                        float angle = -CurvedUIRaycaster.AngleSigned(directionFromCyllinderCenter.ModifyY(0), referenceAngle < 0 ? Vector3.back : Vector3.forward, Vector3.up);
                        
                        //convert angle to canvas coordinates
                        Vector2 canvasSize = objCurvedARScreen.GetComponent<CurvedUISettings>().GetComponent<Canvas>().GetComponent<RectTransform>().rect.size;

                        //map the intersection point to 2d point in canvas space
                        pointOnCanvas.x = angle.Remap(CurvedUIVertexEffect.m_curvedVertsAngles.Min(),
                            CurvedUIVertexEffect.m_curvedVertsAngles.Max(), -canvasSize.x / 2.0f, canvasSize.x / 2.0f);
                        pointOnCanvas.y = localHitPoint.y;

                    }
                    else
                    {
                        Vector3 localHitPoint = canvasLargeDisplay.transform.worldToLocalMatrix.MultiplyPoint3x4(hitWorldPosition);
                        pointOnCanvas.x = localHitPoint.x;
                        pointOnCanvas.y = localHitPoint.y;
                    }

                    vecGlobalPosition = pointOnCanvas;

                    if (PerspectARConfig.strUserName == "VID:HLVFirst" && PerspectARConfig.bEyeGaze)
                    {
                        //Debug.Log("Sending gazedataunityFirst");
                        var data = new { type = "gazedataunityFirst", coord = pointOnCanvas.ToString(), userID = PerspectARConfig.strUserName };
                        string json = JsonConvert.SerializeObject(data);
                        WebSocketConnect.websocket.SendText(json);
 
                    }
      
                    if (PerspectARConfig.strUserName == "VID:HLVSecond" && PerspectARConfig.bEyeGaze)
                    {
                        //Debug.Log("Sending gazedataunitySecond");
                        var data = new { type = "gazedataunitySecond", coord = pointOnCanvas.ToString(), userID = PerspectARConfig.strUserName };
                        string json = JsonConvert.SerializeObject(data);
                        WebSocketConnect.websocket.SendText(json);
     
                    }


                }
                else
                {
                    //Debug.Log(CoreServices.InputSystem.EyeGazeProvider.GazeTarget.name);
                }
            }

            else
            {
                ///Debug.Log("Something wrong with the eye gaze api.");
            }
        }
        else
        {
            //Debug.Log(CurvedUIVertexEffect.m_curvedVertsAngles.Count);
        }
    }
}