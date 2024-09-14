using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using Selectable = UnityEngine.UI.Selectable;
using UnityEngine.Windows;
using Unity.VisualScripting;
//using UnityEditor.Advertisements;

namespace CurvedUI
{
    
#if CURVEDUI_GOOGLEVR
    public class CurvedUIRaycaster : GvrPointerGraphicRaycaster
#else
    public class CurvedUIRaycaster : GraphicRaycaster
#endif
    {

        Canvas myCanvas;
        CurvedUISettings mySettings;
        GameObject colliderContainer;
        public static Vector3 cyllinderMidPoint;

        #region LIFECYCLE
        protected override void Awake()
        {
            base.Awake();
            mySettings = GetComponentInParent<CurvedUISettings>();


            if (mySettings == null) return;
            myCanvas = mySettings.GetComponent<Canvas>();

            cyllinderMidPoint = new Vector3(0, 0, -mySettings.GetCyllinderRadiusInCanvasSpace());

            //this must be set to false to make sure proper interactions.
            //Otherwise, Unity may ignore Selectables on edges of heavily curved canvas.
            ignoreReversedGraphics = true;
        }

        protected override void Start()
        {
            if (mySettings == null) return;

            CreateCollider();

        }

        protected virtual void Update()
        {
         
        }
        #endregion

        #region COLLIDER MANAGEMENT

        /// <summary>
        /// Creates a mesh collider for curved canvas based on current angle and curve segments.
        /// </summary>
        /// <returns>The collider.</returns>
        protected void CreateCollider()
        {
            //remove all colliders on this object
            List<Collider> Cols = new List<Collider>();
            Cols.AddRange(this.GetComponents<Collider>());
            for (int i = 0; i < Cols.Count; i++)
            {
                Destroy(Cols[i]);
            }

            if (!mySettings.BlocksRaycasts) return; //null;

            //create a collider based on mapping type
            switch (mySettings.Shape)
            {

                case CurvedUISettings.CurvedUIShape.CYLINDER:
                    {

                        //creating a convex (lower performance - many parts) collider for when we have a rigidbody attached
                        if (mySettings.ForceUseBoxCollider || GetComponent<Rigidbody>() != null || GetComponentInParent<Rigidbody>() != null)
                        {
                            if (colliderContainer != null)
                                GameObject.Destroy(colliderContainer);
                            colliderContainer = CreateConvexCyllinderCollider();
                        }
                        else // create a faster single mesh collier when possible
                        {
  
                            SetupMeshColliderUsingMesh(CreateCyllinderColliderMesh());
                        }
                        return;
                    }
                default:
                    {
                        return;
                    }
            }

        }

        //bool overrideEventData = true;

        public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }

        bool showDebug = false;


        /// <summary>
        /// Adds neccessary components and fills them with given mesh data.
        /// </summary>
        /// <param name="meshie"></param>
        void SetupMeshColliderUsingMesh(Mesh meshie)
        {
            MeshFilter mf = this.AddComponentIfMissing<MeshFilter>();
            MeshCollider mc = this.gameObject.AddComponent<MeshCollider>();
            mf.mesh = meshie;
            mc.sharedMesh = meshie;
        }


