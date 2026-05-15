using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSnake
{
    internal class GameSpace
    {
        private Point coordinate { get; } // Used to track position, typically called and stored by the object within the game
        private bool isEmpty { get; set; } // Used to track if the space is empty or occupied
        private bool isFood { get; set; } // Used to track if this space's entity is food

        public GameSpace(Point cord)
        {
            this.coordinate = cord;
            this.isEmpty = true;
            this.isFood = false;
        }

        /// <returns>A point representing the coordinate value of this space (relative to the 2D Array)</returns>
        public Point GetCoordinate()
        {
            return this.coordinate;
        }

        public bool IsEmpty()
        {
            return this.isEmpty;
        }
        public void setEmpty(bool value)
        {
            this.isEmpty = value;
        }

        public bool IsFood()
        {
            return this.isFood;
        }

        public void setFood(bool value)
        {
            this.isFood = value;
        }

        override public String ToString()
        {
            if (!isEmpty)
            {
                if (isFood)
                {
                    return "()";
                }
                else
                {
                    return "[]";
                }
            }
            return ". ";
        }
    }
}
