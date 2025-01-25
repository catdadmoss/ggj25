using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "EnemyScriptableObject", menuName = "Scriptable Objects/EnemyScriptableObject")]
public class EnemyScriptableObject : ScriptableObject
{
    [SerializeField]
    [OnChangedCall("onGravityModifierChanged")]
    private float gravityModifier;

    [SerializeField]
    [OnChangedCall("onImageChanged")]
    private Sprite image;

    [SerializeField]
    [OnChangedCall("onColliderRadiusChanged")]
    private float colliderRadius;

    public float GravityModifier
    {
        get { return gravityModifier; }
        set { gravityModifier = value; onGravityModifierChanged(); }
    }

    public Sprite Image
    {
        get { return image; }
        set { image = value; onImageChanged(); }
    }

    public float ColliderRadius
    {
        get { return colliderRadius; }
        set { colliderRadius = value; onColliderRadiusChanged(); }
    }

    public delegate void GravityMultiDelegate(float gravity);
    public GravityMultiDelegate gravityDelegate;

    public delegate void ImageMultiDelegate(Sprite image);
    public ImageMultiDelegate imageDelegate;

    public delegate void RadiusMultiDelegate(float radius);
    public RadiusMultiDelegate radiusDelegate;

    public void onGravityModifierChanged()
    {
        if (gravityDelegate == null)
        {
            return;
        }
        this.gravityDelegate(this.gravityModifier);
    }

    public void onImageChanged()
    {
        if (imageDelegate == null)
        {
            return;
        }
        this.imageDelegate(this.image);
    }

    public void onColliderRadiusChanged()
    {
        if(radiusDelegate==null)
        {
            return;
        }
        this.radiusDelegate(this.colliderRadius);
    }
}
