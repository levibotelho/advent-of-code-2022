using System;
using System.Diagnostics;

using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Seven
    {
        public static void Run()
        {
            var lines = GetLines();
            var tree = GetTree(lines);
            var dirs = GetDirs(tree);
            var candidateSize = dirs.Where(x => x.Size <= 100000).Sum(x => x.Size);
            Console.WriteLine($"Total candidate directory size: {candidateSize}");

            const long totalSpace = 70000000;
            var usedSpace = tree.Size;
            var availableSpace = totalSpace - usedSpace;
            const long updateSpace = 30000000;
            var requiredSpace = updateSpace - availableSpace;
            Debug.Assert(requiredSpace > 0);
            var deleteCandidate = dirs.OrderBy(x => x.Size).First(x => x.Size >= requiredSpace);
            Console.WriteLine($"Delete candidate directory name: {deleteCandidate.Name}, size: {deleteCandidate.Size}");
        }

        static IReadOnlyList<Node> GetDirs(Node tree)
        {
            static void GetDirsInner(Node node, List<Node> dirs)
            {
                if (!node.IsDir)
                {
                    return;
                }

                dirs.Add(node);
                Debug.Assert(node.Children != null);
                foreach (var child in node.Children)
                {
                    GetDirsInner(child, dirs);
                }
            }

            var dirs = new List<Node>();
            GetDirsInner(tree, dirs);
            return dirs;
        }

        static Node GetTree(IEnumerable<string> lines)
        {
            var root = Node.NewDir("/");
            var path = new Stack<Node>();
            path.Push(root);
            var isListing = false;
            foreach (var line in lines)
            {
                var elements = line.Split(' ');
                var current = path.Peek();
                switch (elements)
                {
                    case ["$", "cd", var dir]:
                        isListing = false;
                        switch (dir)
                        {
                            case "..":
                                path.Pop();
                                break;
                            case "/":
                                while (path.Count > 1)
                                {
                                    path.Pop();
                                }
                                break;
                            default:
                                Debug.Assert(current.Children != null);
                                var next = current.Children.Find(x => x.Name == dir);
                                if (next == null)
                                {
                                    throw new InvalidOperationException($"directory not found: {dir}");
                                }
                                path.Push(next);
                                break;
                        }
                        break;
                    case ["$", "ls"]:
                        isListing = true;
                        break;
                    case ["dir", var name]:
                        Debug.Assert(isListing);
                        Debug.Assert(current.Children != null);
                        current.Children.Add(Node.NewDir(name));
                        break;
                    case [var sizeStr, var name] when long.TryParse(sizeStr, out var size):
                        Debug.Assert(current.Children != null);
                        current.Children.Add(Node.NewFile(name, size));
                        foreach (var dir in path)
                        {
                            dir.IncrementSize(size);
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"unhandled input: {line}");
                }
            }

            return root;
        }

        class Node
        {
            private Node() { }

            public required string Name { get; init; }
            public long Size { get; private set; }
            public List<Node>? Children { get; init; }
            public bool IsDir => Children != null;

            public static Node NewDir(string name)
            {
                return new Node { Name = name, Children = new List<Node>() };
            }

            public static Node NewFile(string name, long size)
            {
                return new Node { Name = name, Size = size };
            }

            public void IncrementSize(long value)
            {
                Size += value;
            }
        }
    }
}
