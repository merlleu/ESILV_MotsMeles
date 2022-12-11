using System;

namespace PooProject {
    /// <summary>
    /// Plateau contains the grid of letters and is responsible for generating it.
    /// </summary>
    public class Plateau {
        #region Fields
        private char[,] grid; // the grid of letters
        private int difficulty; // the difficulty of the grid
        private string[] words; // the list of words in the grid
        private int width; // the width of the grid
        private int height; // the height of the grid
        private Dictionnaire dictionnary; // the dictionnary used to generate the grid
        private Direction[] doable_directions; // the directions that can be used to generate the grid, depending on the difficulty
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Plateau with the given dictionnary.
        /// </summary>
        public Plateau(Dictionnaire dictionnary) {
            this.dictionnary = dictionnary;
            this.grid = new char[0,0];
            this.words = new string[0];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Words in the grid.
        /// </summary>
        public string[] Words {
            get => words;
        }

        /// <summary>
        /// Width of the grid.
        /// </summary>
        public int Width {
            get => width;
        }

        /// <summary>
        /// Height of the grid.
        /// </summary>
        public int Height {
            get => height;
        }

        /// <summary>
        /// List of directions that can be used to generate the grid.
        /// It depends on the difficulty.
        /// </summary>
        public Direction[] DoableDirections {
            get => doable_directions;
        }
        #endregion
        
        #region Grid generation
        /// <summary>
        /// Loads the grid with the given difficulty.
        /// </summary>
        /// <param name="difficulty">The difficulty of the grid.</param>
        public void Load(int difficulty) {
            this.difficulty = difficulty;
            GetDimensions();
            this.doable_directions = Direction.GetAvailableDirections(difficulty);
            GenerateGrid(dictionnary, difficulty, width, height);
        }

        /// <summary>
        /// Sets the width and height of the grid depending on the difficulty.
        /// [!] this.difficulty must be set before calling this method.
        /// </summary>
        void GetDimensions() {
            // width & height MUST be between 3 and 25.
            width = 5 + difficulty * 3;
            height = 5 + difficulty * 3;
        }

        /// <summary>
        /// Generates a grid of characters by randomly placing words from a dictionary into the grid.
        /// </summary>
        /// <param name="dictionnary">The dictionary to use for selecting words to place in the grid.</param>
        /// <param name="difficulty">An integer representing the difficulty of the generated grid. This determines the number of words placed in the grid.</param>
        /// <param name="width">The width of the grid, in characters.</param>
        /// <param name="height">The height of the grid, in characters.</param>
        void GenerateGrid(Dictionnaire dictionnary, int difficulty, int width, int height) {
            grid = new char[width, height]; // fill with \0
            Random r = new Random();
            
            // The basic idea is: 
            // 1. Until we have placed enough words:
            // 2. Pick a random direction.
            // 3. Pick a random starting point.
            // 4. Pick a random word length.
            // 5. Find a word that fits in the selected direction and at the selected starting point.
            // 6. Place the word in the grid and add it to the list of placed words.

            words = new string[3 + difficulty *  5]; // initialize the list of placed words
            for (int i = 0; i < words.Length;) { // repeat until we have placed enough words
                Direction direction = doable_directions[r.Next(0, doable_directions.Length - 1)]; // pick a random direction

                int x = r.Next(0, width); // pick a random starting point
                int y = r.Next(0, height);

                int length = r.Next(4, width); // pick a random word length

                // check if the selected direction and starting point will allow a word of the selected length to fit in the grid
                if (!CheckInBounds(x + direction.X * (length), y + direction.Y * (length))) {
                    continue; // if not, skip to the next iteration
                }
                
                // get a list of words with the selected length from the dictionary
                string[] w = dictionnary.GetWords(length);
                if (w == null || w.Length == 0) { // if no words were found, skip to the next iteration
                    continue;
                }
                int wordIndex = r.Next(0, w.Length); // pick a random index from the list of words to start searching for a word to place

                for (int j = wordIndex; j < w.Length; j++) { // search for a (not already present) word that fits in the selected direction and at the selected starting point
                    if (CanPlaceWord(w[j], x, y, direction) && !words.Contains(w[j])) { // if a word is found, place it in the grid and add it to the list of placed words
                        PlaceWord(w[j], x, y, direction);
                        words[i] = w[j];
                        i++;
                        Console.WriteLine($"[{i}] Placed {w[j]} at {x}, {y} in direction {direction.ToString()}, {j}{wordIndex}/{w.Length}");
                        break;
                    }
                }
            }

            // fill the rest of the grid with random letters
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    if (grid[i, j] == '\0') {
                        grid[i, j] = (char)((int)'A' + r.Next(0, 26));
                    }
                }
            }

