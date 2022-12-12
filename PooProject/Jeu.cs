using System;

namespace PooProject {
    /// <summary>
    /// Jeu is the main class of the program.
    /// It has all the available context and is responsible for the game logic.
    /// It is created after the dictionnary has been loaded and the grid created.
    /// </summary>
    public class Jeu {
        #region Game Fields
        private Joueur[] players; // list of players
        private Plateau grid; // current grid
        // private List<Plateau> grid_history;

        private Dictionnaire dictionnary;
        
        private int timeout; // number of seconds before the turn ends
        private int difficulty; // round number in range [1; 5]
        private int cur_player; // current player index
        private string save_path = "game.csv"; // path to save file
        private DateTime round_end_time = DateTime.Now; // time when the turn ends
        #endregion

        #region Word selector UI variables
        private int ui_x = -1; // selected X coordinate, -1 if none
        private int ui_y = -1; // selected Y coordinate, -1 if none
        private Direction ui_direction = new Direction(0, 0); // selected direction, 0,0 if none
        private int ui_wordLength = 2; // selected word length, 2 if none
        #endregion
   
        #region Constructors
        /// <summary>
        /// Creates a new game.
        /// </summary>
        public Jeu() {
            this.dictionnary = new Dictionnaire();
            this.grid = new Plateau(dictionnary);
            // this.grid_history = new List<Plateau>();
            this.players = LoadPlayers();
            this.timeout = AskTimeout();
            this.difficulty = 1;
            this.save_path = "game.csv";
        }

        /// <summary>
        /// Load game from save file.
        /// </summary>
        /// <param name="save_path">Path to the save file.</param>
        public Jeu(string save_path) {
            this.save_path = save_path;
            FromFile();
            grid = new Plateau(dictionnary);
        }
        #endregion

        #region Constructors input helpers
        /// <summary>
        /// Ask user for the round duration.
        /// </summary>
        /// <returns>The round duration in seconds.</returns>
        static int AskTimeout() {
            Console.Write("Entrez le temps de réflexion par joueur (en secondes, min: 1) > ");
            int timeout;
            int.TryParse(Console.ReadLine(), out timeout);

            if (timeout <= 0) {
                Console.WriteLine("Temps de réflexion invalide.");
                timeout =  AskTimeout();
            }
            return timeout;
        }
        
        /// <summary>
        /// Ask user for the number of players and their names.
        /// </summary>
        /// <returns>An array of players.</returns>
        static Joueur[] LoadPlayers() {
            Console.Write("Combien de joueurs ? (1-9) > ");
            // try to parse the input as an int
            int nb;
            int.TryParse(Console.ReadLine(), out nb);
            
            Joueur[] players;
            if (nb >= 1 && nb <= 9) {
                players = new Joueur[nb];
                for (int i = 0; i < nb; i++) {
                    Console.Write($"Nom du joueur {i + 1} >");
                    players[i] = new Joueur(Console.ReadLine());
                }
            } else {
                Console.WriteLine("Nombre de joueurs invalide.");
                players = LoadPlayers();
            }
            
            return players;
        }

        #endregion

        #region Game logic
        /// <summary>
        /// Start the game.
        /// </summary>
        /// <param name="resume">If true, the round will start at it's latest saved point in time.</param>
        public void Start(bool resume = false) {
            for (;difficulty <= 5; difficulty ++) {
                if (!resume) cur_player = 0;
                for (; cur_player < players.Length; cur_player++) {
                    StartPlayerTurn(players[cur_player], resume);
                    if (resume) resume = false;
                }
            }

            // end of game
            ShowGameSummary();
        }

        /// <summary>
        /// Start a player turn.
        /// </summary>
        /// <param name="player">The player whose turn it is.</param>
        /// <param name="resume">If true, the player words won't be cleared.</param>
        void StartPlayerTurn(Joueur player, bool resume) {
            // first, we set the timeout
            round_end_time = DateTime.Now.AddSeconds(timeout);
            if (!resume) player.ClearWords();
            // grid_history.Add(grid);
            grid = new Plateau(dictionnary);
            
            // if we resume, we load the grid from the save file instead of creating a new one
            string grid_path = save_path.Replace("game", "grid");
            if (resume) grid.FromFile(grid_path);
            else grid.Load(difficulty);

            // Save current game
            ToFile();
            grid.ToFile(grid_path);

            LoopPlayerTurn(player);
        }

