using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMeshSimplifier;

public class ARMeshLine
{

	private List<Vector3> _positions;
	private int positionCount;

	private int _sides = 3;
	private float _radiusOne = 0.02f;
	private float _radiusTwo = 0.06f;
	private bool _useWorldSpace = true;
	private bool _useTwoRadii = false;

	private Vector3[] _vertices;
	
	private Mesh _mesh;
	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;
	private Vector3 prevPointDistance = Vector3.zero;
	GameObject container;
	private LineSettings settings;

	public ARMeshLine(LineSettings l)
	{
		this.settings = l;
	}

	public void AddNewLineRenderer(Transform parent, GameObject anchorContainer, Vector3 position)
	{
		
		if (!anchorContainer)
		{
			anchorContainer = new GameObject($"MeshRenderer");
			anchorContainer.transform.parent = parent;
		}
		anchorContainer.tag = settings.lineTagName;

		_meshRenderer = anchorContainer.AddComponent<MeshRenderer>();
		_meshFilter = anchorContainer.AddComponent<MeshFilter>();
		_mesh = new Mesh();
		_meshFilter.mesh = _mesh;
		_meshRenderer.material = settings.defaultMaterial;
		_radiusOne = settings.startWidth;
		_radiusTwo = settings.endWidth;
		container = anchorContainer;
		_positions = new List<Vector3>();
		_positions.Add(position);
		_positions.Add(position);

	}

	private void OnEnable()
	{
		_meshRenderer.enabled = true;
	}

	private void OnDisable()
	{
		_meshRenderer.enabled = false;
	}

	void Update()
	{
	}

	private void OnValidate()
	{
		_sides = Mathf.Max(3, _sides);
	}

	public void AddPoint(Vector3 position)
    {
		bool firstPoint = (prevPointDistance == Vector3.zero);
		if (firstPoint)
		{
			prevPointDistance = position;
		}

		if (prevPointDistance != null &&
			Mathf.Abs(Vector3.Distance(prevPointDistance, position)) >= settings.minDistanceBeforeNewPoint)
		{

			Vector3 smoothedPos = prevPointDistance;
			float smoothTime = settings.dampen;
			float xVelocity = 0.0f;
			float yVelocity = 0.0f;
			float zVelocity = 0.0f;

			if (firstPoint)
			{
				smoothedPos = position;
			}
			else
			{
				smoothedPos.x = Mathf.SmoothDamp(prevPointDistance.x, position.x, ref xVelocity, smoothTime);
				smoothedPos.y = Mathf.SmoothDamp(prevPointDistance.y, position.y, ref yVelocity, smoothTime); //-0.6f
				smoothedPos.z = Mathf.SmoothDamp(prevPointDistance.z, position.z, ref zVelocity, smoothTime);
			}

			

			_positions.Add(smoothedPos);
			positionCount += 1;
			prevPointDistance = smoothedPos;

			// applies simplification if reminder is 0
			if (positionCount % settings.applySimplifyAfterPoints == 0 &&
				settings.allowSimplification)
			{
				//float quality = 0.8f;
    //            var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier
    //            {
    //                Vertices = _mesh.vertices
    //            };
    //            for (int i = 0; i < _mesh.subMeshCount; i++)
				//{
				//	meshSimplifier.AddSubMeshTriangles(_mesh.GetTriangles(i));
				//}

				//// This is where the magic happens, lets simplify!
				//meshSimplifier.SimplifyMeshLossless();


				//int oldCount = _mesh.vertexCount;

				//var newMesh = new Mesh();
				//newMesh.subMeshCount = meshSimplifier.SubMeshCount;
				//newMesh.vertices = meshSimplifier.Vertices;

				//for (int i = 0; i < meshSimplifier.SubMeshCount; i++)
				//{
				//	newMesh.SetTriangles(meshSimplifier.GetSubMeshTriangles(i), 0);
				//}

				//_meshFilter.mesh = newMesh;
				//_mesh = newMesh;
				//_positions = new List<Vector3>(_mesh.vertices);

				

				//positionCount = _positions.Count;

				//Debug.Log("Before:[" + oldCount + "] After: [" + _mesh.vertexCount +"]");


				_mesh.Optimize();
				
			}
		
			GenerateMesh();

		}
	}

