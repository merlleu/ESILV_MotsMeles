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

        private int ui_x = -1;
        private int ui_y = -1;
        private Direction ui_direction = new Direction(0, 0);
        private int ui_wordLength = 2;

        private DateTime ui_end_time = DateTime.Now;
        
        public Jeu(Dictionnaire dictionnary) {
            this.dictionnary = dictionnary;
            this.grid = new Plateau(dictionnary);
            this.grid_history = new List<Plateau>();
            this.players = LoadPlayers();
            this.timeout = 300;
            this.difficulty = 1;
        }

        /// Ask user for the round duration.
        static int AskTimeout() {
            Console.WriteLine("Entrez le temps de réflexion par joueur (en secondes) > ");
            int timeout = int.Parse(Console.ReadLine());

            if (timeout < 0) {
                Console.WriteLine("Le temps de réflexion doit être positif.");
                timeout =  AskTimeout();
            }
            return timeout;
        }
        
        // Ask user for the number of players and their names.
        static Joueur[] LoadPlayers() {
            return new Joueur[] {
                new Joueur("Joueur 1")
            };

            Console.Write("Player count (1-9) > ");
            int nb = int.Parse(Console.ReadLine());
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
        
        public void Start() {
            for (;difficulty <= 5; difficulty ++) {
                foreach (Joueur player in players) {
                    StartPlayerTurn(player);
                }
            }
            
        }

        void StartPlayerTurn(Joueur player) {
            // first, we set the timeout
            ui_end_time = DateTime.Now.AddSeconds(timeout);
            // player.ClearWords();
            // grid_history.Add(grid);
            grid = new Plateau(dictionnary);
            grid.Load(difficulty);
            LoopPlayerTurn(player);
        }

        void LoopPlayerTurn(Joueur player) {
            bool playing = true;
            while (playing) {
                ui_x = ui_y = 0;
                ui_direction = new Direction(0, 0);
                ui_wordLength = 2;
                Console.Clear();
                grid.ToFile("grid.csv");
                RefreshPlayerUI(player);

                int step = 0;
                while (step < 3) {
                    if (step == 0) step += AskForPosition(player);
                    else if (step == 1) step += AskForDirection(player);
                    else if (step == 2) step += AskForWordLength(player);
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
                if (DateTime.Now > ui_end_time) {
                    playing = false;
                }
                if (player.Words.Count == grid.Words.Length) {
                    player.Won_Round();
                    playing = false;
                }
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
            } while(keyinfo.Key != ConsoleKey.Enter );

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
            } while(keyinfo.Key != ConsoleKey.Enter );

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
            } while(keyinfo.Key != ConsoleKey.Enter );

            Console.WriteLine();
            return 1;
        }
    }
}