            Array.Sort(words); // sort the list of placed words alphabetically
        }

        /// <summary>
        /// Checks if the word can be placed in the grid at the given position in the given direction.
        /// For each cell in the word, we check if the cell is empty or if it contains the same letter as the word.
        /// </summary>
        /// <param name="word">The word to check.</param>
        /// <param name="x">The x coordinate of the starting point.</param>
        /// <param name="y">The y coordinate of the starting point.</param>
        /// <param name="direction">The direction in which to check the word.</param>
        /// <returns>True if the word can be placed in the grid at the given position in the given direction, false otherwise.</returns>
        bool CanPlaceWord(string word, int x, int y, Direction direction) {
            bool canPlace = true;
            for (int i = 0; i < word.Length && canPlace; i++) {
                x += direction.X;
                y += direction.Y;
                if (grid[x, y] != '\0' && grid[x, y] != word[i]) {
                    canPlace = false;
                }
            }
            return canPlace;
        }
        
        /// <summary>
        /// Places the word in the grid at the given position in the given direction.
        /// You should check if the word can be placed in the grid before calling this method.
        /// </summary>
        /// <param name="word">The word to place.</param>
        /// <param name="x">The x coordinate of the starting point.</param>
        /// <param name="y">The y coordinate of the starting point.</param>
        /// <param name="direction">The direction in which to place the word.</param>
        void PlaceWord(string word, int x, int y, Direction direction) {
            for (int i = 0; i < word.Length; i++) {
                x += direction.X;
                y += direction.Y;
                grid[x, y] = word[i];
            }
        }

        /// <summary>
        /// This method checks if a cell is in the grid.
        /// </summary>
        /// <param name="x">The x coordinate of the cell.</param>
        /// <param name="y">The y coordinate of the cell.</param>
        /// <returns>True if the cell is in the grid, false otherwise.</returns>
        public bool CheckInBounds(int x, int y) {
            return x >= 0 && y >= 0 && x < width && y < height;
        }
        #endregion

        #region Word search
        /// <summary>
        /// Returns the word in the grid at the given position in the given direction.
        /// If it goes out of the grid, the word will be truncated.
        /// </summary>
        /// <param name="x">The x coordinate of the starting point.</param>
        /// <param name="y">The y coordinate of the starting point.</param>
        /// <param name="direction">The direction of the word.</param>
        /// <param name="length">The length of the word.</param>
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

        #endregion

        #region UI Renderers & Helpers
        /// <summary>
        /// This method logs the grid to the console.
        /// The grid cells colors represents how many words are found on the cell.
        /// </summary>
        /// <param name="player">The player to check the words for.</param>
        /// <param name="ui_x">The x coordinate of the starting point.</param>
        /// <param name="ui_y">The y coordinate of the starting point.</param>
        /// <param name="ui_dir">The direction of the word.</param>
        /// <param name="ui_wordLength">The length of the word.</param>
        public void DisplayTableau(Joueur player, int ui_x, int ui_y, Direction ui_dir, int ui_wordLength) {
            for (int y = 0; y < height; y++) {
                DisplayTableauRow(y, ui_x, ui_y, ui_dir, ui_wordLength);
            }
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"Progression : {player.Words.Count} / {words.Length} ({((int) (player.Words.Count / (float) words.Length * 100))}%)");
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

