using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Maze;

public class Maze
{
    public delegate MazeJunction Modification(MazeJunction input);

    public float mazeSpacing = 6;
    public int mazeSize = 10;
    public List<MazeJunction> mazeJunctions;

    public Vector2Int[] neighbours = new Vector2Int[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
    public Vector2Int[] corners = new Vector2Int[] { Vector2Int.right + Vector2Int.up, Vector2Int.left + Vector2Int.up, Vector2Int.up + Vector2Int.right, Vector2Int.down + Vector2Int.right };
    public Vector2Int[] cornersNormalized = new Vector2Int[] { Vector2Int.right, Vector2Int.up, Vector2Int.one, Vector2Int.zero };

    public enum Direction
    {
        PositiveX,
        PositiveY,
        NegativeX,
        NegativeY,
        None,
    }

    public static Vector2Int DirectionToVector(Direction dir)
    {
        return new Vector2Int(dir == Direction.PositiveX ? 1 : 0 + dir == Direction.NegativeX ? -1 : 0, dir == Direction.PositiveY ? 1 : 0 + dir == Direction.NegativeY ? -1 : 0);
    }

    public static Direction Rotate(Direction dir, int amount)
    {
        return (Direction)(((int)dir + amount) % 4);
    }

    public static Direction Reverse(Direction dir)
    {
        return (Direction)(((int)dir + 2) % 4);
    }

    public static Quaternion DirectionToRotation(Direction dir)
    {
        return Quaternion.Euler(0, -90f * ((int)dir != 4 ? (int)dir : 0), 0);
    }

    public static bool Bool4ContainsDirection(Direction dir, Bool4 bool4)
    {
        switch (dir)
        {
            case Direction.PositiveX:
                return bool4.xp;
            case Direction.NegativeX:
                return bool4.xn;
            case Direction.PositiveY:
                return bool4.yp;
            case Direction.NegativeY:
                return bool4.yn;
            case Direction.None:
                return !bool4.xp && !bool4.xn && !bool4.yp && !bool4.yn;
        }

        return false;
    }

    public static Bool4 SetDirection(Direction dir, Bool4 bool4, bool value)
    {
        switch (dir)
        {
            case Direction.PositiveX:
                bool4.xp = value;
                break;
            case Direction.NegativeX:
                bool4.xn = value;
                break;
            case Direction.PositiveY:
                bool4.yp = value;
                break;
            case Direction.NegativeY:
                bool4.yn = value;
                break;
            case Direction.None:
                break;
        }

        return bool4;
    }

    public static List<Direction> RetrieveDirections(Bool4 bool4)
    {
        List<Direction> output = new List<Direction>();

        if (bool4.xp)
            output.Add(Direction.PositiveX);

        if (bool4.xn)
            output.Add(Direction.NegativeX);

        if (bool4.yp)
            output.Add(Direction.PositiveY);

        if (bool4.yn)
            output.Add(Direction.NegativeY);

        return output;

    }

    public static Direction RandomDirection()
    {
        return (Direction)UnityEngine.Random.Range(0, 4);
    }

    public enum Turn
    {
        Left,
        Right,
        Around,
        None
    }

    public static Turn GetTurnDirection(Direction currentDirection, Direction targetDirection)
    {
        if (currentDirection == targetDirection || targetDirection == Direction.None)
        {
            return Turn.None;
        }
        if (currentDirection == Reverse(targetDirection))
        {
            return Turn.Around;
        }

        switch (currentDirection)
        {
            case Direction.PositiveX:
                return targetDirection == Direction.PositiveY ? Turn.Left : Turn.Right;

            case Direction.PositiveY:
                return targetDirection == Direction.NegativeX ? Turn.Left : Turn.Right;

            case Direction.NegativeX:
                return targetDirection == Direction.NegativeY ? Turn.Left : Turn.Right;

            case Direction.NegativeY:
                return targetDirection == Direction.PositiveX ? Turn.Left : Turn.Right;

            default:
                return Turn.None;
        }
    }

    public struct Bool4
    {
        public Bool4(bool xp, bool xn, bool yp, bool yn)
        {
            this.xp = xp;
            this.xn = xn;
            this.yp = yp;
            this.yn = yn;
        }

        public bool xp { get; set; }
        public bool xn { get; set; }
        public bool yp { get; set; }
        public bool yn { get; set; }

        public static Bool4 operator +(Bool4 a, Bool4 b) => new Bool4(a.xp || b.xp, a.xn || b.xn, a.yp || b.yp, a.yn || b.yn);
        public static Bool4 operator *(Bool4 a, Bool4 b) => new Bool4(a.xp && b.xp, a.xn && b.xn, a.yp && b.yp, a.yn && b.yn);
        public static Bool4 operator !(Bool4 a) => new Bool4(!a.xp, !a.xn, !a.yp, !a.yn);

        public static Bool4 zero = new Bool4(false, false, false, false);
        public static Bool4 one = new Bool4(true, true, true, true);
    }

    public struct MazeJunction
    {
        /// <summary>
        /// Creates a new maze junction
        /// </summary>
        /// <param name="position">Position of the junction node</param>
        /// <param name="connections">The connections of the node (X+,X-,Y+,Y-)</param>
        public MazeJunction(Vector2Int position, Bool4 connections)
        {
            this.position = position;
            this.connections = connections;
            walls = Bool4.zero;
            frontierCell = false;
            mazeCell = false;
            direction = Direction.None;
        }

        /// <summary>
        /// Creates a new maze junction without connections
        /// </summary>
        /// <param name="position">Position of the junction node</param>
        public MazeJunction(Vector2Int position)
        {
            this.position = position;
            this.connections = Bool4.zero;
            walls = Bool4.zero;
            frontierCell = false;
            mazeCell = false;
            direction = Direction.None;
        }

        public Vector2Int position { get; set; }
        public Bool4 connections { get; set; }
        public Bool4 walls { get; set; }

        public static MazeJunction empty = new MazeJunction(Vector2Int.zero, Bool4.zero);

        public bool frontierCell { get; set; }

        public bool mazeCell { get; set; }

        public Direction direction { get; set; }

        public void SetMazeCell(bool value)
        {
            mazeCell = value;
        }
        public void SetFrontierCell(bool value)
        {
            frontierCell = value;
        }

        public void SetDirection(Direction direction)
        {
            this.direction = direction;
        }
    }

    public Maze(float mazeSpacing, int mazeSize, bool openEdges)
    {
        this.mazeSize = mazeSize;
        this.mazeSpacing = mazeSpacing;
        mazeJunctions = new List<MazeJunction>();

        //Generate new empty maze
        for (int j = 0; j < mazeSize; j++)
        {
            for (int i = 0; i < mazeSize; i++)
            {
                mazeJunctions.Add(new MazeJunction(new Vector2Int(i, j),
                    new Bool4(
                        i == mazeSize - 1 && openEdges,
                        i == 0 && openEdges,
                        j == mazeSize - 1 && openEdges,
                        j == 0 && openEdges
                    )
                ));
            }
        }
    }
    
    public Vector3 MazePosToWorldPos(Vector2 mazePos)
    { 
        return new Vector3(mazePos.x, 0, mazePos.y) * mazeSpacing - new Vector3(0.5f, 0, 0.5f) * mazeSize * mazeSpacing + new Vector3(0.5f, 0, 0.5f) * mazeSpacing;
    }

    public Vector2Int WorldPosToMazePos(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt((worldPos.x / mazeSize / mazeSpacing + 0.5f) * mazeSize), Mathf.FloorToInt((worldPos.z / mazeSize / mazeSpacing + 0.5f) * mazeSize));
    }

