using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.Linq;
using DefaultNamespace;
using NUnit.Framework;
using UnityEngine.Serialization;


public class JellyController : MonoBehaviour
{
    public SpriteShapeController spriteShape;
    public float splineOffset;
    public GameObject physicsPrefab;

    public float outerRadius = 2;
    
    [HideInInspector, SerializeField]
    private float defaultRadius = 2f;
    
    public float expandedRatio = 2f;

    public int numPoints;

    public int neighborJointRadius = 2;

    [Header("Center Joint Properties")] public float centerSpringFrequency = 5f;
    public float centerSpringDamping = 0.5f;

    [Header("Neighbor Joint Properties")] public float neighborSpringFrequency = 5f;
    public float neighborSpringDamping = 0.5f;

    public float tangentSmoothing = 0.3f;
    
    public bool isFloored;

    public Rigidbody2D rigidBody;

    private Vector3 horizontalInput = Vector3.zero;

    public float rollSpeed = 10;
    public float tangetForceMod = 0.1f;

    public float maxAngularVelocity = 10f;
    public float dampingForce = 5f;

    public float slipVelocityThreshold = 1f;
    public float slipVelocity = 0f;

    private Transform rotator;  

    [System.Serializable]
    public class PointDataHolder 
    {
        public SpringJoint2D centerJoint;
        public List<SpringJoint2D> neighborJoints = new List<SpringJoint2D>();
        public GroundDetector groundDetector;
        public Transform transform;
        public Rigidbody2D rigidBody;
    }

    [SerializeField, HideInInspector]
    private List<PointDataHolder> dataHolders = new List<PointDataHolder>();

