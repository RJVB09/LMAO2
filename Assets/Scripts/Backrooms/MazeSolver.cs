using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Maze;

public class Frontier
{
    public Vector2Int position;
    public List<Direction> path;

    public Frontier(Vector2Int position)
    {
        this.position = position;
        path = new List<Direction>();
    }
    public Frontier(Vector2Int position, List<Direction> path = null)
    {
        this.position = position;
        this.path = path != null ? new List<Direction>(path) : new List<Direction>();
    }

    public void Move(Direction direction)
    {
        path.Add(direction);
        position += DirectionToVector(direction);
    }
}

public class MazeSolver
{
    public Maze maze;

    public MazeSolver(Maze maze)
    {
        this.maze = maze;
    }

    public List<Direction> BFS(Vector2Int a, Vector2Int b, int maxSteps)
    {
        if (maze.IsInMaze(a) && maze.IsInMaze(b))
        {
            for (int i = 0; i < maze.mazeSize * maze.mazeSize; i++)
            {
                maze.ModifyCell(i, p => true, c =>
                {
                    c.mazeCell = false;
                    c.frontierCell = false;
                    return c;
                });
            }

            List<Frontier> frontiers = new List<Frontier>();
            frontiers.Add(new Frontier(a));

            List<Direction> path = new List<Direction>();

            int step = 0;
            bool foundPath = false;
            while (step < maxSteps && !foundPath)
            {
                List<Frontier> newFrontiers = new List<Frontier>();
                List<int> markedForRemoval = new List<int>();
                if (frontiers.Count == 0)
                    break;

                foreach (Frontier frontier in frontiers)
                {
                    MazeJunction currentCell = maze.LookUpCell(frontier.position);
                    List<Direction> possibleDirections = maze.PossibleDirections(currentCell.position, !currentCell.connections, p => !p.mazeCell);
                    maze.MarkMaze(currentCell.position, true);

                    if (frontier.position == b)
                    {
                        foundPath = true;
                        path = frontier.path;
                        return path;
                    }
                    else if (possibleDirections.Count == 0)
                    {
                        markedForRemoval.Add(frontiers.IndexOf(frontier));
                    }
                    else if (possibleDirections.Count == 1)
                    {
                        frontier.Move(possibleDirections[0]);
                    }
                    else
                    {
                        for (int i = 0; i < possibleDirections.Count; i++)
                        {
                            Frontier newFrontier = new Frontier(frontier.position, frontier.path);
                            newFrontier.Move(possibleDirections[i]);
                            newFrontiers.Add(newFrontier);
                        }
                        markedForRemoval.Add(frontiers.IndexOf(frontier));
                    }
                }

                //frontiers.RemoveAt(markedForRemoval);
                frontiers.RemoveAll(x => markedForRemoval.Contains(frontiers.IndexOf(x)));
                frontiers = RemoveDuplicates(frontiers);

                frontiers.AddRange(newFrontiers);
                step++;
            }
        }

        return new List<Direction>();
    }

    public void DisplayPath(List<Direction> path, Vector2Int startingPoint, float duration)
    {
        int u = 0;
        Vector2Int pos = startingPoint;
        foreach (Direction dir in path)
        {
            Debug.DrawRay(maze.MazePosToWorldPos(pos) + Vector3.up, Vector3.right * DirectionToVector(dir).x * maze.mazeSpacing + Vector3.forward * DirectionToVector(dir).y * maze.mazeSpacing, Color.red, duration);
            pos += DirectionToVector(dir);
            u++;
        }
    }

    public void DisplayPath(List<Direction> path, Vector2Int startingPoint)
    {
        int u = 0;
        Vector2Int pos = startingPoint;
        foreach (Direction dir in path)
        {
            Debug.DrawRay(maze.MazePosToWorldPos(pos) + Vector3.up, Vector3.right * DirectionToVector(dir).x * maze.mazeSpacing + Vector3.forward * DirectionToVector(dir).y * maze.mazeSpacing, Color.red);
            pos += DirectionToVector(dir);
            u++;
        }
    }

    public void DisplayFrontier(List<Frontier> frontiers, float duration)
    {
        int u = 0;
        foreach (Frontier frontier in frontiers)
        {

            Debug.DrawRay(Vector3.forward * maze.MazePosToWorldPos(frontier.position).z + Vector3.right * maze.MazePosToWorldPos(frontier.position).x, Vector3.up * 2, Color.cyan, duration);
            u++;
        }
    }

    public static List<Frontier> RemoveDuplicates(List<Frontier> frontiers)
    {
        List<Frontier> result = new List<Frontier>();
        HashSet<Vector2Int> seenPositions = new HashSet<Vector2Int>();

        foreach (var frontier in frontiers)
        {
            if (seenPositions.Add(frontier.position))
            {
                result.Add(frontier);
            }
        }

        return result;
    }
}
