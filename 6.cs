using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    public static class Six
    {
        public static void Run()
        {
            var lines = GetLines();
            var packetMarkerIndex = GetMarkerIndex(lines.Single(), 4);
            Console.WriteLine($"Start of packet marker after: {packetMarkerIndex}");
            var messageMarkerIndex = GetMarkerIndex(lines.Single(), 14);
            Console.WriteLine($"Start of message marker after: {messageMarkerIndex}");
        }

        static int GetMarkerIndex(string message, int distinctCharCount)
        {
            var buffer = new Queue<char>(distinctCharCount);
            for (var i = 0; i < message.Length; i++)
            {
                var character = message[i];
                buffer.Enqueue(character);
                if (buffer.Count < distinctCharCount)
                {
                    continue;
                }

                if (buffer.Distinct().Count() == distinctCharCount)
                {
                    // i = last character of buffer
                    return i + 1;
                }
                buffer.Dequeue();
            }

            return -1;
        }
    }
}