    public void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        rotator = transform.Find("Rotator");
    }

    public void Start()
    {
        defaultRadius = outerRadius;
    }

    public void UpdateGrounded()
    {
        isFloored = false;
        foreach (var data in dataHolders)
        {
            if (data.groundDetector.isFloored)
            {
                isFloored = true;
                return;
            }
        }
    }

    public List<CircleCollider2D> GetColliders()
    {
        var res = new List<CircleCollider2D>();
        GetComponentsInChildren<CircleCollider2D>(res);
        for (int i = 0; i < res.Count; i++)
        {
            if (res[i].gameObject == this.gameObject || res[i].transform.name == "Rotator")
            {
                res[i] = res[res.Count - 1];
                res.RemoveAt(res.Count - 1);
                break;
            }
        }

        return res;
    }

    void Update()
    {
        UpdateGrounded();
        horizontalInput = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
        if (Input.GetKey(KeyCode.Space) && isFloored)
        {
            Debug.Log("space key pressed");
            SetRadius(defaultRadius * expandedRatio);
        }
        else
        {
            SetRadius(defaultRadius);
        }

        UpdateVerticies();
        
        // Update rotator position and orientation
        if (rotator != null && dataHolders.Count > 0)
        {
            // Set position to first point
            rotator.localPosition = dataHolders[0].transform.localPosition;
            
            // Calculate outward direction
            Vector2 towardsCenter = (Vector2.zero - (Vector2)rotator.localPosition).normalized;
            Vector2 tangent = new Vector2(-towardsCenter.y, towardsCenter.x); // Perpendicular vector (tangent)
            
            // Set rotation to point in tangential direction
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            rotator.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private float CalculateCurrentAngularVelocity()
    {
        float totalAngularVelocity = 0f;
        foreach (var pointData in dataHolders)
        {
            Vector2 pointPosition = pointData.transform.localPosition;
            Vector2 tangent = new Vector2(-pointPosition.y, pointPosition.x).normalized;
            float pointAngularVelocity = Vector2.Dot(pointData.rigidBody.linearVelocity, -tangent);
            totalAngularVelocity += pointAngularVelocity;
        }
        return totalAngularVelocity / dataHolders.Count;
    }

    private void ApplyMovementForce(PointDataHolder pointData)
    {
        Vector2 pointPosition = pointData.transform.localPosition;
        Vector2 tangent = new Vector2(-pointPosition.y, pointPosition.x).normalized;
        pointData.rigidBody.AddForce(-tangent * rollSpeed * Time.fixedDeltaTime * horizontalInput.x);
    }

    private void ApplyDampingForce(PointDataHolder pointData)
    {
        Vector2 pointPosition = pointData.transform.localPosition;
        Vector2 tangent = new Vector2(-pointPosition.y, pointPosition.x).normalized;
        float pointVelocity = Vector2.Dot(pointData.rigidBody.linearVelocity, -tangent);
        Vector2 dampingDirection = pointVelocity > 0 ? tangent : -tangent;
        pointData.rigidBody.AddForce(dampingDirection * dampingForce * Mathf.Abs(pointVelocity) * Time.fixedDeltaTime);
    }
    public float CalculateSlipVelocity()
    {
        if (!isFloored)
            return 0f;
            
        Vector2 objectVelocity = rigidBody.linearVelocity;
        float maxVelocity= 0f;
        var objVelocity = objectVelocity.magnitude;
        // Find max velocity difference between object and grounded points
        foreach (var data in dataHolders)
        {
            if (data.groundDetector.isFloored)
            {
                maxVelocity = Mathf.Max(data.rigidBody.linearVelocity.magnitude, maxVelocity);
            }
        }
        
        return Mathf.Abs(maxVelocity - objVelocity);
    }

    public void FixedUpdate()
    {
        if (isFloored)
        {
            float currentAngularVelocity = CalculateCurrentAngularVelocity();
             slipVelocity = CalculateSlipVelocity();
            bool canAccelerate = Mathf.Abs(currentAngularVelocity) < maxAngularVelocity;
            bool isSlipping = slipVelocity > slipVelocityThreshold;
            
            foreach (var pointData in dataHolders)
            {
                if (!Mathf.Approximately(horizontalInput.x, 0) && !isSlipping)
                {
                    if (canAccelerate)
                    {
                        ApplyMovementForce(pointData);
                    }
                }
                else
                {
                    ApplyDampingForce(pointData);
                }
            }
        }
    }

    public void SetRadius(float radius)
    {
        // Update center joints
        foreach (var jointHolder in dataHolders)
        {
            // Update center joint distance
            jointHolder.centerJoint.distance = radius;
            
            // Update neighbor joint distances based on the new radius
            for (int i = 0; i < jointHolder.neighborJoints.Count; i++)
            {
                // Calculate chord length for neighbors based on their index (i+1 represents the neighbor distance)
                float angleToNeighbor = ((i + 1) * 360f / numPoints) * Mathf.Deg2Rad;
                float chordLength = 2 * radius * Mathf.Sin(angleToNeighbor / 2);
                jointHolder.neighborJoints[i].distance = chordLength;
            }
        }
        
        outerRadius = radius;
    }
    
    

    void UpdateVerticies()
    {
        var points = GetColliders();
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 vertex = points[i].transform.localPosition; // Changed to access transform through points[i]
            Vector2 towardsCenter = (Vector2.zero - vertex).normalized;
            float colliderRadius = points[i].radius; // Directly access radius from CircleCollider2D
            try
            {
                spriteShape.spline.SetPosition(i, (vertex - towardsCenter * colliderRadius));
            }
            catch
            {
                Debug.Log("Spline points are too close to each other.. recalculate");
                spriteShape.spline.SetPosition(i, (vertex - towardsCenter * (colliderRadius + splineOffset)));
            }

            Vector2 newRt = -Vector2.Perpendicular(towardsCenter) * tangentSmoothing;

            Vector2 newLt = Vector2.zero - (newRt);
            spriteShape.spline.SetRightTangent(i, newRt);
            spriteShape.spline.SetLeftTangent(i, newLt);
            spriteShape.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }
    }

    [ContextMenu("Spawn Physics Points")]
    public void SpawnPhysicsPoints()
    {
        dataHolders.Clear();
        var points = GetColliders();
        // Clear existing points if any
        foreach (CircleCollider2D point in points)
        {
            if (point != null)
                DestroyImmediate(point.gameObject);
        }

        if (numPoints < 2)
            throw new System.Exception("Number of points must be greater than 2");
        int pointCount = spriteShape.spline.GetPointCount();
        int pointsToRemove = pointCount - numPoints;
        int pointsToAdd = numPoints - pointCount;
        // Update sprite shape vertex count
        for (int i = 0; i < pointsToRemove; i++)
            spriteShape.spline.RemovePointAt(0);

        for (int i = 0; i < pointsToAdd; i++)
            spriteShape.spline.InsertPointAt(i,
                new Vector3((i + 3) * 2f, (i + 3) * 2f, 0)); // Space points diagonally with 2 unit spacing
        spriteShape.spline.isOpenEnded = false;
        // Create new points in a circle

        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * (360f / numPoints) * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * outerRadius,
                Mathf.Sin(angle) * outerRadius,
                0
            );

            GameObject newPoint = Instantiate(physicsPrefab, transform);
            newPoint.transform.localPosition = pos;
            
            var dataHolder = new PointDataHolder();
            dataHolder.transform = newPoint.transform;

            dataHolder.rigidBody = newPoint.GetComponent<Rigidbody2D>();
            dataHolder.groundDetector = newPoint.GetComponent<GroundDetector>();

            // Add spring joint to center
            dataHolder.centerJoint = newPoint.AddComponent<SpringJoint2D>();
            dataHolder.centerJoint.connectedBody = GetComponent<Rigidbody2D>();
            dataHolder.centerJoint.autoConfigureDistance = false;
            dataHolder.centerJoint.distance = outerRadius;
            dataHolder.centerJoint.frequency = centerSpringFrequency;
            dataHolder.centerJoint.dampingRatio = centerSpringDamping;
            
            dataHolders.Add(dataHolder);
        }

        points = GetColliders();
        // Add spring joints to neighbors
        for (int i = 0; i < numPoints; i++)
        {
            var point = points[i];
            var jointHolder = dataHolders[i];
            
            for (int j = 1; j <= neighborJointRadius; j++)
            {
                if (j == 0)
                    continue;
                int neighborIndex = (i + j + numPoints) % numPoints;
                if (points[neighborIndex] != null)
                {
                    SpringJoint2D neighborJoint = point.gameObject.AddComponent<SpringJoint2D>();
                    neighborJoint.connectedBody = points[neighborIndex].GetComponent<Rigidbody2D>();
                    neighborJoint.autoConfigureDistance = false;
                    neighborJoint.distance = 
                        Vector2.Distance(point.transform.position, points[neighborIndex].transform.position);
                    neighborJoint.frequency = neighborSpringFrequency;
                    neighborJoint.dampingRatio = neighborSpringDamping;
                    
                    jointHolder.neighborJoints.Add(neighborJoint);
                }
            }
        }

        UpdateVerticies();
    }

    // Optional helper method to get joints for a specific point
    public PointDataHolder GetJointsForPoint(int pointIndex)
    {
        if (pointIndex >= 0 && pointIndex < dataHolders.Count)
        {
            return dataHolders[pointIndex];
        }
        return null;
    }
}