using System;
using System.Diagnostics;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Twenty
    {
        public static void Run()
        {
            var lines = GetLines();
            var mixed = new LinkedList<int>(lines.Select(int.Parse));
            var originalOrder = CreateOriginalOrder(mixed);

            // This many shifts puts the value back in its original place. It is length - 1 because
            // it equals the number of spaces between other items in which the current item can sit.
            // e.g. if there are three items, item 1 can only go to 213 before ending up at 231 which
            // is the same as 123.
            var maxShiftCount = mixed.Count - 1;

            foreach (var node in originalOrder)
            {
                // Convert all shifts to forward shifts for simplicity.
                var shift = node.Value;
                if (node.Value < 0)
                {
                    var addend = (int)Math.Ceiling((double)-shift / maxShiftCount) * maxShiftCount;
                    Debug.Assert(addend != 0);
                    shift += addend;
                }

                shift %= maxShiftCount;
                if (shift == 0)
                {
                    continue;
                }

                var predecessor = node;
                for (var i = 0; i < shift; i++)
                {
                    Debug.Assert(predecessor != null);
                    predecessor = predecessor.Next ?? mixed.First;
                }

                Debug.Assert(predecessor != node && predecessor != node.Previous);

                mixed.Remove(node);
                Debug.Assert(predecessor != null);
                mixed.AddAfter(predecessor, node);
            }

            var j = int.MinValue;
            var current = mixed.First;
            var score = 0;
            while (true)
            {
                Debug.Assert(current != null);
                if (j < 0 && current.Value == 0)
                {
                    j = 0;
                }

                if (j == 1000 || j == 2000 || j == 3000)
                {
                    score += current.Value;
                    if (j == 3000)
                    {
                        break;
                    }
                }

                current = current.Next ?? mixed.First;
                j++;
            }

            Console.WriteLine(score);
        }

        static LinkedListNode<int>[] CreateOriginalOrder(LinkedList<int> list)
        {
            var originalOrder = new LinkedListNode<int>[list.Count];
            var next = list.First;
            var i = 0;
            while (next != null)
            {
                originalOrder[i] = next;
                next = next.Next;
                i++;
            }

            return originalOrder;
        }
    }
}
