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
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    matrice_relation = new int[worksheet.Dimension.End.Row - 2, 3];

                    for (int ligne = 2; ligne < worksheet.Dimension.End.Row; ligne++)
                    {
                        int idStation = Convert.ToInt32(worksheet.Cells[ligne, 1].Value);
                        int idVoisin = Convert.ToInt32(worksheet.Cells[ligne, 3].Value);
                        int doubleSens = Convert.ToInt32(worksheet.Cells[ligne, 4].Value);
                        int poids = Convert.ToInt32(worksheet.Cells[ligne, 5].Value);

                        if (idStation != null && idVoisin!= null)
                        {
                            matrice_relation[ligne - 2, 0] = idStation;
                            matrice_relation[ligne - 2, 1] = idVoisin;
                            matrice_relation[ligne - 2, 2] = doubleSens;
                            matrice_relation[ligne - 2, 3] = poids;
                        }
                    }
                }
            }

            for (int i = 0; i < matrice_relation.GetLength(0); i++)
            {
                for (int j = 0; j < matrice_relation.GetLength(1); j++)
                {
                    Console.Write(matrice_relation[i, j] + " ");
                }
                Console.WriteLine();
            }

            Graphe<int> GrapheMetro = new Graphe<int>(matrice_relation);

            int ordre = GrapheMetro.OrdreDuGraphe(matrice_relation);

            GrapheMetro.GenererListeAdjacence(matrice_relation, ordre);

            Visuel VisuelGraphe = new Visuel(GrapheMetro.GenererListeAdjacence(matrice_relation, ordre));

            VisuelGraphe.DessinerGraphe();
        }
    }

}
