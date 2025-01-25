using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.Linq;
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

    [System.Serializable]
    public class PointJointHolder
    {
        public SpringJoint2D centerJoint;
        public List<SpringJoint2D> neighborJoints = new List<SpringJoint2D>();
    }

    [SerializeField, HideInInspector]
    private List<PointJointHolder> jointHolders = new List<PointJointHolder>();

    public void Start()
    {
        defaultRadius = outerRadius;
    }

    public List<CircleCollider2D> GetColliders()
    {
        var res = new List<CircleCollider2D>();
        GetComponentsInChildren<CircleCollider2D>(res);
        for (int i = 0; i < res.Count; i++)
        {
            if (res[i].gameObject == this.gameObject)
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
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("space key pressed");
            SetRadius(defaultRadius * expandedRatio);
        }
        else
        {
            SetRadius(defaultRadius);
        }
        UpdateVerticies();
 
    }

    public void SetRadius(float radius)
    {
        // Update center joints
        foreach (var jointHolder in jointHolders)
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
        jointHolders.Clear();
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
            
            var jointHolder = new PointJointHolder();
            
            // Add spring joint to center
            jointHolder.centerJoint = newPoint.AddComponent<SpringJoint2D>();
            jointHolder.centerJoint.connectedBody = GetComponent<Rigidbody2D>();
            jointHolder.centerJoint.autoConfigureDistance = false;
            jointHolder.centerJoint.distance = outerRadius;
            jointHolder.centerJoint.frequency = centerSpringFrequency;
            jointHolder.centerJoint.dampingRatio = centerSpringDamping;
            
            jointHolders.Add(jointHolder);
        }

        points = GetColliders();
        // Add spring joints to neighbors
        for (int i = 0; i < numPoints; i++)
        {
            var point = points[i];
            var jointHolder = jointHolders[i];
            
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
    public PointJointHolder GetJointsForPoint(int pointIndex)
    {
        if (pointIndex >= 0 && pointIndex < jointHolders.Count)
        {
            return jointHolders[pointIndex];
        }
        return null;
    }
}