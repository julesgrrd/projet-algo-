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
        string fichier = "C:\\Users\\alexi\\OneDrive - De Vinci\\ESILV\\A2\\Problème scientifique informatique\\Compléments utiles pour le livrable 1  1er Mars-20250129\\soc-karate.mtx";
        string[] tab = File.ReadAllLines(fichier);
        string[] infoligne = tab.Skip(24).ToArray();
        int[,] matrice_relation = new int[infoligne.Length, 2];
        for (int i = 0; i < infoligne.Length; i++)
        {
            string[] numbers = infoligne[i].Split(' ');
            matrice_relation[i, 0] = Convert.ToInt32(numbers[0]);  
            matrice_relation[i, 1] = Convert.ToInt32(numbers[1]);  
        }


        
    }
}

        }
    }
}