    public MazeJunction LookUpCell(Vector2Int pos)
    {
        int index = mazeSize * pos.y + pos.x;

        return mazeJunctions[index];
    }

    public List<MazeJunction> LookUpNeighbours(Vector2Int pos, Predicate<MazeJunction> predicate)
    {
        List<MazeJunction> mazeJunctionsNeighbours = new List<MazeJunction>();

        foreach (Vector2Int neighbour in neighbours)
        {
            Vector2Int neighbourPos = pos + neighbour;
            if (neighbourPos.x < 0 || neighbourPos.y < 0 || neighbourPos.x >= mazeSize || neighbourPos.y >= mazeSize)
                continue;

            int index = mazeSize * neighbourPos.y + neighbourPos.x;

            MazeJunction cell = mazeJunctions[index];

            if (predicate(cell))
            {
                mazeJunctionsNeighbours.Add(cell);
            }
        }

        return mazeJunctionsNeighbours;
    }

    public void ModifyCell(Vector2Int pos, Predicate<MazeJunction> predicate, Modification modification)
    {
        int index = mazeSize * pos.y + pos.x;

        if (predicate(mazeJunctions[index]))
            mazeJunctions[index] = modification(mazeJunctions[index]);
    }

    public void ModifyCell(int index, Predicate<MazeJunction> predicate, Modification modification)
    {
        if (predicate(mazeJunctions[index]))
            mazeJunctions[index] = modification(mazeJunctions[index]);
    }

