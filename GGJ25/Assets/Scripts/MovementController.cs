using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private CollectorController collectorController;
    [SerializeField] private float rollSpeed;
    [SerializeField] private float jumpForce;
    private bool isFloored=false;
    private bool jumpInput = false;
    private Vector3 horizontalInput = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody= GetComponent<Rigidbody2D>();
        collectorController = GetComponent<CollectorController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isFloored=true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
        }
    }

    private void Update()
    {
        horizontalInput = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
        jumpInput = Input.GetButtonDown("Jump");
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(!horizontalInput.Equals(Vector3.zero))
        {
            if(isFloored)
            {
                rigidBody.AddForce(horizontalInput * rollSpeed * Time.fixedDeltaTime * collectorController.Size);
            }
            else
            {
                rigidBody.AddTorque(-1*rollSpeed * Time.fixedDeltaTime * horizontalInput.x);
            }

        }
        if (isFloored && jumpInput)
        {
            rigidBody.AddForce(Vector2.up*jumpForce*collectorController.Size);
            isFloored = false;
        }
    }
}
