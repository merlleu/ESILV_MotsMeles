using System;

namespace PooProject {
    /// <summary>
    /// This class represents a direction in two-dimensional space.
    /// It has two private fields `x` and `y` that represent the x and y
    /// components of the direction, respectively.
    /// </summary>
    public class Direction {
        #region Fields
        private int x;
        private int y;
        #endregion

        #region Constructors

        /// <summary>
        /// Parse the direction from a string.
        /// Valid strings are: N,S,E,O,NE,NO,SE,SO.
        /// If the string is invalid, the direction is set to 0,0.
        /// </summary>
        public Direction(string s) {
            bool valid = true;
            // to simplify the code, we match letter by letter
            for (int i = 0; i < s.Length && valid; i++) {
                switch (s[i]) {
                    case 'N':
                        y = -1;
                        break;
                    case 'S':
                        y = 1;
                        break;
                    case 'E':
                        x = 1;
                        break;
                    case 'O':
                        x = -1;
                        break;
                    default:
                        x = 0;
                        y = 0;
                        valid = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a direction two integers x and y.
        /// </summary>
        public Direction(int x, int y) {
            this.x = x;
            this.y = y;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the x component of the direction.
        /// x and y are either -1, 0 or 1.
        /// </summary>
        public int X {
            get => x;
        }

        /// <summary>
        /// Returns the y component of the direction.
        /// x and y are either -1, 0 or 1.
        /// </summary>
        public int Y {
            get => y;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Checks if the direction is valid (according to the game rules).
        /// </summary>
        public bool IsValid(int difficulty) {
            bool ok = false;
            switch (difficulty) {
                case 1:
                    ok = (x == 0 || y == 0);
                    break;
                case 2:
                    ok = (x == 0 || y == 0 || Math.Abs(x) == Math.Abs(y));
                    break;
                case 3:
                    ok = (Math.Abs(x) <= 1 && Math.Abs(y) <= 1);
                    break;
                case 4:
                case 5:
                    ok = true;
                    break;
            }
            if (x == 0 && y == 0) {
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// A direction is valid if it is not 0,0.
        /// This methods returns if the direction is valid.
        /// </summary>
        public bool IsNull() {
            return x == 0 && y == 0;
        }

        #endregion

        #region static methods

        /// <summary>
        /// Returns an array of all the valid directions for a given difficulty.
        /// </summary>
        public static Direction[] GetAvailableDirections(int difficulty) {
            // instead of using a list, we use a pre-allocated array of 8 (max number of directions).
            // this is faster than using a List because we'll do tons of operations on these arrays.
            // we also use a counter to keep track of the number of directions.

            Direction[] existing_directions = GetAllDirections();
            Direction[] directions = new Direction[8];
            int d_index = 0;

            // for each direction, we check if it's valid and add it to the array if it is.
            for(int i = 0; i < existing_directions.Length; i++) {
                if (existing_directions[i].IsValid(difficulty)) {
                    directions[d_index] = existing_directions[i];
                    d_index++;
                }
            }

            // we now have to resize the array to the correct size.
            Direction[] result = new Direction[d_index];
            Array.Copy(directions, result, d_index);

            return result;
        }

        /// <summary>
        /// Returns an array of all the directions.
        /// The order is: E, SE, S, SO, O, NO, N, NE.
        /// </summary>
        public static Direction[] GetAllDirections() {
            return new Direction[] {
                new Direction(1, 0),
                new Direction(1, 1),
                new Direction(0, 1),
                new Direction(-1, 1),
                new Direction(-1, 0),
                new Direction(-1, -1),
                new Direction(0, -1),
                new Direction(1, -1)
            };
        }

        #endregion

        #region Representations

        /// <summary>
        /// Returns a string representation of the direction.
        /// </summary>
        public override string ToString() {
            string repr = "";
            switch ((x,y)) {
                case (0,1):
                    repr = "S";
                    break;
                case (1,0):
                    repr = "E";
                    break;
                case (-1,0):
                    repr = "O";
                    break;
                case (0,-1):
                    repr = "N";
                    break;
                case (1,1):
                    repr = "SE";
                    break;
                case (-1,-1):
                    repr = "NO";
                    break;
                case (-1,1):
                    repr = "SO";
                    break;
                case (1,-1):
                    repr = "NE";
                    break;
                default: // unknown variant, always invalid !
                    repr = "?";
                    break;
            }
            return repr;
        }
        
        #endregion
    }
}