    public void MarkMaze(Vector2Int pos, bool value)
    {
        int index = mazeSize * pos.y + pos.x;
        Modification modification = c => { c.mazeCell = value; return c; };

        mazeJunctions[index] = modification(mazeJunctions[index]);
    }

    public void MarkFrontier(Vector2Int pos, bool value)
    {
        int index = mazeSize * pos.y + pos.x;
        Modification modification = c => { c.frontierCell = value; return c; };

        mazeJunctions[index] = modification(mazeJunctions[index]);
    }

    public void ModifyNeighbours(Vector2Int pos, Predicate<MazeJunction> predicate, Modification modification)
    {
        foreach (Vector2Int neighbour in neighbours)
        {
            Vector2Int neighbourPos = pos + neighbour;
            if (!IsInMaze(neighbourPos))
                continue;

            int index = mazeSize * neighbourPos.y + neighbourPos.x;

            if (predicate(mazeJunctions[index]))
                mazeJunctions[index] = modification(mazeJunctions[index]);
        }
    }

    public void Connect(Vector2Int a, Vector2Int b)
    {
        if (((Vector2)(a - b)).sqrMagnitude == 1)
        {
            Vector2Int diff = b - a;
            int indexA = mazeSize * a.y + a.x;
            int indexB = mazeSize * b.y + b.x;

            MazeJunction newA = mazeJunctions[indexA];
            newA.connections += new Bool4(diff.x == 1, diff.x == -1, diff.y == 1, diff.y == -1);
            MazeJunction newB = mazeJunctions[indexB];
            newB.connections += new Bool4(diff.x == -1, diff.x == 1, diff.y == -1, diff.y == 1);

            mazeJunctions[indexA] = newA;
            mazeJunctions[indexB] = newB;
        }
    }

    public bool IsInMaze(Vector2Int position)
    {
        return !(position.x < 0 || position.y < 0 || position.x >= mazeSize || position.y >= mazeSize);
    }

    public List<Direction> PossibleDirections(Vector2Int position)
    {
        List<Direction> directions = new List<Direction>();

        if (position.x > 0)
            directions.Add(Direction.NegativeX);

        if (position.x < mazeSize - 1)
            directions.Add(Direction.PositiveX);

        if (position.y > 0)
            directions.Add(Direction.NegativeY);

        if (position.y < mazeSize - 1)
            directions.Add(Direction.PositiveY);

        return directions;
    }

