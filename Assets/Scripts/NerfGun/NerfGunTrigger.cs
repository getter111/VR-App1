using UnityEngine;
using UnityEngine.InputSystem;

public class NerfGunTrigger : MonoBehaviour
{
    
    // Assign this via Inspector to your new "TriggerValue" action
    // (or any float InputAction you created).
    [SerializeField]
    private InputActionReference triggerActionProperty;
    
    public Vector3 pullOffset;
    Vector3 startingPosition;
        
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingPosition = transform.localPosition;
    }

    private void Update()
    {
        // Read the 0–1 value from the trigger
        float triggerValue = triggerActionProperty.action.ReadValue<float>();

        Vector3 offset = Vector3.Lerp(startingPosition, pullOffset, triggerValue);
        transform.localPosition = offset;
    }
}
