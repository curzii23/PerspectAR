using CurvedUI;
using UnityEngine;

public class Transparent : MonoBehaviour
{

    CurvedUISettings mySettings = null;
    Canvas myCanvas;

    void Start()
    {
        mySettings = GetComponentInParent<CurvedUISettings>();

        if (mySettings == null) return;
        myCanvas = mySettings.GetComponent<Canvas>();

    }

    
    void Update()
    {
        //width with respect to the scale and full width of the large display (so basically the half width of the transparent window)
        float width = (transform.localScale.x * PerspectARConfig.iWidth) / 2;

        if (Camera.main != null)
        {
            Vector3 vecUsersWorldPose = Camera.main.transform.position;

            if (PerspectARConfig.bDynamicAdjustment && !PerspectARConfig.bCalib)
            {
                Vector3 userPose = transform.root.gameObject.transform.InverseTransformPoint(vecUsersWorldPose);
     
                // Calculate the direction from the camera to the main object in world space
                Vector3 cameraToMainObjectDirection = (transform.root.gameObject.transform.position - vecUsersWorldPose).normalized;

                float dotProduct = Vector3.Dot(Camera.main.transform.forward, cameraToMainObjectDirection);

              
                if (dotProduct > 0)
                {
                    Vector3 localPoseCanvas = myCanvas.transform.InverseTransformPoint(vecUsersWorldPose);
                    float maxX = PerspectARConfig.iHalfWidth - width;

                    bool withinBounds = localPoseCanvas.x >= -maxX && localPoseCanvas.x <= maxX;
                    bool withinDistance = Mathf.Abs(userPose.z) >= PerspectARConfig.minDistance && Mathf.Abs(userPose.z) <= PerspectARConfig.maxDistance;


                    if (withinBounds)
                    {
                        float newX = Mathf.Clamp(localPoseCanvas.x, -maxX, maxX);
                        Vector3 temp = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
                        transform.localPosition = temp;

                        if (withinDistance)
                        {
                            float distanceFromObject = Mathf.Abs(userPose.z);
                            float mappedValue = Mathf.InverseLerp(PerspectARConfig.minDistance, PerspectARConfig.maxDistance, distanceFromObject);
                            float scaleX = Mathf.Lerp(PerspectARConfig.fMinScale, 1.0f, mappedValue);
                            transform.localScale = new Vector3(scaleX, 1f, 1f);
                        }
                    }
                    else
                    {
                        // If the object reaches the boundaries, adjust its position based on scaling
                        float distanceFromObject = Mathf.Abs(userPose.z);
                        float mappedValue = Mathf.InverseLerp(PerspectARConfig.minDistance, PerspectARConfig.maxDistance, distanceFromObject);
                        float scaleX = Mathf.Lerp(PerspectARConfig.fMinScale, 1.0f, mappedValue);

                        // Adjust the position based on the sign of X
                        float adjustedX = maxX * Mathf.Sign(localPoseCanvas.x);
                        Vector3 adjustedPosition = new Vector3(adjustedX, transform.localPosition.y, transform.localPosition.z);
                        transform.localPosition = adjustedPosition;
                        transform.localScale = new Vector3(scaleX, 1f, 1f);
                    }
                }
                else
                {
                    Vector3 localPoseCanvas = myCanvas.transform.InverseTransformPoint(vecUsersWorldPose);
                    float maxX = PerspectARConfig.iHalfWidth - width; // Adjust the maximum X position

                    bool withinBounds = localPoseCanvas.x >= -maxX && localPoseCanvas.x <= maxX;

                    if (withinBounds)
                    {
                        float newX = Mathf.Clamp(localPoseCanvas.x, -maxX, maxX);
                        Vector3 temp = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
                        transform.localPosition = temp;
                    }
                }
            }
            else
            {
                if (PerspectARConfig.bCalib)
                {
                    Vector3 localPoseCanvas = myCanvas.transform.InverseTransformPoint(vecUsersWorldPose);
                    float maxX = PerspectARConfig.iHalfWidth - width;

                    bool withinBounds = localPoseCanvas.x >= -maxX && localPoseCanvas.x <= maxX;

                    if (withinBounds)
                    {
                        float newX = Mathf.Clamp(localPoseCanvas.x, -maxX, maxX);
                        Vector3 temp = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
                        transform.localPosition = temp;
                    }
                }
                else
                {
                    transform.localPosition = Vector3.zero;
                    transform.localScale = Vector3.one;
                }
            }
            }
        }
}