using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Editor for DartWobbleController to add testing functionalities.
/// </summary>
[CustomEditor(typeof(DartWobbleController))]
public class DartWobbleControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        DrawDefaultInspector();

        // Reference to the target script
        DartWobbleController wobbleController = (DartWobbleController)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Manual Wobble Testing", EditorStyles.boldLabel);

        // Direction Field
        wobbleController.testDirection = EditorGUILayout.Vector3Field("Test Direction", wobbleController.testDirection);

        // BendStrength Field
        wobbleController.testBendStrength = EditorGUILayout.FloatField("Test Bend Strength", wobbleController.testBendStrength);

        EditorGUILayout.Space();

        // Trigger Button
        if (GUILayout.Button("Trigger Wobble"))
        {
            // Ensure we're in Play mode
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode Required", "Please enter Play mode to test the wobble effect.", "OK");
            }
            else
            {
                // Initiate wobble using test parameters
                wobbleController.StartManualWobble(wobbleController.testDirection, wobbleController.testBendStrength);
            }
        }

        // Optionally, display a warning if useTestParameters is true during Play mode
#if UNITY_EDITOR
        if (wobbleController.useTestParameters && Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Wobble is using test parameters. To stop using them, uncheck 'Use Test Parameters'.", MessageType.Info);
        }
#endif
    }
}