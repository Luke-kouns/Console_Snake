using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSnake
{
    internal class Storage
    {
        // Threading 
        public static ManualResetEventSlim movementStop = new();
        private static CancellationToken cts = new();
        public static Thread MovementThread = new(() => MovementControl(cts));


        static public int Score { get; set; }

        /// <summary>
        /// Holds the coordinates of the snake's body, they will correspond to the game space coordinates that hold the body parts.
        /// </summary>
        static public List<Point> SnakeBody = [];
        static public GameSpace[,] Grid = new GameSpace[20, 20];

        static public Point MovementVector { get; set; }

        public static void CreateGrid()
        {
            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int j = 0; j < Grid.GetLength(1); j++)
                {
                    Grid[i, j] = new GameSpace(new Point(i, j));
                }
            }
        }

        /// <summary>
        /// Initialzes the game, what else could it possibly do???
        /// </summary>
        public static void StartGame()
        {
            Console.Clear();

            Score = 0;

            addSegment = false;

            // Resets the snakes body to be one central spot and sets the movement vector to be left, this is the default starting position and direction for the snake.
            SnakeBody.Clear();
            SnakeBody.Add(new Point(Grid.GetLength(0) / 2, Grid.GetLength(1) / 2));
            MovementVector = new Point(-1, 0);

            // Generates a new emptyy grid.
            CreateGrid();

            // puts the intital snake on the grid
            Grid[Grid.GetLength(0) / 2, Grid.GetLength(1) / 2].setEmpty(false);

            // Places the initial food
            PlaceFood();

            // Lets the movement thread run;
            movementStop.Set();
        }

        
        /// <returns>A list of all remaining empty spaces on the grid</returns>
        private static List<GameSpace> GetEmptySpaces()
        {
            List<GameSpace> emptySpaces = [];
            foreach (GameSpace g in Grid)
            {
                if (g.IsEmpty())
                {
                    emptySpaces.Add(g);
                }
            }
            return emptySpaces;
        }

        /// <summary>
        /// Places food on the grid, the position is random, but it will be on an empty space.
        /// If there is no empty spaces, then the player won the game, and the function returns false.
        /// </summary>
        /// <returns>True if a food is successfully spawned, False if there is no valid space</returns>
        private static bool PlaceFood()
        {
            Random rand = new Random();

            // Gets a list all all empty spaces remaining on the grid.
            List<GameSpace> emptySpaces = GetEmptySpaces();

            // Placing food only works if there is room to place food, if there is no room, then the game is won, and false is retruned.
            if (emptySpaces.Count == 0)
            {
                return false;
            }

            // Gets a random position until the empty one if found.
            int index = rand.Next(emptySpaces.Count);
            emptySpaces[index].setEmpty(false);
            emptySpaces[index].setFood(true);
            return true;
        }

        /// <summary>
        /// Displays the current grid.
        /// </summary>
        public static void DisplayGrid()
        {
            Console.SetCursorPosition(0, 0);   
            Console.ForegroundColor = ConsoleColor.Green;
            for (int i = 0; i < Grid.GetLength(0) + 2; i++)
            {
                Console.Write("==");
            }
            Console.Write("\n");
            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                Console.Write("<!");
                for (int j = 0; j < Grid.GetLength(1); j++)
                {
                    if (Grid[i, j].IsFood())
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    Console.Write(Grid[i, j].ToString());
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("!>\n");
            }
            for (int i = 0; i < Grid.GetLength(0) + 2; i++)
            {
                Console.Write("==");
            }
            Console.Write($"\nScore: {Score}");
        }

        private static bool addSegment = false;
        /// <summary>
        /// Moves the snake accourding to the input direction. If the snake collides with anouther object, this function handles it.
        /// </summary>
        /// <param name="vector">The direction to move in</param>
        private static void MoveSnake(Point vector)
        {
            //Console.WriteLine($"Moving Snake with vector {vector.X}, {vector.Y}");
            // Gets the position of the new head segment
            Point newFirst = new(SnakeBody[0].X + vector.X, SnakeBody[0].Y + vector.Y);
            //Console.WriteLine($"New head position will be {newFirst.X}, {newFirst.Y}");

            // If the new head segment's position is outside the bounds of the grid (AKA it hit a wall), then end the game.
            if (newFirst.X < 0 || newFirst.X >= Grid.GetLength(0) || newFirst.Y < 0 || newFirst.Y >= Grid.GetLength(1))
            {
                // End Game
                GameOverScreen(true);
                return;
            }

            // Inserts new head segment
            SnakeBody.Insert(0, newFirst);
            // If a segment is not being added, remove the last segment, effectivly moving the snake
            if (addSegment)
            {
                addSegment = false;
            }
            else
            {
                Point last = SnakeBody[SnakeBody.Count - 1];
                Grid[last.Y, last.X].setEmpty(true);
                SnakeBody.Remove(last);
            }

            // Checks for collisions with other objects in the grid.
            if (Grid[newFirst.Y, newFirst.X].IsFood())
            {
                addSegment = true;
                Score++;

                // Places new food and checks for a victory.
                if (!PlaceFood()) { GameOverScreen(false); return; }
            }
            else if (!Grid[newFirst.Y, newFirst.X].IsEmpty())
            {
                GameOverScreen(true);
                return;
            }
            
            // Finally, updates the grid to reflect the new positions.
            foreach (Point p in SnakeBody)
            {
                Grid[p.Y, p.X].setEmpty(false);
                Grid[p.Y, p.X].setFood(false);
            }
        }

        public static void GameOverScreen(bool isLoss)
        {
            movementStop.Reset();

            Console.SetCursorPosition(0, 0);
            Console.Clear();
            Console.WriteLine("                        " + (isLoss ? "Game Over!" : "You Win!"));
            Console.WriteLine("                        " + ($"Score: {Score}"));
            Console.WriteLine("\n\n\n                    " + "Press Enter To Restart");

            Console.ReadLine();
            StartGame();
        }

        private static int baseTimeToUpdate = 50;
        private static int timeToUpdate = 50;
        /// <summary>
        /// Controls the involuntary movment of the snake, plus updating the display when it is time.
        /// </summary>
        /// <param name="cts">The cancellation token for ending this thread</param>
        private static void MovementControl(CancellationToken cts)
        {
            while (!cts.IsCancellationRequested)
            {
                movementStop.Wait();
                DisplayGrid();
                while (timeToUpdate > 0)
                {

                    timeToUpdate -= 10;
                    Thread.Sleep(10);
                }
                // Resets the update time
                timeToUpdate = baseTimeToUpdate;

                MoveSnake(MovementVector);
            }
        }
    }
}
