using System;

namespace PooProject {
    
    public class Joueur {
        private string nom;
        private List<string> words;
        private int score;

        public int Score {
            get => score;
        }

        public String Nom {
            get => nom;
        }

        public List<string> Words {
            get => words;
        }

        public Joueur(string nom) {
            this.nom = nom;
            words = new List<string>();
            score = 0;
        }

        public void Add_Mot(string mot) {
            words.Add(mot);
            score += mot.Length;
        }
        
        public override string ToString() {
            return $"{nom}, {score} points ({words.Count} mots)";
        }
    }
}