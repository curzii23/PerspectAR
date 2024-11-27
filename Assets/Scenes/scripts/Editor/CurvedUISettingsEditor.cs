using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.EventSystems;
#if CURVEDUI_TMP || TMP_PRESENT
using TMPro;
#endif 

#if CURVEDUI_STEAMVR_2
using Valve.VR;
using System.Reflection;
using System;
#endif 


namespace CurvedUI { 
 

[CustomEditor(typeof(CurvedUISettings))]
    [ExecuteInEditMode]
    public class CurvedUISettingsEditor : Editor {

#pragma warning disable 414
        bool ShowRemoveCurvedUI = false;
		bool loadingCustomDefine = false;
        static bool CUIeventSystemPresent = false;
        private bool inPrefabMode = false;

#pragma warning restore 414



        #region LIFECYCLE
        void Awake()
		{
			AddCurvedUIComponents();
        }

        void OnEnable()
		{
            //Debug.Log("OnEnable");
            //if we're firing OnEnable, this means any compilation has ended. We're good!
            loadingCustomDefine = false;

            //lets see if we have any EventSystems present on the scene.
            //var eventSystems = (FindObjectsOfType(typeof(EventSystem)));
            // If yes, check if it's a CurvedUIEventSystem 
            //CUIeventSystemPresent = eventSystems.Any(x => x is CurvedUIEventSystem);

            //hacky way to make sure event is connected only once, but it works!
            EditorApplication.hierarchyChanged -= AddCurvedUIComponents;
            EditorApplication.hierarchyChanged -= AddCurvedUIComponents;
            EditorApplication.hierarchyChanged += AddCurvedUIComponents;

        }
#endregion


        public override void OnInspectorGUI()
        {
            //Debug.Log("OnInspectorGUI");
            //initial settings------------------------------------//
            CurvedUISettings myTarget = (CurvedUISettings)target;
            if (target == null) return;
            GUI.changed = false;
            EditorGUIUtility.labelWidth = 150;
            inPrefabMode = (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null);


            //Version----------------------------------------------//
            //UILayout.Label("Version " + CurvedUISettings.Version, EditorStyles.miniLabel);

            
            //Control methods--------------------------------------//
            //DrawControlMethods();


            //shape settings----------------------------------------//
            GUILayout.Label("Shape", EditorStyles.boldLabel);

            // Display a label for the field
            //EditorGUILayout.LabelField("Large Display Reference");
            //// Allow the user to select a game object and assign it to LargeDisplayObject
            //myTarget.LargeDisplayObject = (GameObject)EditorGUILayout.ObjectField(myTarget.LargeDisplayObject, typeof(GameObject), true);


            myTarget.Shape = (CurvedUISettings.CurvedUIShape)EditorGUILayout.EnumPopup("Canvas Shape", myTarget.Shape);
            switch (myTarget.Shape)
            {
                case CurvedUISettings.CurvedUIShape.CYLINDER:
                {
                    myTarget.Angle = EditorGUILayout.IntSlider("Angle", myTarget.Angle, -360, 360);
                    myTarget.PreserveAspect = EditorGUILayout.Toggle("Preserve Aspect", myTarget.PreserveAspect);
                    break;
                }
            }//end of shape settings-------------------------------//



            //180 degree warning ----------------------------------//
            if ((myTarget.Shape != CurvedUISettings.CurvedUIShape.RING && myTarget.Angle.Abs() > 180) ||
                (myTarget.Shape == CurvedUISettings.CurvedUIShape.SPHERE && myTarget.VerticalAngle > 180))
                Draw180DegreeWarning();


            GUILayout.Space(20);

            //final settings
            if (GUI.changed && myTarget != null)
                EditorUtility.SetDirty(myTarget);
        }


#region CUSTOM GUI ELEMENTS
        void DrawControlMethods()
        {
            GUILayout.Label("Global Settings", EditorStyles.boldLabel);
        }

		/// <summary>
		/// Draws the define switcher for different control methods. 
		/// Because different control methods use different API's that may not always be available, 
		/// CurvedUI needs to be recompile with different custom defines to fix this. This method 
		/// manages the defines.
		/// </summary>
		/// <param name="defineToSet">Switcho.</param>
		void DrawCustomDefineSwitcher(string defineToSet)
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Press the [Enable] button to recompile scripts for this control method. Afterwards, you'll see more settings here.", EditorStyles.helpBox);

			GUILayout.BeginHorizontal();
			GUILayout.Space(50);
			if (GUILayout.Button(loadingCustomDefine ? "Please wait..." : "Enable."))
            {
                SwitchToDefine(defineToSet);
            }
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
        }

        private void SwitchToDefine(string defineToSet)
        {
            loadingCustomDefine = true;

            //retrieve current defines
            string str = "";
            str += PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        
            //add this one, if not present.
            if (defineToSet != "" && !str.Contains(defineToSet))
                str += ";" + defineToSet;

            //Submit defines. This will cause recompilation
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, str);       
        }

        void Draw180DegreeWarning()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(150);
            EditorGUILayout.HelpBox("Cavas with angle bigger than 180 degrees will not be interactable. \n" +
                "This is caused by Unity Event System requirements. Use two canvases facing each other for fully interactive 360 degree UI.", MessageType.Warning);
            GUILayout.EndHorizontal();
            GUILayout.Space(10); 
        }
#endregion




#region HELPER FUNCTIONS
        /// <summary>
        ///Travel the hierarchy and add CurvedUIVertexEffect to every gameobject that can be bent.
        /// </summary>
        private void AddCurvedUIComponents()
        {
            //Debug.Log("Running AddCurvedComponents");
            if (target == null) return;
            (target as CurvedUISettings).AddEffectToChildren();
        }



		/// <summary>
		/// Removes all CurvedUI components from this canvas.
		/// </summary>
#endregion

    }
}

