using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class EnemyController : MonoBehaviour
{
    [SerializeField]
    protected EnemyScriptableObject enemyScriptableObject;

    protected float gravityModifier;
    protected SpriteRenderer myRenderer;
    protected CircleCollider2D myCollider;

    private bool hasAwaken=false;

    private void Awake()
    {
        this.searchForGravityModifier();
        this.searchForSpriteRenderer();
        this.searchForCircleCollider();
    }

    private void OnEnable()
    {
        this.searchForGravityModifier();
        this.searchForSpriteRenderer();
        this.searchForCircleCollider();
    }

    private void OnDestroy()
    {
        enemyScriptableObject.gravityDelegate -= onModifierChange;
        enemyScriptableObject.imageDelegate -= onImageChange;
        enemyScriptableObject.radiusDelegate -= onRadiusChange;
    }

    private void OnDisable()
    {
        enemyScriptableObject.gravityDelegate -= onModifierChange;
        enemyScriptableObject.imageDelegate -= onImageChange;
        enemyScriptableObject.radiusDelegate -= onRadiusChange;
    }

    private void searchForGravityModifier()
    {
        gravityModifier = enemyScriptableObject.GravityModifier;
        enemyScriptableObject.gravityDelegate -= onModifierChange;
        enemyScriptableObject.gravityDelegate += onModifierChange;
    }

    private void searchForSpriteRenderer()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        if (myRenderer == null)
        {
            myRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        myRenderer.sprite = enemyScriptableObject.Image;
        enemyScriptableObject.imageDelegate -= onImageChange;
        enemyScriptableObject.imageDelegate += onImageChange;
    }

    private void searchForCircleCollider()
    {
        myCollider = GetComponent<CircleCollider2D>();
        if (myCollider == null)
        {
            myCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        myCollider.radius = enemyScriptableObject.ColliderRadius;
        enemyScriptableObject.radiusDelegate -= onRadiusChange;
        enemyScriptableObject.radiusDelegate += onRadiusChange;
    }

    private void onModifierChange(float modifier)
    {
        //this.searchForGravityModifier();
        StartCoroutine(ModifierWait());
    }

    private void onImageChange(Sprite image)
    {
        //this.searchForSpriteRenderer();
        StartCoroutine(ImageWait()); 
    }

    private void onRadiusChange(float radius)
    {
        //this.searchForCircleCollider();
        StartCoroutine(RadiusWait());
    }

    IEnumerator ModifierWait()
    {
        yield return new WaitForSeconds(.1f);
        gravityModifier = this.enemyScriptableObject.GravityModifier;
    }

    IEnumerator ImageWait()
    {
        yield return new WaitForSeconds(.1f);
        myRenderer.sprite = this.enemyScriptableObject.Image;
    }

    IEnumerator RadiusWait()
    {
        yield return new WaitForSeconds(.1f);
        Debug.Log(this.enemyScriptableObject.ColliderRadius);
        myCollider.radius = this.enemyScriptableObject.ColliderRadius;
    }
}
