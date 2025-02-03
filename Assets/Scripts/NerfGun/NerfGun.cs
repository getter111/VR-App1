using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class NerfGun : MonoBehaviour
{
    [Header("Dart Settings")]
    [SerializeField] private GameObject Dart;
    [SerializeField] private Transform DartSpawnLocation;
    [SerializeField] private float launchForce = 10f; // Configurable force

    [Header("Nerf Gun Configuration")]
    [SerializeField] private Collider[] colliders;
    public UnityEvent OnFire;

    private XRGrabInteractable grabInteractable;
    private bool canFire = true;
    private bool triggerReleased;
    private bool chamberStoppedRotating;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
    }

    private void OnReleased(SelectExitEventArgs args)
    {
    }
    
    public void Fire()
    {
        if (canFire)
        {
            OnFire.Invoke();

            // Instantiate dart
            GameObject dartInstance = Instantiate(Dart, DartSpawnLocation.position, DartSpawnLocation.rotation);
            dartInstance.GetComponent<Dart>().Initialize(colliders);

            // Apply force if dart has a Rigidbody
            Rigidbody dartRb = dartInstance.GetComponent<Rigidbody>();
            if (dartRb != null)
            {
                dartRb.AddForce(DartSpawnLocation.forward * launchForce, ForceMode.Impulse);
            }
            else
            {
                Debug.LogWarning("Fired dart has no Rigidbody! It won't move.");
            }

            canFire = false;
        }
    }

    public void TriggerReleased()
    {
        triggerReleased = true;
        CheckCanFire();
    }

    public void ChamberStoppedRotating()
    {
        chamberStoppedRotating = true;
        CheckCanFire();
    }

    public void EnableFiring()
    {
        if (triggerReleased)
        {
            canFire = true;
        }
    }

    private void CheckCanFire()
    {
        if (triggerReleased && chamberStoppedRotating)
        {
            canFire = true;
        }
        else
        {
            canFire = false;
        }
    }
}