        /// <summary>
        /// Player turn loop.
        /// </summary>
        /// <param name="player">The player whose turn it is.</param>
        void LoopPlayerTurn(Joueur player) {
            bool playing = true;
            while (playing) {
                ui_x = ui_y = 0;
                ui_direction = new Direction(0, 0);
                ui_wordLength = 2;
                Console.Clear();
                RefreshPlayerUI(player);

                int step = 0;
                while (step < 3 && DateTime.Now <= round_end_time) {
                    if (step == 0) step += AskForPosition(player);
                    else if (step == 1) step += AskForDirection(player);
                    else if (step == 2) step += AskForWordLength(player);
                }

                if (DateTime.Now > round_end_time) {
                    playing = false;
                    continue;
                }

                // Now we have all the user inputs
                // We can check if the word is valid
                string word = grid.GetWordForCellVector(ui_x, ui_y, ui_direction, ui_wordLength);
                if (!grid.Words.Contains(word)) {
                    Console.Clear(); 
                    Console.WriteLine("Le mot n'est pas valide...");
                    Console.WriteLine("Appuyez sur une touche pour continuer...");
                    Console.ReadKey();
                } else {
                    if (player.Words.Contains(word)) {
                        Console.Clear(); 
                        Console.WriteLine("Vous avez déjà utilisé ce mot...");
                        Console.WriteLine("Appuyez sur une touche pour continuer...");
                        Console.ReadKey();
                    } else {
                        player.Add_Mot(word);
                    }
                }

                // The round is ended if the player has no more time or if he has no more words to play
                
                if (player.Words.Count == grid.Words.Length) {
                    player.Won_Round();
                    playing = false;
                }

                ToFile();
            }

            player.ClearWords();

            if (player.Words.Count != grid.Words.Length && DateTime.Now > round_end_time) {
                ShowTimeOut(player);
            }
            
        }

        /// <summary>
        /// This method returns the indexes of the players
        /// Sorted by HIGHEST SCORE first then by their names if they have the same score. 
        /// </summary>
        /// <returns>An array of player indexes sorted by their ranking.</returns>
        int[] GetPlayerRanking() {
            int[] ranking = new int[players.Length];
            for (int i = 0; i < players.Length; i++) {
                ranking[i] = i;
            }

            for (int i = 0; i < players.Length; i++) {
                for (int j = i + 1; j < players.Length; j++) {
                    if (
                        players[ranking[i]].Score < players[ranking[j]].Score || 
                        (
                            players[ranking[i]].Score == players[ranking[j]].Score && 
                            players[ranking[i]].Nom.CompareTo(players[ranking[j]].Nom) > 0
                        )
                    ) {
                        int tmp = ranking[i];
                        ranking[i] = ranking[j];
                        ranking[j] = tmp;
                    }
                }
            }

            return ranking;
        }
        #endregion

        #region UI renderers & helpers
        /// <summary>
        /// Displays the ranking on top of the screen
        /// It shows a bar filled with players
        /// [Player 1 (10)] [Player 2 (5)] [Player 3 (0)]
        /// </summary>
        void ShowRankingHeader() {
            int total_width = Console.WindowWidth - 2;
            int used_width = 0;
            int total_points = 0;
            for (int i = 0; i < players.Length; i++) {
                // we calculate the minimum width of the bar
                used_width += players[i].Nom.Length + 4 + players[i].Score.ToString().Length + 2;
                total_points += 50 + players[i].Score;
            }

            // we calculate the remaining width
            int remaining_width = total_width - used_width;
            if (remaining_width < 0) remaining_width = 0;

            // sort the players by score
            int[] ranking = GetPlayerRanking();

            // we display the ranking
            // the more points a player has, the more space he gets
            for (int i = 0; i < ranking.Length; i++) {
                Joueur player = players[ranking[i]];
                Console.BackgroundColor = GetPlayerColorPrimary(ranking[i]);
                Console.Write($" {player.Nom} ({player.Score})");

                int bar_width = (int) Math.Round((double) (50+player.Score) / total_points * remaining_width);
                if (bar_width > 0) Console.Write(new string(' ', bar_width));
                Console.Write(" ");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
            }

            Console.WriteLine();
        }
        
