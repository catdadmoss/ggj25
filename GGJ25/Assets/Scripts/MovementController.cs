using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private CollectorController collectorController;
    [SerializeField] private float rollSpeed;
    [SerializeField] private float jumpForce;
    private bool isFloored=false;
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
            isFloored = false;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        var horizontalInput = new Vector3(Input.GetAxis("Horizontal"),0f,0f);
        rigidBody.AddForce(horizontalInput*rollSpeed*Time.fixedDeltaTime*collectorController.Size);
        if(isFloored && Input.GetButtonDown("Jump") )
        {
            rigidBody.AddForce(Vector2.up*jumpForce*collectorController.Size);
        }
    }
}
