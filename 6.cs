using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
	public static class Six
	{
		public static void Run()
		{
            var lines = GetLines();
			var packetMarkerIndex = GetPacketMarkerIndex(lines.Single());
			Console.WriteLine($"Start of packet marker after: {packetMarkerIndex}");
        }

		static int GetPacketMarkerIndex(string message)
		{
			var buffer = new Queue<char>(4);
			for (var i = 0; i < message.Length; i++)
			{
                var character = message[i];
                buffer.Enqueue(character);
                if (buffer.Count < 4)
				{
					continue;					
				}
				                
				if (buffer.Distinct().Count() == 4)
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