        GameObject CreateConvexCyllinderCollider(bool vertical = false)
        {

            GameObject go = new GameObject("_CurvedUIColliders");
            go.layer = this.gameObject.layer;
            go.transform.SetParent(this.transform);
            go.transform.ResetTransform();

            Mesh meshie = new Mesh();

            Vector3[] Vertices = new Vector3[4];
            (myCanvas.transform as RectTransform).GetWorldCorners(Vertices);
            meshie.vertices = Vertices;

            //rearrange them to be in an easy to interpolate order and convert to canvas local spce
            if (vertical)
            {
                Vertices[0] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[1]);
                Vertices[1] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[2]);
                Vertices[2] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[0]);
                Vertices[3] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[3]);
            }
            else
            {
                Vertices[0] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[1]);
                Vertices[1] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[0]);
                Vertices[2] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[2]);
                Vertices[3] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[3]);
            }

            meshie.vertices = Vertices;

            //create a new array of vertices, subdivided as needed
            List<Vector3> verts = new List<Vector3>();
            int vertsCount = Mathf.Max(8, Mathf.RoundToInt(mySettings.BaseCircleSegments * Mathf.Abs(mySettings.Angle) / 360.0f));

            for (int i = 0; i < vertsCount; i++)
            {
                verts.Add(Vector3.Lerp(meshie.vertices[0], meshie.vertices[2], (i * 1.0f) / (vertsCount - 1)));
            }

            //curve the verts in canvas local space
            if (mySettings.Angle != 0)
            {
                Rect canvasRect = myCanvas.GetComponent<RectTransform>().rect;
                float radius = mySettings.GetCyllinderRadiusInCanvasSpace();

                for (int i = 0; i < verts.Count; i++)
                {

                    Vector3 newpos = verts[i];
                    if (vertical)
                    {
                        float theta = (verts[i].y / canvasRect.size.y) * mySettings.Angle * Mathf.Deg2Rad;
                        newpos.y = Mathf.Sin(theta) * radius;
                        newpos.z += Mathf.Cos(theta) * radius - radius;
                        verts[i] = newpos;
                    }
                    else
                    {
                        float theta = (verts[i].x / canvasRect.size.x) * mySettings.Angle * Mathf.Deg2Rad;
                        newpos.x = Mathf.Sin(theta) * radius;
                        newpos.z += Mathf.Cos(theta) * radius - radius;
                        verts[i] = newpos;
                    }
                }
            }


            //create our box colliders and arrange them in a nice cyllinder
            float boxDepth = mySettings.GetTesslationSize(
                this.transform.GetComponent<RectTransform>().rect.size 
                * this.transform.GetComponent<RectTransform>().localScale, 
                false).x / 10;

            for (int i = 0; i < verts.Count - 1; i++)
            {
                GameObject newBox = new GameObject("Box collider");
                newBox.layer = this.gameObject.layer;
                newBox.transform.SetParent(go.transform);
                newBox.transform.ResetTransform();
                newBox.AddComponent<BoxCollider>();

                if (vertical)
                {
                    newBox.transform.localPosition = new Vector3(0, (verts[i + 1].y + verts[i].y) * 0.5f, (verts[i + 1].z + verts[i].z) * 0.5f);
                    newBox.transform.localScale = new Vector3(boxDepth, Vector3.Distance(Vertices[0], Vertices[1]), Vector3.Distance(verts[i + 1], verts[i]));
                    newBox.transform.localRotation = Quaternion.LookRotation((verts[i + 1] - verts[i]), Vertices[0] - Vertices[1]);
                }
                else
                {
                    newBox.transform.localPosition = new Vector3((verts[i + 1].x + verts[i].x) * 0.5f, 0, (verts[i + 1].z + verts[i].z) * 0.5f);
                    newBox.transform.localScale = new Vector3(boxDepth, Vector3.Distance(Vertices[0], Vertices[1]), Vector3.Distance(verts[i + 1], verts[i]));
                    newBox.transform.localRotation = Quaternion.LookRotation((verts[i + 1] - verts[i]), Vertices[0] - Vertices[1]);
                }

            }

            return go;

        }

       
        Vector3 userPose = new Vector3();
        Vector3 puserPose = new Vector3();

        Mesh CreateCyllinderColliderMesh(bool vertical = false)
        {

            Mesh meshie = new Mesh();

            Vector3[] Vertices = new Vector3[4];
            (myCanvas.transform as RectTransform).GetWorldCorners(Vertices);
            meshie.vertices = Vertices;

            //rearrange them to be in an easy to interpolate order and convert to canvas local spce
            Vertices[0] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[1]);
            Vertices[1] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[0]);
            Vertices[2] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[2]);
            Vertices[3] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[3]);


            meshie.vertices = Vertices;

            //create a new array of vertices, subdivided as needed
            List<Vector3> verts = new List<Vector3>();
            int vertsCount = Mathf.Max(8, Mathf.RoundToInt(mySettings.BaseCircleSegments * Mathf.Abs(mySettings.Angle) / 360.0f));


            for (int i = 0; i < vertsCount; i++)
            {
                verts.Add(Vector3.Lerp(meshie.vertices[0], meshie.vertices[2], (i * 1.0f) / (vertsCount - 1)));
                verts.Add(Vector3.Lerp(meshie.vertices[1], meshie.vertices[3], (i * 1.0f) / (vertsCount - 1)));
            }

            
            float theta = 0.0f;
            float margine = 0.0f;


            //curve the verts in canvas local space
            if (mySettings.Angle != 0)
            {
                float radius = mySettings.GetCyllinderRadiusInCanvasSpace();

                for (int i = 0; i < verts.Count; i++)
                {

                    Vector3 newpos = verts[i];

                    if (Camera.main != null)
                    {

                        userPose = myCanvas.transform.InverseTransformPoint(Camera.main.transform.position);
 
                        if (userPose.x >= -PerspectARConfig.iHalfWidth
                                && userPose.x <= PerspectARConfig.iHalfWidth)
                        {
                            puserPose = userPose;
                        }
                        else
                        {
                            userPose.x = puserPose.x;
                        }

                        float temp1 = (verts[i].x) - (userPose.x);
                        theta = (temp1 / PerspectARConfig.iWidth) * mySettings.Angle * Mathf.Deg2Rad;
                        margine = userPose.x;

                    }
                    else
                    {
                        theta = (verts[i].x / (PerspectARConfig.iWidth)) * mySettings.Angle * Mathf.Deg2Rad;
                    }

                    radius += newpos.z; // change the radius depending on how far the element is moved in z direction from canvas plane
                    newpos.x = Mathf.Sin(theta) * radius + margine;
                    newpos.z += Mathf.Cos(theta) * radius - radius;

                    verts[i] = newpos;
                }
            }

            meshie.vertices = verts.ToArray();

            //create triangles drom verts
            List<int> tris = new List<int>();
            for (int i = 0; i < verts.Count / 2 - 1; i++)
            {

                //forward tris
                tris.Add(i * 2 + 2);
                tris.Add(i * 2 + 1);
                tris.Add(i * 2 + 0);

                tris.Add(i * 2 + 2);
                tris.Add(i * 2 + 3);
                tris.Add(i * 2 + 1);

            }

            meshie.triangles = tris.ToArray();

            return meshie;

        }


        #endregion

        #region TESSELATION
        void ModifyQuad(List<Vector3> verts, int vertexIndex, Vector2 requiredSize)
        {
            
            // Read the existing quad vertices
            List <Vector3> quad = new List<Vector3>();
            for (int i = 0; i < 4; i++)
                quad.Add(verts[vertexIndex + i]);

            // horizotal and vertical directions of a quad. We're going to tesselate parallel to these.
            Vector3 horizontalDir = quad[2] - quad[1];
            Vector3 verticalDir = quad[1] - quad[0];

            // Find how many quads we need to create
            int horizontalQuads = Mathf.CeilToInt(horizontalDir.magnitude * (1.0f / Mathf.Max(1.0f, requiredSize.x)));
            int verticalQuads = Mathf.CeilToInt(verticalDir.magnitude * (1.0f / Mathf.Max(1.0f, requiredSize.y)));

            // Create the quads!
            float yStart = 0.0f;
            for (int y = 0; y < verticalQuads; ++y)
            {

                float yEnd = (y + 1.0f) / verticalQuads;
                float xStart = 0.0f;

                for (int x = 0; x < horizontalQuads; ++x)
                {
                    float xEnd = (x + 1.0f) / horizontalQuads;

                    //Add new quads to list
                    verts.Add(TesselateQuad(quad, xStart, yStart));
                    verts.Add(TesselateQuad(quad, xStart, yEnd));
                    verts.Add(TesselateQuad(quad, xEnd, yEnd));
                    verts.Add(TesselateQuad(quad, xEnd, yStart));

                    //begin the next quad where we ened this one
                    xStart = xEnd;
                }
                //begin the next row where we ended this one
                yStart = yEnd;
            }
        }


        Vector3 TesselateQuad(List<Vector3> quad, float x, float y)
        {

            Vector3 ret = Vector3.zero;

            //1. calculate weighting factors
            List<float> weights = new List<float>(){
                (1-x) * (1-y),
                (1-x) * y,
                x * y,
                x * (1-y),
            };

            //2. interpolate pos using weighting factors
            for (int i = 0; i < 4; i++)
                ret += quad[i] * weights[i];
            return ret;
        }

        #endregion

        public void RebuildCollider()
        {
            cyllinderMidPoint = new Vector3(0, 0, -mySettings.GetCyllinderRadiusInCanvasSpace());
            CreateCollider();
        }

    }

}