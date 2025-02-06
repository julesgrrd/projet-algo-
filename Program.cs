namespace Rendu_1_bis
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    internal class Program
    {
        static void Main(string[] args)
        {
            string fichier = "..\\net7.0\\soc-karate.mtx";

            if (!File.Exists(fichier))
            {
                Console.WriteLine("Le fichier n'existe pas.");
                return;
            }

            string[] tab = File.ReadAllLines(fichier);

            string[] infoligne = tab.Skip(24).ToArray();
            int[,] matrice_relation = new int[infoligne.Length, 2];
            
            for (int i = 0; i < infoligne.Length; i++)
            {
                string[] numbers = infoligne[i].Split(' ');
                matrice_relation[i, 0] = Convert.ToInt32(numbers[0]);
                matrice_relation[i, 1] = Convert.ToInt32(numbers[1]);
            }
            
            Console.WriteLine("Matrice extraite du fichier:");
            for (int i = 0; i < matrice_relation.GetLength(0); i++)
            {
                for(int j=0; j<matrice_relation.GetLength(1); j++)
                {
                    Console.Write(matrice_relation[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.ReadKey();
        }
    }
}
