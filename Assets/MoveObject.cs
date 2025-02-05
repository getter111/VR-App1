using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()  
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");  

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }
}
