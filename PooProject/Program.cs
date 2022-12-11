using System;

namespace PooProject {
    public class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args) {
            // reset console colors and clear the screen
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            // ask if the user wants to load a previous game
            bool load_save = AskForLoadPreviousGame("game.csv");

            // create game
            
            Jeu game = load_save ? new Jeu("game.csv") : new Jeu();
            
            // start game
            game.Start(load_save);
        }


        /// <summary>
        /// If a save file is present, ask the user if he wants to load it.
        /// </summary>
        /// <param name="saveFile">The path to the save file.</param>
        /// <returns>True if the user wants to load the save file, false otherwise.</returns>
        private static bool AskForLoadPreviousGame(string saveFile) {
            if (!System.IO.File.Exists(saveFile)) {
                return false;
            }
            Console.Write("Voulez-vous charger la partie précédente ? (o/n) > ");
            string answer = Console.ReadLine();
            if (answer == "o") {
                return true;
            } else if (answer == "n") {
                return false;
            } else {
                Console.WriteLine("Réponse invalide.");
                return AskForLoadPreviousGame(saveFile);
            }
        }
    }
}