    public List<Direction> PossibleDirections(Vector2Int position, Bool4 connections)
    {
        List<Direction> directions = new List<Direction>();

        if (position.x > 0 && !connections.xn)
            directions.Add(Direction.NegativeX);

        if (position.x < mazeSize - 1 && !connections.xp)
            directions.Add(Direction.PositiveX);

        if (position.y > 0 && !connections.yn)
            directions.Add(Direction.NegativeY);

        if (position.y < mazeSize - 1 && !connections.yp)
            directions.Add(Direction.PositiveY);

        return directions;
    }

    public static int NumberOfDirections(Bool4 connections)
    {
        int count = 0;

        if (connections.xn)
            count++;

        if (connections.xp)
            count++;

        if (connections.yn)
            count++;

        if (connections.yp)
            count++;

        return count;
    }

    public List<Direction> PossibleDirections(Vector2Int position, Bool4 connections, Predicate<MazeJunction> predicate)
    {
        List<Direction> directions = new List<Direction>();

        if (position.x > 0 && !connections.xn)
            if (predicate(LookUpCell(position + DirectionToVector(Direction.NegativeX))))
                directions.Add(Direction.NegativeX);

        if (position.x < mazeSize - 1 && !connections.xp)
            if (predicate(LookUpCell(position + DirectionToVector(Direction.PositiveX))))
                directions.Add(Direction.PositiveX);

        if (position.y > 0 && !connections.yn)
            if (predicate(LookUpCell(position + DirectionToVector(Direction.NegativeY))))
                directions.Add(Direction.NegativeY);

        if (position.y < mazeSize - 1 && !connections.yp)
            if (predicate(LookUpCell(position + DirectionToVector(Direction.PositiveY))))
                directions.Add(Direction.PositiveY);

        return directions;
    }

    public void GeneratePrimMaze(float loopChance, int maxIter, Vector2Int rootPos)
    {
        List<MazeJunction> frontierCells;
        Vector2Int randomFrontierCell;
        List<MazeJunction> nearbyMazeCells;
        Vector2Int nearbyMazeCell;

        ModifyCell(rootPos, p => true, c =>
        {
            c.mazeCell = true;
            return c;
        });

        ModifyNeighbours(rootPos, p => true, c =>
        {
            c.frontierCell = true;
            return c;
        });

        for (int i = 0; i < maxIter; i++)
        {
            frontierCells = mazeJunctions.FindAll(m => m.frontierCell);
            if (frontierCells.Count > 0)
            {
                randomFrontierCell = frontierCells[UnityEngine.Random.Range(0, frontierCells.Count)].position;

                nearbyMazeCells = LookUpNeighbours(randomFrontierCell, p => p.mazeCell);
                nearbyMazeCell = nearbyMazeCells[UnityEngine.Random.Range(0, nearbyMazeCells.Count)].position;

                Connect(randomFrontierCell, nearbyMazeCell);

                ModifyCell(randomFrontierCell, p => true, c =>
                {
                    if (UnityEngine.Random.Range(0f, 1f) > loopChance)
                        c.frontierCell = false;
                    c.mazeCell = true;
                    return c;
                });

                ModifyNeighbours(randomFrontierCell, p => true, c =>
                {
                    if (c.mazeCell)
                        return c;
                    else
                        c.frontierCell = true;

                    return c;
                });
            }
            else
                break;
        }
    }

