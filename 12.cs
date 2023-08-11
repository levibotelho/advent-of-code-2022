using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Twelve
    {
        public static void Run()
        {
            var lines = GetLines();
            var shortestS = GetShortestPathS(lines);
            Console.WriteLine($"Shortest path from S: {shortestS}");
            var shortestA = GetShortestPathA(lines);
            Console.WriteLine($"Shortest path from any \"a\": {shortestA}");
        }

        static int GetShortestPathA(IEnumerable<string> lines)
        {
            // Get the shortest path tree from the end outwards, then find the "a" vertex with the smallest distance.
            var (vertices, start, _) = ConstructGraph(lines, true);
            var unvisited = new HashSet<Vertex>(vertices);
            CalculateShortestPath(start, null, unvisited);
            return vertices.Where(x => x.Elevation == 'a').Min(x => x.Distance);
        }

        static int GetShortestPathS(IEnumerable<string> lines)
        {
            var (vertices, start, end) = ConstructGraph(lines, false);
            var unvisited = new HashSet<Vertex>(vertices);
            CalculateShortestPath(start, end, unvisited);
            return end.Distance;
        }

        static void CalculateShortestPath(Vertex start, Vertex? end, HashSet<Vertex> unvisited)
        {
            foreach (var successor in start.Successors)
            {
                successor.Distance = Math.Min(successor.Distance, start.Distance + 1);
            }

            start.Visited = true;
            unvisited.Remove(start);
            if (end != null && end.Visited)
            {
                return;
            }

            var next = unvisited.MinBy(x => x.Distance);
            if (next != null && next.Distance != int.MaxValue)
            {
                CalculateShortestPath(next, end, unvisited);
            }
        }

        static (Vertex[] vertices, Vertex start, Vertex end) ConstructGraph(IEnumerable<string> lines, bool isInverted)
        {
            Vertex? start = null, end = null;
            var vertices = lines.Select(x => x.Select(x => new Vertex(x, isInverted)).ToArray()).ToArray();
            for (var iRow = 0; iRow < vertices.Length; iRow++)
            {
                var row = vertices[iRow];
                for (var iCol = 0; iCol < row.Length; iCol++)
                {
                    static void TryAddSuccessor(Vertex predecessor, Vertex successor, bool isInverted)
                    {
                        if (!isInverted && successor.Elevation <= predecessor.Elevation + 1)
                        {
                            predecessor.Successors.Add(successor);
                        }
                        else if (isInverted && successor.Elevation >= predecessor.Elevation - 1)
                        {
                            predecessor.Successors.Add(successor);
                        }
                    }

                    var vertex = row[iCol];
                    if (vertex.IsStart)
                    {
                        start = vertex;
                    }

                    if (vertex.IsEnd)
                    {
                        end = vertex;
                    }

                    if (iCol > 0)
                    {
                        TryAddSuccessor(vertex, row[iCol - 1], isInverted);
                    }

                    if (iCol < row.Length - 1)
                    {
                        TryAddSuccessor(vertex, row[iCol + 1], isInverted);
                    }

                    if (iRow > 0)
                    {
                        TryAddSuccessor(vertex, vertices[iRow - 1][iCol], isInverted);
                    }

                    if (iRow < vertices.Length - 1)
                    {
                        TryAddSuccessor(vertex, vertices[iRow + 1][iCol], isInverted);
                    }
                }
            }

            if (start != null && end != null)
            {
                var allVertices = vertices.SelectMany(x => x).ToArray();
                return (allVertices, start, end);
            }

            throw new ArgumentException("graph does not have start/end");
        }

        class Vertex
        {
            public Vertex(char elevation, bool isInverted)
            {
                switch (elevation)
                {
                    case 'S':
                        Elevation = 'a';
                        IsStart = !isInverted;
                        IsEnd = isInverted;
                        Distance = !isInverted ? 0 : int.MaxValue;
                        break;
                    case 'E':
                        Elevation = 'z';
                        IsStart = isInverted;
                        IsEnd = !isInverted;
                        Distance = !isInverted ? int.MaxValue : 0;
                        break;
                    default:
                        Elevation = elevation;
                        Distance = int.MaxValue;
                        break;
                }
            }

            public int Elevation { get; }
            public bool IsStart { get; }
            public bool IsEnd { get; }
            public int Distance { get; set; }
            public bool Visited { get; set; }
            public List<Vertex> Successors { get; } = new();
        }
    }
}