	private void GenerateMesh()
	{

		if (_mesh == null || _positions == null || _positions.Count <= 1)
		{
			//_mesh = new Mesh();
			return;
		}

		var verticesLength = _sides * _positions.Count;
		if (_vertices == null || _vertices.Length != verticesLength)
		{
			_vertices = new Vector3[verticesLength];

			var indices = GenerateIndices();
			var uvs = GenerateUVs();

			if (verticesLength > _mesh.vertexCount)
			{
				_mesh.vertices = _vertices;
				_mesh.triangles = indices;
				_mesh.uv = uvs;
			}
			else
			{
				_mesh.triangles = indices;
				_mesh.vertices = _vertices;
				_mesh.uv = uvs;
			}
		}

		var currentVertIndex = 0;

		for (int i = 0; i < _positions.Count; i++)
		{
			var circle = CalculateCircle(i);
			foreach (var vertex in circle)
			{
				_vertices[currentVertIndex++] = _useWorldSpace ? container.transform.InverseTransformPoint(vertex) : vertex;
			}
		}

		_mesh.vertices = _vertices;
		_mesh.RecalculateNormals();
		_mesh.RecalculateBounds();
		_meshFilter.mesh = _mesh;
	}

	private Vector2[] GenerateUVs()
	{
		var uvs = new Vector2[_positions.Count * _sides];

		for (int segment = 0; segment < _positions.Count; segment++)
		{
			for (int side = 0; side < _sides; side++)
			{
				var vertIndex = (segment * _sides + side);
				var u = side / (_sides - 1f);
				var v = segment / (_positions.Count - 1f);

				uvs[vertIndex] = new Vector2(u, v);
			}
		}

		return uvs;
	}

	private int[] GenerateIndices()
	{
		// Two triangles and 3 vertices
		var indices = new int[_positions.Count * _sides * 2 * 3];

		var currentIndicesIndex = 0;
		for (int segment = 1; segment < _positions.Count; segment++)
		{
			for (int side = 0; side < _sides; side++)
			{
				var vertIndex = (segment * _sides + side);
				var prevVertIndex = vertIndex - _sides;

				// Triangle one
				indices[currentIndicesIndex++] = prevVertIndex;
				indices[currentIndicesIndex++] = (side == _sides - 1) ? (vertIndex - (_sides - 1)) : (vertIndex + 1);
				indices[currentIndicesIndex++] = vertIndex;


				// Triangle two
				indices[currentIndicesIndex++] = (side == _sides - 1) ? (prevVertIndex - (_sides - 1)) : (prevVertIndex + 1);
				indices[currentIndicesIndex++] = (side == _sides - 1) ? (vertIndex - (_sides - 1)) : (vertIndex + 1);
				indices[currentIndicesIndex++] = prevVertIndex;
			}
		}

		return indices;
	}

	private Vector3[] CalculateCircle(int index)
	{
		var dirCount = 0;
		var forward = Vector3.zero;

		// If not first index
		if (index > 0)
		{
			forward += (_positions[index] - _positions[index - 1]).normalized;
			dirCount++;
		}

		// If not last index
		if (index < _positions.Count - 1)
		{
			forward += (_positions[index + 1] - _positions[index]).normalized;
			dirCount++;
		}

		// Forward is the average of the connecting edges directions
		forward = (forward / dirCount).normalized;
		var side = Vector3.Cross(forward, forward + new Vector3(.123564f, .34675f, .756892f)).normalized;
		var up = Vector3.Cross(forward, side).normalized;

		var circle = new Vector3[_sides];
		var angle = 0f;
		var angleStep = (2 * Mathf.PI) / _sides;

		var t = index / (_positions.Count - 1f);
		var radius = _useTwoRadii ? Mathf.Lerp(_radiusOne, _radiusTwo, t) : _radiusOne;

		for (int i = 0; i < _sides; i++)
		{
			var x = Mathf.Cos(angle);
			var y = Mathf.Sin(angle);

			circle[i] = _positions[index] + side * x * radius + up * y * radius;

			angle += angleStep;
		}

		return circle;
	}
}