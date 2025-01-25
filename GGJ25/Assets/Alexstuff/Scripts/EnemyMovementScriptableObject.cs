using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMovementScriptableObject", menuName = "Scriptable Objects/EnemyMovementScriptableObject")]
public class EnemyMovementScriptableObject : ScriptableObject
{
    [SerializeField]
    private Vector2 movementVector;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    [OnChangedCall("onGravityScaleChanged")]
    private float gravityScale;
    [SerializeField]
    private bool needsToBeOnGround;

    public Vector2 MovementVector { get => movementVector; set => movementVector = value; }
    public float MaxSpeed { get => maxSpeed; set => maxSpeed = value; }
    public float GravityScale
    {
        get { return gravityScale; }
        set { gravityScale = value; onGravityScaleChanged(); }
    }
    public bool NeedsToBeOnGround { get => needsToBeOnGround; set => needsToBeOnGround = value; }


    public delegate void GravityMultiDelegate(float gravity);
    public GravityMultiDelegate gravityDelegate;

    public void onGravityScaleChanged()
    {
        if (gravityDelegate == null)
        {
            return;
        }
        this.gravityDelegate(this.gravityScale);
    }
}
