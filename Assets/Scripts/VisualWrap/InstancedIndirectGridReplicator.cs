// https://docs.unity3d.com/560/Documentation/ScriptReference/Graphics.DrawMeshInstancedIndirect.html

using UnityEngine;
using System.Collections.Generic;

public class InstancedIndirectGridReplicator
{

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private ComputeBuffer positionBuffer;


    public Vector3Int repetitionCount;
    public Vector3 repetitionSpacing;
    private int instanceCount; // count of how many instances of the current mesh,material combo / GameObject we will draw
    private Bounds bound = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));


    private struct MyRenderObject
    {
        public Mesh mesh;
        public Material material;
        public Transform transform;
        public ComputeBuffer argsBuffer;
    }
    private LinkedList<MyRenderObject> currentMyRenderObjectList = new LinkedList<MyRenderObject>();

    // Class Constructor
    public InstancedIndirectGridReplicator(Vector3Int repetitionCount, Vector3 repetitionSpacing)
    {
        this.repetitionCount = repetitionCount;
        this.repetitionSpacing = repetitionSpacing;
        SetupPostionBuffer();
    }

    public void AddAllChildGameObjects(GameObject parent)
    {
        foreach (Renderer childR in parent.GetComponentsInChildren<Renderer>())
        {
            AddGameObject(childR.gameObject);
        }
    }

    public void AddGameObject(GameObject thing)
    {
        Mesh instanceMesh = thing.GetComponent<MeshFilter>().mesh;  // the current mesh we are rendering
        Material instanceMaterial = thing.GetComponent<Renderer>().material;

        if (instanceMaterial.shader.name != "Instanced/InstancedIndirectSurfaceShader")
        {
            Debug.LogWarning("GameObject " + thing.name + " found with Material " + instanceMaterial.name + "  which doesn't' use InstancedIndirectSurfaceShader!!!");
            return;
        }
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);

        MyRenderObject ro = new MyRenderObject();
        ro.mesh = instanceMesh;
        ro.material = instanceMaterial;
        ro.transform = thing.transform;
        ro.argsBuffer = GenerateIndirectArgsBuffer(instanceMesh);
        currentMyRenderObjectList.AddLast(ro);
        // thing.GetComponent<Renderer>().enabled = false; // disable the Renderer so it doesn't render in the same spot as the instanced mesh for performance reasons.
    }

    public void RenderFrame()
    {
        foreach (MyRenderObject ro in currentMyRenderObjectList)
        {
            Mesh instanceMesh = ro.mesh;  // the current mesh we are rendering
            Material instanceMaterial = ro.material;  // the material to go on the current mesh we are rendering
            Transform instanceTransform = ro.transform;
            instanceMaterial.SetMatrix("_myworldToLocalMatrix", instanceTransform.worldToLocalMatrix);
            instanceMaterial.SetMatrix("_mylocalToWorldMatrix", instanceTransform.localToWorldMatrix);
            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, bound, ro.argsBuffer);
        }
    }

    ComputeBuffer GenerateIndirectArgsBuffer(Mesh instanceMesh)
    {
        //setup indirect args
        ComputeBuffer argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0; // < assumes no submeshes
        args[0] = numIndices;
        args[1] = (uint)instanceCount;
        argsBuffer.SetData(args);
        return argsBuffer;
    }

    void SetupPostionBuffer()
    {
        // count of repetition in each axis in just one direction, excluding center point
        int repSingleDirectionCountX = repetitionCount.x;
        int repSingleDirectionCountY = repetitionCount.y;
        int repSingleDirectionCountZ = repetitionCount.z;

        int repSizeX = repSingleDirectionCountX * 2 + 1;
        int repSizeY = repSingleDirectionCountY * 2 + 1;
        int repSizeZ = repSingleDirectionCountZ * 2 + 1;

        instanceCount = repSizeX * repSizeY * repSizeZ; // subtract 1 if the center spot is ignored

        // Positions & Colors
        if (positionBuffer != null) positionBuffer.Release();

        positionBuffer = new ComputeBuffer(instanceCount, 16);
        Vector4[] positions = new Vector4[instanceCount];


        int xIndex = 0;
        int yIndex = 0;
        int zIndex = 0;

        int i = 0;
        for (xIndex = 0; xIndex < repSizeX; ++xIndex)
        {
            for (yIndex = 0; yIndex < repSizeY; ++yIndex)
            {
                for (zIndex = 0; zIndex < repSizeZ; ++zIndex)
                {
                    float xPos = (xIndex - repSingleDirectionCountX) * repetitionSpacing.x;
                    float yPos = (yIndex - repSingleDirectionCountY) * repetitionSpacing.y;
                    float zPos = (zIndex - repSingleDirectionCountZ) * repetitionSpacing.z;
                    positions[i] = new Vector4(xPos, yPos, zPos, 1);
                    ++i;
                }
            }
        }

        positionBuffer.SetData(positions);
    }

    public void cleanupForDeletion()
    {
        // cleanup statements...
        if (positionBuffer != null) positionBuffer.Release();
        positionBuffer = null;

        foreach (MyRenderObject ro in currentMyRenderObjectList)
        {
            if (ro.argsBuffer != null) ro.argsBuffer.Release();
        }
    }

    ~InstancedIndirectGridReplicator()  // finalizer /destructor
    {
        cleanupForDeletion();
    }
}
