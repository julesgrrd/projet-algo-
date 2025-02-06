namespace Projet_info
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    internal class Program
    {
        static void Main(string[] args)
        {
           
                string fichier = "C:\\Users\\Jules\\OneDrive - De Vinci\\Documents\\Année 2\\Semestre 2\\info\\Projet info\\Association-soc-karate\\soc-karate.mtx";
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
                    Console.WriteLine($"{matrice_relation[i, 0]} {matrice_relation[i, 1]}");
                }
            }
        }
    }
