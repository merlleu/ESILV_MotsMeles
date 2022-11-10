using System;

namespace PooProject {
    public class Direction {
        private int x;
        private int y;

        /// Parse the direction from a string.
        /// Valid strings are: N,S,E,O,NE,NO,SE,SO.
        /// If the string is invalid, the direction is set to 0,0.
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
        
        /// Creates a direction two integers x and y.
        public Direction(int x, int y) {
            this.x = x;
            this.y = y;
        }


        /// Returns the x component of the direction.
        /// x and y are either -1, 0 or 1.
        public int X {
            get => x;
        }
        
        /// Returns the y component of the direction.
        /// x and y are either -1, 0 or 1.
        public int Y {
            get => y;
        }

        /// Checks if the direction is valid (according to the game rules).
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

        /// Returns an array of all the valid directions for a given difficulty.
        public static Direction[] GetAvailableDirections(int difficulty) {
            // instead of using a list, we use a pre-allocated array of 8 (max number of directions).
            // this is faster than using a List because we'll do tons of operations on these arrays.
            // we also use a counter to keep track of the number of directions.

            Direction[] directions = new Direction[8];
            int d_index = 0;

            // we use a nested loop to generate all the possible directions
            // (from -1 to 1 for x and y)
            // for each direction, we check if it's valid and add it to the array if it is.
            // (0,0) is NEVER a valid direction.
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    Direction d = new Direction(i, j);
                    if (d.IsValid(difficulty)) {
                        directions[d_index] = d;
                        d_index++;
                    }
                }
            }

            // we now have to resize the array to the correct size.
            Direction[] result = new Direction[d_index];
            Array.Copy(directions, result, d_index);

            return result;
        }



        /// A direction is valid if it is not 0,0.
        /// This methods returns if the direction is valid.
        public bool IsNull() {
            return x == 0 && y == 0;
        }


        /// Returns a string representation of the direction.
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

        public string Details() {
            return $"Direction({ToString()},{x},{y})";
        }
        public bool Egal(Direction other) {
            return x == other.x && y == other.y;
        }
    }
}