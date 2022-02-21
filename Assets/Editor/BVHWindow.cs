 using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
 using UnityEngine;
 using UnityEditor;
using CommonVars;
 using System.Xml;


public class EditModeFunctions : EditorWindow {
     [MenuItem("Window/BVH Options")]
     public static void ShowWindow() {
         GetWindow<EditModeFunctions>("BVH Rebuild");
     }
     List<ProgReportData> ProgIds = new List<ProgReportData>();

      private void OnStartAsync() {
         EditorUtility.SetDirty(Camera.main.GetComponent<AssetManager>());
         Camera.main.GetComponent<AssetManager>().Begin();
         this.ProgIds = new List<ProgReportData>(Camera.main.GetComponent<AssetManager>().ProgIds);
      }
      private void OnStartAsyncCombined() {
         EditorUtility.SetDirty(Camera.main.GetComponent<AssetManager>());
         Camera.main.GetComponent<AssetManager>().BuildCombined();
         this.ProgIds = new List<ProgReportData>(Camera.main.GetComponent<AssetManager>().ProgIds);
      }
      public bool UseAtrous = false;
      public float Atrous_CW = 0.1f;
      public float Atrous_NW = 0.1f;
      public float Atrous_PW = 0.1f;
      public int Atrous_Kernel_Sizes = 1;
      public int BounceCount = 24;
      public bool UseRussianRoulette = true;
      private RayTracingMaster RayMaster;
      private bool ShowAssetSelection = false;
      string[] folders;
      List<string> SceneIds = new List<string>();
      private List<string> FolderNames = new List<string>();
      int XmlSelection;
      private void OnGUI() {
         RayMaster = Camera.main.GetComponent<RayTracingMaster>();
         Rect TestingButton = new Rect((position.width - 10) / 2 + 10,10, (position.width - 10) / 2 - 10, 20);
         Rect ConstructionButton = new Rect(10,10,(position.width - 10) / 2, 20);
         Rect TLASButton =         new Rect(10,35,(position.width - 10) / 2, 20);
         Rect CombinedBuilderButton = new Rect(10,60,(position.width - 10) / 2, 20);
         Rect MatUpdateButton =    new Rect(10, 85, (position.width - 10) / 2, 20);
         Rect XMLParse   =         new Rect(10,110,(position.width - 10) / 2, 20);
         Rect BounceCountInput =   new Rect(Mathf.Max((position.width - 10) / 4,145), 135, (position.width - 10) / 4, 20);
         Rect BounceCountLabel =   new Rect(10, 135, Mathf.Max((position.width - 10) / 4,145), 20);
         Rect RussianRouletteToggle = new Rect(10, 160, (position.width - 10) / 2, 20);
         Rect AtrousToggle =       new Rect(10, 185, (position.width - 10) / 2, 20);
         Rect XMLSelectionGrid = new Rect((position.width - 10) / 2 + 10,35, (position.width - 10) / 2 - 10, 175);
         int AtrousVertOffset = 210;
         if (GUI.Button(TestingButton, "Button to test stuff")) {
            SceneIds.Clear();
            if(ShowAssetSelection) {ShowAssetSelection = false;} else {ShowAssetSelection = true;}
            folders = AssetDatabase.GetSubFolders("Assets/Models");
            foreach(var folder in folders) {

               SceneIds.Add(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("scene", new[] {folder})[0]));
            }
            FolderNames = new List<string>();
            for(int i = 0; i < SceneIds.Count; i++) {
               string Name = SceneIds[i].Replace("Assets/", "");
               Name = Name.Replace("Models/", "");
               Name = Name.Replace("/scene.xml", "");
               FolderNames.Add(Name);
            }

         }
         if(ShowAssetSelection) {
            int i = 0;
            GUI.BeginGroup(XMLSelectionGrid);
            for(i = 0; i < FolderNames.Count; i++) {
                  if(GUI.Button(new Rect(0, i * 25, (position.width - 10) / 2 - 10, 20), FolderNames[i])) {
                     XmlSelection = i;
                     ShowAssetSelection = false;
                     break;
                  }
            }
            GUI.EndGroup();
            if(i != FolderNames.Count) {
               GameObject TargetObject = FindObjectsOfType<XMLParser>()[0].gameObject;
               int ChildCount = TargetObject.transform.childCount;
               List<GameObject> DeletedObjects = new List<GameObject>();
               foreach(Transform child in TargetObject.transform) {
                     DeletedObjects.Add(child.gameObject);
               }
               if(ChildCount != 0) {
                  for(int i3 = ChildCount - 1; i3 >= 0; i3--) {
                     GameObject child = DeletedObjects[i3];
                     child.SetActive(false);
                    DestroyImmediate(child, true);
                  }
               }

               string NewPath = SceneIds[XmlSelection];
               NewPath = NewPath.Replace("scene.xml", "models");
               var MeshesToLoad = AssetDatabase.FindAssets("t:Object", new[] {NewPath});
               foreach(var TargetMesh in MeshesToLoad) {
                  Instantiate(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(TargetMesh), typeof(Object)), TargetObject.transform);
               }
               Debug.Log(SceneIds[XmlSelection]);
               EditorUtility.SetDirty(TargetObject.GetComponent<XMLParser>());
               TargetObject.GetComponent<XMLParser>().LoadXml(SceneIds[XmlSelection]);
            }

         }
         if (GUI.Button(ConstructionButton, "Construct BVH's")) {  
            OnStartAsync();
         }
         if (GUI.Button(TLASButton, "UPDATE TLAS")) {
            EditorUtility.SetDirty(Camera.main.GetComponent<AssetManager>());
            Camera.main.GetComponent<AssetManager>().UpdateTLAS();
         }
         if (GUI.Button(CombinedBuilderButton, "Build Aggregated BVH")) {
            OnStartAsyncCombined();
         }
         if (GUI.Button(MatUpdateButton, "Update Materials")) {
            EditorUtility.SetDirty(Camera.main.GetComponent<AssetManager>());
            Camera.main.GetComponent<AssetManager>().UpdateMaterials();
         }
         GUI.Label(BounceCountLabel, "Max Bounces");
         BounceCount = EditorGUI.IntField(BounceCountInput, BounceCount);
         RayMaster.bouncecount = BounceCount;
         UseRussianRoulette = GUI.Toggle(RussianRouletteToggle, UseRussianRoulette, "Use Russian Roulette");
         RayMaster.UseRussianRoulette = UseRussianRoulette;
         UseAtrous = GUI.Toggle(AtrousToggle, UseAtrous, "Use Atrous Denoiser");
         RayMaster = Camera.main.GetComponent<RayTracingMaster>() as RayTracingMaster;
         if(UseAtrous) {
            Rect Atrous_Color_Weight = new Rect(Mathf.Max((position.width - 10) / 4,145), AtrousVertOffset, (position.width - 10) / 4, 20);
            Rect Atrous_Color_Weight_Lable = new Rect(10, AtrousVertOffset, Mathf.Max((position.width - 10) / 4,145), 20);
            Rect Atrous_Normal_Weight = new Rect(Mathf.Max((position.width - 10) / 4,145), AtrousVertOffset + 25, (position.width - 10) / 4, 20);
            Rect Atrous_Normal_Weight_Lable = new Rect(10, AtrousVertOffset + 25, Mathf.Max((position.width - 10) / 4,145), 20);
            Rect Atrous_Position_Weight = new Rect(Mathf.Max((position.width - 10) / 4,145), AtrousVertOffset + 50, (position.width - 10) / 4, 20);
            Rect Atrous_Position_Weight_Lable = new Rect(10, AtrousVertOffset + 50, Mathf.Max((position.width - 10) / 4,145), 20);
            Rect Atrous_Kernel_Size = new Rect(Mathf.Max((position.width - 10) / 4,145), AtrousVertOffset + 75, (position.width - 10) / 4, 20);
            Rect Atrous_Kernel_Size_Lable = new Rect(10, AtrousVertOffset + 75, Mathf.Max((position.width - 10) / 4,145), 20);
            
            GUI.Label(Atrous_Color_Weight_Lable, "Atrous Color Weight");
            Atrous_CW = GUI.HorizontalSlider(Atrous_Color_Weight, Atrous_CW, 0.0f, 1.0f);
            GUI.Label(Atrous_Normal_Weight_Lable, "Atrous Normal Weight");
            Atrous_NW = GUI.HorizontalSlider(Atrous_Normal_Weight, Atrous_NW, 0.0f, 1.0f);
            GUI.Label(Atrous_Position_Weight_Lable, "Atrous Position Weight");
            Atrous_PW = GUI.HorizontalSlider(Atrous_Position_Weight, Atrous_PW, 0.0f, 1.0f);
            GUI.Label(Atrous_Kernel_Size_Lable, "Atrous Kernel Size");
            Atrous_Kernel_Sizes = EditorGUI.IntField(Atrous_Kernel_Size, Atrous_Kernel_Sizes);
            AtrousVertOffset += 100;
            RayMaster.c_phiGlob = Atrous_CW;
            RayMaster.n_phiGlob = Atrous_NW;
            RayMaster.p_phiGlob = Atrous_PW;
            RayMaster.AtrousKernelSizes = Atrous_Kernel_Sizes;
         }
         RayMaster.UseAtrous = UseAtrous;

         for(int i = ProgIds.Count - 1; i >= 0 ; i--) {//Thisll give you the progress bars
            var i2 = i;
            EditorGUI.ProgressBar(new Rect(0, i2 * 25 + AtrousVertOffset, position.width, 20), UnityEditor.Progress.GetProgress(ProgIds[i2].Id), "Mesh Name: " + ProgIds[i2].Name + ", Triangle Count: " + ProgIds[i2].TriCount);
            if(UnityEditor.Progress.GetProgress(ProgIds[i2].Id) == 0.99f) {
               UnityEditor.Progress.Remove(ProgIds[i2].Id);
               ProgIds.RemoveAt(i2);
            }
         }

     }

void OnInspectorUpdate() {
   Repaint();
}


}
 


 