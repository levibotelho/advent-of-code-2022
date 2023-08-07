using AdventOfCode;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Day?");
var dayString = Console.ReadLine();
int day;
while (!int.TryParse(dayString, out day))
{
    Console.WriteLine("Invalid day.");
    dayString = Console.ReadLine();
}

switch (day)
{
    case 1:
        One.Run();
        break;
    case 2:
        Two.Run();
        break;
    case 3:
        Three.Run();
        break;
    default:
        Console.WriteLine("Unsupported day. Exiting.");
        break;
}