        /// <summary>
        /// Gets the color of a player.
        /// </summary>
        /// <param name="player_index">The index of the player.</param>
        /// <returns>The color of the player.</returns>
        ConsoleColor GetPlayerColorPrimary (int player_index) {
            switch (player_index) {
                case 0: return ConsoleColor.Red;
                case 1: return ConsoleColor.Green;
                case 2: return ConsoleColor.Blue;
                case 3: return ConsoleColor.Yellow;
                case 4: return ConsoleColor.Magenta;
                case 5: return ConsoleColor.Cyan;
                case 6: return ConsoleColor.DarkRed;
                case 7: return ConsoleColor.DarkGreen;
                case 8: return ConsoleColor.DarkBlue;
                default: return ConsoleColor.White;
            }
        }

        /// <summary>
        /// Renders main game UI for a player.
        /// </summary>
        /// <param name="player">The player to render the UI for.</param>
        void RefreshPlayerUI (Joueur player) {
            Console.Clear();
            ShowRankingHeader();
            string TimeLeft = (round_end_time - DateTime.Now).ToString(@"mm\:ss");
            WriteWithColor($"[Round {difficulty} | ", ConsoleColor.White);
            WriteWithColor($"{player.Nom}", GetPlayerColorPrimary(cur_player));
            WriteWithColor("] Temps restant : ", ConsoleColor.White);
            WriteWithColor($"{TimeLeft}\n\n", ConsoleColor.Blue);
            Console.ForegroundColor = ConsoleColor.White;
            
            grid.DisplayTableau(player, ui_x, ui_y, ui_direction, ui_wordLength);
            Console.WriteLine();
        }

        /// <summary>
        /// Displays time out message.
        /// </summary>
        /// <param name="player">The player who timed out.</param>
        void ShowTimeOut(Joueur player) {
            Console.Clear();
            ShowRankingHeader();
            WriteWithColor($"[Round {difficulty} | ", ConsoleColor.White);
            WriteWithColor($"{player.Nom}", GetPlayerColorPrimary(cur_player));
            WriteWithColor("] ", ConsoleColor.White);
            WriteWithColor("Temps écoulé !\n\n\n", ConsoleColor.Red);
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Appuyez sur ENTRER pour continuer...");
            Console.ReadLine();
        }

        /// <summary>
        /// At the end of the game
        /// It displays the ranking.
        /// </summary>
        void ShowGameSummary() {
            Console.Clear();
            ShowRankingHeader();
            Console.WriteLine();
            
            Console.WriteLine("Résumé de la partie :");
            int[] ranking = GetPlayerRanking();
            for (int i = 0; i < ranking.Length; i++) {
                Joueur player = players[ranking[i]];
                ConsoleColor color = GetPlayerColorPrimary(ranking[i]);
                WriteWithColor($"{i+1} - ", ConsoleColor.White);
                WriteWithColor(player.Nom, color);
                WriteWithColor(" : ", ConsoleColor.White);
                WriteWithColor(""+player.Score, color);
                WriteWithColor(" points(", ConsoleColor.White);
                WriteWithColor(""+player.RoundsWon, color);
                WriteWithColor(" rounds gagnés)", ConsoleColor.White);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
            }
            
            Console.ReadKey();

        }

        /// <summary>
        /// Write to console with a specific text color.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="color">The color to use.</param>
        void WriteWithColor(string text, ConsoleColor color) {
            Console.ForegroundColor = color;
            Console.Write(text);
        }

        
        /// <summary>
        /// Ask user for a position.
        /// </summary>
        /// <param name="player">The player to ask for a position.</param>
        /// <returns>1 if we go to the next step, -1 to go back</returns>
        int AskForPosition(Joueur player) {
            ConsoleKeyInfo keyinfo;
            do {
                RefreshPlayerUI(player);
                Console.Write("Sélectionnez une lettre (ZQSD / Touches fléchées) puis appuyez sur Entrée > ");
                int pos = Console.CursorLeft;
                keyinfo = Console.ReadKey();

                // Handle arrows & ZQSD
                switch (keyinfo.Key) {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.Z:
                        if (ui_y > 0) ui_y--;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        if (ui_y < grid.Height - 1) ui_y++;
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.Q:
                        if (ui_x > 0) ui_x--;
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        if (ui_x < grid.Width - 1) ui_x++;
                        break;
                }

                Console.SetCursorPosition(pos, Console.CursorTop);
                Console.Write("   ");
                Console.SetCursorPosition(pos, Console.CursorTop);


            } while(keyinfo.Key != ConsoleKey.Enter && DateTime.Now <= round_end_time);

            Console.WriteLine();
            return 1;
        }

