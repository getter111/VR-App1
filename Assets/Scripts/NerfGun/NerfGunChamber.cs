using System;
using UnityEngine;
using UnityEngine.Events;

public class NerfGunChamber : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float maxSpeed = 100f;      // Degrees per second
    [SerializeField] private float acceleration = 300f;  // Degrees per second squared
    [SerializeField] private float stopThreshold = 0.5f; // When to snap to target
    [SerializeField] private bool useInstantStop = true; // Toggle sudden stop

    private float currentSpeed = 0f;
    private float currentRotation = 0f;
    private bool isRotating = false;
    private float targetRotation = 0f;
    
    public UnityEvent OnRotationComplete; // Optional event to notify when done

    // Update is called once per frame
    void Update()
    {
        if (isRotating)
        {
            RotateTowardTarget();
        }
    }

    public void StartRotation(float newRotation)
    {
        if (!isRotating) // Prevent overriding mid-rotation
        {
            targetRotation += newRotation;
            isRotating = true;
            currentSpeed = 0f;
        }
    }
    
    private void RotateTowardTarget()
    {
        // Compute remaining distance
        float remainingDistance = Mathf.Abs(targetRotation - currentRotation);

        // Smooth acceleration
        if (remainingDistance > stopThreshold)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            // Stop immediately or smoothly
            if (useInstantStop)
                currentSpeed = 0f;
            else
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, acceleration * Time.deltaTime * 2);
            
            isRotating = false;
            currentSpeed = 0f;
            currentRotation = targetRotation; // Snap to exact target
            transform.localRotation = Quaternion.Euler(currentRotation, 0, 0);

            // Invoke event if needed
            //Debug.Log(Time.time + ": Rotation Complete");
            OnRotationComplete?.Invoke();
            return;
        }

        // Apply rotation
        float deltaRotation = currentSpeed * Time.deltaTime;
        if (deltaRotation > remainingDistance) // Prevent overshooting
        {
            deltaRotation = remainingDistance;
            isRotating = false;
            currentSpeed = 0f;
            
            // Invoke event if needed
            //Debug.Log(Time.time + ": Rotation Complete");
            OnRotationComplete?.Invoke();
        }

        currentRotation += deltaRotation * Mathf.Sign(targetRotation - currentRotation);
        transform.localRotation = Quaternion.Euler(currentRotation, 0, 0);
    }
}
