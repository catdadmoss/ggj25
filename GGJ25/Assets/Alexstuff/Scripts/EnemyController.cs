using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


/*[RequireComponent(typeof(Rigidbody2D))]*/
[RequireComponent(typeof(SpriteRenderer))]
[ExecuteAlways]
public class EnemyController : MonoBehaviour
{
    [SerializeField]
    protected EnemyScriptableObject enemyScriptableObject;
    private SpriteRenderer myRenderer;

    protected SpriteRenderer MyRenderer
    {
        get 
        { 
            if(myRenderer == null)
            {
                myRenderer = GetComponent<SpriteRenderer>();
            }
            return myRenderer; 
        }
        set { myRenderer = value; }
    }

    public float GetGravityModifier() 
    {
        return enemyScriptableObject.GravityModifier;
    }

    public string GetName()
    {
        return enemyScriptableObject.EnemyName;
    }

    private void Awake()
    {
        enemyScriptableObject.imageDelegate -= onImageChange;
        enemyScriptableObject.imageDelegate += onImageChange;
        MyRenderer.sprite = enemyScriptableObject.Image;
    }

    private void OnEnable()
    {
        enemyScriptableObject.imageDelegate -= onImageChange;
        enemyScriptableObject.imageDelegate += onImageChange;
        MyRenderer.sprite = enemyScriptableObject.Image;
    }

    private void OnDestroy()
    {

        enemyScriptableObject.imageDelegate -= onImageChange;
    }

    private void OnDisable()
    {
        enemyScriptableObject.imageDelegate -= onImageChange;
    }

    private void onImageChange(Sprite image)
    {
        //this.searchForSpriteRenderer();
        StartCoroutine(ImageWait()); 
    }

    IEnumerator ImageWait()
    {
        yield return new WaitForSeconds(.1f);
        MyRenderer.sprite = this.enemyScriptableObject.Image;
    }

    public void OnEnemyCollected()
    {
        Destroy(gameObject.GetComponent<Rigidbody2D>());

        EnemyMovementController movementController = gameObject.GetComponent<EnemyMovementController>();
        if (movementController != null)
        {
            movementController.enabled = false;
            //Destroy(movementController);
        }
       
       /* EnemyController ce = collision.gameObject.GetComponent<EnemyController>();
        if (ce != null)
        {
            ce.enabled = false;
            Destroy(ce);
        }*/
        foreach (Transform child in transform)
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("EnemyMovement"))
            {
                // Destroy(child.gameObject);
                child.gameObject.SetActive(false);
                Debug.Log(child.gameObject);
            }
        }

        this.enabled = false;
    }
}
