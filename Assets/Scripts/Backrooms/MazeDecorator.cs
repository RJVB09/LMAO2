using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Maze;

public class MazeDecorator
{
    public Maze maze;
    GameObject mazeGameobject;

    public delegate GameObject ScatterModifier(MazeJunction junction, GameObject scatter);

    public static GameObject wallPlacement(MazeJunction junction, GameObject scatter)
    {
        List<Direction> walls = RetrieveDirections(!junction.connections);
        scatter.transform.rotation = DirectionToRotation(walls[UnityEngine.Random.Range(0, walls.Count)]);
        return scatter;
    }

    List<Vector2Int> occupiedPositions = new List<Vector2Int>();

    public MazeDecorator(Maze maze)
    {
        this.maze = maze;
        mazeGameobject = new GameObject();
        mazeGameobject.name = "Maze";
    }

    public void GenerateWallMarkers(Bool4 mask)
    {
        for (int i = 0; i < maze.mazeSize * maze.mazeSize; i++)
        {
            maze.ModifyCell(i, p => true, c =>
            {
                c.walls = mask * !c.connections;
                return c;
            });
        }
    }

    public void MarkOccupied(Vector2Int position)
    {
        occupiedPositions.Add(position);
    }

    public void CreateWall(Transform parent, Vector2 origin, Direction direction, float length, float thickness, float height, Mesh wallMesh, Material wallMaterial)
    {
        GameObject wall = new GameObject();
        MeshFilter mf = wall.AddComponent<MeshFilter>();
        MeshRenderer mr = wall.AddComponent<MeshRenderer>();
        BoxCollider c = wall.AddComponent<BoxCollider>();
        Transform t = wall.transform;

        wall.name = "Wall";
        mf.sharedMesh = wallMesh;
        mr.material = wallMaterial;

        Vector2 dir = (Vector2)DirectionToVector(direction) * length;

        wall.transform.SetParent(parent);

        t.position = new Vector3(origin.x + dir.x * 0.5f, height * 0.5f, origin.y + dir.y * 0.5f);
        //t.localScale = new Vector3(Mathf.Abs(dir.x), height, Mathf.Abs(dir.y)) + new Vector3(dir.x == 0 ? thickness : 0, 0, dir.y == 0 ? thickness : 0);
        t.localScale = new Vector3(length, height, thickness);
        t.rotation = DirectionToRotation(direction);
        wall.isStatic = true;
    }

    public void CreateCorner(Transform parent, Vector2 origin, float thickness, float height, Mesh wallMesh, Material wallMaterial)
    {
        GameObject corner = new GameObject();
        MeshFilter mf = corner.AddComponent<MeshFilter>();
        MeshRenderer mr = corner.AddComponent<MeshRenderer>();
        BoxCollider c = corner.AddComponent<BoxCollider>();
        Transform t = corner.transform;

        corner.name = "Corner";
        mf.sharedMesh = wallMesh;
        mr.material = wallMaterial;

        corner.transform.SetParent(parent);

        t.position = new Vector3(origin.x, height * 0.5f, origin.y);
        //t.localScale = new Vector3(Mathf.Abs(dir.x), height, Mathf.Abs(dir.y)) + new Vector3(dir.x == 0 ? thickness : 0, 0, dir.y == 0 ? thickness : 0);
        t.localScale = new Vector3(thickness * 0.5f, height, thickness * 0.5f);
        corner.isStatic = true;
    }

    public void ScatterObject(float chance, GameObject obj, bool occupyPosition, bool ignoreOccupation)
    {
        foreach (MazeJunction junction in maze.mazeJunctions)
        {
            if (UnityEngine.Random.Range(0f, 1f) <= chance && (ignoreOccupation || !occupiedPositions.Contains(junction.position)))
            {
                GameObject clone = UnityEngine.Object.Instantiate(obj, maze.MazePosToWorldPos(junction.position), Quaternion.identity, mazeGameobject.transform);
                clone.SetActive(true);
                if (occupyPosition)
                    occupiedPositions.Add(junction.position);
            }
        }
    }

