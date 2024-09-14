using TMPro;
using UnityEngine;
using Vuplex.WebView;

public class Command : MonoBehaviour
{
    public GameObject objUserInterface;
    public GameObject objQRCode;
    public GameObject objCurvedARSCreen;
    public GameObject objLargeDisplay;

    public TextMeshProUGUI textMeshPro;
    public TextMeshProUGUI textMeshProStatus;
    public CanvasWebViewPrefab webViewPrefab;


    bool bInitialPositionSet = false;

    void Update()
    {
        textMeshProStatus.text = transform.position.ToString() + ":" + transform.rotation.ToString();

        if (bInitialPositionSet)
        {
            
            Vector3 pointInLocalSpace = objLargeDisplay.transform.InverseTransformPoint(PerspectARConfig.fTransparentWindowDepth);

            // Modify the z value to match the large display local space's z value
            pointInLocalSpace.z = 0;
            
            // Transform point back to world space
            Vector3 pointOnLargeDisplay = objLargeDisplay.transform.TransformPoint(pointInLocalSpace);

            // Calculate the direction from largedisplayReference to the fTransparentWindowDepth point
            Vector3 direction = pointOnLargeDisplay - PerspectARConfig.fTransparentWindowDepth;

            // Project this direction onto the large display's forward axis to constrain it along the Z-axis of the large display
            Vector3 constrainedDirection = objLargeDisplay.transform.forward * Vector3.Dot(direction, objLargeDisplay.transform.forward);

            // Calculate the distance to move
            float distance = constrainedDirection.magnitude;

            // Define a small tolerance to avoid shaking
            float tolerance = 0.01f;

            if (distance > tolerance)
            {
                // Smoothly move the virtual screen along the constrained direction vector
                float speed = 5.0f;
                objCurvedARSCreen.transform.position = Vector3.Lerp(objCurvedARSCreen.transform.position, objCurvedARSCreen.transform.position + constrainedDirection.normalized * distance, speed * Time.deltaTime);
            }


        }

    }

    public void ClearCacheReload()
    {
        if (webViewPrefab.WebView.IsInitialized)
        {
            Web.ClearAllData();
            webViewPrefab.WebView.Reload();
        }

    }

    public void HideVirtualObjects()
    {
        objQRCode.SetActive(false);
        objUserInterface.SetActive(false);
    }

    public void ShowVirtualObjects()
    {

        objQRCode.SetActive(true);
        objUserInterface.SetActive(true);

    }

    public void MoveUP()
    {
        if (transform != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.y += 0.01f;
            transform.position = newPosition;
        }
    }

    public void MoveDown()
    {
        if (transform != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.y -= PerspectARConfig.positionStep;
            transform.position = newPosition;
        }
    }

    public void MoveLeft()
    {
        if (transform != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.x -= PerspectARConfig.positionStep;
            transform.position = newPosition;
        }
    }

    public void MoveRight()
    {
        if (transform != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.x += PerspectARConfig.positionStep;
            transform.position = newPosition;
        }
    }

    public void DepthMinus()
    {
        if (transform != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.z -= PerspectARConfig.positionStep;
            transform.position = newPosition;
        }
    }

    public void DepthPlus()
    {
        if (transform != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.z += PerspectARConfig.positionStep;
            transform.position = newPosition;
        }
    } 

    
    public void RotateUp()
    {
        if (transform != null)
        {
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.x += PerspectARConfig.rotationStep;
            transform.rotation = Quaternion.Euler(newRotation);
        }
    }

    public void RotateDown()
    {
        if (transform != null)
        {
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.x -= PerspectARConfig.rotationStep;
            transform.rotation = Quaternion.Euler(newRotation);
        }
    }

    public void RotateLeft()
    {
        if (transform != null)
        {
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.y -= PerspectARConfig.rotationStep;
            transform.rotation = Quaternion.Euler(newRotation);
        }
    }

    public void RotateRight()
    {
        if (transform != null)
        {
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.y += PerspectARConfig.rotationStep;
            transform.rotation = Quaternion.Euler(newRotation);
        }
    }

    public void RotateClockwise()
    {
        if (transform != null)
        {
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.z += PerspectARConfig.rotationStep;
            transform.rotation = Quaternion.Euler(newRotation);
        }
    }

    public void RotateCounterClockwise()
    {
        if (transform != null)
        {
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.z -= PerspectARConfig.rotationStep;
            transform.rotation = Quaternion.Euler(newRotation);
        }
    }


}