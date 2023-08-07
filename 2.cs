using System;
using static AdventOfCode.Helpers;

namespace AdventOfCode
{
    enum Choice
    {
        Rock,
        Paper,
        Scissors,
    }

    enum Result
    {
        Lose,
        Draw,
        Win,
    }

    public static class Two
    {
        public static void Run()
        {
            var lines = GetLines().Where(x => x.Length == 3);
            var points1 = lines.Select(GetPart1Points).Sum();
            Console.WriteLine($"Total points part 1: {points1}");
            var points2 = lines.Select(GetPart2Points).Sum();
            Console.WriteLine($"Total points part 2: {points2}");
        }

        static Choice GetUs(Choice them, Result result)
        {
            return (them, result) switch
            {
                (Choice.Rock, Result.Lose) => Choice.Scissors,
                (Choice.Rock, Result.Win) => Choice.Paper,
                (Choice.Paper, Result.Lose) => Choice.Rock,
                (Choice.Paper, Result.Win) => Choice.Scissors,
                (Choice.Scissors, Result.Lose) => Choice.Paper,
                (Choice.Scissors, Result.Win) => Choice.Rock,
                (_, Result.Draw) => them,
                _ => throw new ArgumentOutOfRangeException("invalid them/result combination", (Exception?)null)
            };
        }

        static int GetPart1Points(string code)
        {
            var usCode = code[2];
            var themCode = code[0];

            var us = ParseChoice(usCode);
            var them = ParseChoice(themCode);
            var result = GetResult(us, them);

            var movePoints = GetMovePoints(us);
            var resultPoints = GetResultPoints(result);
            return movePoints + resultPoints;
        }

        static int GetPart2Points(string code)
        {
            var themCode = code[0];
            var resultCode = code[2];

            var them = ParseChoice(themCode);
            var result = ParseResult(resultCode);
            var us = GetUs(them, result);

            var movePoints = GetMovePoints(us);
            var resultPoints = GetResultPoints(result);
            return movePoints + resultPoints;
        }

        static int GetMovePoints(Choice us)
        {
            return us switch
            {
                Choice.Rock => 1,
                Choice.Paper => 2,
                Choice.Scissors => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(us)),
            };
        }

        static Result GetResult(Choice us, Choice them)
        {
            return (us, them) switch
            {
                (Choice.Rock, Choice.Paper) => Result.Lose,
                (Choice.Rock, Choice.Scissors) => Result.Win,
                (Choice.Paper, Choice.Rock) => Result.Win,
                (Choice.Paper, Choice.Scissors) => Result.Lose,
                (Choice.Scissors, Choice.Rock) => Result.Lose,
                (Choice.Scissors, Choice.Paper) => Result.Win,
                _ when us == them => Result.Draw,
                _ => throw new ArgumentOutOfRangeException("invalid us/them combination", (Exception?)null)
            };
        }

        static int GetResultPoints(Result result)
        {
            return result switch
            {
                Result.Lose => 0,
                Result.Draw => 3,
                Result.Win => 6,
                _ => throw new ArgumentOutOfRangeException(nameof(result))
            };
        }

        static Choice ParseChoice(char code)
        {
            return code switch
            {
                'A' or 'X' => Choice.Rock,
                'B' or 'Y' => Choice.Paper,
                'C' or 'Z' => Choice.Scissors,
                _ => throw new ArgumentOutOfRangeException(nameof(code)),
            };
        }

        static Result ParseResult(char code)
        {
            return code switch
            {
                'X' => Result.Lose,
                'Y' => Result.Draw,
                'Z' => Result.Win,
                _ => throw new ArgumentOutOfRangeException(nameof(code)),
            };
        }
    }
}

