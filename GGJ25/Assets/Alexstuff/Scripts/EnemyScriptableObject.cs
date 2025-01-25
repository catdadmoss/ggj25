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
    [OnChangedCall("onNameChanged")]
    private string enemyName;

    public float GravityModifier
    {
        get { return gravityModifier; }
        set { gravityModifier = value; }
    }

    public string EnemyName { get => enemyName; set => enemyName = value; }
    public Sprite Image
    {
        get { return image; }
        set { image = value; onImageChanged(); }
    }

    public delegate void ImageMultiDelegate(Sprite image);
    public ImageMultiDelegate imageDelegate;

    public void onImageChanged()
    {
        if (imageDelegate == null)
        {
            return;
        }
        this.imageDelegate(this.image);
    }
}
