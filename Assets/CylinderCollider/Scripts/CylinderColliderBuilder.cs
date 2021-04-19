namespace Assets.CylinderCollider.Scripts
{
  using System.Collections.Generic;

  using UnityEngine;

  /// <summary>
  /// A class that allows to build a cylinder collider based on box colliders
  /// </summary>
  [ExecuteInEditMode]
  public class CylinderColliderBuilder : MonoBehaviour
  {
    public const int MinimumBoxCount = 4;

    private readonly Quaternion rotationXAxis = Quaternion.AngleAxis(90.0f, Vector3.forward);

    private readonly Quaternion rotationYAxis = Quaternion.identity;

    private readonly Quaternion rotationZAxis = Quaternion.AngleAxis(90.0f, Vector3.right);

    private readonly Color colorGizmosBuilded = new Color(0.0f, 1.0f, 0.0f, 0.25f);

    private readonly Color colorGizmosBuilding = Color.green;

    [SerializeField]
    [Tooltip("If enabled, the box colliders that make up this cylinder are used for triggering events, and is ignored by the physics engine")]
    private bool isTrigger;

    [SerializeField]
    [Tooltip("The material used by the box colliders that make up this cylinder")]
    private PhysicMaterial material;

    [SerializeField]
    [Tooltip("The position of the cylinder in the object's local space")]
    private Vector3 center;

    [SerializeField]
    [Tooltip("The radius of the cylinder's local width")]
    private float radius = 0.5f;

    [SerializeField]
    [Tooltip("The total height of the cylinder")]
    private float height = 1.0f;

    [SerializeField]
    [Tooltip("The total number of box colliders to use to build this cylinder")]
    private int boxCount = MinimumBoxCount;

    [SerializeField]
    [Tooltip("The axis of the cylinder's lengthwise orientation in the object's local space")]
    private DirectionEnum direction = DirectionEnum.YAxis;

    [SerializeField]
    [HideInInspector]
    private List<Collider> colliders = new List<Collider>();

    public enum DirectionEnum
    {
      XAxis,
      YAxis,
      ZAxis
    }

    /// <summary>
    /// If enabled, the box colliders that make up this cylinder are used for triggering events, and is ignored by the physics engine
    /// </summary>
    public bool IsTrigger
    {
      get
      {
        return this.isTrigger;
      }

      set
      {
        this.isTrigger = value;

        for (int i = 0; i < this.colliders.Count; i++)
        {
          var colliderComponent = this.colliders[i];
          if (colliderComponent != null)
          {
            colliderComponent.isTrigger = this.isTrigger;
          }
        }
      }
    }

    /// <summary>
    /// The shared material used by the box colliders that make up this cylinder
    /// </summary>
    public PhysicMaterial Material
    {
      get
      {
        return this.material;
      }

      set
      {
        this.material = value;

        for (int i = 0; i < this.colliders.Count; i++)
        {
          var colliderComponent = this.colliders[i];
          if (colliderComponent != null)
          {
            colliderComponent.sharedMaterial = this.material;
          }
        }
      }
    }

    /// <summary>
    /// The position of the cylinder in the object's local space
    /// </summary>
    public Vector3 Center
    {
      get
      {
        return this.center;
      }

      set
      {
        this.center = value;
      }
    }

    /// <summary>
    /// The radius of the cylinder's local width
    /// </summary>
    public float Radius
    {
      get
      {
        return this.radius;
      }

      set
      {
        this.radius = Mathf.Max(0, value);
      }
    }

    /// <summary>
    /// The total height of the cylinder
    /// </summary>
    public float Height
    {
      get
      {
        return this.height;
      }

      set
      {
        this.height = Mathf.Max(0, value);
      }
    }

    /// <summary>
    /// The total number of box colliders to use to build this cylinder
    /// </summary>
    public int BoxCount
    {
      get
      {
        return this.boxCount;
      }

      set
      {
        this.boxCount = Mathf.Max(MinimumBoxCount, value);
      }
    }

    /// <summary>
    /// The axis of the cylinder's lengthwise orientation in the object's local space
    /// </summary>
    public DirectionEnum Direction
    {
      get
      {
        return this.direction;
      }

      set
      {
        this.direction = value;
      }
    }

    /// <summary>
    /// Gets a value that indicates if the cylinder is built
    /// </summary>
    public bool IsBuilt
    {
      get
      {
        for (var i = 0; i < this.colliders.Count; i++)
        {
          if (this.colliders[i] != null)
          {
            return true;
          }
        }

        return false;
      }
    }

    /// <summary>
    /// Builds the box colliders that will make up the cylinder
    /// </summary>
    public void Build()
    {
      this.Clean();

      var rotationY = 0.0f;
      var rotationStep = this.CalculateRotationStep();
      var boxSize = this.CalculateBoxSize();
      var rotationFromDirection = this.GetRotationFromDirection();

      for (int i = 0; i < this.boxCount; i++)
      {
        this.CreateBoxCollider(i + 1, boxSize, rotationFromDirection * Quaternion.Euler(0.0f, rotationY, 0.0f));

        rotationY += rotationStep;
      }
    }

    /// <summary>
    /// Destroys the box colliders that make up this cylinder
    /// </summary>
    public void Clean()
    {
      foreach (var colliderComponent in this.colliders)
      {
        if (colliderComponent != null && colliderComponent.gameObject != null)
        {
          if (Application.isPlaying)
          {
            GameObject.DestroyImmediate(colliderComponent.gameObject);
          }
#if UNITY_EDITOR
          else
          {
            UnityEditor.Undo.DestroyObjectImmediate(colliderComponent.gameObject);
          }
#endif
        }
      }

      this.colliders.Clear();
    }

    protected void OnEnable()
    {
      for (var i = 0; i < this.colliders.Count; i++)
      {
        var colliderComponent = this.colliders[i];
        if (colliderComponent != null)
        {
          colliderComponent.enabled = true;
        }
      }
    }

    protected void OnDisable()
    {
      for (var i = 0; i < this.colliders.Count; i++)
      {
        var colliderComponent = this.colliders[i];
        if (colliderComponent != null)
        {
          colliderComponent.enabled = false;
        }
      }
    }

    protected void OnValidate()
    {
      this.radius = Mathf.Max(0, this.radius);
      this.height = Mathf.Max(0, this.height);
      this.boxCount = Mathf.Max(MinimumBoxCount, this.boxCount);

      foreach (var colliderComponent in this.colliders)
      {
        if (colliderComponent != null)
        {
          colliderComponent.isTrigger = this.isTrigger;
          colliderComponent.sharedMaterial = this.material;
        }
      }
    }

    protected void OnDrawGizmosSelected()
    {
      Gizmos.color = this.IsBuilt ? this.colorGizmosBuilded : this.colorGizmosBuilding;

      var rotationY = 0.0f;
      var rotationStep = this.CalculateRotationStep();
      var boxSize = this.CalculateBoxSize();
      var rotationFromDirection = this.GetRotationFromDirection();

      for (int i = 0; i < this.boxCount; i++)
      {
        Gizmos.matrix = this.transform.localToWorldMatrix * Matrix4x4.TRS(this.center, rotationFromDirection * Quaternion.Euler(0.0f, rotationY, 0.0f), Vector3.one);

        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        
        rotationY += rotationStep;
      }

      Gizmos.matrix = Matrix4x4.identity;
    }

    private void CreateBoxCollider(int index, Vector3 size, Quaternion rotation)
    {
      var boxCollider = new GameObject(string.Format("CylinderBox ({0})", index)).AddComponent<BoxCollider>();

      boxCollider.size = size;
      boxCollider.isTrigger = this.isTrigger;
      boxCollider.material = this.material;

      boxCollider.transform.localPosition = this.center;
      boxCollider.transform.localRotation = rotation;
      boxCollider.transform.SetParent(this.transform, false);
      boxCollider.transform.hideFlags = HideFlags.NotEditable;

      this.colliders.Add(boxCollider);

#if UNITY_EDITOR
      if (!Application.isPlaying)
      {
        UnityEditor.Undo.RegisterCreatedObjectUndo(boxCollider.gameObject, "Inspector");
      }
#endif
    }

    private Vector3 CalculateBoxSize()
    {
      var rotationStep = this.CalculateRotationStep();

      var first = Vector3.forward * this.radius;
      var second = Quaternion.Euler(0.0f, rotationStep, 0.0f) * first;

      var firstRightDirection = Vector3.right;
      var secondLeftDirection = Quaternion.Euler(0.0f, rotationStep, 0.0f) * (-firstRightDirection);

      var intersection = this.PointIntersection(first, firstRightDirection, second, secondLeftDirection);

      return new Vector3(Vector3.Distance(first, intersection) * 2.0f, this.height, this.radius * 2.0f);
    }

    private float CalculateRotationStep()
    {
      return 180.0f / this.boxCount;
    }

    private Vector3 PointIntersection(Vector3 point1, Vector3 directionLine1, Vector3 point2, Vector3 directionLine2)
    {
      point2.x -= point1.x;
      point2.y -= point1.y;
      point2.z -= point1.z;

      Vector3 crossLines = Vector3.Cross(directionLine1, directionLine2);

      var aux = Vector3.Dot(Vector3.Cross(point2, directionLine2), crossLines) / crossLines.sqrMagnitude;
      point1.x += directionLine1.x * aux;
      point1.y += directionLine1.y * aux;
      point1.z += directionLine1.z * aux;

      return point1;
    }

    private Quaternion GetRotationFromDirection()
    {
      switch (this.direction)
      {
        case DirectionEnum.XAxis:
          return this.rotationXAxis;

        case DirectionEnum.ZAxis:
          return this.rotationZAxis;
      }

      return this.rotationYAxis;
    }
  }
}