        /// <summary>
        /// This method logs a row of the grid to the console.
        /// </summary>
        /// <param name="y">The y coordinate of the row.</param>
        /// <param name="ui_x">The x coordinate of the starting point.</param>
        /// <param name="ui_y">The y coordinate of the starting point.</param>
        /// <param name="ui_dir">The direction of the word.</param>
        /// <param name="ui_wordLength">The length of the word.</param>
        private void DisplayTableauRow(int y, int ui_x, int ui_y, Direction ui_dir, int ui_wordLength) {
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

        /// <summary>
        /// This method returns the color of a cell in the grid.
        /// A cell is green if it is the starting point of the word.
        /// A cell is red if it is part of the word.
        /// Else, the cell is white.
        /// </summary>
        /// <param name="x">The x coordinate of the cell.</param>
        /// <param name="y">The y coordinate of the cell.</param>
        /// <param name="ui_x">The x coordinate of the starting point.</param>
        /// <param name="ui_y">The y coordinate of the starting point.</param>
        /// <param name="ui_dir">The direction of the word.</param>
        /// <param name="ui_wordLength">The length of the word.</param>
        /// <returns>The color of the cell.</returns>
        private ConsoleColor GetCellColor(int x, int y, int ui_x, int ui_y, Direction ui_dir, int ui_wordLength) {
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

        #endregion

        #region Load/Save
        /// <summary>
        /// Represents the grid as a string.
        /// First line is: difficulty;width;height;word count
        /// Second line is: {word_1};{word_2};...{word_n}
        /// Then the following lines are the grid, each line is a row.
        /// Each cell is separated by a semicolon.
        /// </summary>
        /// <returns>The grid as a string.</returns>
        public override string ToString() {
            string s = "";
            s += $"{difficulty};{width};{height};{words.Length}\n";
            for (int i = 0; i < words.Length; i++) {
                s += $"{words[i]};";
            }
            s += "\n";
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    s += grid[j, i] + ";";
                }
                s += "\n";
            }
            return s;
        }
        
        /// <summary>
        /// Dumps the grid to a file.
        /// </summary>
        /// <param name="nomfile">The relative path of the file.</param>
        public void ToFile(string nomfile) {
            File.WriteAllText(nomfile, ToString());
        }

        /// <summary>
        /// Loads a grid from a file.
        /// </summary>
        /// <param name="nomfile">The relative path of the file.</param>
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
            Console.WriteLine("grid.GetLength(0): " + grid.GetLength(0) + ", grid.GetLength(1): " + grid.GetLength(1) + ", width: " + width + ", height: " + height);
            for (int i = 0; i < height; i++) {
                string[] cells = lines[i + 2].Split(';');
                Console.WriteLine($"cells: {cells.Length}, width: {width}, height: {height}, i: {i}");
                for (int j = 0; j < width; j++) {
                    var t = cells[j][0];
                    grid[j, i] = t;
                }
            }

            doable_directions = Direction.GetAvailableDirections(difficulty);
        }
        #endregion
    
        #region Misc
        /// <summary>
        /// DEPRECATED: This method is no longer used.
        /// It's old purpose was to help find all the words that can be found on a randomly generated grid.
        /// </summary>
        private void FindWordsForCell(int x, int y, Direction direction, List<string> words) {
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

        /// <summary>
        /// DEPRECATED: This method is no longer used.
        /// It's an efficient way to validate if a user coordinate corresponds to a valid word.
        /// We don't use it anymore because we can just check if GetWordForCellVector(...) is included in the list of words.
        /// </summary>
        private bool Test_Plateau(string mot, int ligne, int colonne, Direction direction) {
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
        #endregion
    }
}