    public void GenerateEllerMaze()
    {
        int[] setList = new int[mazeSize];
        bool[] down = new bool[mazeSize];

        for (int i = 0; i < mazeSize; i++)
            setList[i] = i;

        int lastSplitIndex = 0;
        int highestSetNumber;

        for (int k = 0; k < mazeSize - 1; k++)
        {
            lastSplitIndex = 0;

            for (int i = 0; i < mazeSize; i++)
            {
                bool join = UnityEngine.Random.Range(0, 2) == 0;

                if (join && i < mazeSize - 1 && setList[i + 1] != setList[i])
                {
                    setList[i + 1] = setList[i];
                    Connect(new Vector2Int(i + 1, k), new Vector2Int(i, k));
                }
                else
                {
                    //down[i] = true;
                    //down[lastSplitIndex] = true;

                    down[UnityEngine.Random.Range(lastSplitIndex, i + 1)] = true;

                    for (int j = lastSplitIndex; j < i + 1; j++)
                        down[j] = UnityEngine.Random.Range(0, 2) == 0;

                    lastSplitIndex = i + 1;
                }
            }

            for (int i = 0; i < mazeSize; i++)
                if (down[i])
                {
                    Connect(new Vector2Int(i, k), new Vector2Int(i, k + 1));
                }

            highestSetNumber = Mathf.Max(setList);
            for (int i = 0; i < mazeSize; i++)
                if (!down[i])
                {
                    setList[i] = highestSetNumber + i + 1;
                }
        }
    }

    public void GeneratePrimCircularRoot(float loopChance, int maxIter)
    {
        List<MazeJunction> frontierCells;
        Vector2Int randomFrontierCell;
        List<MazeJunction> nearbyMazeCells;
        Vector2Int nearbyMazeCell;

        //https://www.desmos.com/calculator/gqgnmhqa82

        int W = (mazeSize % 2) * (Mathf.CeilToInt(mazeSize / 4f) * 2 + 1) + ((mazeSize + 1) % 2) * (Mathf.CeilToInt(mazeSize / 4f) * 2);
        int offset = Mathf.FloorToInt((mazeSize - W) / 2f);

        for (int i = 0; i < W; i++)
        {
            ModifyCell(offset * Vector2Int.one + new Vector2Int(i,0), p => true, c =>
            {
                c.mazeCell = true;
                return c;
            });

            ModifyNeighbours(offset * Vector2Int.one + new Vector2Int(i, 0), p => true, c =>
            {
                c.frontierCell = true;
                return c;
            });

            ModifyCell(offset * Vector2Int.one + new Vector2Int(i, W - 1), p => true, c =>
            {
                c.mazeCell = true;
                return c;
            });

            ModifyNeighbours(offset * Vector2Int.one + new Vector2Int(i, W - 1), p => true, c =>
            {
                c.frontierCell = true;
                return c;
            });
        }

        for (int i = 1; i < W - 1; i++)
        {
            ModifyCell(offset * Vector2Int.one + new Vector2Int(0, i), p => true, c =>
            {
                c.mazeCell = true;
                return c;
            });

            ModifyNeighbours(offset * Vector2Int.one + new Vector2Int(0, i), p => true, c =>
            {
                c.frontierCell = true;
                return c;
            });

            ModifyCell(offset * Vector2Int.one + new Vector2Int(W - 1, i), p => true, c =>
            {
                c.mazeCell = true;
                return c;
            });

            ModifyNeighbours(offset * Vector2Int.one + new Vector2Int(W - 1, i), p => true, c =>
            {
                c.frontierCell = true;
                return c;
            });
        }

        for (int i = 0; i < maxIter; i++)
        {
            frontierCells = mazeJunctions.FindAll(m => m.frontierCell);
            if (frontierCells.Count > 0)
            {
                randomFrontierCell = frontierCells[UnityEngine.Random.Range(0, frontierCells.Count)].position;

                nearbyMazeCells = LookUpNeighbours(randomFrontierCell, p => p.mazeCell);
                nearbyMazeCell = nearbyMazeCells[UnityEngine.Random.Range(0, nearbyMazeCells.Count)].position;

                Connect(randomFrontierCell, nearbyMazeCell);

                ModifyCell(randomFrontierCell, p => true, c =>
                {
                    if (UnityEngine.Random.Range(0f, 1f) > loopChance)
                        c.frontierCell = false;
                    c.mazeCell = true;
                    return c;
                });

                ModifyNeighbours(randomFrontierCell, p => true, c =>
                {
                    if (c.mazeCell)
                        return c;
                    else
                        c.frontierCell = true;

                    return c;
                });
            }
            else
                break;
        }
    }

