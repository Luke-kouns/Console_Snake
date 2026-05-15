// Main
﻿using System;

namespace ConsoleSnake
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.CursorVisible = false;

            Console.WriteLine("Welcome to GrEeEeEeN Snake!");
            Console.WriteLine("Press enter to play!");
            Console.ReadLine();

            // Starts Threads.
            Storage.MovementThread.Start();
            Storage.StartGame();

            while (true)
            {
                // If the game is over, then stop taking inputs.
                Storage.movementStop.Wait();

                ConsoleKey key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.W:
                        Storage.MovementVector = new(0, -1);
                        break;
                    case ConsoleKey.A:
                        Storage.MovementVector = new(-1, 0);
                        break;
                    case ConsoleKey.S:
                        Storage.MovementVector = new(0, 1);
                        break;
                    case ConsoleKey.D:
                        Storage.MovementVector = new(1, 0);
                        break;
                }
            }
        }
    }
}
