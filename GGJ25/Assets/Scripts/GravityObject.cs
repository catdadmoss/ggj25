using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityObject : MonoBehaviour
{
    public enum GravityObjectType { OnlyPullsOthersByGravitation, OnlyPulledByOtherObjects, PullsAndGetsPulled, None }

    #region InspectorSettings
    [HeaderAttribute("Gravit�ci�s t�rgy t�pusa")]
    [TooltipAttribute("Hogyan vesz r�szt ez a t�rgy a gravit�ci�s mechanizmusban?")]
    [SerializeField] private GravityObjectType gravityObjectType = GravityObjectType.PullsAndGetsPulled;

    [HeaderAttribute("Saj�t gravit�ci�s mez� be�ll�t�sai")]
    [TooltipAttribute("Ezzel az �rt�kkel �ll�tod be ennek a t�rgynak a gravit�ci�s erej�t")]
    [SerializeField] private float gravityStrength = 9.780318f;
    [SerializeField] private readonly float magnitudeCorrection = Mathf.Pow(10,11);

    [TooltipAttribute("Ezzel az �rt�kkel �ll�tod be a t�rgynak a gravit�ci�s erej�nek hat�sugar�t")]
    [SerializeField] private float gravityRange = 100.0f;

    [TooltipAttribute("Melyik tengelyen vonzza mag�hoz a t�rgyakat?")]
    [SerializeField] private bool affectsPositionX = true, affectsPositionY = true, affectsPositionZ = true;

    [TooltipAttribute("A j�t�k bet�lt�sekor haszn�lja-e a t�rgy a gravit�ci�s hat�sokat? Ez a v�ltoz� fut�sid�ben is meghat�rozza, hogy hatnak-e a gravit�ci�s hat�sok erre a t�rgyra.")]
    [SerializeField] private bool gravityEnabled = true;

    [HeaderAttribute("Gravit�ci�s mez�j�nek hat�sa")]
    [TooltipAttribute("Melyik t�rgyakat vonzza mag�hoz?")]
    [SerializeField] private bool pullsObjectsWithoutGravitationalField = true;
    [SerializeField] private bool pullsObjectsThatHasTheirOwnGravitation = true;

    [HeaderAttribute("Fizikai tulajdons�gok fel�l�r�sa")]
    [TooltipAttribute("A t�rgy saj�t Transform komponense helyett megadhatsz m�sik Transformot.")]
    [SerializeField] private bool overrideCenterOfMass;
    [SerializeField] private Transform customCenterOfMass;
    [SerializeField] private bool overrideMass;
    [SerializeField] private float customMass;
    #endregion

    #region BackgroundVariables
    // Ezek automatikusan be�ll�tott �rt�kek
    private static List<GravityObject> AllGravityObjects = new List<GravityObject>();
    private Rigidbody2D rigidBody;
    private Transform centerOfMass;
    private float mass;
    private bool pullsOtherGravitationalObjects;
    private readonly float G = 0.00000000006674f;
    #endregion

    #region PublicProperties
    // M�sik komponens �ll�that a t�rgy saj�t gravit�ci�s mez�j�nek hat�sain
    public bool GravityEnabled
    {
        get { return gravityEnabled; }
        set { gravityEnabled = value; }
    }

    // M�sik komponens �ll�that a gravit�ci� er�ss�g�n
    public float GravityStrength
    {
        get { return gravityStrength; }
        set { gravityStrength = value; }
    }

    // M�sik komponens �ll�that a gravit�ci� hat�sugar�n
    public float GravityRange
    {
        get { return gravityRange; }
        set { gravityRange = value; }
    }

    // M�sik komponens �ll�that a gravit�ci� er�ss�g�n
    public GravityObjectType GravityType
    {
        get { return gravityObjectType; }
        set { gravityObjectType = value; }
    }

    // M�sik komponens �ll�that a t�megk�z�pponton
    public Transform CenterOfMass
    {
        get { return centerOfMass; }
        set { centerOfMass = value; }
    }

    // M�sik komponens �ll�that a t�rgy s�ly�n
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
            Debug.Log($"{this.gameObject.name} hozz�adva a gravit�ci�s t�rgyak list�j�hoz, aminek �gy {AllGravityObjects.Count} eleme van.");
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
        // A t�rgy s�ly�nak meghat�roz�sa
        rigidBody = this.GetComponent<Rigidbody2D>();
        if (overrideMass)
        {
            mass = customMass;
        }
        else
        {
            mass = rigidBody.mass;
        }

        // A t�rgy t�megk�z�ppontj�nak meghat�roz�sa
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

        // Ha ez a t�rgy tud vonzani m�s t�rgyakat...
        if (gravityEnabled && pullsOtherGravitationalObjects)
        {
            foreach (GravityObject go in AllGravityObjects)
            {
                if (!go.Equals(this))
                {
                    // ...akkor az �sszes olyan gravit�ci�s t�rgyra hat, aminek a t�pusa ezt megengedi
                    if ((pullsObjectsWithoutGravitationalField & go.GravityType == GravityObjectType.OnlyPulledByOtherObjects)
                        || (pullsObjectsThatHasTheirOwnGravitation & go.GravityType == GravityObjectType.PullsAndGetsPulled))
                    {
                        // Gravit�ci� ir�ny�nak meghat�roz�sa
                        Vector3 directionfGravity = centerOfMass.position - go.centerOfMass.position;

                        // Csak akkor vonzzuk a m�sik t�rgyat, ha az a megadott hat�sugaron bel�l van
                        if (directionfGravity.magnitude < gravityRange)
                        {
                            // Gravit�ci�s er� kisz�m�t�sa : G * m1 * m2 / r^2
                            float fGravity = (G * mass * go.mass) * gravityStrength * magnitudeCorrection /
                                Mathf.Pow(Vector3.Distance(centerOfMass.position, go.centerOfMass.position), 2);

                            // Az er� ir�ny�nak megad�sa
                            directionfGravity = directionfGravity.normalized * fGravity;

                            // A gravit�ci�s er� tengelyekre korl�toz�sa az Inspector be�ll�t�sok alapj�n
                            go.rigidBody.AddForce(
                                new Vector3(
                                affectsPositionX == true ? directionfGravity.x : 0,
                                affectsPositionY == true ? directionfGravity.y : 0,
                                affectsPositionZ == true ? directionfGravity.z : 0),
                                ForceMode2D.Force);
#if UNITY_EDITOR
                            Debug.Log($"Gravit�ci�s er� alkalmazva: {directionfGravity.ToString()}");
#endif
                        }
                    }
                }
            }
        }
        #endregion
    }
}
