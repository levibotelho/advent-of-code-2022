using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
	public static class Eight
	{
		public static void Run()
		{
			var lines = GetLines().Select(x => x.Trim());
			var trees = lines
				.Select(x => x
					.Select(x => {
						var height = (int)char.GetNumericValue(x);
						return new Tree { Height = height };
					})
					.ToArray()
				)
				.ToArray();

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
			Console.WriteLine($"Visible tree count: {visibleCount}");
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
		}
	}
}
