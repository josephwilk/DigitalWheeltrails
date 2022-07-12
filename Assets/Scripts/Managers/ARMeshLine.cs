using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMeshSimplifier;

public class ARMeshLine
{
	public MeshFilter _meshFilter;
	public MeshRenderer _meshRenderer;
	private LineSettings settings;
	//public Mesh mesh;

	public ARMeshLine(LineSettings l)
	{
		this.settings = l;
		//this.mesh = new Mesh();
	}

	public void addPoint(Vector3 point)
    {

    }


    public void AddNewMeshRenderer(Transform parent, GameObject anchorContainer, Mesh mesh)
	{	
		if (!anchorContainer)
		{
			anchorContainer = new GameObject($"MeshRenderer");
			anchorContainer.transform.parent = parent;
		}
		anchorContainer.tag = settings.lineTagName;

		_meshRenderer = anchorContainer.AddComponent<MeshRenderer>();
		_meshFilter = anchorContainer.AddComponent<MeshFilter>();
		_meshFilter.mesh = mesh;
		_meshRenderer.material = settings.defaultMaterial;
	}

}