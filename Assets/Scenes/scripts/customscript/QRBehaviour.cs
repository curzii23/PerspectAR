using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.QR;
using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using UnityEngine.Timeline;
#if WINDOWS_UWP
using Windows.Perception.Spatial.Preview;
using Windows.Perception.Spatial;
#endif


public class QRBehaviour : MonoBehaviour
{
    #region Variables
    public bool IsTrackerRunning { get; private set; }
    public bool IsSupported { get; private set; }
    public event EventHandler<bool> QRCodesTrackingStateChanged;
    public event EventHandler<QRCodeEventArgs<QRCode>> QRCodeAdded;
    public event EventHandler<QRCodeEventArgs<QRCode>> QRCodeUpdated;
    public event EventHandler<QRCodeEventArgs<QRCode>> QRCodeRemoved;
    private QRCodeWatcher qrTracker;
    //private bool ignoreReset = false;
    private bool enumerationComplete = false;
    private bool capabilityInitialized = false;
    private QRCodeWatcherAccessStatus accessStatus;
    private System.Threading.Tasks.Task<QRCodeWatcherAccessStatus> capabilityTask;
#if WINDOWS_UWP
    private SpatialCoordinateSystem rootSpatialCoordinateSystem;
    private Queue<QRCodeInformation> spatialCoordinateSystems = new Queue<QRCodeInformation>();
#endif
    #endregion
    #region Unity Marker Object
    
    public TextMeshProUGUI textMeshPro;
    public AudioSource audioSource;

    private float width;

    #endregion
    async void Start()
    {
        
        IsSupported = QRCodeWatcher.IsSupported();
        capabilityTask = QRCodeWatcher.RequestAccessAsync();
        accessStatus = await capabilityTask;
        capabilityInitialized = true;
        if (!IsSupported)
        {
            // Do something to notify the user it isn't supported.
        }
        // Get the root spatial coordinate system.
#if WINDOWS_UWP
        rootSpatialCoordinateSystem = SpatialLocator.GetDefault().CreateStationaryFrameOfReferenceAtCurrentLocation().CoordinateSystem;
        //rootSpatialCoordinateSystem = (SpatialCoordinateSystem)System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr());
#endif
    }
    void Update()
    {

        // Wait until QR tracking is ready before we start looking for QR codes.
        if (qrTracker == null && capabilityInitialized && IsSupported && accessStatus == QRCodeWatcherAccessStatus.Allowed)
            SetupQRTracking();
        //UnityEngine.XR.WindowsMR.WindowsMREnvironment.OriginSpatialCoordinateSystem;
#if WINDOWS_UWP
        lock (spatialCoordinateSystems)
        {
            while (spatialCoordinateSystems.Count > 0)
            {
                QRCodeInformation qrCodeInformation = spatialCoordinateSystems.Dequeue();
                // Do what you want with the QR code information. Examples below.
                // Get the text from the QR code.
                string qrCodeText = qrCodeInformation.Data;
                //textMeshPro.text = qrCodeText;
                // Get the relative transform from the unity origin
                // THIS IS OUTDATED.
                System.Numerics.Matrix4x4? relativePose = qrCodeInformation.QRSpatialCoordinateSystem.TryGetTransformTo(rootSpatialCoordinateSystem);
                if (relativePose != null)
                {
                    System.Numerics.Vector3 scale;
                    System.Numerics.Quaternion rotation1;
                    System.Numerics.Vector3 translation1;
                    System.Numerics.Matrix4x4 newMatrix = relativePose.Value;
                    // Platform coordinates are all right handed and unity uses left handed matrices. so we convert the matrix
                    // from rhs-rhs to lhs-lhs
                    // Convert from right to left coordinate system
                    newMatrix.M13 = -newMatrix.M13;
                    newMatrix.M23 = -newMatrix.M23;
                    newMatrix.M43 = -newMatrix.M43;
                    newMatrix.M31 = -newMatrix.M31;
                    newMatrix.M32 = -newMatrix.M32;
                    newMatrix.M34 = -newMatrix.M34;
                    System.Numerics.Matrix4x4.Decompose(newMatrix, out scale, out rotation1, out translation1);
                    Vector3 translation = new Vector3(translation1.X, translation1.Y, translation1.Z);
                    Quaternion rotation = new Quaternion(rotation1.X, rotation1.Y, rotation1.Z, rotation1.W);
                    // Pose is the position and rotation of the QR code.
                    Pose pose = new Pose(translation, rotation);
                    // THIS IS THE ACTUAL WAY TO DO IT!
                    QRPosition(qrCodeInformation.SpatialGraphNodeID);
                }
                else
                    spatialCoordinateSystems.Enqueue(qrCodeInformation);// Re-queue it to process again.
            }
        }
#endif
    }

    private bool isSoundPlaying = false;

    void LateUpdate()
    {
        // Check if the sound has finished playing
        if (isSoundPlaying && !audioSource.isPlaying)
        {
            // Reset the flag indicating that the sound is no longer playing
            isSoundPlaying = false;
        }
    }

