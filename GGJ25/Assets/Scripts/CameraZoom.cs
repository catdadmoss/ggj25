using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraZoom : MonoBehaviour
{
    public enum ZoomBehaviorType { ChangeOnlyDistanceToReferenceObject, ChangeOnlyFieldOfView, ChangeDistanceAndFieldOfView, None }
    public enum ZoomDirection { Xleft , Xright , Yup , Ydown, Zforward, Zbackward }

    #region InspectorSettings
    [HeaderAttribute("C�lt�rgy (pl. a j�t�kos)")]
    [TooltipAttribute("A dolog, amit a kamera n�z, �s amihez k�pest meghat�rozzuk a kamera t�vols�g�t a j�t�kt�rt�l")]
    [SerializeField] private Transform referenceGameObject;

    [HeaderAttribute("Zoom viselked�s kiv�laszt�sa")]
    [TooltipAttribute("A kamera a kizoomol�skor t�volodhat a ")]
    [SerializeField] private ZoomBehaviorType zoomBehaviorType = ZoomBehaviorType.ChangeDistanceAndFieldOfView;

    [HeaderAttribute("Zoom ir�ny�nak be�ll�t�sa")]
    [TooltipAttribute("Ez akkor l�nyeges, ha a zoom viselked�s szerint a kamer�nak t�volodnia kell a c�lt�rgyt�l")]
    [SerializeField] private ZoomDirection zoomDirection = ZoomDirection.Zbackward;

    [HeaderAttribute("El�re be�ll�tott zoom szintek")]
    [TooltipAttribute("A kezdeti 100% t�rgyt�l val� t�vols�g mellett itt megadhatsz el�re sz�mokat, amire �tviheted a kamer�t")]
    [SerializeField] private float[] presetZoomLevels = new float[] { 100, 110, 120, 130, 140, 150 };

    [HeaderAttribute("Zoom �s Field of View k�z�tti szorz�")]
    [TooltipAttribute("Zoom �s Field of View k�z�tti szorz�. Ez akkor l�nyeges, ha a zoom viselked�s szerint a kamera v�ltoztatja a l�t�sz�g�t.")]
    [SerializeField] private float zoomDistanceAndFieldOfViewConversionRate = 1.2f;

    [HeaderAttribute("Zoom sebess�ge")]
    [TooltipAttribute("Milyen gyorsan v�ltson a kamera a zoom szintjei k�z�tt")]
    [SerializeField] private float zoomSpeed = 0.01f;
    #endregion

    private float zoomLevel = 100;
    [HeaderAttribute("Manu�lis fel�l�r�s Editor Playben")]
    [TooltipAttribute("A kamera zoom szintj�t itt v�ltoztathatod Editorban tesztel�s c�lj�b�l")]
    [SerializeField] private float targetZoomLevel = 100;
    private float startDistance;
    private float startFOV;
    private Camera cameraComponent;

    #region PublicConstructors
    // M�sik komponens �ll�that a zoom konkr�t szintj�n
    public float ZoomLevel 
    {
        get { return zoomLevel; }
        set { zoomLevel = value; }
    }
    #endregion

    // M�sik komponens lek�rheti az el�re be�ll�tott zoom szinteket
    public float[] GetAllPresetZoomLevels()
    {
        return presetZoomLevels;
    }

    // M�sik komponens lek�rhet egy kifejezett zoom szintet
    public float GetZoomLevelPreset(int level)
    {
        if (level < 0 || level >= presetZoomLevels.Length)
        {
            Debug.LogWarning("Nem l�tezik a zoom szint, amit lek�rt�l!");
            return zoomLevel;
        }
        else
        {
            if (presetZoomLevels.Length > 0)
            {
                Debug.LogWarning("Nincsenek el�re be�ll�tott zoom �rt�kek megadva!");
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
        // A kamera kezd��rt�keinek meghat�roz�sa, ez sz�m�t majd 100-as zoom �rt�knek
        targetZoomLevel = 100;

        if (referenceGameObject == null)
        {
            Debug.LogError("Nem adtad meg, hogy mihez k�pest v�ltoztassuk a kamera t�vols�g�t!");
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

        Debug.Log($"Start t�vols�g: {startDistance}, start FoV: {startFOV}");
    }

    void Update()
    {
        if (zoomLevel != targetZoomLevel)
        {
            zoomLevel = Mathf.Lerp(zoomLevel, targetZoomLevel, zoomSpeed);
            Debug.Log($"Zoom szint: {zoomLevel}");
        }

        // Ha v�ltoztatnunk kell a kamera t�vols�g�n, akkor ezt a c�lt�rgyhoz k�pest tessz�k a kamera viselked�s�nek megfelel� ir�nyban
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

        //Ha v�ltoztatnunk kell a kamera l�t�sz�g�n, akkor ezt a c�lt�rgyt�l val� t�vols�g f�ggv�ny�ben tessz�k
        if (zoomBehaviorType == ZoomBehaviorType.ChangeDistanceAndFieldOfView || zoomBehaviorType == ZoomBehaviorType.ChangeOnlyFieldOfView)
        {
            cameraComponent.fieldOfView = Mathf.Clamp(startFOV * zoomLevel / 100 * zoomDistanceAndFieldOfViewConversionRate, 0.00001f, 180f);
        }
    }
}
