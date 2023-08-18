using System;
using System.Diagnostics;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Eighteen
    {
        public static void Run()
        {
            var lines = GetLines().ToArray();
            PrintTotalSurface(lines);
            PrintExteriorSurface(lines);
        }

        static void PrintExteriorSurface(IEnumerable<string> lines)
        {
            var cubes = GetCubes(lines).ToArray();

            // +1 for length +1 for flood fill margin
            var sizeX = cubes.Max(x => x.X) + 2;
            var sizeY = cubes.Max(x => x.Y) + 2;
            var sizeZ = cubes.Max(x => x.Z) + 2;
            var nodes = new Node[sizeX, sizeY, sizeZ];
            foreach (var cube in cubes)
            {
                nodes[cube.X, cube.Y, cube.Z] = cube;
            }

            for (var x = 0; x < nodes.GetLength(0); x++)
            {
                for (var y = 0; y < nodes.GetLength(1); y++)
                {
                    for (var z = 0; z < nodes.GetLength(2); z++)
                    {
                        nodes[x, y, z] ??= new Node(x, y, z, false);
                    }
                }
            }

            static void Visit(Node current, Action<Node> markCubeVisited, Node[,,] nodes)
            {
                if (current.IsCube)
                {
                    markCubeVisited(current);
                    return;
                }

                if (current.IsVisited)
                {
                    return;
                }

                current.IsVisited = true;

                if (current.X > 0)
                {
                    Visit(nodes[current.X - 1, current.Y, current.Z], x => { x.IsVisitedXMinus = true; }, nodes);
                }

                if (current.X < nodes.GetLength(0) - 1)
                {
                    Visit(nodes[current.X + 1, current.Y, current.Z], x => { x.IsVisitedXPlus = true; }, nodes);
                }

                if (current.Y > 0)
                {
                    Visit(nodes[current.X, current.Y - 1, current.Z], x => { x.IsVisitedYMinus = true; }, nodes);
                }

                if (current.Y < nodes.GetLength(1) - 1)
                {
                    Visit(nodes[current.X, current.Y + 1, current.Z], x => { x.IsVisitedYPlus = true; }, nodes);
                }

                if (current.Z > 0)
                {
                    Visit(nodes[current.X, current.Y, current.Z - 1], x => { x.IsVisitedZMinus = true; }, nodes);
                }

                if (current.Z < nodes.GetLength(2) - 1)
                {
                    Visit(nodes[current.X, current.Y, current.Z + 1], x => { x.IsVisitedZPlus = true; }, nodes);
                }
            }

            Visit(nodes[0, 0, 0], x => { }, nodes);

            var count = 0;
            foreach (var node in nodes)
            {
                if (!node.IsCube)
                {
                    continue;
                }

                if (node.IsVisitedXMinus)
                {
                    count++;
                }

                if (node.IsVisitedXPlus)
                {
                    count++;
                }

                if (node.IsVisitedYMinus)
                {
                    count++;
                }

                if (node.IsVisitedYPlus)
                {
                    count++;
                }

                if (node.IsVisitedZMinus)
                {
                    count++;
                }

                if (node.IsVisitedZPlus)
                {
                    count++;
                }
            }

            Console.WriteLine("Surface count: " + count);
        }

        class Node
        {
            public Node(int x, int y, int z, bool isCube)
            {
                X = x;
                Y = y;
                Z = z;
                IsCube = isCube;
            }

            public int X { get; }
            public int Y { get; }
            public int Z { get; }
            public bool IsCube { get; set; }

            public bool IsVisitedXMinus { get; set; }
            public bool IsVisitedXPlus { get; set; }
            public bool IsVisitedYMinus { get; set; }
            public bool IsVisitedYPlus { get; set; }
            public bool IsVisitedZMinus { get; set; }
            public bool IsVisitedZPlus { get; set; }
            public bool IsVisited { get; set; }
        }

        static IEnumerable<Node> GetCubes(IEnumerable<string> lines)
        {
            // Add 1 to allow for an empty ring for flood fill.
            return lines.Select(x =>
            {
                var split = x.Split(',');
                return new Node(int.Parse(split[0]) + 1, int.Parse(split[1]) + 1, int.Parse(split[2]) + 1, true);
            }).ToArray();
        }

        static void PrintTotalSurface(IEnumerable<string> lines)
        {
            var cubes = GetCubes(lines).ToArray();

            // A shared surface occurs when two cubes share two coordinates and the third is off by one.
            var sizeX = cubes.Max(x => x.X) + 1;
            var sizeY = cubes.Max(x => x.Y) + 1;
            var sizeZ = cubes.Max(x => x.Z) + 1;

            var xy = new List<int>[sizeX, sizeY];
            var xz = new List<int>[sizeX, sizeZ];
            var yz = new List<int>[sizeY, sizeZ];

            foreach (var point in cubes)
            {
                (xy[point.X, point.Y] ??= new List<int>()).Add(point.Z);
                (xz[point.X, point.Z] ??= new List<int>()).Add(point.Y);
                (yz[point.Y, point.Z] ??= new List<int>()).Add(point.X);
            }

            var sharedSideCount = 0;
            foreach (var collection in new[] { xy, xz, yz })
            {
                foreach (var list in collection)
                {
                    if (list == null)
                    {
                        continue;
                    }

                    list.Sort();
                    for (var i = 1; i < list.Count; i++)
                    {
                        var last = list[i - 1];
                        var current = list[i];
                        if (current == last + 1)
                        {
                            // Two cubes sharing a face removes two faces from the count.
                            sharedSideCount += 2;
                        }
                    }
                }
            }

            var totalSurfaceArea = cubes.Length * 6 - sharedSideCount;
            Console.WriteLine($"Total surface area: {totalSurfaceArea}");
        }
    }
}
