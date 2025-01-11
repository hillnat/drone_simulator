using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LineRenderer))]
public class LineRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        DrawDefaultInspector();

        // Get the LineRenderer component
        LineRenderer lineRenderer = (LineRenderer)target;

        // Add a custom button to the inspector
        if (GUILayout.Button("Update Positions from Children"))
        {
            UpdateLineRendererPositions(lineRenderer);
        }
    }

    private void UpdateLineRendererPositions(LineRenderer lineRenderer)
    {
        // Get the transform of the object the LineRenderer is attached to
        Transform parentTransform = lineRenderer.transform;

        // Count the number of child objects
        int childCount = parentTransform.childCount;

        if (childCount == 0)
        {
            Debug.LogWarning("No child objects found to populate LineRenderer positions.");
            return;
        }

        // Create an array to store the positions of the child objects
        Vector3[] positions = new Vector3[childCount];

        // Populate the array with the local positions of the child objects
        for (int i = 0; i < childCount; i++)
        {
            positions[i] = parentTransform.GetChild(i).position;
        }

        // Set the positions on the LineRenderer
        lineRenderer.positionCount = childCount;
        lineRenderer.SetPositions(positions);

        Debug.Log("LineRenderer positions updated based on child objects.");
    }
}
