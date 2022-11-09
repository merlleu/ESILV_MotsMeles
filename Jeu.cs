using System;

namespace PooProject {
    /// Jeu is the main class of the program.
    /// It has all the available context and is responsible for the game logic.
    /// It is created after the dictionnary has been loaded and the grid created.
    public class Jeu {
        // list of players
        private Joueur[] players;

        // current grid & previous grids
        private Plateau grid;
        private List<Plateau> grid_history;

        private Dictionnaire dictionnary;
        // number of seconds before the turn ends
        private int timeout;
        private int difficulty;
        private int cur_player;

        private int ui_x = -1;
        private int ui_y = -1;
        private Direction ui_direction = new Direction(0, 0);
        private int ui_wordLength = 2;
        private string save_path;

        private DateTime ui_end_time = DateTime.Now;
        
        public Jeu() {
            this.dictionnary = new Dictionnaire();
            this.grid = new Plateau(dictionnary);
            this.grid_history = new List<Plateau>();
            this.players = LoadPlayers();
            this.timeout = AskTimeout();
            this.difficulty = 1;
            this.save_path = "game.csv";
        }


        /// Load Jeu from save file.
        public Jeu(string save_path) {
            this.save_path = save_path;
            FromFile();
            grid = new Plateau(dictionnary);

        }

        /// Ask user for the round duration.
        static int AskTimeout() {
            Console.Write("Entrez le temps de réflexion par joueur (en secondes) > ");
            int timeout = int.Parse(Console.ReadLine());

            if (timeout < 0) {
                Console.WriteLine("Le temps de réflexion doit être positif.");
                timeout =  AskTimeout();
            }
            return timeout;
        }
        
        // Ask user for the number of players and their names.
        static Joueur[] LoadPlayers() {
            Console.Write("Player count (1-9) > ");
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
        
        public void Start(bool resume = false) {
            for (;difficulty <= 5; difficulty ++) {
                cur_player = 0;
                for (; cur_player < players.Length; cur_player++) {
                    StartPlayerTurn(players[cur_player], resume);
                    if (resume) resume = false;

                    cur_player++;
                }
            }
            
        }

        void StartPlayerTurn(Joueur player, bool resume) {
            // first, we set the timeout
            ui_end_time = DateTime.Now.AddSeconds(timeout);
            player.ClearWords();
            // grid_history.Add(grid);
            grid = new Plateau(dictionnary);
            
            // if we resume, we load the grid from the save file instead of creating a new one
            if (resume) grid.FromFile("grid.csv");
            else grid.Load(difficulty);

            // Save current game
            ToFile();
            grid.ToFile("grid.csv");

            LoopPlayerTurn(player);
        }

        void LoopPlayerTurn(Joueur player) {
            bool playing = true;
            while (playing) {
                ui_x = ui_y = 0;
                ui_direction = new Direction(0, 0);
                ui_wordLength = 2;
                Console.Clear();
                RefreshPlayerUI(player);

                int step = 0;
                while (step < 3 && DateTime.Now <= ui_end_time) {
                    if (step == 0) step += AskForPosition(player);
                    else if (step == 1) step += AskForDirection(player);
                    else if (step == 2) step += AskForWordLength(player);
                }

                if (DateTime.Now > ui_end_time) {
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
            
        }

        void RefreshPlayerUI (Joueur player, bool keep_cursor = false) {
            Console.Clear();
            string TimeLeft = (ui_end_time - DateTime.Now).ToString(@"mm\:ss");
            Console.WriteLine($"[Round {difficulty} | {player.Nom}] Temps restant : {TimeLeft}\n");
            grid.DisplayTableau(player, ui_x, ui_y, ui_direction, ui_wordLength);
            Console.WriteLine();
        }

        

        /// Ask user for a position and return it.
        /// The position is a single letter
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


            } while(keyinfo.Key != ConsoleKey.Enter && DateTime.Now <= ui_end_time);

            Console.WriteLine();
            return 1;
        }

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
                        ui_direction = grid.DoableDirections[dir_index];
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        if (dir_index < grid.DoableDirections.Length - 1) dir_index++;
                        ui_direction = grid.DoableDirections[dir_index];
                        break;
                    
                    case ConsoleKey.Escape:
                        ui_direction = new Direction(0, 0);
                        return -1;
                }
            } while(keyinfo.Key != ConsoleKey.Enter && DateTime.Now <= ui_end_time);

            Console.WriteLine();
            return 1;
        }

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
            } while(keyinfo.Key != ConsoleKey.Enter && DateTime.Now <= ui_end_time);

            Console.WriteLine();
            return 1;
        }

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
                players[i-1] = new Joueur(player_data[0], int.Parse(player_data[1]), int.Parse(player_data[2]));
            }
        }
    }
}