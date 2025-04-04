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
        public string[,] matrice_nomStation { get; set; }

        public Graphe(T[,] matrice_relation, string[,] matrice_nomStation)
        {
            this.matrice_relation = matrice_relation;
            this.matrice_nomStation = matrice_nomStation;
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

    int nbLigne = matrice_relation.GetLength(0);

    for (int i = 0; i < nbLigne; i++)
    {
        int idStation1 = Convert.ToInt32(matrice_relation[i, 0]);                 
        int idStation2 = Convert.ToInt32(matrice_relation[i, 1]);
        int doubleSens = Convert.ToInt32(matrice_relation[i, 2]);

        if (idStation1==0 || idStation2 ==0 || idStation1>ordre || idStation2 >ordre)
        {
            continue;
        }

        ListeAdjacence[idStation1 - 1].Add(idStation2);            /// Pour tous les liens de la matrice relation, on ajoute le voisin du sommet dans la liste d'adjacence du sommet

        if (doubleSens==1)                                         /// Si "doubleSens==1", la relation est réciproque donc on ajoute également le somet dans la liste d'adjacence du voisin.
        {
            ListeAdjacence[idStation2 - 1].Add(idStation1);
        }
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
    int infini = 9999999;

    for (int i=0; i<ordre; i++)
    {
        for (int j=0; j<ordre; j++)
        {
            if (i == j)
            {
                MatriceAdjacence[i, j] = 0;                    /// Tous les éléments sur la diagonale de la matrice sont mis à zéro
            } else
            {
                MatriceAdjacence[i, j] = infini;               /// Les autres sont mis à infini en attendant d'être modifié par la suite si il existe un lien
            }
        }
    }


    for (int i = 0; i < matrice_relation.GetLength(0); i++)
    {
        int idStation1 = Convert.ToInt32(matrice_relation[i, 0]);
        int idStation2 = Convert.ToInt32(matrice_relation[i, 1]);
        int doubleSens = Convert.ToInt32(matrice_relation[i, 2]);
        int poids = Convert.ToInt32(matrice_relation[i, 3]);

        if (idStation1 == 0 || idStation2 == 0 || idStation1 > ordre || idStation2 > ordre)
        {
            continue;
        }

        if (doubleSens==1)     
        {
            MatriceAdjacence[Convert.ToInt32(matrice_relation[i, 0]) - 1, Convert.ToInt32(matrice_relation[i, 1]) - 1] = poids;     /// on rempli la matrice grâce à la matrice relation : matriceAdjacence[i,j] correspond au poids du lien entre i et j lorsque les sommets i et j sont voisins.
            MatriceAdjacence[Convert.ToInt32(matrice_relation[i, 1]) - 1, Convert.ToInt32(matrice_relation[i, 0]) - 1] = poids;     /// Les relations sont réciproques donc on fait l'opération dans les deux sens
        } else
        {
            MatriceAdjacence[Convert.ToInt32(matrice_relation[i, 0]) - 1, Convert.ToInt32(matrice_relation[i, 1]) - 1] = poids;     ///Ici la relation n'est pas réciproque donc on complète dans un sens seulement
        }
       
    }

    return MatriceAdjacence;
}


public void SommaireMetro(string[,] matrice_nomStation)
{

    Console.WriteLine("\n******Bienvenue sur le SOMMAIRE des stations de métro.*****\n**Chaque station de métro est associée à son identifiant.**");
    for (int i = 0; i < matrice_nomStation.GetLength(0); i++)
    {
        for (int j = 0; j < matrice_nomStation.GetLength(1); j++)
        {
            if (Convert.ToInt32(matrice_nomStation[i, 0]) < 10)
            {
                Console.Write(matrice_nomStation[i, j] + "   ");
            }
            else if (Convert.ToInt32(matrice_nomStation[i, 0]) < 100)
            {
                Console.Write(matrice_nomStation[i, j] + "  ");
            }
            else
            {
                Console.Write(matrice_nomStation[i, j] + " ");
            }


        }
        Console.WriteLine();
    }
}

public List<int> AlgorithmeFloydWarshall(int[,] matriceAdjacence, int ordre, Noeud<T>[] noeuds)
{
    List<int> PCC = new List<int>(); /// L'algorithme retournera une liste de station parcourue représentant le chemin le plus court.
    int infini = 9999999;

    Console.WriteLine("\nNous allons utiliser l'algorithme de Floyd-Warshall pour déterminer le chemin le plus court entre deux sommets.");

    /// On demande à l'utilisateur s'il souhaite afficher de nouveau le sommaire, afin de trouver l'identifiant des stations
    string reponse = "";
    Console.WriteLine("\nSouhaitez-vous afficher le sommaire des stations de métro ? ");
    do
    {
        Console.WriteLine("Entrez OUI ou NON : ");
        reponse = Console.ReadLine().ToLower(); /// Permet de fonctionner si l'utilisateur écrit en minus
    } while (reponse != "oui" && reponse != "non");

    if (reponse == "oui")
    {
        SommaireMetro(matrice_nomStation); /// Lance la fonction SommaireMetro qui permet d'afficher le sommaire
    }

    /// On demande à l'utilisateur de rentrer l'identifiant d'une station d'une station de départ et l'identifiant d'une station d'arrivée
    int depart;
    Console.WriteLine("\nSaisir le numéro de la station de départ en vous référant au sommaire : ");
    depart = Convert.ToInt32(Console.ReadLine());
    while (depart <= 0 || depart > ordre)
    {
        Console.WriteLine("Le numéro saisi n'est pas correcte. Saisir le numéro de la station de départ en vous référant au sommaire : ");
        depart = Convert.ToInt32(Console.ReadLine());
    }

    int arrivee;
    Console.WriteLine("\nSaisir le numéro de la station d'arrivée en vous référant au sommaire : ");
    arrivee = Convert.ToInt32(Console.ReadLine());
    while (arrivee <= 0 || arrivee > ordre)
    {
        Console.WriteLine("Le numéro saisi n'est pas correcte. Saisir le numéro de la station d'arrivee en vous référant au sommaire : ");
        arrivee = Convert.ToInt32(Console.ReadLine());
    }


    int[,] distance = new int[ordre, ordre];
    int[,] predecesseur = new int[ordre, ordre];

    
    for (int i=0; i<ordre; i++)
    {
        for (int j=0; j<ordre; j++)
        {
            distance[i, j] = matriceAdjacence[i, j];

            if (matriceAdjacence[i,j] == infini || i==j)
            {
                predecesseur[i, j] = -1;
            } else
            {
                predecesseur[i, j] = i;
            }
        }
    }

    for (int k=0; k<ordre; k++)
    {
        for (int i=0; i<ordre; i++)
        {
            for (int j=0; j<ordre; j++)
            {
                if (distance[i,k] != infini && distance[k,j] != infini && distance[i,k] + distance[k,j] < distance[i,j])
                {
                    distance[i, j] = distance[i, k] + distance[k, j];
                    predecesseur[i, j] = predecesseur[k, j];
                }
            }
        }
    }

    int stationActuelle = arrivee - 1;
    if (predecesseur[depart-1, arrivee-1] == -1)
    {
        Console.WriteLine("Aucun chemin n'existe pour relier ces deux stations");
    } else
    {
        while (stationActuelle !=-1)
        {
            PCC.Insert(0, stationActuelle + 1);
            stationActuelle = predecesseur[depart - 1, stationActuelle];
        }

        /// Calcul du poids total du PCC
        int poidsTotal = 0;
        for (int i=0; i<PCC.Count-1; i++)
        {
            int station1 = PCC[i] - 1;
            int station2 = PCC[i + 1] - 1;
        }
        /// On affiche le chemin du chemin le plus court, avec le temps total
        Console.WriteLine("\nLe chemin le plus court entre la station de métro numéro " + depart + " : " + noeuds[depart - 1].nom + " et la station numéro " + arrivee + " : " + noeuds[arrivee - 1].nom + " est : ");
        for (int i=0; i<PCC.Count; i++)
        {
            Console.Write(noeuds[PCC[i] - 1].nom);
            if (i<PCC.Count -1)
            {
                Console.Write(" --> ");
            }
        }
        Console.WriteLine();
    }

    return PCC;
}
    

         public List<int> AlgorithmeDijkstra(Noeud<T>[] noeuds, int[,] matriceAdjacence, int ordre)
{
    List<int> CheminLePlusCourt = new List<int>();                           /// L'algorithme retournera une liste de station parcourue représentant le chemin le plus court.
    Console.WriteLine("Nous allons utiliser l'algorithme de Dijkstra pour déterminer le chemin le plus court entre deux sommet.");


    /// On demande à l'utilisateur s'il souhaite afficher de nouveau le sommaire, afin de trouver l'identifiant des stations
    string reponse;
    Console.WriteLine("\nSouhaitez-vous afficher le sommaire des stations de métro ? ");
    do
    {
        Console.WriteLine("Entrez OUI ou NON : ");
        reponse = Console.ReadLine().ToLower(); /// Permet de fonctionner si l'utilisateur écrit en minus
    } while (reponse != "oui" && reponse != "non");

    if (reponse == "oui")
    {
        SommaireMetro(matrice_nomStation); /// Lance la fonction SommaireMetro qui permet d'afficher le sommaire
    }

    /// On demande à l'utilisateur une station de départ et une station d'arrivée.
    Console.WriteLine("Saisir le numéro de la station de départ en vous référant au sommaire :");
    int départ = Convert.ToInt32(Console.ReadLine());                        
    while (départ > ordre || départ <= 0)
    {
        Console.WriteLine("Le numéro de station n'est pas valide : il faut saisir un numéro existant");          /// L'utilisateur saisi la station de départ jusqu'à ce qu'elle soit valide
        départ = Convert.ToInt32(Console.ReadLine());
    }

    Console.WriteLine("Saisir le numéro de la station d'arrivée en vous référant au sommaire :");
    int arrivée = Convert.ToInt32(Console.ReadLine());
    while (arrivée > ordre || arrivée <= 0)
    {
        Console.WriteLine("Le numéro de station n'est pas valide : il faut saisir un numéro existant");          /// L'utilisateur saisi la station d'arrivée jusqu'à ce qu'elle soit valide
        arrivée = Convert.ToInt32(Console.ReadLine());
    }
    int infini = 999999999;
    int sommet = départ;

    int[] distance = new int[ordre];            /// On initialise un tableau des distances et un tableau contenant les prédecesseurs de chaque sommet
    int[] predecesseur = new int[ordre];
   

    for (int i=0; i<ordre; i++)                                   
    {
            distance[i] = infini;                       /// On initalise toutes les distances à plus l'infini 
            predecesseur[i] = -1;                       /// On initialise touts les prédecesseurs à -1 (nous avons choisi -1 pour représenté null) 
        
    }
    distance[départ - 1] = 0;                            /// La distance du sommet de départ est mise à 0

  

    while (noeuds[arrivée-1].couleur == "blanc")                 /// On éxécute l'algorithme jusqu'à ce que le sommet d'arrivée soit visité
    {
       
        int PlusCourteDistance = infini;   
        for (int i = 0; i < ordre; i++)
        {
            if (noeuds[i].couleur == "blanc" && distance[i] < PlusCourteDistance)
            {
                
                PlusCourteDistance = distance[i];                    /// On cherche le sommet non visité pour lequel la distance au sommet de départ est la plus courte
                sommet = i + 1;      
            }
        }
        
        noeuds[sommet - 1].couleur = "rouge";               /// Ce sommet est ensuite marqué comme visité
       

        for (int i = 0; i < noeuds[sommet-1].listevoisins.Count; i++)                      /// On éxécute ce qui suit pour chaque voisin du sommet actuel
        {
            int voisin = noeuds[sommet - 1].listevoisins[i] - 1;
            if (noeuds[voisin].couleur == "blanc" && matriceAdjacence[sommet-1, voisin]!= infini)       
            {
               
                if (Convert.ToInt32(distance[sommet - 1]) + matriceAdjacence[sommet - 1, voisin] < distance[voisin])     /// Si le sommet est non visité, qu'il existe un lien entre le sommet et le voisin et qu'il respecte la condition du if alors : ...
                {
                   
                    distance[voisin] = Convert.ToInt32(distance[sommet-1]) + matriceAdjacence[sommet - 1, voisin];        /// ... on met à jour la distance du voisin    
                    predecesseur[voisin] = sommet-1;                                                     /// Le prédécesseur du voisin devient le sommet actuel
                    
                }
            }
        }
    }


    /// Nous allons maintenant afficher le chemin le plus court en reconstruisant la liste des predecesseurs.

    int SommetArr = arrivée - 1;
    while (SommetArr != -1)                        /// On remonte le chemin parcouru à l'envers pour le reconstruire 
    {
        CheminLePlusCourt.Insert(0, SommetArr + 1);              /// On ajoute donc le sommet au début du chemin
        SommetArr = predecesseur[SommetArr];                     /// puis le prédecesseur du sommet deviens le nouveau sommet et ainsi de suite, jusqu'à remonter jusqu'au sommet de départ
    }

    /// On calcule le temps du trajet
    int tempsTotal = 0;                  
    for (int i = 0; i < CheminLePlusCourt.Count - 1; i++)         /// Le temps totale est initalisé à 0 puis pour chaque lien entre une StationInitale et une stationSuivante, on ajoute le poids de ce lien au temps total à l'aide de la matrice d'adjacence
    {
        int stationInitiale = CheminLePlusCourt[i] - 1;
        int stationSuivante = CheminLePlusCourt[i + 1] - 1;
        tempsTotal += matriceAdjacence[stationInitiale, stationSuivante];    /// A chaque itération, on chercher le temps entre les deux stations dans la matrice d'adjacence.
    }

     ///On affiche le chemin le plus court
    Console.WriteLine("Le chemin le plus court entre la station de métro " + noeuds[départ-1].nom + " (n°" + noeuds[départ-1].idNoeud + ") et la station de métro " + noeuds[arrivée-1].nom + " (n°" + noeuds[arrivée-1].idNoeud + ") est le trajet : ");
    for (int i = 0; i < CheminLePlusCourt.Count; i++)
    {
        if (i == CheminLePlusCourt.Count - 1)
        {
            Console.Write(noeuds[CheminLePlusCourt[i] - 1].nom);
        }
        else
        {
            Console.Write(noeuds[CheminLePlusCourt[i] - 1].nom + " --> ");          /// on affiche le chemin le plus court en associant chaque chiffre de la liste obtenue à son nom de station.
        }

    }
    Console.WriteLine();
    Console.WriteLine("Ce trajet durera : " + tempsTotal + " minutes.");         /// On affiche le temps total du trajet




    for (int i = 0; i < noeuds.Length; i++)      /// à la fin du programme, on remet la couleur de chaque noeud à blanc pour pouvoir réaliser le reste des fonctions.
    {
        noeuds[i].couleur = "blanc";
    }



    return CheminLePlusCourt;
}



        public List<int> AlgorithmeBellmanFord(Noeud<T>[] noeuds, int ordre, int[,] matriceAdjacence )
        {
            List<int> CheminLePlusCourt = new List<int>();                           /// L'algorithme retournera une liste de station parcourue représentant le chemin le plus court.
            Console.WriteLine("Nous allons utiliser l'algorithme de Bellman-Ford pour déterminer le chemin le plus court entre deux sommet.");

            /// On demande à l'utilisateur s'il souhaite afficher de nouveau le sommaire, afin de trouver l'identifiant des stations
            string reponse = "";
            Console.WriteLine("\nSouhaitez-vous afficher le sommaire des stations de métro ? ");
            do
            {
                Console.WriteLine("Entrez OUI ou NON : ");
                reponse = Console.ReadLine().ToLower(); /// Permet de fonctionner si l'utilisateur écrit en minus
            } while (reponse != "oui" && reponse != "non");

            if (reponse == "oui")
            {
                SommaireMetro(matrice_nomStation); /// Lance la fonction SommaireMetro qui permet d'afficher le sommaire
            }

            /// On demande à l'utilisateur une station de départ et une station d'arrivée.
            Console.WriteLine("Saisir le numéro de la station de départ en vous référant au sommaire :");
            int départ = Convert.ToInt32(Console.ReadLine());
            while (départ > ordre || départ <= 0)
            {
                Console.WriteLine("Le numéro de station n'est pas valide : il faut saisir un numéro existant");          /// L'utilisateur saisi la station de départ jusqu'à ce qu'elle soit valide
                départ = Convert.ToInt32(Console.ReadLine());
            }

            Console.WriteLine("Saisir le numéro de la station d'arrivée en vous référant au sommaire :");
            int arrivée = Convert.ToInt32(Console.ReadLine());
            while (arrivée > ordre || arrivée <= 0)
            {
                Console.WriteLine("Le numéro de station n'est pas valide : il faut saisir un numéro existant");          /// L'utilisateur saisi la station d'arrivée jusqu'à ce qu'elle soit valide
                arrivée = Convert.ToInt32(Console.ReadLine());
            }

            int infini = 999999999;
            

            int[] distance = new int[ordre];               /// On initialise un tableau des distances et un tableau contenant les prédecesseurs de chaque sommet
            int[] predecesseur = new int[ordre];

            for (int i = 0; i < ordre; i++)                                      
            {
                distance[i] = infini;                 /// On initalise toutes les distances à plus l'infini 
                predecesseur[i] = -1;                 /// On initialise touts les prédecesseurs à -1 (nous avons choisi -1 pour représenté null) 
            }
            distance[départ - 1] = 0;            /// La distance du sommet de départ est mise à 0


            

            for (int i = 0; i < ordre - 1; i++)  /// Le nombre d'itération sera limité au nombre d'arêtes du graphe
            {
                for (int j = 1; j <= ordre; j++)
                {
                    for (int h = 0; h < noeuds[j-1].listevoisins.Count; h++)        /// On parcours chaque sommet et chaque élément de sa liste de voisins ce qui reviens à parcourir chaque arête du graphe
                    {
                        int voisin = noeuds[j-1].listevoisins[h] - 1;
                        if (Convert.ToInt32(distance[j - 1]) + matriceAdjacence[j-1, voisin] < distance[voisin])
                        {
                            distance[voisin] = Convert.ToInt32(distance[j - 1]) + matriceAdjacence[j - 1, voisin];          /// Si la condition du if est respectée alors on met à jour la distance du voisin
                            predecesseur[voisin] = j - 1;                  /// Le prédecesseur du voisin deviens la source du lien
                        }
                    }
                }
            }

           /// Pour que l'algorithme de Bellmann-Ford puisse s'appliquer, il faut s'assurer qu'il n'y pas de cycle absorbant
           /// Il s'agit en fait de refaire une dernière itération et de voir si les distances changent
           /// vérification de l'inexistance de cycle de poids strictement négatifs en fin d'algorithme:
           
            
            for (int j = 1; j <= ordre; j++)
            {
                for (int h = 0; h < noeuds[j - 1].listevoisins.Count; h++)       /// On vérifie donc chaque arêtes de la même manière
                {
                    int voisin = noeuds[j - 1].listevoisins[h] - 1;
                    if (Convert.ToInt32(distance[j - 1]) + matriceAdjacence[j - 1, voisin] < distance[voisin])
                    {
                        Console.WriteLine(" Il y a un circuit de poids négatif. ");
                        CheminLePlusCourt = null;       /// Si il y a un cycle absorbant, l'algorithme ne fonctionne pas donc nous mettons le CheminLePlusCourt à null
                    }
                }
            }

            /// Nous allons maintenant afficher le chemin le plus court en reconstruisant la liste des predecesseurs.

            int SommetArr = arrivée - 1;
            while (SommetArr != -1)                  /// On remonte le chemin parcouru à l'envers pour le reconstruire
            {
                CheminLePlusCourt.Insert(0, SommetArr + 1);          /// On ajoute donc le sommet au début du chemin
                SommetArr = predecesseur[SommetArr];               /// puis le prédecesseur du sommet deviens le nouveau sommet et ainsi de suite, jusqu'à remonter jusqu'au sommet de départ
            }

          

            /// On calcule le temps du trajet
            int tempsTotal = 0;
            for (int i = 0; i < CheminLePlusCourt.Count - 1; i++)          /// Le temps totale est initalisé à 0 puis pour chaque lien entre une StationInitale et une stationSuivante, on ajoute le poids de ce lien au temps total à l'aide de la matrice d'adjacence
            {
                int stationInitiale = CheminLePlusCourt[i] - 1;
                int stationSuivante = CheminLePlusCourt[i + 1] - 1;
                tempsTotal += matriceAdjacence[stationInitiale, stationSuivante];    /// A chaque itération, on chercher le temps entre les deux stations dans la matrice d'adjacence.
            }



            ///On affiche le chemin le plus court
            Console.WriteLine("Le chemin le plus court entre la station de métro " + noeuds[départ - 1].nom + " (n°" + noeuds[départ - 1].idNoeud + ") et la station de métro " + noeuds[arrivée - 1].nom + " (n°" + noeuds[arrivée - 1].idNoeud + ") est le trajet : ");
            for (int i = 0; i < CheminLePlusCourt.Count; i++)
            {
                if (i == CheminLePlusCourt.Count - 1)
                {
                    Console.Write(noeuds[CheminLePlusCourt[i] - 1].nom);
                }
                else
                {
                    Console.Write(noeuds[CheminLePlusCourt[i] - 1].nom + " --> ");          /// on affiche le chemin le plus court en transformant la associant chaque élément de la liste de chiffre obtenue à son nom de station.
                }

            }
            Console.WriteLine();
            Console.WriteLine("Ce trajet durera : " + tempsTotal + " minutes.");  /// On affiche le temps total du trajet



            for (int i = 0; i < noeuds.Length; i++)      /// à la fin du programme, on remet la couleur de chaque noeud à blanc pour pouvoir réaliser le reste des fonctions.
            {
                noeuds[i].couleur = "blanc";
            }
             

            return CheminLePlusCourt;
        }

