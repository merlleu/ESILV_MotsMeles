using System;

namespace PooProject {
    /// <summary>
    /// Dictionnaire contains all the available words, accessible by their length.
    /// This class is responsible for asking the user for language and 
    /// loading the corresponding file.
    /// Dictionnaire is loaded exactly once, then its content can't be changed.
    /// </summary>
    public class Dictionnaire {
        #region Fields
        /// <summary>
        /// dictionnary contains all the words, accessible by their length.
        /// </summary>
        private string[][] dictionnary;
        private GameLanguage language;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the `Dictionnaire` class.
        /// This constructor asks the user for the language of the dictionary
        /// and loads the corresponding dictionary file.
        /// </summary>
        public Dictionnaire() {
            // Ask user for language and set language property
            GetLanguage();
            // Load dictionary from file
            dictionnary = LoadDictionnaire();
        }

        /// <summary>
        /// Initializes a new instance of the `Dictionnaire` class with the specified language.
        /// This constructor loads the dictionary file for the specified language.
        /// </summary>
        /// <param name="lang">The string representation of the language for the dictionary.</param>
        public Dictionnaire(string lang) {
            // Set language from string parameter
            language = (GameLanguage) GetLanguageFromString(lang);
            // Load dictionary from file
            dictionnary = LoadDictionnaire();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Property for getting the language of the dictionary.
        /// </summary>
        public GameLanguage Language {
            get => language;
        }
        #endregion

        #region Enums
        /// <summary>
        /// An enum representing the dictionnary languages available.
        /// </summary>
        public enum GameLanguage {
            ENGLISH,
            FRENCH
        }
        #endregion

        #region Constructors input helpers
        /// <summary>
        /// Reads the language from user input, show error 
        /// message if input is invalid, 
        /// else return the parsed language.
        /// Valid inputs are "english", "en", "french" and "fr".
        /// </summary>
        public void GetLanguage() {
            Console.Write("Langue (en/fr, defaut=fr) > ");
            string input = Console.ReadLine();
            GameLanguage? lang = GetLanguageFromString(input);
            if (lang != null) {
                this.language = (GameLanguage)lang;
            } else {
                Console.WriteLine("Langue inconnue, veuillez réessayer.");
                GetLanguage();
            }
        }
        
        /// <summary>
        /// Converts the `GameLanguage` enum to a string.
        /// </summary>
        /// <param name="language">The `GameLanguage` value to convert.</param>
        /// <returns>The string representation of the language.</returns>
        public static string GetLanguageString(GameLanguage language) {
            string lang = "";
            switch (language) {
                case GameLanguage.ENGLISH:
                    lang = "english";
                    break;
                case GameLanguage.FRENCH:
                    lang = "french";
                    break;
            }
            return lang;
        }

        /// <summary>
        /// Parse the language from a string.
        /// Valid strings are "english" and "french".
        /// If the string is empty or invalid, the language is set to french.
        /// </summary>
        /// <param name="language">The string representation of the language to parse.</param>
        /// <returns>The parsed `GameLanguage` value, or null if the input is invalid.</returns>
        public static GameLanguage? GetLanguageFromString(string language) {
            GameLanguage? lang = null;
            switch (language) {
                case "english":
                case "en":
                    lang = GameLanguage.ENGLISH;
                    break;
                case "french":
                case "fr":
                case "":
                    lang = GameLanguage.FRENCH;
                    break;
            }

            return lang;
        }

        #endregion

        #region Loading
        /// <summary>
        /// This method loads the dictionary from a file in the current directory,
        /// parses it, and returns its content as a jagged array of strings.
        /// The file should have the following syntax:
        /// {letter_count}
        /// {word_1}...{word_n}
        /// {letter_count}
        /// {word_1}...{word_n}
        /// ...
        /// The last line of the file should contain the maximum word length.
        /// </summary>
        /// <returns>A jagged array of strings containing the words from the dictionary.</returns>
        private string[][] LoadDictionnaire() {
            // Get the file path for the current language
            string path = GetLanguageFilePath();
            // Read all lines from the file
            string[] lines = File.ReadAllLines(path);

            // Get the maximum word length (second last line of the file)
            int maxLength = int.Parse(lines[lines.Length - 2]);

            // Create a jagged array of strings with the maximum size
            // dictionnary[0] will always be null because there are no words of length 0.
            string[][] dictionnary = new string[maxLength + 1][];

            // Parse the words from the file and add them to the dictionary
            for (int i = 0; i < lines.Length - 1; i += 2) {
                // Parse the letter count from the current line
                int letterCount = int.Parse(lines[i]);
                // Split the words from the next line by space into an array
                string[] words = lines[i + 1].Split(' ');
                // Add the words to the dictionary
                dictionnary[letterCount] = words;
            }
            
            return dictionnary;
        }

        /// <summary>
        /// Get the file path of the language file for the current `Language` property.
        /// </summary>
        /// <returns>The file path of the language file.</returns>
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
            return "resources\\" + path;
        }
        #endregion

        #region Getters
        /// <summary>
        /// Get the words from the dictionary with the specified number of letters.
        /// </summary>
        /// <param name="letterCount">The number of letters in the words to get.</param>
        /// <returns>An array of strings containing the words with the specified number of letters,
        /// or null if there are no such words in the dictionary.</returns>
        public string[] GetWords(int letterCount) {
            string[] words = null;
            if (
                dictionnary != null &&
                letterCount >= 1 &&
                letterCount < dictionnary.Length
            ) {
                // If the dictionary is not null and the letter count is valid,
                // return the words with the specified number of letters
                words = dictionnary[letterCount];
            }
            return words;
        }

        /// <summary>
        /// Searches for the specified word in the dictionary using a recursive binary search algorithm.
        /// </summary>
        /// <param name="mot">The word to search for in the dictionary.</param>
        /// <returns>True if the word was found in the dictionary, false otherwise.</returns>
        public bool RechDichoRecursif(string mot) {
            string[] words = GetWords(mot.Length);
            bool found = false;
            if (words != null && words.Length > 0) {
                // Recursive Dichotomic search.
                found = RechDichoRecursifInner(words, mot, 0, words.Length - 1);
            }
            return found;
        }

        /// <summary>
        /// Helper function for the recursive binary search algorithm used to search for a word in the dictionary.
        /// </summary>
        /// <param name="arr">The array of words to search in.</param>
        /// <param name="mot">The word to search for.</param>
        /// <param name="start">The start index of the search range in the array.</param>
        /// <param name="end">The end index of the search range in the array.</param>
        /// <returns>True if the word was found in the specified search range of the array, false otherwise.</returns>
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
        #endregion

        #region Representations
        /// <summary>
        /// Returns a string representation of the `Dictionnaire` object.
        /// The string shows the language of the dictionary and the number of words
        /// for each letter count.
        /// </summary>
        /// <returns>A string representation of the `Dictionnaire` object.</returns>
        public override string ToString() {
            string result = "Dictionnaire: " + this.language + "\n";
            // Iterate over the dictionary and add the number of words
            // for each letter count to the result string
            for (int letterCount = 1; letterCount < dictionnary.Length; letterCount++) {
                if (dictionnary[letterCount] != null) {
                    result += letterCount + " letters: " + dictionnary[letterCount].Length + " words\n";
                }
            }
            return result;
        }
        #endregion
    }
}