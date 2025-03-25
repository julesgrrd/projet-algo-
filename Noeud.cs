using System.Diagnostics;
using System.Collections.Generic;

namespace ProjetPSI
{
    public class Noeud <T>
    {
        public int idNoeud { get; set; }
        public T nom { get; set; }
        public double latitude { get; set; }      
        public double longitude { get; set; }
        public int codeInsee { get; set; }
        public string couleur { get; set; }
        public List<int> listevoisins { get; set; }

        public Noeud(int idNoeud, T nom, double latitude, double longitude, int codeInsee, string couleur, List<int> listevoisins)           /// on écrit le constructeur de la classe Noeud contenant les différents attributs de la classe.
        {
            this.idNoeud = idNoeud;
            this.nom = nom;
            this.latitude = latitude;
            this.longitude = longitude;
            this.codeInsee = codeInsee;
            this.couleur = couleur;
            this.listevoisins = listevoisins;
        }

        public Noeud (int idNoeud, string couleur, List<int> listevoisins)
        {
            this.idNoeud = idNoeud;
            this.couleur = couleur;
            this.listevoisins = listevoisins;
        } 

        public void AjouterVoisin (int idVoisin)
        {
            if (listevoisins.Contains(idVoisin)==false)
            {
                listevoisins.Add(idVoisin);
            }
        }


    }
}
