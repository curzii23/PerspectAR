using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;

public class RoomScanner : MonoBehaviour
{
    private IMixedRealitySpatialAwarenessMeshObserver meshObserver;
    public Material meshMaterial;
    private bool isScanning = false;

    void Start()
    {
        // Get the spatial awareness system
        var spatialAwarenessSystem = CoreServices.SpatialAwarenessSystem;
        if (spatialAwarenessSystem != null)
        {
            // Get the mesh observer
            // Cast to the IMixedRealityDataProviderAccess to get access to the data providers
            var dataProviderAccess = spatialAwarenessSystem as IMixedRealityDataProviderAccess;

            meshObserver = dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>("OpenXR Spatial Mesh Observer");
           
            if (meshObserver != null)
            {
           
                // Set the mesh material
                meshObserver.MeshPhysicsLayer = 2;
                meshObserver.LevelOfDetail = SpatialAwarenessMeshLevelOfDetail.Coarse;
                meshObserver.VisibleMaterial = meshMaterial;
            }
        }
    }

    void Update()
    {
        if (isScanning)
        {
            // Optionally, you can update the scanning visualization here
        }
    }

    public void StartScanning()
    {



        if (meshObserver != null)
        {
            meshObserver.Resume();
            isScanning = true;
            meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;

        }
    }

    public void StopScanning()
    {

        if (meshObserver != null)
        {
            meshObserver.Suspend();
            isScanning = false;
            meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
        }

    }
}
