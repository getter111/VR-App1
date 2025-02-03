using UnityEngine;

/// <summary>
/// Controls the procedural wobble effect of a dart using Shader Graph parameters.
/// Attach this script to the Dart GameObject.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class DartWobbleController : MonoBehaviour
{
    // Shader property IDs for efficiency
    private static readonly int DirectionID = Shader.PropertyToID("_Direction");
    private static readonly int BendStrengthID = Shader.PropertyToID("_BendStrength");

    // Reference to the Renderer component
    private Renderer dartRenderer;

    // MaterialPropertyBlock to modify shader properties without altering the material asset
    private MaterialPropertyBlock propertyBlock;

    // Wobble state variables
    private bool isWobbling = false;
    private Vector3 wobbleDirection = Vector3.zero;
    private float initialBendStrength = 0f;

    // Oscillation parameters
    [Header("Wobble Settings")]
    [Tooltip("Frequency of the wobble oscillation.")]
    public float wobbleFrequency = 5f; // Adjust for faster or slower oscillations

    [Tooltip("Damping factor to reduce oscillation over time.")]
    public float wobbleDamping = 5f;   // Higher values dampen faster

    [Tooltip("Multiplier to scale the initial BendStrength based on velocity.")]
    public float bendStrengthMultiplier = 0.1f; // Adjust based on desired initial wobble

    // Timer to track wobble progression
    private float wobbleTimer = 0f;

    // Maximum duration for the wobble effect (optional)
    [Tooltip("Maximum duration in seconds for the wobble effect.")]
    public float maxWobbleDuration = 5f;

    // Testing parameters
    [Header("Editor Testing")]
    [Tooltip("Direction vector for manual wobble testing.")]
    public Vector3 testDirection = Vector3.up;

    [Tooltip("BendStrength value for manual wobble testing.")]
    public float testBendStrength = 0.1f;

    // Indicator for testing
    [Tooltip("Whether to use manual testing parameters.")]
    public bool useTestParameters = false;

    void Awake()
    {
        // Initialize references
        dartRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (isWobbling)
        {
            wobbleTimer += Time.deltaTime;

            // Calculate damped oscillation using an exponential decay multiplied by a sine wave
            float dampingFactor = Mathf.Exp(-wobbleDamping * wobbleTimer);
            float oscillation = dampingFactor * Mathf.Sin(wobbleFrequency * wobbleTimer);

            // Current BendStrength based on oscillation
            float currentBendStrength = initialBendStrength * oscillation;

            // Update shader properties
            dartRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(BendStrengthID, currentBendStrength);
            dartRenderer.SetPropertyBlock(propertyBlock);

            // Terminate wobble after max duration or when BendStrength is negligible
            if (wobbleTimer >= maxWobbleDuration || Mathf.Abs(currentBendStrength) < 0.001f)
            {
                isWobbling = false;
                wobbleTimer = 0f;

                // Reset BendStrength to zero to stop the wobble
                dartRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(BendStrengthID, 0f);
                dartRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        // Editor testing (only in Play mode)
#if UNITY_EDITOR
        if (useTestParameters && Application.isPlaying)
        {
            StartManualWobble(testDirection, testBendStrength);
            // Prevent continuous triggering
            useTestParameters = false;
        }
#endif
    }

    /// <summary>
    /// Initiates the wobble effect based on the dart's velocity and the surface normal it stuck to.
    /// Call this method when the dart sticks to a surface.
    /// </summary>
    /// <param name="initialVelocity">The velocity of the dart at the moment of sticking.</param>
    /// <param name="surfaceNormal">The normal vector of the surface the dart stuck to.</param>
    public void StartWobble(Vector3 initialVelocity, Vector3 surfaceNormal)
    {
        Debug.Log("Initial velocity:" + initialVelocity.ToString("F6"));
        Debug.Log("Surface normal:" + surfaceNormal.ToString("F6"));
        
        // Project the velocity onto the surface to get the parallel component
        Vector3 velocityParallel = Vector3.ProjectOnPlane(initialVelocity, surfaceNormal);

        Debug.Log("Parallel velocity:" + velocityParallel.ToString("F6"));
        Debug.Log("Parallel velocity magnitude:" + velocityParallel.magnitude);
        
        // If the parallel velocity is negligible, do not initiate wobble
        if (velocityParallel.magnitude < 0.1f)
            return;

        // Set the wobble direction based on the parallel velocity
        wobbleDirection = velocityParallel.normalized;

        // Set the initial BendStrength based on the velocity magnitude
        initialBendStrength = velocityParallel.magnitude * bendStrengthMultiplier;
        
        Debug.Log("Initial bend strength:" + velocityParallel.magnitude);

        // Update shader properties
        dartRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetVector(DirectionID, wobbleDirection);
        dartRenderer.SetPropertyBlock(propertyBlock);

        // Start the wobble effect
        isWobbling = true;
        wobbleTimer = 0f;
    }

    /// <summary>
    /// Initiates the wobble effect manually using test parameters from the Inspector.
    /// </summary>
    /// <param name="manualDirection">The manual direction vector.</param>
    /// <param name="manualBendStrength">The manual bend strength value.</param>
    public void StartManualWobble(Vector3 manualDirection, float manualBendStrength)
    {
        // Validate the manual direction
        if (manualDirection == Vector3.zero)
        {
            Debug.LogWarning("Manual Wobble: Direction vector is zero. Wobble not initiated.");
            return;
        }

        // Normalize the manual direction
        wobbleDirection = manualDirection.normalized;

        // Set the BendStrength based on manual input
        initialBendStrength = manualBendStrength;

        // Update shader properties
        dartRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetVector(DirectionID, wobbleDirection);
        dartRenderer.SetPropertyBlock(propertyBlock);

        // Start the wobble effect
        isWobbling = true;
        wobbleTimer = 0f;
    }

    /// <summary>
    /// Visualizes the wobble direction in the Scene view.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Only draw if wobbling or using test parameters
        if (isWobbling || (useTestParameters && !Application.isPlaying))
        {
            Vector3 dir = wobbleDirection;
            if (useTestParameters && !isWobbling)
            {
                dir = testDirection.normalized;
            }

            // Set Gizmo color
            Gizmos.color = Color.yellow;

            // Draw a line representing the wobble direction
            Gizmos.DrawLine(transform.position, transform.position + dir * 1f);

            // Draw an arrowhead for better visualization
            DrawArrow(transform.position + dir * 1f, dir, 0.2f, 20);
        }
    }

    /// <summary>
    /// Helper method to draw an arrowhead in Gizmos.
    /// </summary>
    /// <param name="start">Start position of the arrowhead.</param>
    /// <param name="direction">Direction of the arrowhead.</param>
    /// <param name="arrowSize">Size of the arrowhead.</param>
    /// <param name="arrowAngle">Angle of the arrowhead sides.</param>
    void DrawArrow(Vector3 start, Vector3 direction, float arrowSize, float arrowAngle)
    {
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowAngle, 0) * Vector3.forward;
        Gizmos.DrawLine(start, start + right * arrowSize);
        Gizmos.DrawLine(start, start + left * arrowSize);
    }
}
