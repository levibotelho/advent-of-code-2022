using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Eight
    {
        public static void Run()
        {
            var lines = GetLines().Select(x => x.Trim());
            var visibleCount = GetVisibleCount(lines);
            Console.WriteLine($"Visible tree count: {visibleCount}");
            var scenicScore = GetHighestScenicScore(lines);
            Console.WriteLine($"Highest scenic score: {scenicScore}");
        }

        static int GetHighestScenicScore(IEnumerable<string> lines)
        {
            var trees = GetTrees(lines);
            // Would have been more efficient to use indexes only and skip linking, but this is fast and works.
            LinkTrees(trees);
            var highScore = -1;
            // Skip first/last rows due to them containing zero values
            foreach (var row in trees.Skip(1).SkipLast(1))
            {
                foreach (var tree in row.Skip(1).SkipLast(1))
                {
                    var up = GetVisibleCount(tree, x => x.Up);
                    var down = GetVisibleCount(tree, x => x.Down);
                    var left = GetVisibleCount(tree, x => x.Left);
                    var right = GetVisibleCount(tree, x => x.Right);
                    var score = up * down * left * right;
                    highScore = Math.Max(score, highScore);
                }
            }
            return highScore;
        }

        static int GetVisibleCount(Tree tree, Func<Tree, Tree?> direction)
        {
            var count = 0;
            Tree? current = tree;
            do
            {
                current = direction(current);
                if (current == null)
                {
                    break;
                }
                count++;
            } while (current.Height < tree.Height);
            return count;
        }

        static void LinkTrees(Tree[][] trees)
        {
            for (var i = 0; i < trees.Length; i++)
            {
                var row = trees[i];
                for (var j = 0; j < row.Length; j++)
                {
                    var tree = row[j];
                    if (j > 0)
                    {
                        tree.Left = row[j - 1];
                    }
                    if (j < row.Length - 1)
                    {
                        tree.Right = row[j + 1];
                    }
                    if (i > 0)
                    {
                        tree.Up = trees[i - 1][j];
                    }
                    if (i < trees.Length - 1)
                    {
                        tree.Down = trees[i + 1][j];
                    }
                }
            }
        }

        static int GetVisibleCount(IEnumerable<string> lines)
        {
            var trees = GetTrees(lines);
            var visibleCount = 0;
            for (var i = 0; i < trees.Length; i++)
            {
                var row = trees[i];
                visibleCount += GetUpdateVisible(row);
                visibleCount += GetUpdateVisible(row.Reverse());
            }

            for (var i = 0; i < trees[0].Length; i++)
            {
                visibleCount += GetUpdateVisible(IterateCol(trees, i));
                visibleCount += GetUpdateVisible(IterateCol(trees, i).Reverse());
            }

            return visibleCount;
        }

        static Tree[][] GetTrees(IEnumerable<string> lines)
        {
            return lines
                .Select(x => x
                    .Select(x =>
                    {
                        var height = (int)char.GetNumericValue(x);
                        return new Tree { Height = height };
                    })
                    .ToArray()
                )
                .ToArray();
        }

        static int GetUpdateVisible(IEnumerable<Tree> line)
        {
            var count = 0;
            var last = -1;
            foreach (var tree in line)
            {
                if (tree.Height > last)
                {
                    if (!tree.Checked)
                    {
                        tree.Checked = true;
                        count++;
                    }
                    last = tree.Height;
                }
                else if (tree.Height == last)
                {
                    continue;
                }
            }
            return count;
        }

        static IEnumerable<T> IterateCol<T>(T[][] numbers, int i)
        {
            foreach (var row in numbers)
            {
                yield return row[i];
            }
        }

        class Tree
        {
            public int Height { get; init; }
            public bool Checked { get; set; }
            public Tree? Up { get; set; }
            public Tree? Down { get; set; }
            public Tree? Left { get; set; }
            public Tree? Right { get; set; }
            public int ScenicScore { get; set; }
        }
    }
}
