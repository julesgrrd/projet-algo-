namespace ProjetPSI
{
    using OfficeOpenXml;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Xamarin.Forms;
    using Xamarin.Forms.Shapes;
    using OfficeOpenXml;
    using System.Runtime.CompilerServices;

    public class Program
    {
        static void Main(string[] args)
        {
            string fichier = "..\\net7.0\\MetroParis.xlsx";                /// On accède au fichier excel MetroParis contenant tous les liens du graphe
            int[,] matrice_relation = null;
            string[] tableau_nomStation = null;

            if (!File.Exists(fichier))
            {
                Console.WriteLine("Le fichier n'existe pas.");             /// Si il n'existe pas, l'utilisateur est informé
                return;
            }
            else
            {
                FileInfo fichierInfo = new FileInfo(fichier);
                ExcelPackage.License.SetNonCommercialPersonal("ProjetPSI");
                using (var package = new ExcelPackage(fichierInfo))
                {
                    ExcelWorksheet worksheet1 = package.Workbook.Worksheets[1];
                    ExcelWorksheet worksheet2 = package.Workbook.Worksheets[0];

                    matrice_relation = new int[worksheet1.Dimension.End.Row - 2, 4];
                    tableau_nomStation = new string[worksheet2.Dimension.End.Row - 1];

                    for (int ligne = 2; ligne < worksheet1.Dimension.End.Row; ligne++)
                    {
                        int idStation = Convert.ToInt32(worksheet1.Cells[ligne, 1].Value);
                        int idVoisin = Convert.ToInt32(worksheet1.Cells[ligne, 3].Value);
                        int doubleSens = Convert.ToInt32(worksheet1.Cells[ligne, 4].Value);
                        int poids = Convert.ToInt32(worksheet1.Cells[ligne, 5].Value);

                        if (idStation != null && idVoisin!= null)
                        {
                            matrice_relation[ligne - 2, 0] = idStation;
                            matrice_relation[ligne - 2, 1] = idVoisin;
                            matrice_relation[ligne - 2, 2] = doubleSens;
                            matrice_relation[ligne - 2, 3] = poids;
                        }
                    }

                    for (int ligne=2; ligne<=329; ligne++)
                    {
                        string nomStation = Convert.ToString(worksheet2.Cells[ligne, 3].Value);
                        tableau_nomStation[ligne-2] = nomStation;
                    }
                }
            }       


            static void SommaireMetro (string[] tableau_nomStation )
            {
                Console.WriteLine("Bienvenue sur le SOMMAIRE des stations de métro. Chaque station de métro est associé à son identifiant.");
                for (int i = 1; i <= tableau_nomStation.Length; i++)
                {
                    Console.WriteLine(i + " : " + tableau_nomStation[i - 1]);
                }
            }

            Graphe<int> GrapheMetro = new Graphe<int>(matrice_relation);

            int ordre = GrapheMetro.OrdreDuGraphe(matrice_relation);
            int taille = GrapheMetro.TailleDuGraphe(matrice_relation);
            List<int>[] ListeAdjacence = GrapheMetro.GenererListeAdjacence(matrice_relation, ordre);
            bool Oriente = GrapheMetro.GrapheOriente(ListeAdjacence);

            SommaireMetro(tableau_nomStation);

            // Visuel VisuelGraphe = new Visuel(GrapheMetro.GenererListeAdjacence(matrice_relation, ordre));

            // VisuelGraphe.DessinerGraphe();
        }
    }

}