    public void GenerateWilsonMaze(int maxIter)
    {
        int randomIndex = UnityEngine.Random.Range(0, mazeJunctions.Count);
        int startIndex = UnityEngine.Random.Range(0, mazeJunctions.Count);

        if (startIndex == randomIndex && startIndex > 0)
            startIndex -= 1;
        else
            startIndex += 1;

        Vector2Int pointerPos = new Vector2Int(startIndex % mazeSize, Mathf.FloorToInt(startIndex / (float)mazeSize));

        ModifyCell(randomIndex, p => true, c =>
        {
            c.mazeCell = true;
            return c;
        });

        for (int j = 0; j < 10; j++)
        {
            int i = 0;
            while (i < maxIter)
            {
                Vector2Int randomDir = neighbours[UnityEngine.Random.Range(0, 4)];
                Vector2Int futurePos = pointerPos + randomDir;

                if (futurePos.x < 0 || futurePos.x >= mazeSize)
                    randomDir.x = -randomDir.x;
                if (futurePos.y < 0 || futurePos.y >= mazeSize)
                    randomDir.y = -randomDir.y;

                futurePos = pointerPos + randomDir;

                MazeJunction futureCell = LookUpCell(futurePos);
            }
        }
    }

    public void GenerateOriginShift(float wallBreakChance, float generationTimeMultiplier)
    {
        Vector2Int originPos = Vector2Int.zero;

        //Initialize maze
        for (int j = 0; j < mazeSize; j++)
        {
            for (int i = 0; i < mazeSize; i++)
            {
                Direction dir = Direction.None;

                if (i == 0)
                    dir = Direction.NegativeY;
                else
                    dir = Direction.NegativeX;

                if (i == 0 && j == 0)
                    dir = Direction.None;

                ModifyCell(new Vector2Int(i, j), p => true, c =>
                {
                    c.direction = dir;
                    return c;
                });
            }
        }

        //Start shifting the origin
        for (int i = 0; i < mazeSize * mazeSize * 10 * generationTimeMultiplier; i++)
        {
            List<Direction> directions = PossibleDirections(originPos);

            Direction randomDirection = directions[UnityEngine.Random.Range(0, directions.Count)];

            ModifyCell(originPos, p => true, c =>
            {
                c.direction = randomDirection;
                return c;
            });

            originPos += DirectionToVector(randomDirection);

            ModifyCell(originPos, p => true, c =>
            {
                c.direction = Direction.None;
                return c;
            });
        }

        //Build connections
        for (int j = 0; j < mazeSize; j++)
        {
            for (int i = 0; i < mazeSize; i++)
            {
                MazeJunction currentCell = LookUpCell(new Vector2Int(i, j));

                Vector2Int connectingCellCoords = new Vector2Int(i, j) + DirectionToVector(currentCell.direction);

                Connect(new Vector2Int(i, j), connectingCellCoords);
            }
        }

        //Random connections 
        for (int j = 0; j < mazeSize; j++)
        {
            for (int i = 0; i < mazeSize; i++)
            {
                if (UnityEngine.Random.Range(0f,1f) <= wallBreakChance)
                {
                    MazeJunction currentCell = LookUpCell(new Vector2Int(i, j));

                    List<Direction> directions = PossibleDirections(originPos, currentCell.connections);

                    if (directions.Count == 0)
                        continue;

                    Direction randomDirection = directions[UnityEngine.Random.Range(0, directions.Count)];

                    Vector2Int connectingCellCoords = new Vector2Int(i, j) + DirectionToVector(randomDirection);
                    if (!IsInMaze(connectingCellCoords))
                        continue;

                    Connect(new Vector2Int(i, j), connectingCellCoords);
                }
            }
        }
    }
}
