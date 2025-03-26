using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjetPSI
{
    public class Graphe <T>
    {
        public T[,] matrice_relation { get; set; }

        public Graphe(T[,] matrice_relation)
        {
            this.matrice_relation = matrice_relation;
        }

        public int OrdreDuGraphe(T[,] matrice_relation)         /// L'ordre du graphe est le nombre de sommet qu'il contient
        {
            int ordre = 0;
            for (int i = 0; i < matrice_relation.GetLength(0); i++)
            {
                for (int j = 0; j < matrice_relation.GetLength(1); j++)
                {
                    if (Convert.ToInt32(matrice_relation[i, j]) > ordre)
                    {
                        ordre = Convert.ToInt32(matrice_relation[i, j]);               /// l'ordre sera donc le nombre le plus élevé de la matrice_relation, extraite du fichier
                    }
                }
            }
            Console.WriteLine("L'ordre du graphe est : " + ordre);
            return ordre;
        }

        public int TailleDuGraphe(T[,] matrice_relation)     /// La taille du graphe est le nombre d'arêtes qu'il contient soit le nombre de lignes de la matrice_relation
        {
            int taille = matrice_relation.GetLength(0);
            Console.WriteLine("La taille du graphe est : " + taille + ". Il y a donc " + taille + " arêtes.");
            return taille;

        }

        public bool GrapheOriente(List<int>[] ListeAdjacence)           ///On souhaite vérifier si le graphe est orienté ou non
        {
            int a;
            bool result = true;
            for (int i = 0; i < ListeAdjacence.Length; i++)
            {
                if (result == false)
                {
                    break;
                }
                for (int j = 0; j < ListeAdjacence[i].Count; j++)       /// On parcours la liste d'adjacence d'un sommet
                {
                    a = ListeAdjacence[i][j];                         /// On crée une variable contenant un sommet voisin
                    if (ListeAdjacence[a - 1].Contains(i + 1) == true)    /// On vérifie que la liste adjacence du sommet voisin contient le sommet inital
                    {
                        continue;                                     /// Si oui, on continue et on vérifie pour tous les sommets de toutes les listes
                    }
                    else
                    {
                        result = false;                               /// Sinon, on sort de la boucle, car le graphe n'est pas orienté
                        break;
                    }
                }
            }
            if (result == true)
            {
                Console.WriteLine("Le graphe est non-orienté : les relations sont réciproques");
            }
            else
            {
                Console.WriteLine("Le graphe est orienté : les relations ne sont pas réciproques");
            }
            return result;
        }






        public List<int>[] GenererListeAdjacence(T[,] matrice_relation, int ordre)
        {
            List<int>[] ListeAdjacence = new List<int>[ordre];       /// On crée un tableau de liste ou le nombre d'éléments du tableau sera l'ordre du graphe et à chaque éléments on associe une liste (liste de voisins)

            for (int i = 0; i < ordre; i++)
            {
                ListeAdjacence[i] = new List<int>();                 /// On crée une liste pour chaque élément du tableau
            }

            for (int i = 0; i < matrice_relation.GetLength(0); i++)
            {

                ListeAdjacence[Convert.ToInt32(matrice_relation[i, 0]) - 1].Add(Convert.ToInt32(matrice_relation[i, 1]));       /// On ajoute à la Liste d'adjacence du nombre de la colonne de gauche celui de la colonne de droite de la matrice_relation
                ListeAdjacence[Convert.ToInt32(matrice_relation[i, 1]) - 1].Add(Convert.ToInt32(matrice_relation[i, 0]));       /// On fait également l'inverse car le graphe est non-orienté donc les relations sont réciproques
            }

            Console.WriteLine("Liste d'adjacence :");               /// On l'affiche
            for (int i = 0; i < ListeAdjacence.Length; i++)
            {
                Console.Write(i + 1 + ": ");
                for (int j = 0; j < ListeAdjacence[i].Count; j++)
                {

                    Console.Write(ListeAdjacence[i][j] + ",");

                }
                Console.WriteLine();
            }


            return ListeAdjacence;

        }


        public int[,] GenererMatriceAdjacence(T[,] matrice_relation, int ordre)
        {
            int[,] MatriceAdjacence = new int[ordre, ordre];                /// On crée une matrice d'adjacence de même largeur et hauteur correspondant à l'ordre du graphe
            for (int i = 0; i < matrice_relation.GetLength(0); i++)
            {
                MatriceAdjacence[Convert.ToInt32(matrice_relation[i, 0]) - 1, Convert.ToInt32(matrice_relation[i, 1]) - 1] = 1;     /// on rempli la matrice grâce à la matrice relation : on met des 1 lorsque les sommets i et j sont voisins.
                MatriceAdjacence[Convert.ToInt32(matrice_relation[i, 1]) - 1, Convert.ToInt32(matrice_relation[i, 0]) - 1] = 1;
            }

            Console.WriteLine("Matrice d'adjacence :");    /// On l'affiche
            for (int i = 0; i < ordre; i++)
            {
                for (int j = 0; j < ordre; j++)
                {
                    Console.Write(MatriceAdjacence[i, j] + " ");
                }
                Console.WriteLine();
            }


            return MatriceAdjacence;
        }

        public List<int> ParcoursEnProfondeurDabord(Noeud<T>[] noeuds, int ordre)
        {
            List<int> SommetsVisites = new List<int>();      /// On crée la liste des sommets visités qui sera retournée à la fin du programme

            int SommetDepart = Convert.ToInt32(Console.ReadLine());           /// L'utilisateur saisi le sommet de départ
            while (SommetDepart > ordre || SommetDepart <= 0)
            {
                Console.WriteLine("Le sommet n'est pas valide : il faut saisir un sommet existant");
                SommetDepart = Convert.ToInt32(Console.ReadLine());           /// l'utilisateur saisi le sommet de départ jusqu'à ce qu'il soit valide
            }


            Stack<int> pile = new Stack<int>();           /// On créé la pile qui va stocker les sommets visités et on y ajoute le sommet de départ.
            pile.Push(SommetDepart);


            noeuds[SommetDepart - 1].couleur = "jaune";       /// Le sommet de départ passe en jaune
            int Sommet = SommetDepart;

            int AvantDernier = SommetDepart;                  /// On crée une variable qui stocke l'avant dernier sommet parcouru : cela sera utile pour étudier l'existence de cycles
            bool cycle = false;
            int[] tabcycle = new int[noeuds.Length];       /// le tableau de cycle crée permettra de renvoyer un exemple de cycle
            int nbelements = 0;
            int n = 0;

            while (pile.Count != 0)                         /// l'algorithme s'arrêtera quand la pile sera vide
            {
                SommetsVisites.Add(Sommet);
                for (int i = 0; i < noeuds[Sommet - 1].listevoisins.Count; i++)           /// On parcours la liste d'adjacence du Sommet actuel (donc la liste de voisins)
                {
                    if (noeuds[noeuds[Sommet - 1].listevoisins[i] - 1].couleur == "blanc")           ///Si le voisin regardé est blanc alors :
                    {
                        AvantDernier = pile.Peek();
                        pile.Push(noeuds[Sommet - 1].listevoisins[i]);                               /// on l'ajoute à la pile
                        noeuds[noeuds[Sommet - 1].listevoisins[i] - 1].couleur = "jaune";            /// il devient jaune
                        Sommet = noeuds[Sommet - 1].listevoisins[i];                                 /// il devient le nouveau Sommet actuel

                        break;                                                                       /// on sort de la boucle pour étudier ses voisins à lui et ainsi de suite
                    }
                    else if (noeuds[noeuds[Sommet - 1].listevoisins[i] - 1].couleur == "jaune" || noeuds[noeuds[Sommet - 1].listevoisins[i] - 1].couleur == "rouge")
                    {
                        /// Le "if" suivant correspond à l'étude de l'existence de cycles
                        if (noeuds[noeuds[Sommet - 1].listevoisins[i] - 1].couleur == "jaune" && noeuds[Sommet - 1].listevoisins[i] != AvantDernier && cycle == false && Sommet != AvantDernier)
                        {

                            int[] tabpile = pile.ToArray();          /// On converti la pile en tableau pour pouvoir isoler le cycle
                            Array.Reverse(tabpile);                  /// Nous inversons ce tableau afin de ne pas avoir à décrémenter dans les boucles à suivre, mais plutôt incrémenter.

                            for (int j = 0; j < tabpile.Length; j++)               /// On parcours le tableau correspondant à la pile
                            {
                                if (tabpile[j] == noeuds[Sommet - 1].listevoisins[i] && tabpile.Length - j > 2)      /// On chercher l'indice j du sommet à partir duquel le cycle commence (soit le sommet correspondant au voisin étudié). De plus, un cycle doit contenir stricement plus de deux sommets.  
                                {
                                    nbelements = tabpile.Length - j;               /// le nombre d'éléments du cycle trouvé sera donc trouvé en calculant : taille de la pile - les chiffres précédent le sommet de début du cycle (ceux qui ne sont pas dans le cycle et qui sont au nombre de j)

                                    cycle = true;
                                    n = j;
                                    for (int k = 0; k < tabpile.Length - j; k++)         /// On rempli le tableau autant de fois qu'il y a d'éléments dans le cycle (la fin est à tabpile.Length-j car la pile à été inversé donc les éléments non-souhaités sont à la fin du tableau)
                                    {
                                        tabcycle[k] = tabpile[n];      /// Le tableau du cycle est rempli avec les bons éléments du tableau de la pile.
                                        n += 1;
                                    }
                                    break;          /// Lorsque le tableau est rempli et que nous avons un cycle comme exemple, nous sortons de la boucle et continuons l'algorithme.

                                }
                            }
                        }

                        if (i < (noeuds[Sommet - 1].listevoisins.Count - 1))
                        {
                            continue;                                   /// Si le sommet voisin est jaune ou rouge mais qu'il existe un ou plusieurs autres sommets voisins alors on continue;
                        }
                        else
                        {
                            noeuds[Sommet - 1].couleur = "rouge";        /// Si tous les sommets voisins du Sommet sont jaunes ou rouges alors le sommet actuel passe en rouge car il a fini d'être étudié.
                            pile.Pop();                                  /// Il est donc retiré de la pile (il sera à ce moment la en haut de la pile donc pile.Pop qui retire le premier élément de la pile permet d'effectuer cette action).
                            if (pile.Count != 0)
                            {
                                Sommet = pile.Peek();                    ///Si la pile n'est pas vide à la fin de la boucle, alors le premier élément de la pile devient le nouveau sommet.
                            }
                            break;
                        }
                    }
                }
            }
            Console.Write(SommetsVisites[0]);
            for (int i = 1; i < SommetsVisites.Count; i++)
            {
                Console.Write(" ; " + SommetsVisites[i]);               /// On affiche la liste des sommets visités, soit le chemin complet de parcours du graphe.
            }
            Console.WriteLine();


            if (cycle == true)
            {
                Console.Write("Il existe un cycle, par exemple : ");
                for (int i = 0; i < nbelements; i++)
                {
                    Console.Write(tabcycle[i] + " ");         /// Si il y a un cycle, on affiche le tableau contenant l'exemple de cycle
                }
            }
            else { Console.WriteLine("Il n'existe pas de cycle"); }


            for (int i = 0; i < noeuds.Length; i++)      /// à la fin du programme, on remet la couleur de chaque noeud à blanc pour pouvoir réaliser le reste des fonctions.
            {
                noeuds[i].couleur = "blanc";
            }

            return SommetsVisites;
        }


        public bool GrapheConnexe(List<int> SommetsVisites, int ordre)           /// un graphe est connexe si à la fin du parcours en profondeur à partir d'un sommet arbitraire, tous les sommets ont été visités
        {
            bool connexite = false;
            for (int i = 1; i <= ordre; i++)
            {
                for (int j = 0; j < SommetsVisites.Count; j++)            /// On parcours la liste des sommets visités
                {
                    connexite = false;
                    if (SommetsVisites[j] == i)                          /// Si pour chaque i allant de 1 à l'ordre du graphe on trouve ce nombre dans la liste des sommets visités, alors connexité passe à true et on sors de la deuxième boucle pour étudier la présence du i suivant.
                    {
                        connexite = true;
                        break;
                    }

                }
                if (connexite == false)
                {
                    break;                                     /// Si la connexité reste à false, alors au moins 1 sommet n'a pas été visité donc on sort de la première boucle et l'algorithme se termine : le graphe n'est pas connexe.
                }
            }
            if (connexite == true)
            { Console.WriteLine("Le graphe est connexe car tous les sommets ont été visités."); }
            else { Console.WriteLine("Le graphe n'est pas connexe car tous les sommets n'ont pas été visités."); }

            return connexite;
        }



        public List<int> ParcoursEnLargeurDabord(Noeud<T>[] noeuds, int ordre)
        {
            List<int> SommetsVisites2 = new List<int>();                            /// On initialise la liste de Sommetvisités du parcours en largeur
            int SommetDepart = Convert.ToInt32(Console.ReadLine());
            while (SommetDepart > matrice_relation.Length || SommetDepart <= 0)
            {
                Console.WriteLine("Le sommet n'est pas valide : il faut saisir un sommet existant");          /// Comme précédemment, l'utilisateur saisi le sommet de départ jusqu'à ce qu'il soit valide
                SommetDepart = Convert.ToInt32(Console.ReadLine());
            }

            Queue<int> file = new Queue<int>();                /// Cette fois on crée une queue qui va stocker les sommet visités et on y ajoute le sommet de départ.
            file.Enqueue(SommetDepart);

            noeuds[SommetDepart - 1].couleur = "jaune";                 /// Le sommet de départ devient jaune.
            int Sommet = SommetDepart;

            while (file.Count != 0)                       /// L'algorithme se terminera quand le queue sera vide.
            {
                Sommet = file.Peek();
                SommetsVisites2.Add(Sommet);
                file.Dequeue();                                   /// On retire le premier sommet de la queue puis on met tous ses voisins blanc en jaune et on les ajoute dans la queue.
                for (int i = 0; i < noeuds[Sommet - 1].listevoisins.Count; i++)
                {
                    if (noeuds[noeuds[Sommet - 1].listevoisins[i] - 1].couleur == "blanc")
                    {
                        noeuds[noeuds[Sommet - 1].listevoisins[i] - 1].couleur = "jaune";
                        file.Enqueue(noeuds[Sommet - 1].listevoisins[i]);
                    }
                }

                noeuds[Sommet - 1].couleur = "rouge";         /// Le noeud dont tous les voisins sont passés jaune a été traité complètement : il passe en rouge.

            }

            for (int i = 0; i < SommetsVisites2.Count; i++)
            {
                Console.Write(SommetsVisites2[i] + " ; ");                /// On affiche la liste des Sommets Visités par le parcours en largeur
            }

            for (int i = 0; i < noeuds.Length; i++)      /// à la fin du programme, on remet la couleur de chaque noeud à blanc pour pouvoir réaliser le reste des fonctions.
            {
                noeuds[i].couleur = "blanc";
            }

            return SommetsVisites2;
        }
    }
}
