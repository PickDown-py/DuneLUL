using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
   public override void OnInspectorGUI()
   {
      var mapGen = (MapGenerator) target;

      if (DrawDefaultInspector())
      {
         if (mapGen.autoUpdate)
         {
            mapGen.DrawMapInEditor();
         }
      }

      if (GUILayout.Button("Generate"))
      {
         mapGen.DrawMapInEditor();
      }
   }
}
