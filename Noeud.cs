using System.Collections.Generic;

namespace ProjetPSI
{
    public class Noeud<T>
    {
        public T idNoeud { get; set; }             /// On écrit tous les attributs d'un Noeud
        public string nom { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string ligneStation { get; set; }
        public string couleur { get; set; }
        public List<int> listevoisins { get; set; }

        // Constructeur principal avec voisins
        public Noeud(T idNoeud, string nom, string couleur, List<int> listevoisins)
        {
            this.idNoeud = idNoeud;
            this.nom = nom;
            this.couleur = couleur;
            this.listevoisins = listevoisins ?? new List<int>();
        }


        /// constructeur servant à la visualisation
        public Noeud(T idNoeud, string nom, double longitude, double latitude, string ligneStation, List<int> listevoisins)
        {
            this.idNoeud = idNoeud;
            this.nom = nom;
            this.longitude = longitude;
            this.latitude = latitude;
            this.ligneStation = ligneStation;
            this.couleur = "blanc";
            this.listevoisins = new List<int>();
        }

        // Ajouter un voisin à la liste
        public void AjouterVoisin(int idVoisin)
        {
            if (!listevoisins.Contains(idVoisin))
            {
                listevoisins.Add(idVoisin);
            }
        }
    }
}
