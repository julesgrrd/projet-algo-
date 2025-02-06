using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_info
{
    internal class Graphe
    {
        public string[] tab;
        public int[,] matrice_relation;
        public int[,] matrice_adjacence; 

        public Graphe( int[,] matrice_relation, int[,] matrice_adjacence, string[] fichier) 
        {
            string fichier = "C:\\Users\\Jules\\OneDrive - De Vinci\\Documents\\Année 2\\Semestre 2\\info\\Projet info\\Association-soc-karate\\soc-karate.mtx";
            this.string[] tab = File.ReadAllLines(fichier);
            this.matrice_relation = Generematricerlation(tab);
            this.matrice_adjacence = matrice_adjacence; 
        }


        public int[,] Generematricerelation(string[] tab)
        {
                string[] infoligne = tab.Skip(24).ToArray();
                int[,] matrice_relation = new int[infoligne.Length, 2];
                for (int i = 0; i < infoligne.Length; i++)
                {
                    string[] numbers = infoligne[i].Split(' ');
                    matrice_relation[i, 0] = Convert.ToInt32(numbers[0]);
                    matrice_relation[i, 1] = Convert.ToInt32(numbers[1]);
                }
            return matrice_relation;
        }

        public int[,] generematriceadjacente()
     }
}
