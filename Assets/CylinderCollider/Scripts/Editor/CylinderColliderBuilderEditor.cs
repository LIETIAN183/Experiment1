namespace Assets.CylinderCollider.Scripts.Editor
{
  using System.Collections.Generic;
  using System.Linq;

  using UnityEditor;

  using UnityEngine;

  [CustomEditor(typeof(CylinderColliderBuilder))]
  [CanEditMultipleObjects]
  public class CylinderColliderBuilderEditor : Editor
  {
    private readonly List<CylinderColliderBuilder> cylinderColliderBuilders = new List<CylinderColliderBuilder>();

    public override void OnInspectorGUI()
    {
      this.DrawDefaultInspector();

      EditorGUILayout.Space();

      if (GUILayout.Button("Build"))
      {
        Undo.RegisterCompleteObjectUndo(this.targets, "Inspector");

        foreach (var cylinderColliderBuilder in this.cylinderColliderBuilders)
        {
          cylinderColliderBuilder.Build();
        }
      }

      if (GUILayout.Button("Clean"))
      {
        Undo.RegisterCompleteObjectUndo(this.targets, "Inspector");

        foreach (var cylinderColliderBuilder in this.cylinderColliderBuilders)
        {
          cylinderColliderBuilder.Clean();
        }
      }

      if (GUI.changed && !Application.isPlaying)
      {
        foreach (var cylinderColliderBuilder in this.cylinderColliderBuilders)
        {
          EditorUtility.SetDirty(cylinderColliderBuilder);
        }
      }
    }

    protected void OnEnable()
    {
      this.cylinderColliderBuilders.AddRange(this.targets.Cast<CylinderColliderBuilder>());
    }

    protected void OnDisable()
    {
      this.cylinderColliderBuilders.Clear();
    }
  }
}