        /// <summary>
        /// Ask user for a direction and return it.
        /// </summary>
        /// <param name="player">The player to ask for a direction.</param>
        /// <returns>1 if we go to the next step, -1 to go back</returns>
        int AskForDirection(Joueur player) {
            int dir_index = 0;
            ui_direction = grid.DoableDirections[dir_index];

            ConsoleKeyInfo keyinfo;
            do {
                RefreshPlayerUI(player);
                Console.Write("Direction: ");
                Console.ForegroundColor = ConsoleColor.Blue;
                for (int i = 0; i < grid.DoableDirections.Length; i++) {
                    if (i == dir_index) Console.BackgroundColor = ConsoleColor.White;
                    Console.Write(grid.DoableDirections[i].ToString());
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(" ");
                }
                Console.Write(" ");
                int pos = Console.CursorLeft;

                Console.ForegroundColor = ConsoleColor.White;

                keyinfo = Console.ReadKey();
                // Handle arrows & ZQSD
                switch (keyinfo.Key) {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.Q:
                        if (dir_index > 0) dir_index--;
                        else dir_index = grid.DoableDirections.Length - 1;
                        ui_direction = grid.DoableDirections[dir_index];
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        if (dir_index < grid.DoableDirections.Length - 1) dir_index++;
                        else dir_index = 0;
                        ui_direction = grid.DoableDirections[dir_index];
                        break;
                    
                    case ConsoleKey.Escape:
                        ui_direction = new Direction(0, 0);
                        return -1;
                }
            } while(keyinfo.Key != ConsoleKey.Enter && DateTime.Now <= round_end_time);

            Console.WriteLine();
            return 1;
        }
        
        /// <summary>
        /// Ask user for a word length.
        /// </summary>
        /// <param name="player">The player to ask for a word length.</param>
        /// <returns>1 if we go to the next step, -1 to go back</returns>
        int AskForWordLength(Joueur player) {
            // max length without going out of the grid
            int max_length = 1;
            for (int i = 1; 
                grid.CheckInBounds(ui_x + ui_direction.X * i, ui_y + ui_direction.Y * i); 
                i++) {
                max_length++;
            }

            ConsoleKeyInfo keyinfo;
            do {
                RefreshPlayerUI(player);
                Console.Write("Taille: ");
                Console.ForegroundColor = ConsoleColor.Blue;

                for (int i = 2; i <= max_length; i++) {
                    if (i == ui_wordLength) Console.BackgroundColor = ConsoleColor.White;
                    Console.Write(i);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(" ");
                }

                Console.Write(" ");
                int pos = Console.CursorLeft;

                Console.ForegroundColor = ConsoleColor.White;

                keyinfo = Console.ReadKey();
                // Handle arrows & ZQSD
                switch (keyinfo.Key) {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.Q:
                        if (ui_wordLength > 2) ui_wordLength--;
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        if (ui_wordLength < max_length) ui_wordLength++;
                        break;
                    
                    case ConsoleKey.Escape:
                        ui_wordLength = 2;
                        return -1;
                }
            } while(keyinfo.Key != ConsoleKey.Enter && DateTime.Now <= round_end_time);

            Console.WriteLine();
            return 1;
        }
        #endregion

        #region load/save
        /// <summary>
        /// Save the game to a file.
        /// </summary>
        void ToFile() {
            // file format:
            // difficulty,lang,player_count,timeout,current_player
            // player1_name,player1_score,player1_rounds_won
            // player2_name,player2_score,player2_rounds_won
            // ...

            string s = "";
            s += $"{difficulty};{Dictionnaire.GetLanguageString(dictionnary.Language)};{players.Length};{timeout};{cur_player}\n";
            foreach (Joueur player in players) {
                s+= player.ToString() + "\n";
            }

            File.WriteAllText(save_path, s);
        }

        /// <summary>
        /// Load the game from a file.
        /// </summary>
        void FromFile() {
            string[] lines = File.ReadAllLines(save_path);
            string[] first_line = lines[0].Split(';');
            difficulty = int.Parse(first_line[0]);
            dictionnary = new Dictionnaire(first_line[1]);
            players = new Joueur[int.Parse(first_line[2])];
            timeout = int.Parse(first_line[3]);
            cur_player = int.Parse(first_line[4]);

            for (int i = 1; i < lines.Length; i++) {
                string[] player_data = lines[i].Split(';');
                string[] words = new string[0];
                if (player_data[3] != "") {
                    words = player_data[3].Split(',');
                }
                players[i-1] = new Joueur(player_data[0], int.Parse(player_data[1]), int.Parse(player_data[2]), words.ToList());
            }
        }
        #endregion
    }
}