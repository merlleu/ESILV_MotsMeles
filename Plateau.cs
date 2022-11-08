using System;

namespace PooProject {
    /// Plateau contains the grid of letters and is responsible for generating it.
    public class Plateau {
        private char[,] grid;
        private int difficulty;
        private string[] words;
        private int width;
        private int height;
        private Dictionnaire dictionnary;
        private Direction[] doable_directions;
        
        /// Creates an empty grid
        public Plateau(Dictionnaire dictionnary)  {
            this.dictionnary = dictionnary;
            this.grid = new char[0,0];
            this.words = new string[0];
        }

        public string[] Words {
            get => words;
        }

        public int Width {
            get => width;
        }

        public int Height {
            get => height;
        }

        public Direction[] DoableDirections {
            get => doable_directions;
        }


        public void Load(int difficulty) {
            this.difficulty = difficulty;
            GetDimensions();
            this.doable_directions = Direction.GetAvailableDirections(difficulty);
            this.grid = GenerateGrid(dictionnary, difficulty, width, height);
            GatherGeneratedWords();
        }

        void GetDimensions() {
            // width & height MUST be between 3 and 25.
            width = 5 + difficulty * 2;
            height = 5 + difficulty * 2;
        }

        /// Represents the grid as a string.
        /// First line is: difficulty;width;height;word count
        /// Second line is: {word_1};{word_2};...{word_n}
        /// Then the following lines are the grid, each line is a row.
        /// Each cell is separated by a semicolon.
        public override string ToString() {
            string s = "";
            s += $"{difficulty};{width};{height};{words.Length}\n";
            for (int i = 0; i < words.Length; i++) {
                s += $"{words[i]};";
            }
            s += "\n";
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    s += grid[i, j] + (j == height - 1 ? "": ";");
                }
                s += "\n";
            }
            return s;
        }

        /// Dumps the grid to a file.
        /// nomfile is a relative path.
        public void ToFile(string nomfile) {
            File.WriteAllText(nomfile, ToString());
        } 

        /// Loads a grid from a file.
        /// nomfile is a relative path.
        /// named ToRead in the TD.
        public void FromFile(string nomfile) {
            string[] lines = File.ReadAllLines(nomfile);
            string[] settings = lines[0].Split(';');
            difficulty = int.Parse(settings[0]);
            width = int.Parse(settings[1]);
            height = int.Parse(settings[2]);
            int wordCount = int.Parse(settings[3]);
            words = new string[wordCount];
            for (int i = 0; i < wordCount; i++) {
                words[i] = lines[1].Split(';')[i];
            }

            grid = new char[width, height];
            for (int i = 0; i < width; i++) {
                string[] cells = lines[i + 2].Split(';');
                for (int j = 0; j < height; j++) {
                    grid[i, j] = cells[j][0];
                }
            }
        }

        /// This method generates the grid of letters.
        static char[,] GenerateGrid(Dictionnaire dictionnary, int difficulty, int width, int height) {
            char[,] g = new char[width, height];
            Random r = new Random();
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    // generate a random letter between 'A' and 'Z'
                    // here we use the ASCII uppercase letters range
                    // (faster than array/string index lookup)
                    g[i, j] = (char) r.Next(65, 91);
                }
            }
            return g;
        }

        /// This method checks if a cell is part of a word on the grid.
        /// This method is really heavy and we could implement binary trees or mnemoization to make it faster.
        /// But I guess this is clearly out of scope for this project.
        void GatherGeneratedWords() {
            // we have no idea how many words we'll find, so we use a list.
            List<string> words = new List<string>();

            // we use a nested loop to check every cell of the grid.
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    foreach (Direction direction in doable_directions) {
                        FindWordsForCell(i, j, direction, words);
                    }
                }
            }

            // sort the words alphabetically
            words.Sort();
            // remove duplicates (fast because the list is sorted)
            words = words.Distinct().ToList();
            this.words = words.ToArray();
        }

        /// This method checks if a word is found on a cell in a given direction.
        void FindWordsForCell(int x, int y, Direction direction, List<string> words) {
            // we init x2 and y2 to the current cell
            int x2 = x;
            int y2 = y;
            // word is a buffer, we'll add letters to it until we find a word or we reach the end of the grid.
            string word = "";

            // we check for length between 2 & maxWordLength (5 to 13 depending on the difficulty).
            // we start the loop at 0 to have the entire word but only check for words of length 2 or more.
            for (int i = 0; x2 >= 0 && y2 >= 0 && x2 < width && y2 < height; i++) {
                // we append the letter to the word
                word += grid[x2, y2];

                // if the word is found in the dictionnary, we add it to the list.
                if (dictionnary.RechDichoRecursif(word)) {
                    words.Add(word);
                }

                // we move to the next cell in the given direction.
                x2 += direction.X;
                y2 += direction.Y;
            }
        }

        /// This method returns the string formed by the vector of cells.
        public string GetWordForCellVector(int x, int y, Direction direction, int length) {
            string word = "";
            for (int i = 0; i < length 
                && x >= 0 && y >= 0 && x < width && y < height; i++) {
                word += grid[x,y];
                x += direction.X;
                y += direction.Y;
            }
            return word;
        }


        /// This method checks if coordinates are valid.
        public bool CheckInBounds(int x, int y) {
            return x >= 0 && y >= 0 && x < width && y < height;
        }

        /// This method check wether or not a word is in the grid.
        public bool Test_Plateau(string mot, int ligne, int colonne, Direction direction) {
            // for each letter in the word
            bool ok = true;
            for (int i = 0; i < mot.Length && ok; i++) {
                // we first check if the letter is in the grid
                if (!CheckInBounds(ligne, colonne)) {
                    ok = false;
                } else {
                    // then we check if the letter is the same as the one in the word
                    ok = grid[ligne, colonne] == mot[i];
                    // finally we move to the next letter
                    ligne += direction.X;
                    colonne += direction.Y;
                }
            }

            return ok && dictionnary.RechDichoRecursif(mot);
        }

        /// This method logs the grid to the console.
        /// The grid cells colors represents how many words are found on the cell.
        public void DisplayTableau(Joueur player, int ui_x, int ui_y, Direction ui_dir, int ui_wordLength) {
            for (int y = 0; y < height; y++) {
                DisplayTableauRow(y, ui_x, ui_y, ui_dir, ui_wordLength);
            }
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"Progression : {player.Words.Count} / {words.Length} ({((int) (player.Score / (float) words.Length * 100))}%)");
            Console.Write($"Mots: ");
            foreach (string word in words ) {
                if (!player.Words.Contains(word)) {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(word + " ");
                }
            }

            foreach (string word in player.Words) {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(word + " ");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        void DisplayTableauRow(int y, int ui_x, int ui_y, Direction ui_dir, int ui_wordLength) {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;

            for (int x = 0; x < width; x++) {
                Console.ForegroundColor = GetCellColor(x, y, ui_x, ui_y, ui_dir, ui_wordLength);
                Console.Write(grid[x, y] + " ");
                //  + " (" + x + "," + y + ") ");
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("");
        }

        ConsoleColor GetCellColor(int x, int y, int ui_x, int ui_y, Direction ui_dir, int ui_wordLength) {
            if (x == ui_x && y == ui_y) {
                return ConsoleColor.Green;
            }

            
            for (int i = 0; i < ui_wordLength; i++) {
                if (x == ui_x + ui_dir.X * i && y == ui_y + ui_dir.Y * i) {
                    return ConsoleColor.Red;
                }
            }

            
            return ConsoleColor.White;
        }
    }
}