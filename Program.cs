using System;

namespace PooProject {
    public class Program {
        public static void Main(string[] args) {
            Console.ForegroundColor = ConsoleColor.White;
            // create dictionnary
            Dictionnaire dictionnary = new Dictionnaire();

            // Console.WriteLine(dictionnary.ToString());
            // create game
            Jeu game = new Jeu(dictionnary);
            
            // start game
            game.Start();
        }
    }
}