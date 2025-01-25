using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/*[RequireComponent(typeof(Rigidbody2D))]*/
[ExecuteAlways]
public class EnemyMovementController : MonoBehaviour
{
    [SerializeField]
    protected EnemyMovementScriptableObject movementScriptableObject;

    private Rigidbody2D rigidBody;
    protected bool isOnGround;

    public Rigidbody2D RigidBody
    {
        get 
        { 
            if(rigidBody==null)
            {
                rigidBody = GetComponent<Rigidbody2D>();
            }
            return rigidBody; 
        }
        set { rigidBody = value; }
    }

    // Update is called once per frame
    void Update()
    {
        if((movementScriptableObject.NeedsToBeOnGround && isOnGround) || !movementScriptableObject.NeedsToBeOnGround)
        {
            RigidBody.AddForce(movementScriptableObject.MovementVector, ForceMode2D.Impulse);
            RigidBody.linearVelocity = Vector2.ClampMagnitude(RigidBody.linearVelocity, movementScriptableObject.MaxSpeed);
            //Debug.Log(RigidBody.linearVelocity);
            //rigidBody.linearVelocity+= movementScriptableObject.MovementVector;
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

    private void Awake()
    {
        movementScriptableObject.gravityDelegate -= onGravityScaleChange;
        movementScriptableObject.gravityDelegate += onGravityScaleChange;
        RigidBody.gravityScale = movementScriptableObject.GravityScale;
    }

    private void OnEnable()
    {
        movementScriptableObject.gravityDelegate -= onGravityScaleChange;
        movementScriptableObject.gravityDelegate += onGravityScaleChange;
        RigidBody.gravityScale = movementScriptableObject.GravityScale;
    }

    private void OnDestroy()
    {
        movementScriptableObject.gravityDelegate -= onGravityScaleChange;
    }

    private void OnDisable()
    {
        movementScriptableObject.gravityDelegate -= onGravityScaleChange;
    }

    private void onGravityScaleChange(float modifier)
    {
        StartCoroutine(GravityScaleWait());
    }

    IEnumerator GravityScaleWait()
    {
        yield return new WaitForSeconds(.1f);
        RigidBody.gravityScale= movementScriptableObject.GravityScale;
    }
}
