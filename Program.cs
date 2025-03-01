namespace Problème_scientifique_informatique
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using Xamarin.Forms;
    using Xamarin.Forms.Shapes;

    public class Program
    {
        static void Main(string[] args)
        {

            string fichier = "..\\net8.0\\soc-karate.mtx";                /// On accède au fichier soc-karate contenant tous les liens du graphe

            if (!File.Exists(fichier))
            {
                Console.WriteLine("Le fichier n'existe pas.");             /// Si il n'existe pas, l'utilisateur est informé
                return;
            }


            /// Création de la matrice relation = matrice extraire du fichier
            
            string[] tab = File.ReadAllLines(fichier);                 /// On lit le fichier
            string[] infoligne = tab.Skip(24).ToArray();               /// On isole la partie du fichier dont nous avons besoin, soit à partir de la ligne 25
            int [,] matrice_relation = new int[infoligne.Length, 2];           /// On crée donc une matrice à deux colonnes (noeud source, noeud destination)
                for (int i = 0; i < infoligne.Length; i++)
                { 
                    string[] numbers = infoligne[i].Split(' ');                   /// on sépart l'élément de chaque ligne du tableau pour isoler le sommet source et le sommet destination.
                    matrice_relation[i, 0] = Convert.ToInt32(numbers[0]);       /// On remplit la matrice pour pouvoir accéder aux deux colonnes.
                    matrice_relation[i, 1] = Convert.ToInt32(numbers[1]);
                }

            Console.WriteLine("Matrice extraite du fichier:");              /// On l'affiche
            for (int i = 0; i < matrice_relation.GetLength(0); i++)
            {
                for (int j = 0; j < matrice_relation.GetLength(1); j++)
                {
                    Console.Write(matrice_relation[i, j] + " ");
                }
                Console.WriteLine();
            }


            /// Appel des différentes fonctions du programme
            
            Graphe graphe = new Graphe(matrice_relation);               /// On crée un graphe de la classe Graphe
            

            Console.WriteLine();
            int ordre;
            ordre = graphe.OrdreDuGraphe(matrice_relation);                     /// on appelle les fonctions ordre et taille du graphe
            Console.WriteLine() ;
            graphe.TailleDuGraphe(matrice_relation);
            Console.WriteLine() ;
            
             
            List<int>[] ListeAdjacence = new List<int>[ordre];                      /// On crée la liste d'adjacence du graphe
            ListeAdjacence = graphe.GenererListeAdjacence(matrice_relation, ordre);     /// Elle prend la valeur que retourne la fonction GenererListeAdjacence pour que on puisse l'utiliser dans d'autres fonctions.
            Console.WriteLine();
                                          

            graphe.GenererMatriceAdjacence(matrice_relation, ordre);         /// On appelle les fonctions qui génèrent la matrice d'adjacence et disent si le graphe est orienté.
            Console.WriteLine();

            graphe.GrapheOriente(ListeAdjacence);
            Console.WriteLine();


            Noeud[] noeuds = new Noeud[ordre];          /// On initialise un tableau noeuds de Noeud.
            for (int i=0; i<ordre; i++)
            {
                noeuds[i] = new Noeud(i, "blanc", ListeAdjacence[i]);            /// Tous les noeuds de graphe prennent en paramètre un numéro i, la couleur blanche et une liste de voisins correspondant à la liste d'adjacence du sommet étudié.
            }
            

            List<int> SommetsVisites = new List<int>();               
            Console.WriteLine("Saisir le sommet à partir duquel vous souhaitez effectuer le parcours en profondeur : ");  
            SommetsVisites= graphe.ParcoursEnProfondeurDabord(noeuds);                       /// Le parcours en profondeur est effectué et la liste de Sommet visités initialisé prend la valeur retournée par la fonction
            

            Console.WriteLine();
            graphe.GrapheConnexe(SommetsVisites, ordre);                 /// On lance la fonction permettant de savoir si le graphe est connexe ou non

            Console.WriteLine();
            Console.WriteLine("Saisir le sommet à partir duquel vous souhaitez effectuer le parcours en largeur : ");
            graphe.ParcoursEnLargeurDabord(noeuds);                    /// On effectue le parcours en largeur

            Visuel visualisation = new Visuel(ListeAdjacence);         /// On crée une nouvelle visualisation du graphe
            visualisation.DessinerGraphe();                             /// On dessine le graphe


            Console.ReadKey();
        }
    }
         
}
