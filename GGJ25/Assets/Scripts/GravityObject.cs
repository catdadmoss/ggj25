using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityObject : MonoBehaviour
{
    public enum GravityObjectType { OnlyPullsOthersByGravitation, OnlyPulledByOtherObjects, PullsAndGetsPulled, None }

    #region InspectorSettings
    [HeaderAttribute("Gravitációs tárgy típusa")]
    [TooltipAttribute("Hogyan vesz részt ez a tárgy a gravitációs mechanizmusban?")]
    [SerializeField] private GravityObjectType gravityObjectType = GravityObjectType.PullsAndGetsPulled;

    [HeaderAttribute("Saját gravitációs mezõ beállításai")]
    [TooltipAttribute("Ezzel az értékkel állítod be ennek a tárgynak a gravitációs erejét")]
    [SerializeField] private float gravityStrength = 9.780318f;
    [SerializeField] private readonly float magnitudeCorrection = Mathf.Pow(10,11);

    [TooltipAttribute("Ezzel az értékkel állítod be a tárgynak a gravitációs erejének hatósugarát")]
    [SerializeField] private float gravityRange = 100.0f;

    [TooltipAttribute("Melyik tengelyen vonzza magához a tárgyakat?")]
    [SerializeField] private bool affectsPositionX = true, affectsPositionY = true, affectsPositionZ = true;

    [TooltipAttribute("A játék betöltésekor használja-e a tárgy a gravitációs hatásokat? Ez a változó futásidõben is meghatározza, hogy hatnak-e a gravitációs hatások erre a tárgyra.")]
    [SerializeField] private bool gravityEnabled = true;

    [HeaderAttribute("Gravitációs mezõjének hatása")]
    [TooltipAttribute("Melyik tárgyakat vonzza magához?")]
    [SerializeField] private bool pullsObjectsWithoutGravitationalField = true;
    [SerializeField] private bool pullsObjectsThatHasTheirOwnGravitation = true;

    [HeaderAttribute("Fizikai tulajdonságok felülírása")]
    [TooltipAttribute("A tárgy saját Transform komponense helyett megadhatsz másik Transformot.")]
    [SerializeField] private bool overrideCenterOfMass;
    [SerializeField] private Transform customCenterOfMass;
    [SerializeField] private bool overrideMass;
    [SerializeField] private float customMass;
    #endregion

    #region BackgroundVariables
    // Ezek automatikusan beállított értékek
    private static List<GravityObject> AllGravityObjects = new List<GravityObject>();
    private Rigidbody2D rigidBody;
    private Transform centerOfMass;
    private float mass;
    private bool pullsOtherGravitationalObjects;
    private readonly float G = 0.00000000006674f;
    #endregion

    #region PublicProperties
    // Másik komponens állíthat a tárgy saját gravitációs mezõjének hatásain
    public bool GravityEnabled
    {
        get { return gravityEnabled; }
        set { gravityEnabled = value; }
    }

    // Másik komponens állíthat a gravitáció erõsségén
    public float GravityStrength
    {
        get { return gravityStrength; }
        set { gravityStrength = value; }
    }

    // Másik komponens állíthat a gravitáció hatósugarán
    public float GravityRange
    {
        get { return gravityRange; }
        set { gravityRange = value; }
    }

    // Másik komponens állíthat a gravitáció erõsségén
    public GravityObjectType GravityType
    {
        get { return gravityObjectType; }
        set { gravityObjectType = value; }
    }

    // Másik komponens állíthat a tömegközépponton
    public Transform CenterOfMass
    {
        get { return centerOfMass; }
        set { centerOfMass = value; }
    }

    // Másik komponens állíthat a tárgy súlyán
    public float Mass
    {
        get { return mass; }
        set { mass = value; }
    }
    #endregion

    #region ManagingGravityObjectList
    private void OnEnable()
    {
        if (!AllGravityObjects.Contains(this))
        {
            AllGravityObjects.Add(this);
#if UNITY_EDITOR
            Debug.Log($"{this.gameObject.name} hozzáadva a gravitációs tárgyak listájához, aminek így {AllGravityObjects.Count} eleme van.");
#endif
        }
    }

    private void OnDisable()
    {
        if (AllGravityObjects.Contains(this))
        {
            AllGravityObjects.Remove(this);
        }
    }

    private void OnDestroy()
    {
        if (AllGravityObjects.Contains(this))
        {
            AllGravityObjects.Remove(this);
        }
    }
    #endregion

    private void Start()
    {
        // A tárgy súlyának meghatározása
        rigidBody = this.GetComponent<Rigidbody2D>();
        if (overrideMass)
        {
            mass = customMass;
        }
        else
        {
            mass = rigidBody.mass;
        }

        // A tárgy tömegközéppontjának meghatározása
        if (overrideCenterOfMass)
        {
            centerOfMass = customCenterOfMass;
        }
        else
        {
            centerOfMass = this.transform;
        }

        if (gravityObjectType == GravityObjectType.None && gravityStrength == 0.0f)
        {
            Debug.LogWarning($"Gravity strength of {this.gameObject.name} is set to {gravityStrength.ToString()}.");
        }

        pullsOtherGravitationalObjects = 
            gravityObjectType == GravityObjectType.OnlyPullsOthersByGravitation ||
            gravityObjectType == GravityObjectType.PullsAndGetsPulled;
    }

    #region PullingOtherGravitationalObjects(Update)
    void Update()
    {
        // pullsGravityObjectsWithoutGravitationalField pullsGravityObjectsThatHasTheirOwnGravitation
        if (!overrideMass)
        {
            mass = rigidBody.mass;
        }

        // Ha ez a tárgy tud vonzani más tárgyakat...
        if (gravityEnabled && pullsOtherGravitationalObjects)
        {
            foreach (GravityObject go in AllGravityObjects)
            {
                if (!go.Equals(this))
                {
                    // ...akkor az összes olyan gravitációs tárgyra hat, aminek a típusa ezt megengedi
                    if ((pullsObjectsWithoutGravitationalField & go.GravityType == GravityObjectType.OnlyPulledByOtherObjects)
                        || (pullsObjectsThatHasTheirOwnGravitation & go.GravityType == GravityObjectType.PullsAndGetsPulled))
                    {
                        // Gravitáció irányának meghatározása
                        Vector3 directionfGravity = centerOfMass.position - go.centerOfMass.position;

                        // Csak akkor vonzzuk a másik tárgyat, ha az a megadott hatósugaron belül van
                        if (directionfGravity.magnitude < gravityRange)
                        {
                            // Gravitációs erõ kiszámítása : G * m1 * m2 / r^2
                            float fGravity = (G * mass * go.mass) * gravityStrength * magnitudeCorrection /
                                Mathf.Pow(Vector3.Distance(centerOfMass.position, go.centerOfMass.position), 2);

                            // Az erõ irányának megadása
                            directionfGravity = directionfGravity.normalized * fGravity;

                            // A gravitációs erõ tengelyekre korlátozása az Inspector beállítások alapján
                            go.rigidBody.AddForce(
                                new Vector3(
                                affectsPositionX == true ? directionfGravity.x : 0,
                                affectsPositionY == true ? directionfGravity.y : 0,
                                affectsPositionZ == true ? directionfGravity.z : 0),
                                ForceMode2D.Force);
#if UNITY_EDITOR
                            Debug.Log($"Gravitációs erõ alkalmazva: {directionfGravity.ToString()}");
#endif
                        }
                    }
                }
            }
        }
        #endregion
    }
}
