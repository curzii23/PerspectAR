#define CURVEDUI_PRESENT //If you're an asset creator and want to see if CurvedUI is imported, just use "#if CURVEDUI_PRESENT [your code] #endif"
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.VisualScripting;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.UIElements;
using static UnityEngine.ParticleSystem;

#if CURVEDUI_TMP || TMP_PRESENT
using TMPro;
#endif

namespace CurvedUI
{
    [AddComponentMenu("CurvedUI/CurvedUISettings")]
    [RequireComponent(typeof(Canvas))]
    public class CurvedUISettings : MonoBehaviour
    {
        //Global settings
        #region SETTINGS

        //Global settings
        [SerializeField]
        CurvedUIShape shape;

        [SerializeField]
        float quality = 2f;
        [SerializeField]
        bool interactable = true;
        [SerializeField]
        bool blocksRaycasts = true;
        [SerializeField]

        bool forceUseBoxCollider = false;
        //Cyllinder settings
        [SerializeField]
        int angle = 90;
        [SerializeField]
        bool preserveAspect = true;

        //Sphere settings
        [SerializeField]
        int vertAngle = 90;

        //internal system settings
        int baseCircleSegments = 32;


        //stored variables
        Vector2 savedRectSize;
        float savedRadius;
        Canvas myCanvas;
        RectTransform m_rectTransform;

        

        #endregion

        void Awake()
        {
            // If this canvas is on Default layer, switch it to UI layer..
            // this is to make sure that when using raycasting to detect interactions, 
            // nothing will interfere with it.
            if (gameObject.layer == 0) this.gameObject.layer = 5;

            //save initial variables
            savedRectSize = RectTransform.rect.size;
        }

        
        void Start()
        {

            
            if (Application.isPlaying)
            {
                // lets get rid of any raycasters and add our custom one
                // It will be responsible for handling interactions.
                BaseRaycaster[] raycasters = GetComponents<BaseRaycaster>();
                foreach (BaseRaycaster caster in raycasters)
                {
                    if (!(caster is CurvedUIRaycaster))
                        caster.enabled = true;
                }
                this.gameObject.AddComponentIfMissing<CurvedUIRaycaster>();

                //find if there are any child canvases that may break interactions
                Canvas[] canvases = GetComponentsInChildren<Canvas>();
                foreach (Canvas cnv in canvases)
                {
                    if (cnv.gameObject != this.gameObject)
                    {
                        Transform trans = cnv.transform;
                        string hierarchyName = trans.name;

                        while (trans.parent != null)
                        {
                            hierarchyName = trans.parent.name + "/" + hierarchyName;
                            trans = trans.parent;
                        }
                        Debug.LogWarning("CURVEDUI: Interactions on nested canvases are not supported. You won't be able to interact with any child object of [" + hierarchyName + "]", cnv.gameObject);
                    }
                }
            }

            //find needed references
            if (myCanvas == null)
                myCanvas = GetComponent<Canvas>();

            savedRadius = GetCyllinderRadiusInCanvasSpace();
        }


        void Update()
        {

            GetComponent<CurvedUIRaycaster>().RebuildCollider();

            if (Camera.main && !PerspectARConfig.bCalib)
            {
                if(PerspectARConfig.bDynamicAdjustment)
                {

                    Vector3 userPose = transform.root.gameObject.transform.InverseTransformPoint(Camera.main.transform.position);

                    // Calculate the direction vector from the camera to the reference object
                    Vector3 cameraToReference = (transform.root.gameObject.transform.position - Camera.main.transform.position).normalized;

                    // Calculate the dot product between the user's direction vector and the camera-to-reference direction
                    float dotProduct = Vector3.Dot(Camera.main.transform.forward, cameraToReference);
        
                    if (dotProduct > 0)
                    {
                        float mappedValue = Mathf.InverseLerp(PerspectARConfig.minDistance, PerspectARConfig.maxDistance, Mathf.Abs(userPose.z));
                        
                        angle = (int)Mathf.Lerp(PerspectARConfig.pCompenAngle, 1.0f, mappedValue);
                        Angle = angle;
                        SetUIAngle(angle);
                    }

                }
                else
                {
                    angle = 1;
                    Angle = angle;
                    SetUIAngle(angle);
                }
                
            }

            if (RectTransform.rect.size != savedRectSize)
            {
                savedRectSize = RectTransform.rect.size;
                SetUIAngle(angle);
            }

            PerspectARConfig.izAngle = angle;
        }

        #region PRIVATE

        /// <summary>
        /// Changes the horizontal angle of the canvas.
        /// </summary>
        /// <param name="newAngle"></param>
        void SetUIAngle(int newAngle)
        {

            if (myCanvas == null)
                myCanvas = GetComponent<Canvas>();

            //temp fix to make interactions with angle 0 possible
            if (newAngle == 0) newAngle = 1;

            angle = newAngle;

            savedRadius = GetCyllinderRadiusInCanvasSpace();

            foreach (Graphic graph in GetComponentsInChildren<Graphic>())
                graph.SetAllDirty();

        }


        #endregion

        RectTransform RectTransform
        {
            get
            {
                if (m_rectTransform == null) m_rectTransform = transform as RectTransform;
                return m_rectTransform;
            }
        }

