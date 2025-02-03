using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(DartWobbleController))]
[RequireComponent(typeof(RandomAudioPlayer))]
public class Dart : MonoBehaviour
{
    [Header("Dart Settings")]
    [SerializeField] private float maxAngleToStick = 15f;  // Maximum deviation from normal in degrees
    [SerializeField] private float minVelocityToStick = 2f; // Min speed required to stick
    [SerializeField] private float destroyAfter = 60f;

    public UnityEvent HitObject;
    public UnityEvent StuckToObject;

    private Rigidbody rb;
    private bool stuck = false;
    private Collider dartCollider;
    private Collider[] gunColliders;
    private Vector3 cachedVelocity = Vector3.zero;

    // Reference to the DartWobbleController
    private DartWobbleController wobbleController;
    private RandomAudioPlayer audioPlayer;
    
    public void Initialize(Collider[] ignoreColliders)
    {
        rb = GetComponent<Rigidbody>();
        dartCollider = GetComponent<Collider>();
        wobbleController = GetComponent<DartWobbleController>();
        audioPlayer = GetComponent<RandomAudioPlayer>();

        gunColliders = ignoreColliders;
        foreach (Collider c in gunColliders)
        {
            // Ignore collisions between dart and gun for a short time
            Physics.IgnoreCollision(dartCollider, c, true);
            StartCoroutine(ReenableCollision(0.1f));
        }
    }

    private IEnumerator ReenableCollision(float delay)
    {
        if (gunColliders != null)
        {
            foreach (Collider c in gunColliders)
            {
                yield return new WaitForSeconds(delay);
                Physics.IgnoreCollision(dartCollider, c, false);
            }
        }
    }

    private void Update()
    {
        if (!stuck && rb.linearVelocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }
    
    private void FixedUpdate()
    {
        if (!stuck)
        {
            cachedVelocity = rb.linearVelocity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stuck) return;

        if (collision.gameObject.CompareTag("Dart")) return;

        HitObject.Invoke();

        audioPlayer.PlayRandomClip();
        
        ContactPoint contact = collision.GetContact(0);
        Vector3 impactNormal = contact.normal;
        Vector3 dartDirection = -cachedVelocity.normalized;
        float velocityMagnitude = cachedVelocity.magnitude;

        // Calculate actual angle of incidence
        float incidenceAngle = Vector3.Angle(impactNormal, dartDirection);

        // Check if dart should stick
        bool stickCheck = incidenceAngle <= maxAngleToStick &&
                          velocityMagnitude >= minVelocityToStick;

        if (stickCheck)
        {
            StickToSurface(contact.point, impactNormal, collision.transform);
        }
    }

    private void StickToSurface(Vector3 hitPoint, Vector3 impactNormal, Transform hitObject)
    {
        stuck = true;

        StuckToObject.Invoke();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.position = hitPoint;
        transform.rotation = Quaternion.LookRotation(-impactNormal);
        transform.parent = hitObject;

        if (wobbleController != null)
        {
            wobbleController.StartWobble(cachedVelocity, impactNormal);
        }
        
        Destroy(gameObject, destroyAfter);
    }
}
