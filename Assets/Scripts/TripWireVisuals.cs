using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TripWireVisual : MonoBehaviour
{
    [SerializeField] Transform postA;
    [SerializeField] Transform postB;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponentInParent<LineRenderer>();

        if (postA == null || postB == null)
        {
          //  Debug.LogError("PostA or PostB not assigned in TripWireVisual.");
            enabled = false;
            return;
        }

        lineRenderer.positionCount = 2;
        UpdateLine();
    }

    private void Update()
    {
        UpdateLine();
    }

    private void UpdateLine()
    {
        lineRenderer.SetPosition(0, postA.position);
        lineRenderer.SetPosition(1, postB.position);
    }
}