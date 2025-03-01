using System.Diagnostics;

namespace Problème_scientifique_informatique
{
    public class Noeud
    {
        public int numeroNoeud;
        public string couleur;
        public List<int> listevoisins;

        public Noeud(int numeroNoeud, string couleur, List<int> listevoisins)           /// on écrit le constructeur de la classe Noeud contenant les différents attributs de la classe.
        {
            this.numeroNoeud = numeroNoeud;
            this.couleur = couleur;
            this.listevoisins = listevoisins;
        }

        

    }
}