        public void AddEffectToChildren()
        {
            //Debug.Log("AddEffectToChildren");

            foreach (UnityEngine.UI.Graphic graph in GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            {
                if (graph.GetComponent<CurvedUIVertexEffect>() == null)
                {
                    graph.gameObject.AddComponent<CurvedUIVertexEffect>();
                    graph.SetAllDirty();
                }
            }

            //TextMeshPro experimental support. Go to CurvedUITMP.cs to learn how to enable it.
#if CURVEDUI_TMP || TMP_PRESENT
		    foreach(TextMeshProUGUI tmp in GetComponentsInChildren<TextMeshProUGUI>(true)){
			    if(tmp.GetComponent<CurvedUITMP>() == null){
				    tmp.gameObject.AddComponent<CurvedUITMP>();
				    tmp.SetAllDirty();
			    }
		    }

            foreach (TMP_InputField tmp in GetComponentsInChildren<TMP_InputField>(true))
            {
                tmp.AddComponentIfMissing<CurvedUITMPInputFieldCaret>();
            }
#endif
        }

        public float GetCyllinderRadiusInCanvasSpace()
        {
            float ret;
            if (PreserveAspect)
            {
                ret = (RectTransform.rect.size.x / ((2 * Mathf.PI) * (angle / 360.0f)));
            }
            else
                ret = (RectTransform.rect.size.x * 0.5f) / Mathf.Sin(Mathf.Clamp(angle, -180.0f, 180.0f) * 0.5f * Mathf.Deg2Rad);


            return angle == 0 ? 0 : ret;
        }


        /// <summary>
        /// Tells you how big UI quads can get before they should be tesselate to look good on current canvas settings.
        /// Used by CurvedUIVertexEffect to determine how many quads need to be created for each graphic.
        /// </summary>
        public Vector2 GetTesslationSize(Vector2 ret, bool modifiedByQuality = true)
        {
            //Vector2 ret = RectTransform.rect.size;

            if (Angle != 0 || (!PreserveAspect && vertAngle != 0))
            {
                switch (shape)
                {
                    case CurvedUIShape.CYLINDER: ret /= GetSegmentsByAngle(angle); break;
                }

            }

            return ret / (modifiedByQuality ? Mathf.Clamp(Quality, 0.01f, 10.0f) : 1);
        }

        float GetSegmentsByAngle(float angle)
        {
         
            if (angle.Abs() <= 1)
                return 1;
            else if (angle.Abs() < 90)//proportionaly twice as many segments for small angle canvases
                return baseCircleSegments * (Mathf.Abs(angle).Remap(0, 90, 0.01f, 0.5f));
            else
                return baseCircleSegments * (Mathf.Abs(angle).Remap(90, 360.0f, 0.5f, 1));

        }

        /// <summary>
        /// Tells you how many segmetens should the entire 360 deg. cyllinder or sphere consist of.
        /// Used by CurvedUIVertexEffect
        /// </summary>
        public int BaseCircleSegments
        {
            get { return baseCircleSegments; }
        }

        /// <summary>
        /// The measure of the arc of the Canvas.
        /// </summary>
        public int Angle
        {
            get { return angle; }
            set
            {
                if (angle != value)
                    SetUIAngle(value);
            }
        }

        /// <summary>
        /// Multiplier used to deremine how many segments a base curve of a shape has. 
        /// Default 1. Lower values greatly increase performance. Higher values give you sharper curve.
        /// </summary>
        public float Quality
        {
            get { return quality; }
            set
            {
                if (quality != value)
                {
                    quality = value;
                    SetUIAngle(angle);
                }
            }
        }

        /// <summary>
        /// Current Shape of the canvas
        /// </summary>
        public CurvedUIShape Shape
        {
            get { return shape; }
            set
            {
                if (shape != value)
                {
                    shape = value;
                    SetUIAngle(angle);
                }
            }
        }

        /// <summary>
        /// Vertical angle of the canvas. Used in sphere shape and ring shape.
        /// </summary>
        public int VerticalAngle
        {
            get { return vertAngle; }
            set
            {
                if (vertAngle != value)
                {
                    vertAngle = value;
                    SetUIAngle(angle);
                }
            }
        }

        /// <summary>
        /// Calculated radius of the curved canvas. 
        /// </summary>
        public float SavedRadius
        {
            get
            {
                if (savedRadius == 0)
                    savedRadius = GetCyllinderRadiusInCanvasSpace();

                return savedRadius;
            }
        }

        /// <summary>
        /// If enabled, CurvedUI will try to preserve aspect ratio of original canvas.
        /// </summary>
        public bool PreserveAspect
        {
            get { return preserveAspect; }
            set
            {
                if (preserveAspect != value)
                {
                    preserveAspect = value;
                    SetUIAngle(angle);
                }
            }
        }

        public bool BlocksRaycasts
        {
            get { return blocksRaycasts; }
            set
            {
                if (blocksRaycasts != value)
                {
                    blocksRaycasts = value;

                    //tell raycaster to update its collider now that angle has changed.
                    if (Application.isPlaying && GetComponent<CurvedUIRaycaster>() != null)
                        GetComponent<CurvedUIRaycaster>().RebuildCollider();
                }
            }
        }

        /// <summary>
        /// Can the canvas be interacted with?
        /// </summary>
        public bool Interactable
        {
            get { return interactable; }
            set { interactable = value; }
        }

        /// <summary>
        /// Should The collider for this canvas be created using more expensive box colliders?
        /// DEfault false.
        /// </summary>
        public bool ForceUseBoxCollider
        {
            get { return forceUseBoxCollider; }
            set { forceUseBoxCollider = value; }
        }

        #region ENUMS
        public enum CurvedUIShape
        {
            CYLINDER = 0,
            RING = 1,
            SPHERE = 2,
            CYLINDER_VERTICAL = 3,
        }
        #endregion
    }
}