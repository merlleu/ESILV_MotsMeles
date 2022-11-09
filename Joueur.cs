using System;

namespace PooProject {
    /// This class represents a player.
    /// A player has a name, a score
    public class Joueur {
        private string nom; // name of the player
        private List<string> words; // list of words found by the player
        private int score; // score of the player
        private int rounds_won; // number of rounds won by the player

        public int Score {
            get => score;
        }

        public String Nom {
            get => nom;
        }

        public List<string> Words {
            get => words;
        }

        /// Creates a new player with the given name.
        public Joueur(string nom) {
            this.nom = nom;
            words = new List<string>();
            score = 0;
        }

        /// Creates a new player with the given name, scores and rounds won.
        public Joueur(string nom, int score, int rounds_won) {
            this.nom = nom;
            words = new List<string>();
            this.score = score;
            this.rounds_won = rounds_won;
        }

        /// Adds a word to the list of found words & grant points.
        public void Add_Mot(string mot) {
            words.Add(mot);
            score += mot.Length;
        }
        
        /// Returns a string representation of the player.
        public override string ToString() {
            return $"{nom};{score};{rounds_won}";
        }

        /// Clear the player's list of already found words.
        public void ClearWords() {
            words.Clear();
        }

        /// Grants the player a bonus if he won the round in time.
        public void Won_Round() {
            score += 100;
            rounds_won++;
        }
    }
}