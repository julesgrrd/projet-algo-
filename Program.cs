namespace Projet_info
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        string filePath = "soc-karate.mtx";

        // Lire toutes les lignes du fichier
        string[] lines = File.ReadAllLines(filePath);

        // Ignorer les 23 premières lignes et stocker les données à partir de la ligne 24
        var dataLines = lines.Skip(23).ToArray();

        // Initialiser la matrice avec le bon nombre de lignes
        int[,] matrix = new int[dataLines.Length, 2];

        // Remplir la matrice
        for (int i = 0; i < dataLines.Length; i++)
        {
            string[] numbers = dataLines[i].Split(' ');
            matrix[i, 0] = int.Parse(numbers[0]);  // Première colonne
            matrix[i, 1] = int.Parse(numbers[1]);  // Deuxième colonne
        }

        // Afficher la matrice pour vérification
        Console.WriteLine("Matrice extraite du fichier:");
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            Console.WriteLine($"{matrix[i, 0]} {matrix[i, 1]}");
        }
    }
}

        }
    }
}
