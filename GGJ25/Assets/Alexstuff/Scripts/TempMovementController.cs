using UnityEngine;

public class TempMovementController : MonoBehaviour
{
    protected Rigidbody2D rigidBody;
    [SerializeField]
    protected Vector2 movementVector;
    [SerializeField]
    protected float maxSpeed;
    [SerializeField]
    protected bool useGravity;

    [SerializeField]
    protected bool needsToBeOnGround;
    protected bool isOnGround;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.gravityScale = useGravity ? 1 : 0;
    }

    // Update is called once per frame
    void Update()
    {
        if((needsToBeOnGround && isOnGround) || !needsToBeOnGround)
        {
            rigidBody.AddForce(movementVector);
            rigidBody.linearVelocity = Vector2.ClampMagnitude(rigidBody.linearVelocity, maxSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.gameObject.CompareTag("Ground"))
       // {
            Debug.Log("on ground");
            isOnGround = true;
        //}
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
       // if (collision.gameObject.CompareTag("Ground"))
        //{
            Debug.Log("not on ground");
            isOnGround = false;
       // }
    }
}
