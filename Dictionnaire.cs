using System;
using System.Collections.Generic;

namespace PooProject {
    /// Dictionnaire contains all the available words, accessible by their length.
    /// This class is responsible for asking the user for language and 
    /// loading the corresponding file.
    /// Dictionnaire is loaded exactly once, then its content can't be changed.
    public class Dictionnaire {     
        /// dictionnary contains all the words, accessible by their length.
        private string[][] dictionnary;
        private GameLanguage language;

        public Dictionnaire() {
            language = GetLanguage();
            dictionnary = LoadDictionnaire();
        }

        /// An enum representing the dictionnary languages available.
        public enum GameLanguage {
            ENGLISH,
            FRENCH
        }

        /// Reads the language from user input, show error 
        /// message if input is invalid, 
        /// else return the parsed language.
        /// Valid inputs are "english", "en", "french" and "fr".
        public static GameLanguage GetLanguage() {
            return GameLanguage.FRENCH;
            // Console.Write("Langue (en/fr, defaut=fr) > ");
            // string input = Console.ReadLine();
            // if (input == "english" || input == "en") {
            //     return GameLanguage.ENGLISH;
            // } else if (input == "french" || input == "fr" || input == "") {
            //     return GameLanguage.FRENCH;
            // } else {
            //     Console.WriteLine("Invalid input, please try again.");
            //     return GetLanguage();
            // }
        }

        /// This method loads the dictionnary from a file
        /// in the current directory, parses it and returns its content.
        private string[][] LoadDictionnaire() {
            string path = GetLanguageFilePath();
            
            // file syntax is 
            // {letter_count}
            // {word_1}...{word_n}
            // {letter_count}
            // {word_1}...{word_n}
            // ...
            
            string[] lines = File.ReadAllLines(path);
            // Get the maximum word length 
            // (second last line of the file)
            int maxLength = int.Parse(lines[lines.Length - 2]);

            // create a jagged array of strings with the maximum size
            // dictionnary[0] will always be null because 
            // there are no words of length 0.

            string[][] dictionnary = new string[maxLength + 1][];
            for (int i = 0; i < lines.Length; i += 2) {
                // parse letter count
                int letterCount = int.Parse(lines[i]);
                // split words by space into array
                string[] words = lines[i + 1].Split(' ');
                // add words to dictionnary
                dictionnary[letterCount] = words;
            }
            
            return dictionnary;
        }

        private string GetLanguageFilePath() {
            string path;
            switch (language) {
                case GameLanguage.FRENCH:
                    path = "MotsPossiblesFR.txt";
                    break;
                case GameLanguage.ENGLISH:
                    path = "MotsPossiblesEN.txt";
                    break;
                default:
                    // Unreachable.
                    throw new Exception("GameLanguage not supported");
            }
            return path;
        }

        /// We do not allow the user to change the dictionnary once it has been created.
        /// Access dictionnary words.
        public string[] GetWords(int letterCount) {
            string[] r = null;
            if (
                dictionnary != null &&
                letterCount >= 1 &&
                letterCount < dictionnary.Length
            ) {
                r = dictionnary[letterCount];
            }
            return r;
        }

        /// ToString represents the dictionnary: 
        /// it shows the language and the number of words for each letter count.
        public override string ToString() {
            string result = "Dictionnaire: " + this.language + "\n";
            for (int letterCount = 1; letterCount < dictionnary.Length; letterCount++) {
                if (dictionnary[letterCount] != null) {
                    result += letterCount + " letters: " + dictionnary[letterCount].Length + " words\n";
                }
            }
            return result;
        }


        /// Dichotomic search in a sorted array. 
        public bool RechDichoRecursif(string mot) {
            string[] words = GetWords(mot.Length);
            bool found = false;
            if (words != null && words.Length > 0) {
                // Recursive Dichotomic search.
                found = RechDichoRecursifInner(words, mot, 0, words.Length - 1);
            }
            return found;
        }

        /// This method is private because it is only used by RechDichoRecursif.
        /// The basic idea is to split the array in two parts,
        /// and check if the word is in the first or second part
        /// by comparing it to the middle element.
        /// If it is, we split the part in two again, and so on.
        /// If it isn't, we do the same thing with the other part.
        private static bool RechDichoRecursifInner(string[] arr, string mot, int start, int end) {
            bool found = false;
            if (start < end) {
                int middle = (start + end) / 2;
                if (arr[middle] == mot) {
                    found = true;
                } else if (arr[middle].CompareTo(mot) > 0) {
                    found = RechDichoRecursifInner(arr, mot, start, middle - 1);
                } else {
                    found = RechDichoRecursifInner(arr, mot, middle + 1, end);
                }
            }

            return found;
        }
    }
}