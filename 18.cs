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
            var cubes = GetOrderedCubes(lines);
            foreach (var cube in cubes)
            {
                if (cube == null)
                {
                    continue;
                }

                var (sharedFaceCubes, sharedEdgeCubes) = GetNeighboringCubes(cube, cubes);

                // We link the current cube to its neighboring cubes but not the other way around.
                // This is because we'll do the same when we iterate over the other cube's neighbors.
                // It's a little less efficient but lets us avoid tracking which cubes we've linked.
                UpdateFaceLinks(cube, sharedFaceCubes);
                UpdateEdgeLinks(cube, sharedEdgeCubes);
            }

            var exposedFaceCount = CountExposedFaces(cubes);
            Console.WriteLine($"Exterior surface area: {exposedFaceCount}");
        }

        static int CountExposedFaces(Cube[,,] cubes)
        {
            // Find cubes by iterating along the x-axis, taking unmarked instances, and starting from
            // the leftmost side (2). This ensures that we always start iterating on the outside of
            // each cube cluster/graph component.

            var faceCount = 0;
            for (var x = 0; x < cubes.GetLength(0); x++)
            {
                for (var y = 0; y < cubes.GetLength(1); y++)
                {
                    for (var z = 0; z < cubes.GetLength(2); z++)
                    {
                        var cube = cubes[x, y, z];
                        if (cube == null || cube.Visited)
                        {
                            continue;
                        }

                        faceCount += CountFacesInComponent(cube[2]);
                    }
                }
            }

            return faceCount;
        }

        static int CountFacesInComponent(Face face)
        {
            static int Count(Face face, int runningCount)
            {
                if (face.Visited)
                {
                    return runningCount;
                }

                face.SetVisited();
                runningCount++;
                foreach (var connection in face.Connections)
                {
                    runningCount = Count(connection, runningCount);
                }

                return runningCount;
            }

            return Count(face, 0);
        }

        static void UpdateEdgeLinks(Cube cube, IEnumerable<Cube> sharedEdge)
        {
            foreach (var other in sharedEdge)
            {
                // Get the two faces on cube that share the edge with other.
                var sharedEdgeFaces = new List<Face>(2);
                if (other.X < cube.X)
                {
                    // Other cube is to the left, the edge must touch face 2.
                    sharedEdgeFaces.Add(cube[2]);
                }
                else if (other.X > cube.X)
                {
                    sharedEdgeFaces.Add(cube[3]);
                }

                if (other.Y < cube.Y)
                {
                    sharedEdgeFaces.Add(cube[1]);
                }
                else if (other.Y > cube.Y)
                {
                    sharedEdgeFaces.Add(cube[4]);
                }

                if (other.Z < cube.Z)
                {
                    sharedEdgeFaces.Add(cube[0]);
                }
                else if (other.Z > cube.Z)
                {
                    sharedEdgeFaces.Add(cube[5]);
                }

                Debug.Assert(sharedEdgeFaces.Count == 2);

                // Disconnect the shared faces from one another, and connect them to the face on the
                // other cube that is opposite to the face from which they are being disconnected.
                sharedEdgeFaces[0].Connections.Remove(sharedEdgeFaces[1]);
                sharedEdgeFaces[1].Connections.Remove(sharedEdgeFaces[0]);
                sharedEdgeFaces[0].Connections.Add(other[sharedEdgeFaces[1].GetOppositeIndex()]);
                sharedEdgeFaces[1].Connections.Add(other[sharedEdgeFaces[0].GetOppositeIndex()]);
            }
        }

        static void UpdateFaceLinks(Cube cube, IEnumerable<Cube> sharedFace)
        {
            foreach (var other in sharedFace)
            {
                // Get the index of the shared face on each cube.
                var deltaX = other.X - cube.X;
                var deltaY = other.Y - cube.Y;
                var deltaZ = other.Z - cube.Z;

                // As both cubes share a full face they should only be offset by one unit on a single axis.
                Debug.Assert(Math.Abs(deltaX) + Math.Abs(deltaY) + Math.Abs(deltaZ) == 1);

                // Multiplying by Abs(delta) zeroes out all terms except the one we want.
                var cubeFaceIndex = (int)(
                    ((deltaX * 0.5) + 2.5) * Math.Abs(deltaX) +
                    ((deltaY * 1.5) + 2.5) * Math.Abs(deltaY) +
                    ((deltaZ * 2.5) + 2.5) * Math.Abs(deltaZ)
                );
                var otherFaceIndex = Cube.GetOppositeFaceIndex(cubeFaceIndex);

                // Remove all links to and from the face which is hidden by the adjacent block.
                cube.HideFace(cubeFaceIndex);

                // Link identical faces on the other cube to the current cube. Now all faces surrounding
                // the shared face point to the other cube, hiding the shared face from the outside.
                for (var i = 0; i <= Cube.MaxFaceIndex; i++)
                {
                    if (i == cubeFaceIndex || i == otherFaceIndex)
                    {
                        continue;
                    }

                    var cubeFace = cube[i];
                    if (!cubeFace.Connections.Any())
                    {
                        // A cube face starts with connections and can only update or lose them as
                        // blocks are added. If it has no connections it's hidden and must not be
                        // reconnected.
                        continue;
                    }

                    var otherFace = other[i];
                    cubeFace.Connections.Add(otherFace);
                }
            }
        }

        static (IReadOnlyList<Cube> sharedFace, IReadOnlyList<Cube> sharedEdge) GetNeighboringCubes(Cube cube, Cube[,,] cubes)
        {
            Cube? GetAtOffset(int xOffset, int yOffset, int zOffset)
            {
                var x = cube.X + xOffset;
                var y = cube.Y + yOffset;
                var z = cube.Z + zOffset;
                return x >= 0 && x < cubes.GetLength(0) && y >= 0 && y < cubes.GetLength(1) && z >= 0 && z < cubes.GetLength(2)
                    ? cubes[x, y, z]
                    : null;
            }

            // There are 18 cubes that could touch an edge or face of another cube (we don't care
            // about corners). The six that share faces are offset in a single axis, whereas the
            // twelve that share edges are offset in two axes.
            var sharedFace = new List<Cube>();
            var sharedEdge = new List<Cube>();
            for (var xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (var yOffset = -1; yOffset <= 1; yOffset++)
                {
                    for (var zOffset = -1; zOffset <= 1; zOffset++)
                    {
                        var neighbor = GetAtOffset(xOffset, yOffset, zOffset);
                        if (neighbor == null)
                        {
                            continue;
                        }

                        var zeros = new[] { xOffset, yOffset, zOffset }.Count(x => x == 0);
                        switch (zeros)
                        {
                            case 1:
                                // If one value is zero then other is offset in two axes and shares an edge.
                                sharedEdge.Add(neighbor);
                                break;
                            case 2:
                                // If two values are zero then the other is offset in a single axis and shares a face.
                                sharedFace.Add(neighbor);
                                break;
                        }
                    }
                }
            }

            return (sharedFace, sharedEdge);
        }

        static Cube[,,] GetOrderedCubes(IEnumerable<string> lines)
        {
            var cubeCollection = GetCubes(lines).ToArray();
            var sizeX = cubeCollection.Max(x => x.X) + 1;
            var sizeY = cubeCollection.Max(x => x.Y) + 1;
            var sizeZ = cubeCollection.Max(x => x.Z) + 1;
            var cubes = new Cube[sizeX, sizeY, sizeZ];
            foreach (var cube in cubeCollection)
            {
                cubes[cube.X, cube.Y, cube.Z] = cube;
            }

            return cubes;
        }

        class Cube
        {
            internal const int MaxFaceIndex = 5;

            // Faces are indexed from 0-5 with:
            //
            //  0 in front
            //  1 on the bottom
            //  2 on the left
            //  3 on the right
            //  4 on the top
            //  5 in the back
            //
            // Note that opposite face indexes always sum to 5.
            readonly Face[] faces;

            public Cube(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
                faces = Enumerable.Range(0, MaxFaceIndex + 1).Select(x => new Face(x, this)).ToArray();
                LinkFaces();
            }

            public int X { get; }
            public int Y { get; }
            public int Z { get; }
            public bool Visited { get; set; }

            public Face this[int faceIndex] => faces[faceIndex];

            public static int GetOppositeFaceIndex(int index)
            {
                return MaxFaceIndex - index;
            }

            public void HideFace(int faceIndex)
            {
                var face = faces[faceIndex];
                foreach (var connection in face.Connections)
                {
                    connection.Connections.Remove(face);
                }

                face.Connections.Clear();
            }

            void LinkFaces()
            {
                for (var i = 0; i < faces.Length; i++)
                {
                    var face = faces[i];
                    foreach (var adjacent in GetAdjacent(i))
                    {
                        face.Connections.Add(adjacent);
                    }
                }
            }

            IEnumerable<Face> GetAdjacent(int faceIndex)
            {
                return faces.Where((_, i) => i != faceIndex && i + faceIndex != faces.Length - 1);
            }
        }

        class Face
        {
            readonly Cube cube;
            bool visited;

            public Face(int index, Cube cube)
            {
                Index = index;
                this.cube = cube;
            }

            public int Index { get; }
            public HashSet<Face> Connections { get; } = new();
            public bool Visited => visited;

            public int GetOppositeIndex()
            {
                return Cube.GetOppositeFaceIndex(Index);
            }

            public void SetVisited()
            {
                visited = true;
                cube.Visited = true;
            }
        }

        static IEnumerable<Cube> GetCubes(IEnumerable<string> lines)
        {
            return lines.Select(x =>
            {
                var split = x.Split(',');
                return new Cube(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
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
