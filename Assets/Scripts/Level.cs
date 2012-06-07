using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class Level
{
    public readonly int Size;
    public readonly GameObject[][][] Cells;
    public readonly string DialogText;
    public readonly int FairCount;

    public int PlacedCount { get; set; }

    public float SinceAlive { get; set; }

    public Level(int levelSize) : this(levelSize, string.Empty, 3) { }
    public Level(int levelSize, string dialogText, int fairCount)
    {
        Size = levelSize;
        DialogText = dialogText;
        FairCount = fairCount;

        Cells = new GameObject[Size][][];
        for (int i = 0; i < Size; i++)
        {
            Cells[i] = new GameObject[Size][];
            for (int j = 0; j < Size; j++)
                Cells[i][j] = new GameObject[Size];
        }
    }

    GameObject GetCell(Vector3 worldPosition)
    {
        var cellIndex = (worldPosition + new Vector3(Size, Size, Size) / 2).Round();
        if (cellIndex.x < 0 || cellIndex.x >= Size || cellIndex.y < 0 || cellIndex.y >= Size || cellIndex.z < 0 || cellIndex.z >= Size)
            return null;
        return Cells[(int)cellIndex.x][(int)cellIndex.y][(int)cellIndex.z];
    }

    public GameObject GetEmitterAt(Vector3 worldPosition)
    {
        var cell = GetCell(worldPosition);
        return cell == null ? null : cell.tag == "Emitter" ? cell : null;
    }
    public GameObject GetProjectorAt(Vector3 worldPosition)
    {
        var cell = GetCell(worldPosition);
        return cell == null ? null : cell.tag == "Projector" ? cell : null;
    }
    public GameObject GetAccumulatorAt(Vector3 worldPosition)
    {
        var cell = GetCell(worldPosition);
        return cell == null ? null : cell.tag == "Accumulator" ? cell : null;
    }
    public GameObject GetReceiverAt(Vector3 worldPosition)
    {
        var cell = GetCell(worldPosition);
        return cell == null ? null : cell.tag == "Receiver" ? cell : null;
    }

    public GameObject AddEmitterAt(int x, int y, int z, Direction emitInto, Color color)
    {
        var go = Object.Instantiate(Pico.EmitterTemplate) as GameObject;
        Cells[x][y][z] = go;
        var behaviour = go.GetComponent<EmitterBehaviour>();
        behaviour.transform.position = new Vector3(x, y, z) - new Vector3(Size, Size, Size) / 2;
        behaviour.renderer.material.color = color.Round();
        behaviour.EmitInto = emitInto;
        return go;
    }

    public GameObject AddReceiverAt(int x, int y, int z, Func<Level> nextLevel, params Color[] sequence)
    {
        var go = Object.Instantiate(Pico.ReceiverTemplate) as GameObject;
        Cells[x][y][z] = go;
        var behaviour = go.GetComponent<ReceiverBehaviour>();
        behaviour.NextLevel = nextLevel;
        behaviour.transform.position = new Vector3(x, y, z) - new Vector3(Size, Size, Size) / 2;
        behaviour.ColorSequence = sequence.Select(c => c.Round()).ToArray();
        return go;
    }

    public GameObject AddProjectorAt(int x, int y, int z, Direction projectInto)
    {
        var go = Object.Instantiate(Pico.ProjectorTemplate) as GameObject;
        Cells[x][y][z] = go;
        var behaviour = go.GetComponent<ProjectorBehaviour>();
        behaviour.transform.position = new Vector3(x, y, z) - new Vector3(Size, Size, Size) / 2;
        behaviour.ProjectInto = projectInto;
        return go;
    }

    public GameObject AddAccumulatorAt(int x, int y, int z)
    {
        var go = Object.Instantiate(Pico.AccumulatorTemplate) as GameObject;
        Cells[x][y][z] = go;
        var behaviour = go.GetComponent<AccumulatorBehaviour>();
        behaviour.transform.position = new Vector3(x, y, z) - new Vector3(Size, Size, Size) / 2;
        return go;
    }

    public void FallDown()
    {
        foreach (var behaviour in Object.FindObjectsOfType(typeof(EmitterBehaviour)).Cast<EmitterBehaviour>())
            behaviour.FallingDown = true;
        foreach (var behaviour in Object.FindObjectsOfType(typeof(AccumulatorBehaviour)).Cast<AccumulatorBehaviour>())
            behaviour.FallingDown = true;
        foreach (var behaviour in Object.FindObjectsOfType(typeof(ReceiverBehaviour)).Cast<ReceiverBehaviour>())
            behaviour.FallingDown = true;
        foreach (var behaviour in Object.FindObjectsOfType(typeof(ProjectorBehaviour)).Cast<ProjectorBehaviour>())
            if (!behaviour.IsGizmo)
                behaviour.FallingDown = true;
        foreach (var behaviour in Object.FindObjectsOfType(typeof(EnergyBehaviour)).Cast<EnergyBehaviour>())
            behaviour.FallingDown = true;
    }

    public void Destroy()
    {
        foreach (var go in Cells.SelectMany(x => x).SelectMany(x => x))
            if (go != null)
                Object.Destroy(go);

        foreach (var energy in Object.FindObjectsOfType(typeof(EnergyBehaviour)).Cast<EnergyBehaviour>())
            Object.Destroy(energy.gameObject);
    }
}