    public void ScatterObject(float chance, GameObject[] objs, bool occupyPosition, bool ignoreOccupation)
    {
        foreach (MazeJunction junction in maze.mazeJunctions)
        {
            if (UnityEngine.Random.Range(0f, 1f) <= chance && (ignoreOccupation || !occupiedPositions.Contains(junction.position)))
            {
                GameObject clone = UnityEngine.Object.Instantiate(objs[UnityEngine.Random.Range(0, objs.Length)], maze.MazePosToWorldPos(junction.position), Quaternion.identity, mazeGameobject.transform);
                clone.SetActive(true);
                if (occupyPosition)
                    occupiedPositions.Add(junction.position);
            }
        }
    }

    public void ScatterObject(float chance, GameObject obj, Predicate<MazeJunction> filter, ScatterModifier modifier, bool occupyPosition, bool ignoreOccupation)
    {
        List<MazeJunction> filteredJunctions = maze.mazeJunctions.FindAll(filter);

        foreach (MazeJunction junction in filteredJunctions)
        {
            if (UnityEngine.Random.Range(0f, 1f) <= chance && (ignoreOccupation || !occupiedPositions.Contains(junction.position)))
            {
                GameObject clone = UnityEngine.Object.Instantiate(obj, maze.MazePosToWorldPos(junction.position), Quaternion.identity, mazeGameobject.transform);
                clone = modifier(junction, clone);
                clone.SetActive(true);
                if (occupyPosition)
                    occupiedPositions.Add(junction.position);
            }
        }
    }

    public void ScatterObject(float chance, GameObject[] objs, Predicate<MazeJunction> filter, ScatterModifier modifier, bool occupyPosition, bool ignoreOccupation)
    {
        List<MazeJunction> filteredJunctions = maze.mazeJunctions.FindAll(filter);

        foreach (MazeJunction junction in filteredJunctions)
        {
            if (UnityEngine.Random.Range(0f, 1f) <= chance && (ignoreOccupation || !occupiedPositions.Contains(junction.position)))
            {
                GameObject clone = UnityEngine.Object.Instantiate(objs[UnityEngine.Random.Range(0, objs.Length)], maze.MazePosToWorldPos(junction.position), Quaternion.identity, mazeGameobject.transform);
                clone = modifier(junction, clone);
                clone.SetActive(true);
                if (occupyPosition)
                    occupiedPositions.Add(junction.position);
            }
        }
    }

    public void ScatterObject(int count, float minDistance, GameObject obj, Predicate<MazeJunction> filter, ScatterModifier modifier, bool occupyPosition, bool ignoreOccupation)
    {
        List<MazeJunction> filteredJunctions = maze.mazeJunctions.FindAll(filter);
        filteredJunctions = filteredJunctions.FindAll(j => (ignoreOccupation || !occupiedPositions.Contains(j.position)));

        

        int scattersPlaced = 0;

        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, filteredJunctions.Count);
            MazeJunction junction = filteredJunctions[randomIndex];
            Vector2 randomPos = junction.position;

            filteredJunctions.RemoveAll(j => (j.position - randomPos).magnitude < minDistance);

