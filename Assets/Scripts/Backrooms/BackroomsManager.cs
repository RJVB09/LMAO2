using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Maze;
using static MazeDecorator;
using static MazeSolver;

public class BackroomsManager : MonoBehaviour
{
    public float mazeSpacing = 6;
    public int mazeSize = 10;

    public Maze maze;
    MazeDecorator decorator;

    public Mesh wallMesh;
    public Material wallMaterial;
    public float wallThickness = 0.1f;
    public float wallHeight = 2f;

    public GameObject lamp;
    public GameObject computer;
    public GameObject[] props;
    public GameObject[] papers;
    public GameObject exit;

    BackroomsExit[] exits;

    public Transform player;

    public BackroomEntity entity;
    public Text UICounter;

    public int computerAmount = 5;
    public int activatedComputers = 0;

    bool exitsOpened = false;

    public bool openExits = false;

    public bool playerEscaped = false;

    private void Start()
    {
        maze = new Maze(mazeSpacing, mazeSize, false);

        //maze.ModifyCell(Vector2Int.zero, p => true, input => { input.connections = Vector4.one; return input; });

        //maze.GeneratePrimMaze(0f, 1000, mazeSize / 2 * Vector2Int.one);
        //maze.GenerateEllerMaze();

        //maze.GeneratePrimCircularRoot(0.3f, 1000);
        //maze.GenerateWilsonMaze(1000);
        
        //UnityEngine.Random.InitState(43578);

        maze.GenerateOriginShift(0.1f, 1f);
        //
        decorator = new MazeDecorator(maze);

        decorator.GenerateWalls(wallMesh, wallMaterial, wallThickness, wallHeight);

        //Place the exits
        decorator.ScatterObject(1, 1, exit, p => p.position.y == mazeSize - 1 && (p.position.x >= 2 && p.position.x < mazeSize - 2), (j, s) => s, true, false);
        decorator.ScatterObject(1, 1, exit, p => p.position.y == 0 && (p.position.x >= 2 && p.position.x < mazeSize - 2), (j, s) => { s.transform.Rotate(Vector3.up * 180); return s; }, true, false);
        decorator.ScatterObject(1, 1, exit, p => p.position.x == mazeSize - 1 && (p.position.y >= 2 && p.position.y < mazeSize - 2), (j, s) => { s.transform.Rotate(Vector3.up * 90); return s; }, true, false);
        decorator.ScatterObject(1, 1, exit, p => p.position.x == 0 && (p.position.y >= 2 && p.position.y < mazeSize - 2), (j, s) => { s.transform.Rotate(Vector3.up * 270); return s; }, true, false);

        exits = GameObject.FindObjectsOfType<BackroomsExit>();

        //Place all the lights
        decorator.ScatterObject(0.5f, lamp, false, true);

        //Place all 5 computers, mindistance is the mazesize divided by minimal packing radius of circles in a square
        decorator.ScatterObject(computerAmount, mazeSize / 4.828f, computer, p => NumberOfDirections(p.connections) <= 3, wallPlacement, true, false);

        decorator.ScatterObject(0.05f, props, false, false);

        decorator.ScatterObject(1, 1f, papers, p => NumberOfDirections(p.connections) <= 3, wallPlacement, false, false);

        /*
        decorator.ScatterObject(5, 6, computer, p => NumberOfDirections(p.connections) <= 3, (j, s) =>
        {
            List<Direction> walls = RetrieveDirections(!j.connections);
            s.transform.rotation = DirectionToRotation(walls[UnityEngine.Random.Range(0, walls.Count)]);
            return s;
        });
        */

        //solver.DisplayFrontier(frontiers, Mathf.Infinity);

        //CreateWall(mazeGameobject.transform, Vector2.zero, Direction.NegativeX, mazeSpacing, wallThickness, wallHeight);

        entity.maze = maze;
        entity.Spawn();
    }

    private void Update()
    {

        //Debug.Log(maze.WorldPosToMazePos(player.position));

        UICounter.text = "PCs: " + activatedComputers + "/" + computerAmount;
        entity.walkSpeed = Mathf.Lerp(3f, entity.runSpeed, activatedComputers / (float)computerAmount);
        entity.walkTurnSpeed = Mathf.Lerp(3f, entity.runSpeed, activatedComputers / (float)computerAmount);

        foreach (MazeJunction mazeJunction in maze.mazeJunctions)
        {
            Vector3 position = maze.MazePosToWorldPos(mazeJunction.position);
            Debug.DrawRay(position, Vector3.up, mazeJunction.mazeCell ? Color.green : (mazeJunction.frontierCell ? Color.yellow : Color.blue));
            Debug.DrawRay(position + Vector3.up, new Vector3(DirectionToVector(mazeJunction.direction).x, 0, DirectionToVector(mazeJunction.direction).y), Color.magenta);

            if (mazeJunction.connections.xp)
            {
                Debug.DrawLine(position, position + Vector3.right * mazeSpacing * 0.5f, mazeJunction.mazeCell ? Color.green : (mazeJunction.frontierCell ? Color.yellow : Color.blue));
            }
            if (mazeJunction.connections.xn)
            {
                Debug.DrawLine(position, position + Vector3.left * mazeSpacing * 0.5f, mazeJunction.mazeCell ? Color.green : (mazeJunction.frontierCell ? Color.yellow : Color.blue));
            }
            if (mazeJunction.connections.yp)
            {
                Debug.DrawLine(position, position + Vector3.forward * mazeSpacing * 0.5f, mazeJunction.mazeCell ? Color.green : (mazeJunction.frontierCell ? Color.yellow : Color.blue));
            }
            if (mazeJunction.connections.yn)
            {
                Debug.DrawLine(position, position + Vector3.back * mazeSpacing * 0.5f, mazeJunction.mazeCell ? Color.green : (mazeJunction.frontierCell ? Color.yellow : Color.blue));
            }

            if (mazeJunction.walls.xp)
            {
                Debug.DrawLine(position + Vector3.up, position + Vector3.right * mazeSpacing * 0.5f + Vector3.up, Color.white);
            }
            if (mazeJunction.walls.xn)
            {
                Debug.DrawLine(position + Vector3.up, position + Vector3.left * mazeSpacing * 0.5f + Vector3.up, Color.white);
            }
            if (mazeJunction.walls.yp)
            {
                Debug.DrawLine(position + Vector3.up, position + Vector3.forward * mazeSpacing * 0.5f + Vector3.up, Color.white);
            }
            if (mazeJunction.walls.yn)
            {
                Debug.DrawLine(position + Vector3.up, position + Vector3.back * mazeSpacing * 0.5f + Vector3.up, Color.white);
            }
        }

        if ((activatedComputers == computerAmount && !exitsOpened) || openExits)
        {
            exitsOpened = true;
            foreach (BackroomsExit exit in exits)
            {
                exit.available = true;
            }
        }

        if (exitsOpened && !playerEscaped)
            foreach (BackroomsExit exit in exits)
            {
                playerEscaped = playerEscaped || exit.playerWentThrough;
            }
    }
}