    #region QR positioning
    private void QRPosition(Guid SpatialGraphNodeID)
    {

        SpatialGraphNode node = SpatialGraphNode.FromStaticNodeId(SpatialGraphNodeID);
       
        Pose pose;
        if (node != null && node.TryLocate(FrameTime.OnUpdate, out pose))
        {
            if (CameraCache.Main.transform.parent != null)
            {
                pose = pose.GetTransformedBy(CameraCache.Main.transform.parent);
               
            }

            pose.rotation = pose.rotation * Quaternion.Euler(180f, 0f, 0f);
            transform.SetPositionAndRotation(pose.position, pose.rotation);
            GameObject.Find("MixedRealitySceneContent").transform.SetPositionAndRotation(pose.position, pose.rotation);
            textMeshPro.text = "QR Found";

            // Check if AudioSource component is assigned
            if (audioSource != null && !audioSource.isPlaying)
            {
                // Play the sound
                audioSource.Play();
                isSoundPlaying = true;

            }


        }
    }
    #endregion
    #region QR tracking Setup
    private void SetupQRTracking()
    {
        try
        {
            qrTracker = new QRCodeWatcher();
            IsTrackerRunning = false;
            qrTracker.Added += QRCodeWatcher_Added;
            qrTracker.Updated += QRCodeWatcher_Updated;
            qrTracker.Removed += QRCodeWatcher_Removed;
            qrTracker.EnumerationCompleted += QRCodeWatcher_EnumerationCompleted;
            StartQRTracking();
        }
        catch (Exception ex)
        {
            Debug.Log($"QRCodesManager exception setting up the tracker {ex.ToString()}");
        }
    }
    // Made this public for external scripts to be able to start QR tracking.
    public void StartQRTracking()
    {
        if (qrTracker != null && !IsTrackerRunning)
        {
            try
            {
                enumerationComplete = false;
                qrTracker.Start();
                IsTrackerRunning = true;
                QRCodesTrackingStateChanged?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                Debug.Log($"QRCodesManager exception starting QRCodeWatcher {ex.ToString()}");
            }
        }
    }
    #endregion
    #region Events
    private void QRCodeWatcher_Removed(object sender, QRCodeRemovedEventArgs args)
    {
        try
        {
            QRCodeRemoved?.Invoke(this, QRCodeEventArgs.Create(args.Code));
        }
        catch (Exception ex)
        {
            Debug.Log($"QRCodeWatcher_Removed {ex.Message}.");
        }
    }
    private void QRCodeWatcher_Updated(object sender, QRCodeUpdatedEventArgs args)
    {
        try
        {
#if WINDOWS_UWP
            lock (spatialCoordinateSystems)
                spatialCoordinateSystems.Enqueue(new QRCodeInformation(
                    args.Code.Data,
                    SpatialGraphInteropPreview.CreateCoordinateSystemForNode(args.Code.SpatialGraphNodeId),
                    args.Code.PhysicalSideLength,
                    args.Code.SpatialGraphNodeId));
#endif
            QRCodeUpdated?.Invoke(this, QRCodeEventArgs.Create(args.Code));
        }
        catch (Exception ex)
        {
            Debug.Log($"\nQRCodeWatcher_Updated {ex.Message}.");
        }
    }
    private void QRCodeWatcher_Added(object sender, QRCodeAddedEventArgs args)
    {
        try
        {
            if (enumerationComplete)
            {
#if WINDOWS_UWP
                lock (spatialCoordinateSystems)
                    spatialCoordinateSystems.Enqueue(new QRCodeInformation(args.Code.Data, SpatialGraphInteropPreview.CreateCoordinateSystemForNode(args.Code.SpatialGraphNodeId), args.Code.PhysicalSideLength, args.Code.SpatialGraphNodeId));
#endif
            }
            QRCodeAdded?.Invoke(this, QRCodeEventArgs.Create(args.Code));
        }
        catch (Exception ex)
        {
            Debug.Log($"\nQRCodeWatcher_Added {ex.Message}.");
        }
    }
    private void QRCodeWatcher_EnumerationCompleted(object sender, object e) => enumerationComplete = true;
    #endregion
}
public static class QRCodeEventArgs
{
    public static QRCodeEventArgs<TData> Create<TData>(TData data) => new QRCodeEventArgs<TData>(data);
}
[Serializable]
public class QRCodeEventArgs<TData> : EventArgs
{
    public TData Data { get; private set; }
    public QRCodeEventArgs(TData data) => Data = data;
}
public class QRCodeInformation
{
    public string Data { get; set; }
    public float Length { get; set; }
#if WINDOWS_UWP
    public SpatialCoordinateSystem QRSpatialCoordinateSystem { get; set; }
    public Guid SpatialGraphNodeID { get; set; }
    public QRCodeInformation(string data, SpatialCoordinateSystem qrSpatialCoordinateSystem, float length, Guid spatialGraphNodeID)
    {
        Data = data;
        Length = length;
        QRSpatialCoordinateSystem = qrSpatialCoordinateSystem;
        SpatialGraphNodeID = spatialGraphNodeID;
    }
#endif
    public QRCodeInformation() { }
}