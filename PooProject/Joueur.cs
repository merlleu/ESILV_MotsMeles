using System;

namespace PooProject {
    /// <summary>
    /// This class represents a player.
    /// A player has a name, a score, a list of found words, and a number of rounds won.
    /// </summary>
    public class Joueur {
        #region Fields
        private string nom; // name of the player
        private List<string> words; // list of words found by the player
        private int score; // score of the player
        private int rounds_won; // number of rounds won by the player
        #endregion

        #region Properties
        /// <summary>
        /// The player's score.
        /// </summary>
        public int Score {
            get => score;
        }

        /// <summary>
        /// The player's name.
        /// </summary>
        public String Nom {
            get => nom;
        }

        /// <summary>
        /// The list of words found by the player.
        /// </summary>
        public List<string> Words {
            get => words;
        }

        /// <summary>
        /// The number of rounds won by the player.
        /// </summary>
        public int RoundsWon {
            get => rounds_won;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new player with the given name.
        /// </summary>
        /// <param name="nom">The name of the player.</param>
        public Joueur(string nom) {
            this.nom = nom;
            words = new List<string>();
            score = 0;
        }

        /// <summary>
        /// Creates a new player with the given name, scores and rounds won.
        /// </summary>
        /// <param name="nom">The name of the player.</param>
        /// <param name="score">The player's score.</param>
        /// <param name="rounds_won">The number of rounds won by the player.</param>
        /// <param name="words">The list of words found by the player.</param>
        public Joueur(string nom, int score, int rounds_won, List<string> words) {
            this.nom = nom;
            this.score = score;
            this.rounds_won = rounds_won;
            this.words = words;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a word to the list of found words and grants points to the player.
        /// </summary>
        /// <param name="mot">The word to add.</param>
        public void Add_Mot(string mot) {
            words.Add(mot);
            score += mot.Length;
        }
        
        /// <summary>
        /// Clears the list of found words.
        /// Should be called at the end / start of each round.
        /// </summary>
        public void ClearWords() {
            words.Clear();
        }

        /// <summary>
        /// Grants points to the player for winning a round.
        /// </summary>
        public void Won_Round() {
            score += 100;
            rounds_won++;
        }
        #endregion

        #region Representations
        /// <summary>
        /// Returns a string representation of the player.
        /// Using the format: "name;score;rounds_won;words"
        /// </summary>
        /// <returns>A string representation of the player.</returns>
        public override string ToString() {
            string w = string.Join(" ", words);
            return $"{nom};{score};{rounds_won};{w}";
        }
        #endregion
    }
}