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

    public class Program
    {
        static void Main(string[] args)
        {
            string fichier = "..\\net7.0\\MetroParis.xlsx";                /// On accède au fichier excel MetroParis contenant tous les liens du graphe
            int[,] matrice_relation = null;
            string[,] matrice_nomStation = null;

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
                    matrice_nomStation = new string[worksheet2.Dimension.End.Row - 1, 2];

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
                        matrice_nomStation[ligne - 2, 0] = idStation2;
                        matrice_nomStation[ligne - 2,1] = nomStation;
                    }
                }
            }


            static void SommaireMetro(string[,] matrice_nomStation)
            {
                Console.WriteLine("\n******Bienvenue sur le SOMMAIRE des stations de métro.*****\n**Chaque station de métro est associée à son identifiant.**");
                for (int i = 0; i <matrice_nomStation.GetLength(0); i++)
                {
                    for (int j=0; j<matrice_nomStation.GetLength(1);j++)
                    {
                        if (Convert.ToInt32(matrice_nomStation[i,0])<10)
                        {
                            Console.Write(matrice_nomStation[i, j] + "   ");
                        } else if (Convert.ToInt32(matrice_nomStation[i, 0]) < 100)
                        {
                            Console.Write(matrice_nomStation[i, j] + "  ");
                        } else
                        {
                            Console.Write(matrice_nomStation[i, j] + " ");
                        }


                    }
                    Console.WriteLine();
                }
            }

            Graphe<int> GrapheMetro = new Graphe<int>(matrice_relation);

            for (int i = 0; i < matrice_relation.GetLength(0); i++)
            {
                for (int j = 0; j < matrice_relation.GetLength(1); j++)
                {
                    Console.Write(matrice_relation[i, j] + " ");
                }
                Console.WriteLine();
            }

            int ordre = GrapheMetro.OrdreDuGraphe(matrice_relation);
            int taille = GrapheMetro.TailleDuGraphe(matrice_relation);
            List<int>[] ListeAdjacence = GrapheMetro.GenererListeAdjacence(matrice_relation, ordre);
            bool Oriente = GrapheMetro.GrapheOriente(ListeAdjacence);
            int[,] MatriceAdjacence = GrapheMetro.GenererMatriceAdjacence(matrice_relation, 5);

            SommaireMetro(matrice_nomStation);

            //Visuel VisuelGraphe = new Visuel(GrapheMetro.GenererListeAdjacence(matrice_relation, ordre));

            //VisuelGraphe.DessinerGraphe();



            Noeud<int>[] noeuds = new Noeud<int>[ordre];          /// On initialise un tableau noeuds de Noeud.
            for (int i = 0; i < ordre; i++)
            {
                noeuds[i] = new Noeud<int> (i+1, matrice_nomStation[i,1], "blanc", ListeAdjacence[i]);            /// Tous les noeuds de graphe prennent en paramètre un numéro i, la couleur blanche et une liste de voisins correspondant à la liste d'adjacence du sommet étudié.
            }


            Lien<int>[,] liens = new Lien<int>[ordre, ordre];                     /// On initialise une matrice de lien qui contient autant de lien qu'il y en a dans la matrice relation et qui relie un sommet de départ a à un sommet d'arrivée b compris dans l'ordre du graphe
            for (int a = 0; a < ordre; a++)
            {
                for (int b = 0; b < ordre; b++)
                {
                    if (ListeAdjacence[a].Contains(b + 1))                    /// Pour tout a et b, si le sommet b est un voisin de a, alors le lien existe
                    {
                        for (int h = 0; h < matrice_relation.GetLength(0); h++)
                        {
                            if (matrice_relation[h, 0] == a + 1 && matrice_relation[h, 1] == b + 1)          /// on parcours la matrice relation à la recherche de la ligne correspondant au lien a-b avec donc le terme de la première colonne égale à a et celui de la deuxième égale à b
                            {
                                int aa = h;                                                         /// lorsqu'on trouve cette ligne, on note son indice
                                liens[a, b] = new Lien<int>(noeuds[matrice_relation[aa, 0] - 1], noeuds[matrice_relation[aa, 1] - 1], matrice_relation[aa, 3]);              /// on peut maintenant associée à un lien un NOEUD de départ, un NOEUD d'arrivé, et le poids correspondant à ce lien.
                                break;
                            }
                        }

                    }

                }
            }
        }
    }

}