            GameObject clone = UnityEngine.Object.Instantiate(obj, maze.MazePosToWorldPos(randomPos), Quaternion.identity, mazeGameobject.transform);
            clone = modifier(junction, clone);
            clone.SetActive(true);
            scattersPlaced++;
            if (occupyPosition)
                occupiedPositions.Add(junction.position);
        }

        foreach (MazeJunction junction in filteredJunctions)
        {
            Debug.DrawRay(maze.MazePosToWorldPos(junction.position), Vector3.one, Color.yellow, 1000);
        }
    }

    public void ScatterObject(int count, float minDistance, GameObject[] objs, Predicate<MazeJunction> filter, ScatterModifier modifier, bool occupyPosition, bool ignoreOccupation)
    {
        List<MazeJunction> filteredJunctions = maze.mazeJunctions.FindAll(filter);
        filteredJunctions = filteredJunctions.FindAll(j => (ignoreOccupation || !occupiedPositions.Contains(j.position)));



        int scattersPlaced = 0;

        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, filteredJunctions.Count);
            MazeJunction junction = filteredJunctions[randomIndex];
            Vector2 randomPos = junction.position;

            filteredJunctions.RemoveAll(j => (j.position - randomPos).magnitude < minDistance);

            GameObject clone = UnityEngine.Object.Instantiate(objs[UnityEngine.Random.Range(0, objs.Length)], maze.MazePosToWorldPos(randomPos), Quaternion.identity, mazeGameobject.transform);
            clone = modifier(junction, clone);
            clone.SetActive(true);
            scattersPlaced++;
            if (occupyPosition)
                occupiedPositions.Add(junction.position);
        }

        foreach (MazeJunction junction in filteredJunctions)
        {
            Debug.DrawRay(maze.MazePosToWorldPos(junction.position), Vector3.one, Color.yellow, 1000);
        }
    }


    public void GenerateWalls(Mesh wallMesh, Material wallMaterial, float wallThickness, float wallHeight)
    {
        GenerateWallMarkers(!new Bool4(true, false, false, true));
        for (int j = 0; j < maze.mazeSize; j++)
        {
            for (int i = 0; i < maze.mazeSize; i++)
            {
                Vector2Int currentPos = new Vector2Int(i, j);
                MazeJunction currentCell = maze.LookUpCell(currentPos);
                List<Direction> directions = RetrieveDirections(currentCell.walls);

                foreach (Direction direction in directions)
                {
                    Vector2Int pointerPos;
                    Direction wallDirection = Rotate(direction, -1);
                    Vector2Int wallDirectionVector = DirectionToVector(wallDirection);
                    Vector2Int directionVector = DirectionToVector(direction);

                    int k = 0;

                    for (k = 0; k < maze.mazeSize; k++)
                    {
                        pointerPos = currentPos + wallDirectionVector * k;
                        if (!maze.IsInMaze(pointerPos))
                            break;

                        MazeJunction pointerCell = maze.LookUpCell(pointerPos);

                        if (!Bool4ContainsDirection(direction, pointerCell.walls))
                            break;

                        maze.ModifyCell(pointerPos, p => true, c =>
                        {
                            c.walls = SetDirection(direction, c.walls, false);
                            return c;
                        });


                    }
                    Vector2 origin = (Vector2)currentPos + (Vector2)(-DirectionToVector(wallDirection) + DirectionToVector(direction)) * 0.5f;
                    Vector3 position = maze.MazePosToWorldPos(origin);

                    //Debug.Log(Enum.GetName(typeof(Direction), direction) + ", " + Enum.GetName(typeof(Direction), wallDirection) + ", " + k + ", " + origin + ", " + position);

                    if (k != maze.mazeSize)
                        CreateWall(mazeGameobject.transform, new Vector2(position.x, position.z), wallDirection, k * maze.mazeSpacing, wallThickness, wallHeight, wallMesh, wallMaterial);
                }
            }
        }

        for (int j = 0; j < maze.mazeSize; j++)
        {
            for (int i = 0; i < maze.mazeSize; i++)
            {
                Vector2Int currentPos = new Vector2Int(i, j);
                MazeJunction currentCell = maze.LookUpCell(currentPos);


                Vector2 origin = (Vector2)currentPos;
                Vector3 position = maze.MazePosToWorldPos(origin);

                List<Direction> directions = RetrieveDirections(Bool4.one);

                foreach (Direction direction in directions)
                {
                    Vector2Int diagonalVector = DirectionToVector(direction) + DirectionToVector(Rotate(direction, 1));
                    Vector2Int neighbourPos = currentPos + diagonalVector;

                    if (!maze.IsInMaze(neighbourPos))
                        continue;

                    MazeJunction neighbourCell = maze.LookUpCell(neighbourPos);

                    if (!Bool4ContainsDirection(Reverse(direction), neighbourCell.connections) &&
                        !Bool4ContainsDirection(Reverse(Rotate(direction, 1)), neighbourCell.connections) &&
                        Bool4ContainsDirection(direction, currentCell.connections) &&
                        Bool4ContainsDirection(Rotate(direction, 1), currentCell.connections)
                    )
                        CreateCorner(mazeGameobject.transform, new Vector2(position.x, position.z) + (Vector2)diagonalVector * (maze.mazeSpacing - wallThickness * 0.5f) * 0.5f, wallThickness, wallHeight, wallMesh, wallMaterial);

                }
            }
        }
    }
}
