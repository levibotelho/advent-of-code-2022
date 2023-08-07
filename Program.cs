using AdventOfCode;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Enter day...");
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
    default:
        Console.WriteLine("Unsupported day. Press any key to exit.");
        Console.ReadLine();
        return;
}
