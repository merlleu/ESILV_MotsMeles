using System;

namespace PooProject {
    public class Program {
        public static void Main(string[] args) {
            Console.ForegroundColor = ConsoleColor.White;

            // create game
            // load save from file if an argument is supplied.
            Jeu game = args.Length > 0 ? new Jeu(args[0]) : new Jeu();
            
            // start game
            game.Start(args.Length > 0);
        }
    }
}