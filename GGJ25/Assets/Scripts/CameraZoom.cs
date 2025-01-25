using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraZoom : MonoBehaviour
{
    public enum ZoomBehaviorType { ChangeOnlyDistanceToReferenceObject, ChangeOnlyFieldOfView, ChangeDistanceAndFieldOfView, None }
    public enum ZoomDirection { Xleft , Xright , Yup , Ydown, Zforward, Zbackward }

    #region InspectorSettings
    [HeaderAttribute("Céltárgy (pl. a játékos)")]
    [TooltipAttribute("A dolog, amit a kamera néz, és amihez képest meghatározzuk a kamera távolságát a játéktértõl")]
    [SerializeField] private Transform referenceGameObject;

    [HeaderAttribute("Zoom viselkedés kiválasztása")]
    [TooltipAttribute("A kamera a kizoomoláskor távolodhat a ")]
    [SerializeField] private ZoomBehaviorType zoomBehaviorType = ZoomBehaviorType.ChangeDistanceAndFieldOfView;

    [HeaderAttribute("Zoom irányának beállítása")]
    [TooltipAttribute("Ez akkor lényeges, ha a zoom viselkedés szerint a kamerának távolodnia kell a céltárgytól")]
    [SerializeField] private ZoomDirection zoomDirection = ZoomDirection.Zbackward;

    [HeaderAttribute("Elõre beállított zoom szintek")]
    [TooltipAttribute("A kezdeti 100% tárgytól való távolság mellett itt megadhatsz elõre számokat, amire átviheted a kamerát")]
    [SerializeField] private float[] presetZoomLevels = new float[] { 100, 110, 120, 130, 140, 150 };

    [HeaderAttribute("Zoom és Field of View közötti szorzó")]
    [TooltipAttribute("Zoom és Field of View közötti szorzó. Ez akkor lényeges, ha a zoom viselkedés szerint a kamera változtatja a látószögét.")]
    [SerializeField] private float zoomDistanceAndFieldOfViewConversionRate = 1.2f;

    [HeaderAttribute("Zoom sebessége")]
    [TooltipAttribute("Milyen gyorsan váltson a kamera a zoom szintjei között")]
    [SerializeField] private float zoomSpeed = 0.01f;
    #endregion

    private float zoomLevel = 100;
    [HeaderAttribute("Manuális felülírás Editor Playben")]
    [TooltipAttribute("A kamera zoom szintjét itt változtathatod Editorban tesztelés céljából")]
    [SerializeField] private float targetZoomLevel = 100;
    private float startDistance;
    private float startFOV;
    private Camera cameraComponent;

    #region PublicConstructors
    // Másik komponens állíthat a zoom konkrét szintjén
    public float ZoomLevel 
    {
        get { return zoomLevel; }
        set { zoomLevel = value; }
    }
    #endregion

    // Másik komponens lekérheti az elõre beállított zoom szinteket
    public float[] GetAllPresetZoomLevels()
    {
        return presetZoomLevels;
    }

    // Másik komponens lekérhet egy kifejezett zoom szintet
    public float GetZoomLevelPreset(int level)
    {
        if (level < 0 || level >= presetZoomLevels.Length)
        {
            Debug.LogWarning("Nem létezik a zoom szint, amit lekértél!");
            return zoomLevel;
        }
        else
        {
            if (presetZoomLevels.Length > 0)
            {
                Debug.LogWarning("Nincsenek elõre beállított zoom értékek megadva!");
                return zoomLevel;
            }
            else
            {
                return presetZoomLevels[level];
            }
        }
    }

    void Start()
    {
        // A kamera kezdõértékeinek meghatározása, ez számít majd 100-as zoom értéknek
        targetZoomLevel = 100;

        if (referenceGameObject == null)
        {
            Debug.LogError("Nem adtad meg, hogy mihez képest változtassuk a kamera távolságát!");
            referenceGameObject = new GameObject().transform;
        }

        if (zoomDirection == ZoomDirection.Xleft || zoomDirection == ZoomDirection.Xright)
        {
            startDistance = transform.position.x - referenceGameObject.position.x;
        }
        if (zoomDirection == ZoomDirection.Yup || zoomDirection == ZoomDirection.Ydown)
        {
            startDistance = transform.position.y - referenceGameObject.position.y;
        }
        if (zoomDirection == ZoomDirection.Zforward || zoomDirection == ZoomDirection.Zbackward)
        {
            startDistance = transform.position.z - referenceGameObject.position.z;
        }

        cameraComponent = GetComponent<Camera>();

        startFOV = cameraComponent.fieldOfView;

        Debug.Log($"Start távolság: {startDistance}, start FoV: {startFOV}");
    }

    void Update()
    {
        if (zoomLevel != targetZoomLevel)
        {
            zoomLevel = Mathf.Lerp(zoomLevel, targetZoomLevel, zoomSpeed);
            Debug.Log($"Zoom szint: {zoomLevel}");
        }

        // Ha változtatnunk kell a kamera távolságán, akkor ezt a céltárgyhoz képest tesszük a kamera viselkedésének megfelelõ irányban
        if (zoomBehaviorType == ZoomBehaviorType.ChangeDistanceAndFieldOfView || zoomBehaviorType == ZoomBehaviorType.ChangeOnlyDistanceToReferenceObject)
        {
            if (zoomDirection == ZoomDirection.Xleft)
            {
                transform.position = new Vector3(referenceGameObject.position.x - startDistance * zoomLevel / 100, transform.position.y, transform.position.z);
            }
            if (zoomDirection == ZoomDirection.Xright)
            {
                transform.position = new Vector3(referenceGameObject.position.x + startDistance * zoomLevel / 100, transform.position.y, transform.position.z);
            }
            if (zoomDirection == ZoomDirection.Yup)
            {
                transform.position = new Vector3(transform.position.x, referenceGameObject.position.y - startDistance * zoomLevel / 100, transform.position.z);
            }
            if (zoomDirection == ZoomDirection.Ydown)
            {
                transform.position = new Vector3(transform.position.x, referenceGameObject.position.y + startDistance * zoomLevel / 100, transform.position.z);
            }
            if (zoomDirection == ZoomDirection.Zforward)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, referenceGameObject.position.z - startDistance * zoomLevel / 100);
            }
            if (zoomDirection == ZoomDirection.Zbackward)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, referenceGameObject.position.z + startDistance * zoomLevel / 100);
            }
        }

        //Ha változtatnunk kell a kamera látószögén, akkor ezt a céltárgytól való távolság függvényében tesszük
        if (zoomBehaviorType == ZoomBehaviorType.ChangeDistanceAndFieldOfView || zoomBehaviorType == ZoomBehaviorType.ChangeOnlyFieldOfView)
        {
            cameraComponent.fieldOfView = Mathf.Clamp(startFOV * zoomLevel / 100 * zoomDistanceAndFieldOfViewConversionRate, 0.00001f, 180f);
        }
    }
}
