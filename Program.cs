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
    using Projet_PSI;
    using SkiaSharp;

    public class Program
    {
        static void Main(string[] args)
        {
            string fichier = "..\\net7.0\\MetroParis.xlsx";                /// On accède au fichier excel MetroParis contenant tous les liens du graphe
            int[,] matrice_relation = null;
            string[,] matrice_infoStation = null;

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
                    matrice_infoStation = new string[worksheet2.Dimension.End.Row - 1, 4];

                    for (int ligne = 2; ligne < worksheet1.Dimension.End.Row; ligne++)
                    {
                        int idStation1 = Convert.ToInt32(worksheet1.Cells[ligne, 1].Value);
                        int idVoisin = Convert.ToInt32(worksheet1.Cells[ligne, 3].Value);
                        int doubleSens = Convert.ToInt32(worksheet1.Cells[ligne, 4].Value);
                        int poids = Convert.ToInt32(worksheet1.Cells[ligne, 5].Value);

                        if (idStation1 != null && idVoisin != null)
                        {
                            matrice_relation[ligne - 2, 0] = idStation1;
                            matrice_relation[ligne - 2, 1] = idVoisin;
                            matrice_relation[ligne - 2, 2] = doubleSens;
                            matrice_relation[ligne - 2, 3] = poids;
                        }
                    }

                    for (int ligne = 2; ligne <= 329; ligne++)
                    {
                        string idStation2 = Convert.ToString(worksheet2.Cells[ligne, 1].Value);
                        string nomStation = Convert.ToString(worksheet2.Cells[ligne, 3].Value);
                        string longitudeStation = Convert.ToString(worksheet2.Cells[ligne, 4].Value);
                        string latitudeStation = Convert.ToString(worksheet2.Cells[ligne, 5].Value);
                        matrice_infoStation[ligne - 2, 0] = idStation2;
                        matrice_infoStation[ligne - 2, 1] = nomStation;
                        matrice_infoStation[ligne - 2, 2] = longitudeStation;
                        matrice_infoStation[ligne - 2, 3] = latitudeStation;
                    }
                }
            }


            Graphe<int> GrapheMetro = new Graphe<int>(matrice_relation, matrice_infoStation);

            int ordre = GrapheMetro.OrdreDuGraphe(matrice_relation);
            int taille = GrapheMetro.TailleDuGraphe(matrice_relation);
            List<int>[] ListeAdjacence = GrapheMetro.GenererListeAdjacence(matrice_relation, ordre);
            bool Oriente = GrapheMetro.GrapheOriente(ListeAdjacence);
            int[,] MatriceAdjacence = GrapheMetro.GenererMatriceAdjacence(matrice_relation, ordre);

            GrapheMetro.SommaireMetro(matrice_infoStation);

            Noeud<int>[] noeuds = new Noeud<int>[ordre];          /// On initialise un tableau noeuds de Noeud.


            for(int i = 0; i < ordre; i++)
            {
                double longitude = 0;
                double latitude = 0;
                string nom = "";

                nom = matrice_infoStation[i, 1];
                longitude = double.Parse(matrice_infoStation[i, 2]);
                latitude = double.Parse(matrice_infoStation[i, 3]);

                noeuds[i] = new Noeud<int>(i + 1, nom, longitude, latitude, ListeAdjacence[i]);
            }

            Visuel visuel = new Visuel(noeuds, matrice_relation);
            visuel.GenererCarte("metro.png");





            ///GrapheMetro.AlgorithmeFloydWarshall(MatriceAdjacence, ordre, noeuds);
            ///GrapheMetro.AlgorithmeDijkstra(noeuds, MatriceAdjacence, ordre);
        }
    }

}
