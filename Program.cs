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
    using ProjetPSI;
    using SkiaSharp;
    using Projet_PSI;
    using Mysqlx.Crud;
    using System.Diagnostics;
    using MySql.Data.MySqlClient;
    using Google.Protobuf.WellKnownTypes;
    using System.Numerics;
    using MySqlX.XDevAPI;

    public class Program
    {
        static string id;
        static void Main(string[] args)
        {

            /// **************************************************************************PARTIE GRAPHE METRO PARIS*********************************************************************** 
            #region Partie Graphe Metro Paris
            string fichier = "..\\net7.0\\MetroParis.xlsx";                /// On accède au fichier excel MetroParis contenant tous les liens du graphe
                int[,] matrice_relation = null;                             /// On initalise les deux matrices que nous utiliserons par la suite
                string[,] matrice_infoStation = null;

                if (!File.Exists(fichier))
                {
                    Console.WriteLine("Le fichier n'existe pas.");             /// Si le fichier n'existe pas, l'utilisateur est informé
                    return;
                }
                else
                {
                    FileInfo fichierInfo = new FileInfo(fichier);
                    ExcelPackage.License.SetNonCommercialPersonal("ProjetPSI");
                    using (var package = new ExcelPackage(fichierInfo))
                    {
                        ExcelWorksheet worksheet1 = package.Workbook.Worksheets[1];    /// Acces à la seconde feuille 'Arcs' de l'excel MetroParis
                        ExcelWorksheet worksheet2 = package.Workbook.Worksheets[0];    /// Acces à la premiere feuille 'Noeuds' de l'excel MetroParis   

                        matrice_relation = new int[worksheet1.Dimension.End.Row - 2, 4];    /// On enlève 2 au nombre de lignes des deux matrices car on ne prend pas en compte la première ligne de l'excel correspond au titre des colonnes
                        matrice_infoStation = new string[worksheet2.Dimension.End.Row - 1, 5];

                        /// Boucle qui permet de convertir les valeurs de l'excel (feuille 2) en entier
                        for (int ligne = 2; ligne < worksheet1.Dimension.End.Row; ligne++)
                        {
                            int idStation1 = Convert.ToInt32(worksheet1.Cells[ligne, 1].Value);    /// Identifiant de la station
                            int idVoisin = Convert.ToInt32(worksheet1.Cells[ligne, 3].Value);    /// Identifiant d'une station voisine à idStation1 
                            int doubleSens = Convert.ToInt32(worksheet1.Cells[ligne, 4].Value);    /// doubleSens est égale à 1 si la relation entre idStation1 et idVoisin 
                            int poids = Convert.ToInt32(worksheet1.Cells[ligne, 5].Value);    /// temps entre idStation1 et idVoisin       

                                                                                              /// On remplit la matrice avec les données de la feuille Arcs
                            if (idStation1 != null && idVoisin != null) /// Permet de vérifier que l'indice de la station et du voisin ne sont pas null
                            {
                                matrice_relation[ligne - 2, 0] = idStation1;
                                matrice_relation[ligne - 2, 1] = idVoisin;
                                matrice_relation[ligne - 2, 2] = doubleSens;
                                matrice_relation[ligne - 2, 3] = poids;
                            }
                        }

                        /// Boucle qui permet de convertir les valeurs de l'excel (feuille 1) en string
                        for (int ligne = 2; ligne <= 329; ligne++)
                        {
                            string idStation2 = Convert.ToString(worksheet2.Cells[ligne, 1].Value);    /// Identifiant de la station
                            string ligneStation = Convert.ToString(worksheet2.Cells[ligne, 2].Value);    /// Ligne sur laquelle est située de la station
                            string nomStation = Convert.ToString(worksheet2.Cells[ligne, 3].Value);    /// Nom de la station  
                            string longitudeStation = Convert.ToString(worksheet2.Cells[ligne, 4].Value);    /// Longitude de l'emplacement de la station
                            string latitudeStation = Convert.ToString(worksheet2.Cells[ligne, 5].Value);    /// Latitude de l'emplacement de la station

                                                                                                            /// On remplit la matrice avec les données de la feuille Noeuds
                            matrice_infoStation[ligne - 2, 0] = idStation2;
                            matrice_infoStation[ligne - 2, 1] = nomStation;
                            matrice_infoStation[ligne - 2, 2] = longitudeStation;
                            matrice_infoStation[ligne - 2, 3] = latitudeStation;
                            matrice_infoStation[ligne - 2, 4] = ligneStation;
                        }
                    }
                }


                Graphe<int> GrapheMetro = new Graphe<int>(matrice_relation, matrice_infoStation); /// On crée un graphe GrapheMetro à partir de la classe Graphe

                int ordre = GrapheMetro.OrdreDuGraphe(matrice_relation);    /// On appelle la fonction OrdreDuGraphe pour obtenir l'ordre du graphe
                                                                            /// int taille = GrapheMetro.TailleDuGraphe(matrice_relation);    /// On appelle la fonction TailleDuGraphe pour obtenir la taille du graphe
                List<int>[] ListeAdjacence = GrapheMetro.GenererListeAdjacence(matrice_relation, ordre);    /// On appelle la fonction ListeAdjacence pour obtenir la liste d'adjacence du graphe
                                                                                                            /// bool orienté = GrapheMetro.GrapheOriente(ListeAdjacence);    /// On appelle la fonction GrapheOriente pour savoir si c'est orienté
                int[,] MatriceAdjacence = GrapheMetro.GenererMatriceAdjacence(matrice_relation, ordre);    /// On appelle la fonction MatriceAdjacence pour obtenir la matrice d'adjacence du graphe

                                                                                                           /// GrapheMetro.SommaireMetro(matrice_infoStation);    /// On appelle la fonction SommaireMetro qui permet d'afficher le sommaire des stations de métro avec leur identifant et nom


                /// On initialise un tableau de noeuds noeudsVisuel qui servira pour la visualisation du plan de métro
                Noeud<int>[] noeudsVisuel = new Noeud<int>[ordre];
                for (int i = 0; i < ordre; i++)
                {
                    string nom = "";
                    double longitude = 0;
                    double latitude = 0;
                    string ligneStation = "";

                    for (int j = 0; j < ordre; j++)
                    {
                        if (Convert.ToInt32(matrice_infoStation[j, 0]) == i + 1)
                        {
                            nom = matrice_infoStation[j, 1];
                            longitude = double.Parse(matrice_infoStation[j, 2]);
                            latitude = double.Parse(matrice_infoStation[j, 3]);
                            ligneStation = matrice_infoStation[j, 4];
                        }
                    }

                    noeudsVisuel[i] = new Noeud<int>(i + 1, nom, longitude, latitude, ligneStation, ListeAdjacence[i]);    /// Chaque noeud est défini par son identifiant, son nom, sa longitude, sa latitude, la ligne à laquelle elle appartient et la liste de ses voisins
                }

                /// On initialise un tableau de noeuds noeuds pour les algorithmes du PCC
                Noeud<int>[] noeuds = new Noeud<int>[ordre];
                for (int i = 0; i < ordre; i++)
                {
                    string nom = "";
                    for (int j = 0; j < ordre; j++)
                    {
                        if (Convert.ToInt32(matrice_infoStation[j, 0]) == i + 1)
                        {
                            nom = matrice_infoStation[j, 1];
                        }
                    }
                    noeuds[i] = new Noeud<int>(i + 1, nom, "blanc", ListeAdjacence[i]);    /// Tous les noeuds de graphe prennent en paramètre un identifiant i+1, la couleur blanche et une liste de voisins correspondant à la liste d'adjacence du sommet étudié.
                }
            #endregion
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            /// **************************************************************PARTIE MENU PRINCIPAL******************************************************************
            #region Partie Menu Principal
                ///Cette partie là permet d'afficher à l'utilisateur les choix possible pour le travail rendu
            bool continuer = true;///on initialise un bool afin de faire en sorte que l'algorithme puisse continuer à tourner une fois que l'utilisateur a fait une commande
            while (continuer)
            {
                Console.WriteLine("" +
                    "                                      -------------------------------------------------------------------------------\n" +
                    "                                      | Tapez 1 pour accéder à la partie Coloration du Graphe Client Cuisinier !    |\n" +
                    "                                      | Tapez 2 pour accéder à la partie Import/Export XML/Json !                   |\n" +
                    "                                      | Tapez 3 pour accéder à la partie statistique !                              |\n" +
                    "                                      | Tapez 4 pour accéder à la partie interface utilisateur !                    |\n" +
                    "                                      -------------------------------------------------------------------------------");
                string rep = Console.ReadLine().Trim();
                    switch (rep)
                    {
                        case "1":
                            PartieColorationGraphe();
                            break;
                        case "2":
                            PartieImportExport();
                            break;
                        case "3":
                            PartieStatistiques();
                            break;
                        case "4":
                            Utilisateur();
                            break;
                        default:
                            continuer = false;
                            break;
                    } 
                if ( continuer)
                {
                    Console.ReadKey();
                }
            }
            #endregion
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            /// **************************************************************PARTIE GRAPHE Coloration Graphe CLIENT-CUISINIER******************************************************************
            #region Partie Coloration Graphe Client-Cuisinier
            void PartieColorationGraphe()
            {
                string[,] matriceRelation_ClientCuisinier = RelationClientCuisinier.MatriceClientCuisinier();

                for (int i = 0; i < matriceRelation_ClientCuisinier.GetLength(0); i++)
                {
                    for (int j = 0; j < matriceRelation_ClientCuisinier.GetLength(1); j++)
                    {
                        Console.Write(matriceRelation_ClientCuisinier[i, j] + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();

                /// On crée notre graphe avec les relations entre clients et cuisiniers
                Graphe_CuisinierClient graphe_RelationCuisinierClient = new Graphe_CuisinierClient(matriceRelation_ClientCuisinier);

                /// On applique l'algorithme de coloration Welsh-Powells sur le graphe
                graphe_RelationCuisinierClient.Algo_WelshPowell(graphe_RelationCuisinierClient);

                /// On affiche la coloration du graphe
                graphe_RelationCuisinierClient.AfficherColorationNoeud();

                /// On obtient le nombre chromatique du graphe
                int nbChromatique = graphe_RelationCuisinierClient.NombreMinCouleurs();

                /// On vérifie si le graphe est biparti ou non
                graphe_RelationCuisinierClient.GrapheBiparti(nbChromatique);

                /// On vérifie si le graphe est planaire ou non
                graphe_RelationCuisinierClient.GraphePlanaire(nbChromatique);

                /// On affiche les différents groupes indépendants du graphe
                graphe_RelationCuisinierClient.GroupeIndependant(nbChromatique);
            }
            #endregion
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            /// 
            /// ***********************************************************************IMPORT/EXPORT XML/Json**************************************************************************
            #region Partie Import/Export XML/Json
            void PartieImportExport()
            {
                string connectionString = "SERVER=localhost;PORT=3306;DATABASE=datatables;UID=admin;PASSWORD=admin_password;";


                /// Création de "service"
                var service = new ImportExport_XML_Json(connectionString);

                /// Récupération des commandes depuis la BDD
                List<Commande> commandes = service.RécupérerCommande();



                /// Export JSON
                service.ExportVersJson("commandes.json");
                Console.WriteLine("\n \n Export JSON terminé : commandes.json");

                /// Export XML
                service.ExportVersXml("commandes.xml");
                Console.WriteLine("Export XML terminé : commandes.xml");


                /// Import JSON
                List<Commande> commandesJson = service.ImportDepuisJson("commandes.json");
                Console.WriteLine("\n--- Commandes importées depuis JSON ---");
                foreach (var c in commandesJson)
                {
                    Console.WriteLine("Commande" + c.IdCommande + " - Client: " + c.IdClient + ", Cuisinier: " + c.IdCuisinier + " , Date:" + c.DateCommande);
                }

                /// Import XML
                List<Commande> commandesXml = service.ImportDepuisXml("commandes.xml");
                Console.WriteLine("\n--- Commandes importées depuis XML ---");
                foreach (var c in commandesXml)
                {
                    Console.WriteLine("Commande" + c.IdCommande + " - Client:" + c.IdClient + ", Cuisinier:" + c.IdCuisinier + ", Date:" + c.DateCommande);
                }

                /// Ouvre les fichiers automatiquement
                System.Diagnostics.Process.Start("notepad.exe", "commandes.json");
                System.Diagnostics.Process.Start("notepad.exe", "commandes.xml");

                /// attention a bien supprimer les fichiers du bloc note après visualisation (fermer les deux avec la croix) pour que les fichiers se regénère sans souci à chaque itéraation.

            }
            #endregion
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///*********************************************************************************Partie STATISTIQUES***********************************************************************
            #region Partie Statistiques
            void PartieStatistiques()
            {
                string connectionString = "SERVER=localhost;PORT=3306;DATABASE=datatables;UID=admin;PASSWORD=admin_password;";
                Console.WriteLine("\n \n ************************* BIENVENUE DANS LE MENU STATISTIQUES DE L'APPLICATION *************************");

                var stats = new Statistiques(connectionString);


                /// requêtes obligatoires
                stats.NombreLivraisonsParCuisinier();
                stats.CommandeSelonPeriode(new DateTime(), new DateTime());
                stats.MoyennePrixCommandes();
                stats.MoyenneDepenseParComptesClients();
                stats.CommandesSelonPlatEtPeriode("", "", new DateTime(), new DateTime());


                /// Autres requêtes
                stats.SuperCuisiniers();
                stats.AfficherMenusCommandesAuMoinsUneFois();
                stats.ClientsEtCommandes();
                stats.CompterClientsParVille();
                stats.AfficherClientPlatLePlusCher();
            }
            #endregion
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///
            ///*********************************************************************************Interface Utilisateur ***********************************************************************
            #region Partie Interface Utilisateur

            Utilisateur();///on lance la méthode.
            /// <summary>
            /// Pour les identifiants : Particulier = PART001, PART002, PART003, PART004
            ///                         Cuisinier = CUI001, CUI002, CUI003, CUI004
            ///                         Entreprise = ENT001, ENT002, ENT003, ENT004
            ///                         Mot de passe : password
            ///                         ADMIN = admin et mdp = admin_password
            ///                         
            /// Le programme fonctionne comme un arbre de possibilités. En fonction des choix de l'utilisateur, on lance différentes méthodes etc... ) partir de la méthode initiale Utilisateur(). Ainsi, l'intêret de ce fonctionnement est que les 
            /// seuls arguments placés en entrée sont la connexion qui est ouverte au lancement et adapté au type d'utilisateur ainsi que le type de compte. 
            /// Tout d'abord, on demande à l'utilisateur de s'identifier ( Utilisateur()).Dans le cas où l'utilisateur n'a pas de compte, il peut en créer un en spécifiant son type.Après cela, en fonction de son identifiant, on trouve son type de compte qu'on utilisera pour ouvrir une connexion adapté à ce dernier ( Connexion ). 
            /// Ensuite, en fonction de ce même type de compte, on lance les méthodes correspondantes au type ( ChoixCuisinier, ChoixParticulier, ChoixEntreprise ).
            /// Ces méthodes nous permettent de proposer le choix des commandes disponibles à l'utilisateur. Le cuisinier peut choisir d'ajouter un plat puis de préciser les ingrédients et les informations liées au plat ( AjouterPlat) 
            /// Il peut aussi choisir de voir ses commandes disponibles et d'obtenir leurs informations afin de savoir quand agir (InfoLivraison). Il peut obtenir des informations sur ses commandes comme le trajet le plus court, la date etc... Aussi, il lui est possible d'observer une liste des clients. 
            /// Pour les clients, que ce soit une entreprise ou un particulier ne change pas grand chose. Donc les méthodes sont les mêmes. Ainsi, ils peuvent observer la liste des plats et se renseigner sur ce qui les composent ainsi que passer une commande (ListePlat).
            /// De plus, ils peuvent aussi laisser un avis sur leurs commandes. Si ils ont déjà laisser un avis, alors il est mis à jour (AvisPlat). Enfin, ils peuvent choisir d'observer leurs commandes passées(CommandesPassée) et de modifier leur commandes en cours. 
            /// De plus, chacun des utilisateurs peuvent modifier ou supprimer leur compte. 
            
          

            ///Cette méthode gère l'identification de l'utilisateur grâce au login et le mot de passe puis l'envoie sur les menus appropiés au type de compte.
            ///Tout d'abord, on demande l'existance du compte puis les identifiants. En fonction de ces derniers on ouvre la connexion adaptée puis selon le type du compte, on l'oriente sur les différents choix qui sont possibles. 
            void Utilisateur()
            {
                Console.WriteLine("" +
                    "                                          ----------------------------------------\n " +
                    "                                         |    Avez vous un compte ?  (oui/non)  |\n" +
                    "                                          ----------------------------------------");
                string reponse = Console.ReadLine().ToLower().Trim();
                if (reponse == "non" || reponse == "n" || reponse == "no")
                {
                    CreerCompte();
                    Console.Clear();
                }
                Console.WriteLine("" +
                        "                                          --------------------------\n " +
                        "                                          |    Identifiant :      |\n" +
                        "                                          --------------------------");
                string ID = Console.ReadLine().ToUpper().Trim();
                Console.WriteLine("" +
                    "                                           -------------------------\n" +
                    "                                           |     Mot de passe :    |\n" +
                    "                                           -------------------------");
                string Mdp = Console.ReadLine().Trim();
                Console.WriteLine("");
                Console.Clear();
                string typeCompte = "";
                MySqlConnection connecUser = Connexion(ID, Mdp, out typeCompte);
                if (connecUser != null)
                {
                    id = ID;
                    switch (typeCompte)
                    {
                        case "Cuisinier":
                            ChoixCuisinier(connecUser, typeCompte);
                            break;
                        case "Particulier":
                            ChoixParticulier(connecUser, typeCompte);
                            break;
                        case "Entreprise":
                            ChoixEntreprise(connecUser, typeCompte);
                            break;
                        default: Console.WriteLine("type d'utilisateur inconnu"); break;
                    }
                }
            }

            
            ///Cette méthode permet à un nouvel utilisateur de s'inscrire dans la base de donnée en renseignant ses informations personnelles. On les enregistre dans la table compte puis on adapte pour les tables Cuisinier, Client ou Cuisinier. 
            ///On demande le type du compte ( Cuisinier/Particulier/Entreprise). En suite, on demande, valide puis ajoute les informations données par l'utilisateur. Elles sont ensuite insérée dans les tables adaptées.
            void CreerCompte()
            {
                string choix = "";
                string type_compte = "";
                do
                {
                    Console.WriteLine("" +
                            "                                          ---------------------------------------\n " +
                            "                                         |    Création d'un nouveau compte !   |\n" +
                            "                                          |    Choisissez le type du compte :   |\n" +
                            "                                          ---------------------------------------");
                    Console.WriteLine("" +
                            "                                          |    1  -  Cuisinier                  |\n" +
                            "                                          |    2  -  Particulier                |\n" +
                            "                                          |    3  -  Entreprise                 |\n" +
                            "                                          ---------------------------------------");
                    choix = Console.ReadLine().Trim();
                    if (choix == "1")
                    {
                        type_compte = "Cuisinier";
                    }
                    else if (choix == "2")
                    {
                        type_compte = "Particulier";
                    }
                    else if (choix == "3")
                    {
                        type_compte = "Entreprise";
                    }
                    else
                    {
                        Console.WriteLine("" +
                            "                                           ----------------------------------------------------\n" +
                            "                                           |    Choix non valide. Veuillez entrer 1, 2 ou 3.  |\n" +
                            "                                           ----------------------------------------------------");
                    }
                } while (type_compte == "");
                Console.WriteLine("");
                Console.Clear();

                string identifiant;
                bool valide = false;

                string reqRoot = "SERVER=localhost;PORT=3306;DATABASE=datatables;UID=admin;PASSWORD=admin_password";
                do
                {
                    Console.WriteLine("" +
                            "                                           -----------------------------------\n" +
                            "                                           |     Entrez votre identifiant    |\n" +
                            "                                           -----------------------------------");
                    identifiant = Console.ReadLine().Trim();
                    using (MySqlConnection connex = new MySqlConnection(reqRoot))
                    {
                        connex.Open();
                        string verificationId = "SELECT COUNT(*) FROM Compte WHERE idCompte = @id;";
                        using (MySqlCommand verificationCommande = new MySqlCommand(verificationId, connex))
                        {
                            verificationCommande.Parameters.AddWithValue("@id", identifiant);
                            int count = Convert.ToInt32(verificationCommande.ExecuteScalar());

                            if (count > 0)
                            {
                                Console.WriteLine("Cet identifiant est déjà utilisé ! Veuillez en choisir un autre.\n");
                            }
                            else
                            {
                                valide = true;
                            }
                        }
                    }
                }
                while (valide == false);
                Console.WriteLine("" +
                        "                                           --------------------------------------\n" +
                        "                                           |     Entrez votre mot de passe :    |\n" +
                        "                                           -------------------------------------");
                string mdp = Console.ReadLine().Trim();
                Console.WriteLine("" +
                        "                                           -----------------------------\n" +
                        "                                           |     Entrez votre nom :    |\n" +
                        "                                           -----------------------------");
                string nom = Console.ReadLine();
                Console.WriteLine("" +
                        "                                           -------------------------------\n" +
                        "                                           |     Entrez votre prénom :   |\n" +
                        "                                           -------------------------------");
                string prenom = Console.ReadLine();
                Console.WriteLine("" +
                        "                                           ---------------------------\n" +
                        "                                           |     Entre votre rue :   |\n" +
                        "                                           ---------------------------");
                string rue = Console.ReadLine();

                int num;
                valide = false;
                do
                {
                    Console.WriteLine("" +
                            "                                           --------------------------------------\n" +
                            "                                           |     Entrez votre numéro de rue :   |\n" +
                            "                                           --------------------------------------");
                    string saisie = Console.ReadLine().Trim();
                    if (int.TryParse(saisie, out num) == true && num > 0)
                    {
                        valide = true;
                    }
                    else
                    {
                        Console.WriteLine("" +
                            "                                           -----------------------------------------------------------\n" +
                            "                                           |   Numéro invalide ! Veuillez entrer un nombre entier.   |\n" +
                            "                                           -----------------------------------------------------------");
                    }
                } while (valide == false);

                int codepost;
                valide = false;
                do
                {
                    Console.WriteLine("" +
                            "                                           ------------------------------------\n" +
                            "                                           |     Entrez votre code postal :   |\n" +
                            "                                           ------------------------------------");

                    string saisie = Console.ReadLine().Trim();
                    if (int.TryParse(saisie, out codepost) == true && codepost > 0)
                    {
                        valide = true;
                    }
                    else
                    {
                        Console.WriteLine("" +
                            "                                           --------------------------------------------------\n" +
                            "                                           |   Code Postal invalide ! Veuillez réesayer :   |\n" +
                            "                                           --------------------------------------------------");
                    }
                } while (valide == false);

                Console.WriteLine("" +
                        "                                           ------------------------------\n" +
                        "                                           |     Entrez votre ville :   |\n" +
                        "                                           ------------------------------");
                string ville = Console.ReadLine();
                Console.WriteLine("" +
                        "                                           -----------------------------\n" +
                        "                                           |     Entrez votre mail :   |\n" +
                        "                                           -----------------------------");
                string mail = Console.ReadLine().Trim();
                Console.WriteLine("" +
                        "                                           ----------------------------------\n" +
                        "                                           |     Entrez votre téléphone :   |\n" +
                        "                                           ----------------------------------");
                string tel = Console.ReadLine().Trim();
                Console.Clear();
                GrapheMetro.SommaireMetro(matrice_infoStation);
                Console.WriteLine("" +
                        "                                           ---------------------------------------------------------------------\n" +
                        "                                           |     Entrez le numéro de votre station de métro la plus proche :   |\n" +
                        "                                           ---------------------------------------------------------------------");
                int metro = Convert.ToInt32(Console.ReadLine());
            string nom_entreprise = "";
                string nom_referent = "";
                if (type_compte == "Entreprise")
                {
                    Console.WriteLine("" +
                        "                                           ---------------------------------------------\n" +
                        "                                           |     Entrez le nom de votre entreprise :   |\n" +
                        "                                           ---------------------------------------------");
                    nom_entreprise = Console.ReadLine().Trim();
                    Console.WriteLine("" +
                        "                                           -------------------------------------\n" +
                        "                                           |     Entrez le nom du référent :   |\n" +
                        "                                           -------------------------------------");
                    nom_referent = Console.ReadLine().Trim();

                }
                using (MySqlConnection connex = new MySqlConnection(reqRoot))
                {
                    connex.Open();
                    string Ajout = "insert into Compte (idCompte, Nom, Prénom, Rue, Numéro, CodePostal, Ville, AdresseMail, Téléphone, MetroLePlusProche, MotDePasse, Radié, TypeCompte) values (@idCompte, @Nom, @Prénom, @Rue, @Numéro, @CodePostal, @Ville, @AdresseMail, @Téléphone, @Metro, @MotDePasse, 0, @TypeCompte);";
                    using (MySqlCommand plusdenom = new MySqlCommand(Ajout, connex))
                    {
                        plusdenom.Parameters.AddWithValue("@idCompte", identifiant);
                        plusdenom.Parameters.AddWithValue("@Nom", nom);
                        plusdenom.Parameters.AddWithValue("@Prénom", prenom);
                        plusdenom.Parameters.AddWithValue("@Rue", rue);
                        plusdenom.Parameters.AddWithValue("@Numéro", num);
                        plusdenom.Parameters.AddWithValue("@CodePostal", codepost);
                        plusdenom.Parameters.AddWithValue("@Ville", ville);
                        plusdenom.Parameters.AddWithValue("@AdresseMail", mail);
                        plusdenom.Parameters.AddWithValue("@Téléphone", tel);
                        plusdenom.Parameters.AddWithValue("@Metro", metro);
                        plusdenom.Parameters.AddWithValue("@MotDePasse", mdp);
                        plusdenom.Parameters.AddWithValue("@TypeCompte", type_compte);
                        plusdenom.ExecuteNonQuery();
                    }

                    if (type_compte == "Cuisinier")
                    {
                        string reqAjoutCui = " insert into Cuisinier (idCuisinier, idCompte) values (@idCuisinier, @idCompte);";
                        using (MySqlCommand comAjoutCui = new MySqlCommand(reqAjoutCui, connex))
                        {
                            comAjoutCui.Parameters.AddWithValue("@idCuisinier", identifiant);
                            comAjoutCui.Parameters.AddWithValue("@idCompte", identifiant);
                            comAjoutCui.ExecuteNonQuery();
                        }
                    }
                    else if (type_compte == "Particulier")
                    {
                        string reqMaxId = "select max(cast(substring(idClient, 4) as unsigned)) from Client;";
                        int maxClient = 0;
                        using (MySqlCommand Req = new MySqlCommand(reqMaxId, connex))
                        {
                            object retour = Req.ExecuteScalar();
                            maxClient = Convert.ToInt32(retour);
                        }
                        maxClient++;
                        string idClient = "CLI" + maxClient.ToString("D3");
                        string reqClient = "insert into Client (idClient, TypeClient, idCompte) values (@idClient, 'Particulier', @idCompte);";
                        using (MySqlCommand comClient = new MySqlCommand(reqClient, connex))
                        {
                            comClient.Parameters.AddWithValue("@idClient", idClient);
                            comClient.Parameters.AddWithValue("@idCompte", identifiant);
                            comClient.ExecuteNonQuery();
                        }
                        string reqPart = " insert into Particulier (idClient) values (@idClient);";
                        using (MySqlCommand comPart = new MySqlCommand(reqPart, connex))
                        {
                            comPart.Parameters.AddWithValue("@idClient", idClient);
                            comPart.ExecuteNonQuery();
                        }
                    }
                    else if (type_compte == "Entreprise")
                    {
                        string reqMaxId = "select max(cast(substring(idClient, 4) as unsigned)) from Client;";
                        int maxClient = 0;
                        using (MySqlCommand Req = new MySqlCommand(reqMaxId, connex))
                        {
                            object retour = Req.ExecuteScalar();
                            maxClient = Convert.ToInt32(retour);
                        }
                        maxClient++;
                        string idClient = "CLI" + maxClient.ToString("D3");
                        string reqEnt = "insert into Entreprise (idClient, NomEntreprise, NomReferent) values (@idClient, @NomEntreprise, @NomReferent);";
                        using (MySqlCommand comEnt = new MySqlCommand(reqEnt, connex))
                        {
                            comEnt.Parameters.AddWithValue("@idClient", idClient);
                            comEnt.Parameters.AddWithValue("@NomEntreprise", nom_entreprise);
                            comEnt.Parameters.AddWithValue("@NomReferent", nom_referent);
                            comEnt.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine("" +
                                        "                                           ----------------------------------\n" +
                                        "                                           |     Votre compte a été créé    |\n" +
                                        "                                           ----------------------------------");
                }
            }


            ///Grâce à la connexion et au type de compte, on identifie le type d'utilisateur, ainsi on adapte les informations nécessaires. De plus le "typeCompte" nous permet de différencier un particulier d'une entreprise. Les types sont très similaires ( tout les deux issus de la table Client ) 
            ///mais la table Entreprise est différente de Particulier. 
            ///Tout d'abord, on récupère les informations actuelles du compte afin que l'utilisateur sache ses informations, ce qu'il modifie et si c'est ce qu'il veut modifier. 
            ///Pour chaqu'un de ces champs, on les affiches et on donne la possibilité de les modifier ou de les laisser ainsi en laisser un vide. 
            ///On met à jour les tables en fonction du type. 
            void ModifierCompte(MySqlConnection connecUser, string typeCompte)
            {
                Console.Clear();
                Console.WriteLine(
            "                                      ----------------------------------------------------\n" +
            "                                      |      MODIFICATION  DE  VOS  INFORMATIONS         |\n" +
            "                                      ----------------------------------------------------");
                string requeteInfo = "select Nom, Prénom, Rue, Numéro, CodePostal, Ville, AdresseMail, Téléphone, MetroLePlusProche, MotDePasse from compte where idCompte = @id";
                string nom = "", prenom = "", rue = "", ville = "", mail = "", tel = ""; int metro = 0; string mdp = "";
                int numero = 0, codepost = 0;
                using (MySqlCommand commandeInfo = new MySqlCommand(requeteInfo, connecUser))
                {
                    commandeInfo.Parameters.AddWithValue("@id", id);
                    using (var r = commandeInfo.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            nom = r.GetString("Nom");
                            prenom = r.GetString("Prénom");
                            rue = r.GetString("Rue");
                            numero = r.GetInt32("Numéro");
                            codepost = r.GetInt32("CodePostal");
                            ville = r.GetString("Ville");
                            mail = r.GetString("AdresseMail");
                            tel = r.GetString("Téléphone");
                            metro = r.GetInt32("MetroLePlusProche");
                            mdp = r.GetString("MotDePasse");
                        }
                    }
                }
                Console.WriteLine("                                      |  Nom : " + nom);
                string nvnom = Console.ReadLine().Trim();
                if (nvnom != "")
                {
                    nom = nvnom;
                }
                Console.WriteLine("                                      |  Prénom : " + prenom);
                string nvprenom = Console.ReadLine().Trim();
                if (nvprenom != "")
                {
                    prenom = nvprenom;
                }
                Console.WriteLine("                                      |  Rue : " + rue);
                string nvrue = Console.ReadLine().Trim();
                if (nvrue != "")
                {
                    rue = nvrue;
                }
                Console.WriteLine("                                      |  Numéro : " + numero);
                if (int.TryParse(Console.ReadLine(), out int nvnumero))
                {
                    numero = nvnumero;
                }
                Console.WriteLine("                                      |  Code Postal : " + codepost);
                if (int.TryParse(Console.ReadLine(), out int nvCP))
                {
                    codepost = nvCP;
                }
                Console.WriteLine("                                      |  Ville : " + ville);
                string nvville = Console.ReadLine().Trim();
                if (nvville != "")
                {
                    ville = nvville;
                }
                Console.WriteLine("                                      |  Mail : " + mail);
                string nvmail = Console.ReadLine().Trim();
                if (nvmail != "")
                {
                    mail = nvmail;
                }
                Console.WriteLine("                                      |  Téléphone : " + tel);
                string nvtel = Console.ReadLine().Trim();
                if (nvtel != "")
                {
                    tel = nvtel;
                }
                Console.Clear();
                GrapheMetro.SommaireMetro(matrice_infoStation);
                Console.WriteLine("                                      |  Métro le plus proche : " + metro);
                string saisieMetro = Console.ReadLine().Trim();

                if (!string.IsNullOrEmpty(saisieMetro))
                {
                    if (int.TryParse(saisieMetro, out int nvMetro))
                    {
                        metro = nvMetro;
                    }
                    else
                    {
                        Console.WriteLine(
            "                                      ---------------------------------------------------------------\n" +
            "                                      |      Format invalide, on conserve l'ancienne valeur         |\n" +
            "                                      ---------------------------------------------------------------");
                    }
                }
                Console.WriteLine("                                      |  Mot de passe (laisser vide pour conserver)  :");
                string nvMDP = Console.ReadLine().Trim();
                if (nvMDP != "")
                {
                    mdp = nvMDP;
                }
                using (var isole = connecUser.BeginTransaction())
                {
                    string maj = "update Compte set Nom = @Nom, Prénom = @Prenom, Rue = @Rue, Numéro = @Numero, CodePostal = @Cp, Ville = @ville, AdresseMail=@Mail, Téléphone=@Tel, MetroLePlusProche=@Metro, MotDePasse=@Mdp WHERE idCompte=@id;";
                    using (var requete = new MySqlCommand(maj, connecUser, isole))
                    {
                        requete.Parameters.AddWithValue("@Nom", nom);
                        requete.Parameters.AddWithValue("@Prenom", prenom);
                        requete.Parameters.AddWithValue("@Rue", rue);
                        requete.Parameters.AddWithValue("@Numero", numero);
                        requete.Parameters.AddWithValue("@Cp", codepost);
                        requete.Parameters.AddWithValue("@Ville", ville);
                        requete.Parameters.AddWithValue("@Mail", mail);
                        requete.Parameters.AddWithValue("@Tel", tel);
                        requete.Parameters.AddWithValue("@Metro", metro);
                        requete.Parameters.AddWithValue("@Mdp", mdp);
                        requete.Parameters.AddWithValue("@id", id);
                        requete.ExecuteNonQuery();
                    }

                    if (typeCompte == "Entreprise")
                    {
                        Console.WriteLine("                                      |  Nom entreprise (laisser vide pour inchangé) :");
                        string nvEnt = Console.ReadLine().Trim();
                        Console.WriteLine("                                      |  Nom référent (laisser vide pour inchangé) :");
                        string nvRef = Console.ReadLine().Trim();
                        string reqIdClient = "select idClient from Client where idCompte = @id;";
                        string idCli = "";
                        using (var p = new MySqlCommand(reqIdClient, connecUser, isole))
                        {
                            p.Parameters.AddWithValue("@id", id);
                            idCli = p.ExecuteScalar().ToString();
                        }
                        if (idCli != "")
                        {
                            string majEnt = "update Entreprise set NomEntreprise = if(@Ent = '', NomEntreprise, @Ent), NomReferent = if(@Ref = '', NomReferent, @Ref) where idClient = @cli;";
                            using (var p = new MySqlCommand(majEnt, connecUser, isole))
                            {
                                p.Parameters.AddWithValue("@Ent", nvEnt);
                                p.Parameters.AddWithValue("@Ref", nvRef);
                                p.Parameters.AddWithValue("@cli", idCli);
                                p.ExecuteNonQuery();
                            }
                        }
                    }
                    isole.Commit();
                }
                Console.WriteLine(
           "                                      --------------------------------------------\n" +
           "                                      |   Modifications enregistrées avec succès  |\n" +
           "                                      --------------------------------------------");
            }


            ///On supprime définitivement le compte et les données associées. 
            ///Le type du compte nous permet d'identifier les informations à supprimer et où elles sont placées. 
            ///On demande la confirmation à l'utilisateur, ensuite on supprime les données enregistrées dans chaqu'une des tables associées au rôle que nous avons identifié. 
            void SupprimerCompte(MySqlConnection connecUser, string typeCompte)
            {
                Console.Clear();
                Console.WriteLine(
            "                                      ----------------------------------------------------------\n" +
            "                                      |  SUPPRESSION  DEFINITIVE  DU  COMPTE                    |\n" +
            "                                      |  Tapez « OUI » puis Entrée pour confirmer.              |\n" +
            "                                      ----------------------------------------------------------");
                string confirmation = Console.ReadLine().Trim().ToUpper();
                if (confirmation != "YES" && confirmation != "OUI" && confirmation != "O" && confirmation != "Y")
                {
                    Console.WriteLine(
            "                                      ------------------------------------------\n" +
            "                                      |  Suppression annulée                   |\n" +
            "                                      ------------------------------------------");
                }
                else
                {
                    using (var isole = connecUser.BeginTransaction())
                    {
                        if (typeCompte == "Cuisinier")
                        {
                            using (var commande = new MySqlCommand("delete from FaitRetour where idCuisinier = @id;", connecUser, isole))
                            {
                                commande.Parameters.AddWithValue("@id", id);
                                commande.ExecuteNonQuery();
                            }
                            using (var commande = new MySqlCommand("delete from ProposePlat where idCuisinier = @id;", connecUser, isole))
                            {
                                commande.Parameters.AddWithValue("@id", id);
                                commande.ExecuteNonQuery();
                            }
                            using (var commande = new MySqlCommand("delete from CommandeTransactions where idCuisinier = @id;", connecUser, isole))
                            {
                                commande.Parameters.AddWithValue("@id", id);
                                commande.ExecuteNonQuery();
                            }
                            using (var commande = new MySqlCommand("delete from Cuisinier where idCuisinier = @id;", connecUser, isole))
                            {
                                commande.Parameters.AddWithValue("@id", id);
                                commande.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string idClient = "";
                            using (var commande = new MySqlCommand("select idClient from Client where idCompte = @id;", connecUser, isole))
                            {
                                commande.Parameters.AddWithValue("@id", id);
                                idClient = commande.ExecuteScalar().ToString();
                            }
                            if (idClient != "")
                            {
                                using (var commande = new MySqlCommand("delete from FaitRetour where idClient = @client;", connecUser, isole))
                                {
                                    commande.Parameters.AddWithValue("@id", id);
                                    commande.Parameters.AddWithValue("@client", idClient);
                                    commande.ExecuteNonQuery();
                                }
                                using (var commande = new MySqlCommand("delete from CommandeTransactions where idClient = @client;", connecUser, isole))
                                {
                                    commande.Parameters.AddWithValue("@id", id);
                                    commande.Parameters.AddWithValue("@client", idClient);
                                    commande.ExecuteNonQuery();
                                }
                                using (var commande = new MySqlCommand("delete from LigneCommande where idCommande not in (select idCommande from CommandeTransactions);", connecUser, isole))
                                {
                                    commande.ExecuteNonQuery();
                                }
                                if (typeCompte == "Particulier")
                                {
                                    using (var commande = new MySqlCommand("delete from Particulier where idClient = @client;", connecUser, isole))
                                    {
                                        commande.Parameters.AddWithValue("@client", idClient);
                                        commande.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    using (var commande = new MySqlCommand("delete from Entreprise where idClient = @client;", connecUser, isole))
                                    {
                                        commande.Parameters.AddWithValue("@client", idClient);
                                        commande.ExecuteNonQuery();
                                    }
                                }
                                using (var commande = new MySqlCommand("delete from Client where idClient = @client;", connecUser, isole))
                                {
                                    commande.Parameters.AddWithValue("@client", idClient);
                                    commande.ExecuteNonQuery();
                                }
                            }
                        }
                        using (var commande = new MySqlCommand("delete from Compte where idCompte = @id;", connecUser, isole))
                        {
                            commande.Parameters.AddWithValue("@id", id);
                            commande.ExecuteNonQuery();
                        }
                        isole.Commit();
                    }
                    Console.WriteLine(
            "                                      ---------------------------------\n" +
            "                                      |   Compte supprimé. Au revoir ! |\n" +
            "                                      ---------------------------------");
                    Environment.Exit(0);
                }
            }


            ///Cette méthode nous permet de valider l'authentification et ensuite d'ouvrir la connexion MySql adapté au rôle. 
            ///On prend comme paramètres : String ID qui est l'identifiant saisi par l'utilisateur. String Mdp qui est le mot de passe saisi. out string typeCompte qui est la valeur de sortie qui indique le rôle de l'utilisateur.
            ///On ouvre une connexion en tant qu'admin afin d'avoir l'entièreté des droits, on vérifie l'existence de l'utilisateur en récupérant le type du compte. 
            ///Ensuite, la connexion admin est fermée et on ouvre celle adaptée au client en utilisant l'ID et Mdp.
            ///On retourne enfin la connexion authentifiée afin que l'utilisateur puisse avoir uniquement les droits adaptés à son rôle.
            MySqlConnection Connexion(string ID, string Mdp, out string typeCompte)
            {
                MySqlConnection maConnexion = null;
                typeCompte = "";
                string Connexion = "SERVER=localhost;PORT=3306;DATABASE=datatables ;UID=admin;PASSWORD=admin_password";

                using (maConnexion = new MySqlConnection(Connexion))
                {
                    maConnexion.Open();
                    string Requete = "select TypeCompte from Compte where idCompte = @ID and MotDePasse = @Mdp;";
                    using (MySqlCommand commandconnexion = new MySqlCommand(Requete, maConnexion))
                    {
                        commandconnexion.Parameters.AddWithValue("@ID", ID);
                        commandconnexion.Parameters.AddWithValue("@Mdp", Mdp);

                        object reponse = commandconnexion.ExecuteScalar();
                        if (reponse != null)
                        {
                            typeCompte = reponse.ToString();
                        }
                        else
                        {
                            Console.WriteLine("Login ou mot de passe invalide");
                            return null;

                        }
                    }
                }

                string ConnectionUtilisateur = "";
                if (typeCompte == "Cuisinier")
                {
                    ConnectionUtilisateur = "SERVER=localhost;PORT=3306;DATABASE=datatables ;UID=ID_Cuisinier;PASSWORD=MDP_Cuisinier";
                }
                else if (typeCompte == "Particulier")
                {
                    ConnectionUtilisateur = "SERVER=localhost;PORT=3306;DATABASE=datatables ;UID=ID_Particulier;PASSWORD=MDP_Particulier";
                }
                else if (typeCompte == "Entreprise")
                {
                    ConnectionUtilisateur = "SERVER=localhost;PORT=3306;DATABASE=datatables ;UID=ID_Entreprise;PASSWORD=MDP_Entreprise";
                }
                else
                {
                    Console.WriteLine("Type inconnu");
                }
                MySqlConnection ConnexionUser = new MySqlConnection(ConnectionUtilisateur);
                ConnexionUser.Open();
                return ConnexionUser;
            }


            ///On présente le menu spécifique au type de compte "Cuisinier" puis on redirige les actions qui correspondent.
            ///Le typeCompte nous permet une signature cohérente qui est nécessaire avec les méthodes ModifierCompte et SupprimerCompte. 
            ///On affiche les choix possibles puis en fonction de ce que choisi l'utilisateur, on le redirige vers les méthodes correspondantes 
            void ChoixCuisinier(MySqlConnection connecUser, string typeCompte)
            {
                #region affichage 
                bool rester = true;
                while (rester)
                {
                    Console.Clear();
                    Console.WriteLine("" +
                        "                                      -------------------------------------------------------------\n" +
                        "                                      | Cher cuisinier                                            |\n" +
                        "                                      | Tapez 1 pour ajouter un plat !                            |\n" +
                        "                                      | Tapez 2 si vous souhaitez modifier un plat !              |\n" +
                        "                                      | Tapez 3 pour obtenir les informations pour la livraison ! |\n" +
                        "                                      | Tapez 4 si vous souhaitez modifier votre profil !         |\n" +
                        "                                      | Tapez 5 si vous souhaitez supprimez votre profil !        |\n" +
                        "                                      | Tapez 6 pour avoir la liste des clients !                 |\n" +
                        "                                      | Tapez 0 pour sortir de l'interface !                      |\n" +
                        "                                      -------------------------------------------------------------");
                    string entrée = Console.ReadLine();
                    switch (entrée)
                    {
                        case "1":
                            AjouterPlat(connecUser);
                            break;
                        case "2":
                            ModifierPlat(connecUser);
                            break;
                        case "3":
                            InfoLivraison(connecUser);
                            break;
                        case "4":
                            ModifierCompte(connecUser, typeCompte);
                            break;
                        case "5":
                            SupprimerCompte(connecUser, typeCompte);
                            break;
                        case "6":
                            ListeClient(connecUser);
                            break;
                        case "0":
                            rester = false; 
                            break;
                        default:
                            Console.WriteLine(
            "                                      ------------------------------------------\n" +
            "                                      |  Entrée non reconnu                    |\n" +
            "                                      ------------------------------------------");
                            break;
                    }
                }
                #endregion
            }


            ///On présente le menu spécifique au type de compte "Particulier" puis on redirige les actions qui correspondent.
            ///Le typeCompte nous permet une signature cohérente qui est nécessaire avec les méthodes ModifierCompte et SupprimerCompte.
            ///On affiche les choix possibles puis en fonction de ce que choisi l'utilisateur, on le redirige vers les méthodes correspondantes 
            void ChoixParticulier(MySqlConnection connecUser, string typeCompte)
            {
                bool rester = true;
                while (rester)
                {
                    Console.Clear();
                    Console.WriteLine("" +
                    "                                      -------------------------------------------------------------\n" +
                    "                                      | Cher Client                                               |\n" +
                    "                                      | Tapez 1 pour vous renseigner sur les plats !              |\n" +
                    "                                      | Tapez 2 pour donner votre avis sur votre plat !           |\n" +
                    "                                      | Tapez 3 pour voir vos anciennes commandes !               |\n" +
                    "                                      | Tapez 4 pour modifier une de vos commandes !              |\n" +
                    "                                      | Tapez 5 si vous souhaitez modifier votre profil !         |\n" +
                    "                                      | Tapez 6 si vous souhaitez supprimez votre profil !        |\n" +
                    "                                      | Tapez 0 pour sortir de l'interface !                      |\n" +
                    "                                      -------------------------------------------------------------");
                    string entrée = Console.ReadLine();
                    switch (entrée)
                    {
                        case "1":
                            ListePlat(connecUser);
                            break;
                        case "2":
                            AvisPlat(connecUser);
                            break;
                        case "3":
                            CommandesPassée(connecUser);
                            break;
                        case "4":
                            ModifierCommande(connecUser);
                            break;
                        case "5":
                            ModifierCompte(connecUser, typeCompte);
                            break;
                        case "6":
                            SupprimerCompte(connecUser, typeCompte);
                            break;
                        case "0":
                            rester = false;
                            break;
                        default:
                            Console.WriteLine(
            "                                      ------------------------------------------\n" +
            "                                      |  Entrée non reconnu                    |\n" +
            "                                      ------------------------------------------");
                            break;
                    }
                }
            }


            ///On présente le menu spécifique au type de compte "Particulier" puis on redirige les actions qui correspondent.
            ///Le typeCompte nous permet une signature cohérente qui est nécessaire avec les méthodes ModifierCompte et SupprimerCompte .
            ///On affiche les choix possibles puis en fonction de ce que choisi l'utilisateur, on le redirige vers les méthodes correspondantes
            void ChoixEntreprise(MySqlConnection connecUser, string typeCompte)
            {
                bool rester = true;
                while (rester)
                {
                    Console.Clear();
                    Console.WriteLine("" +
                    "                                      -------------------------------------------------------------\n" +
                    "                                      | Chère Entreprise                                          |\n" +
                    "                                      | Tapez 1 pour vous renseigner sur les plats !              |\n" +
                    "                                      | Tapez 2 pour donner votre avis sur votre plat !           |\n" +
                    "                                      | Tapez 3 pour voir vos anciennes commandes !               |\n" +
                    "                                      | Tapez 4 pour modifier une de vos commandes !              |\n" +
                    "                                      | Tapez 5 si vous souhaitez modifier votre profil !         |\n" +
                    "                                      | Tapez 6 si vous souhaitez supprimez votre profil !        |\n" +
                    "                                      | Tapez 0 pour sortir de l'interface !                      |\n" +
                    "                                      -------------------------------------------------------------");
                    string key = Console.ReadLine();
                    switch (key)
                    {
                        case "1":
                            ListePlat(connecUser);
                            break;
                        case "2":
                            AvisPlat(connecUser);
                            break;
                        case "3":
                            CommandesPassée(connecUser);
                            break;
                        case "4":
                            ModifierCommande(connecUser);
                            break;
                        case "5":
                            ModifierCompte(connecUser, typeCompte);
                            break;
                        case "6":
                            SupprimerCompte(connecUser, typeCompte);
                            break;
                        case "0":
                            rester = false;
                            break;
                        default:
                            Console.WriteLine(
            "                                      ------------------------------------------\n" +
            "                                      |  Entrée non reconnu                    |\n" +
            "                                      ------------------------------------------");
                            break;
                    }
                }
            }


            ///Comme son nom l'indique, elle permet à un cuisinier d'ajouter un nouveau plat à son catalogue avec ses informations et les ingrédients.
            ///Attention, le cuisinier est obligé de renseigner les aliments car nous voulons faire attention à ce qui compose les repas proposés pour le bien des clients. 
            ///On commence par générer un nouvel idPlat et idRecette en récupérant l'id max déjà présent dans la base de données puis on l'incrémente et on l'ajoute au préfixe adapté. 
            ///Ensuite, le cuisinier saisie les propriétés du plat comme le nom, le type etc... 
            ///On l'insère après dans les tables associées ( Recette et Plat ). On lie le plat au cuisinier grâce à la table ProposePlat.
            void AjouterPlat(MySqlConnection connecUser)
            {
                bool continuer = true;
                while (continuer)
                {
                    Console.WriteLine(
                    "                                      -----------------------------------------------\n" +
                    "                                      |  Souhaitez vous spécifier les ingrédiens ?  |\n" +
                    "                                      -----------------------------------------------\n");
                    string réponse = Console.ReadLine().ToLower();
                    Console.WriteLine("");
                    Console.Clear();
                    if (réponse == "yes" || réponse == "oui" || réponse == "o")
                    {
                        #region questionSurPlat
                        /*
                        Console.WriteLine("" +
                            "                                      -----------------------------\n" +
                            "                                      |  Quel est l'ID du plat ?  |\n" +
                            "                                      -----------------------------");
                        string Id_plat = Console.ReadLine();*/
                        #region RecupIdPlat
                        string requetePlat = "select max(cast(substring(idPlat, 5) as unsigned)) from Plat;";
                        int IdmaxPlat = 0;
                        using (MySqlCommand commandeIdPlat = new MySqlCommand(requetePlat, connecUser))
                        {
                            object resultat = commandeIdPlat.ExecuteScalar();
                            IdmaxPlat = Convert.ToInt32(resultat);
                        }
                        IdmaxPlat++;
                        string IdPlat = "PLAT" + IdmaxPlat.ToString("D3");
                        #endregion
                        Console.WriteLine("" +
                            "                                      -------------------------------\n" +
                            "                                      |  Quel est le nom du plat ?  |\n" +
                            "                                      -------------------------------");
                        string nom_plat = Console.ReadLine();
                        Console.WriteLine("" +
                            "                                      --------------------------------\n" +
                            "                                      |  Quel est le type du plat ?  |\n" +
                            "                                      --------------------------------");
                        string type_plat = Console.ReadLine();
                        Console.WriteLine("" +
                            "                                      ------------------------------------------------------------------------\n" +
                            "                                      |  Quel est la date de fabrication du plat ? (format année-mois-jour ) |\n" +
                            "                                      ------------------------------------------------------------------------");
                        string Date_fabrication = Console.ReadLine();
                        Console.WriteLine("" +
                            "                                      -----------------------------------------------------------------------\n" +
                            "                                      |  Quel est la date de péremption du plat ? (format année-mois-jour ) |\n" +
                            "                                      -----------------------------------------------------------------------");
                        string Date_peremption = Console.ReadLine();
                        Console.WriteLine("" +
                            "                                      ---------------------------------------------------\n" +
                            "                                      |  Quel est le nombre de personne pour le plat ?  |\n" +
                            "                                      ---------------------------------------------------");
                        int nb_personnes;
                        while (!int.TryParse(Console.ReadLine(), out nb_personnes))
                        {
                            Console.WriteLine("Nombre de personnes invalide.");
                        }
                        Console.WriteLine("" +
                            "                                      ---------------------------------------------\n" +
                            "                                      |  Quel est le prix par personne du plat ?  |\n" +
                            "                                      ---------------------------------------------");
                        decimal prix_personne;
                        while (!decimal.TryParse(Console.ReadLine(), out prix_personne))
                        {
                            Console.WriteLine("Format du prix invalide.");

                        }
                        Console.WriteLine("" +
                            "                                      ---------------------------------------\n" +
                            "                                      |  Quel est la nationalité du plat ?  |\n" +
                            "                                      ---------------------------------------");
                        string nationalité = Console.ReadLine();
                        Console.WriteLine("" +
                            "                                      ------------------------------------------------------------\n" +
                            "                                      |  Le plat correspond à quel type de régime alimentaire ?  |\n" +
                            "                                      ------------------------------------------------------------");
                        string régime = Console.ReadLine();/*
            Console.WriteLine("" +
                "                                      -----------------------------------\n" +
                "                                      |  Quel est l'ID de la recette ?  |\n" +
                "                                      -----------------------------------");
            string Id_recette = Console.ReadLine();*/
                        #region RecupIdRecette
                        string requeteRecette = "select max(cast(substring(idRecette, 4) as unsigned)) from Recette;";
                        int IdmaxRecet = 0;
                        using (MySqlCommand commandeIdRecet = new MySqlCommand(requeteRecette, connecUser))
                        {
                            object resultat = commandeIdRecet.ExecuteScalar();
                            IdmaxRecet = Convert.ToInt32(resultat);
                        }
                        IdmaxRecet++;
                        string IdRecette = "REC" + IdmaxRecet.ToString("D3");
                        #endregion
                        #endregion
                        #region requete 
                        string requete = "insert into Plat (idPlat, NomPlat, TypePlat, DateFabrication, DatePeremption, NbPersonnes, PrixPersonne, Nationalité, Régime, idRecette) values (@idPlat, @NomPlat, @TypePlat, @DateFabrication, @DatePeremption, @NbPersonnes, @PrixPersonne, @Nationalité, @Régime, @idRecette);";
                        string requete2 = "insert into Recette (idRecette, NomRecette) values (@idRecette, @NomRecette)";
                        using (MySqlCommand commandeAjout = new MySqlCommand(requete2, connecUser))
                        {
                            commandeAjout.Parameters.AddWithValue("@idRecette", IdRecette);
                            commandeAjout.Parameters.AddWithValue("@NomRecette", nom_plat);
                            commandeAjout.ExecuteNonQuery();
                        }
                        using (MySqlCommand commandeAjout = new MySqlCommand(requete, connecUser))
                        {
                            commandeAjout.Parameters.AddWithValue("@idPlat", IdPlat);
                            commandeAjout.Parameters.AddWithValue("@NomPlat", nom_plat);
                            commandeAjout.Parameters.AddWithValue("@TypePlat", type_plat);
                            commandeAjout.Parameters.AddWithValue("@DateFabrication", Date_fabrication);
                            commandeAjout.Parameters.AddWithValue("@DatePeremption", Date_peremption);
                            commandeAjout.Parameters.AddWithValue("@NbPersonnes", nb_personnes);
                            commandeAjout.Parameters.AddWithValue("@PrixPersonne", prix_personne);
                            commandeAjout.Parameters.AddWithValue("@Nationalité", nationalité);
                            commandeAjout.Parameters.AddWithValue("@Régime", régime);
                            commandeAjout.Parameters.AddWithValue("@idrecette", IdRecette);
                            commandeAjout.ExecuteNonQuery();
                        }
                        string reqAjoutProp = "insert into ProposePlat ( idCuisinier, idPlat ) values (@idCuisinier, @idPlat );";
                        using (MySqlCommand comAjoutProp = new MySqlCommand(reqAjoutProp, connecUser))
                        {
                            comAjoutProp.Parameters.AddWithValue("@idCuisinier", id);
                            comAjoutProp.Parameters.AddWithValue("@idPlat", IdPlat);
                            comAjoutProp.ExecuteNonQuery();
                        }
                        #endregion
                        #region questionSurIngrédiens
                        Console.WriteLine("");
                        Console.Clear();
                        Console.WriteLine("" +
                        "                                      --------------------------------------------------------------------------------\n" +
                        "                                      |  Pouvez vous spécifier les ingrédiens ? (format ingrédient(quantité), ... )  |\n" +
                        "                                      --------------------------------------------------------------------------------\n");
                        string[] IngreQuant = Console.ReadLine().Split(',');
                        string[] ingredient = new string[IngreQuant.Length];
                        string[] quantite = new string[IngreQuant.Length];
                        for (int i = 0; i < IngreQuant.Length; i++)
                        {
                            IngreQuant[i] = IngreQuant[i].Trim();
                            int indexG = IngreQuant[i].IndexOf('(');
                            int indexD = IngreQuant[i].IndexOf(")");
                            if (indexG > 0 && indexD > indexG)
                            {
                                ingredient[i] = IngreQuant[i].Substring(0, indexG).Trim();
                                quantite[i] = IngreQuant[i].Substring(indexG + 1, indexD - indexG - 1).Trim();
                            }
                            else
                            {
                                Console.WriteLine(
                        "                                      -------------------------------------\n" +
                        "                                      |  Le format entré n'est pas bon !  |\n" +
                        "                                      -------------------------------------\n");
                            }
                        }
                        string requeteIngredient = "insert into ComposePlat (idPlat, idIngrédient, QuantiteIngredient) values (@idPlat, @idIngrédient , @QuantiteIngredient); ";
                        for (int i = 0; i < IngreQuant.Length; i++)
                        {
                            #region RecupIDingrédient
                            string idIng = "";
                            string requeteVerif = "select idIngrédient from Ingrédient where NomIngrédient = @NomIngrédient;";
                            using (MySqlCommand commandeVerif = new MySqlCommand(requeteVerif, connecUser))
                            {
                                commandeVerif.Parameters.AddWithValue("NomIngrédient", ingredient[i]);
                                object retour = commandeVerif.ExecuteScalar();
                                if (retour != null)
                                {
                                    idIng = retour.ToString();
                                }
                                else
                                {
                                    string requeteIngrédient = "select max(cast(substring(idIngrédient, 4) as unsigned )) from Ingrédient;";
                                    int IdmaxIng = 0;
                                    using (MySqlCommand commandeIdIng = new MySqlCommand(requeteIngrédient, connecUser))
                                    {
                                        object reretour = commandeIdIng.ExecuteScalar();
                                        IdmaxIng = Convert.ToInt32(reretour);
                                    }
                                    IdmaxIng++;
                                    idIng = "ING" + IdmaxIng.ToString("D3");
                                    string ajoutIngrédient = "insert into Ingrédient (idIngrédient, NomIngrédient, VolumeIngrédient) values (@idIngrédient, @NomIngrédient, @Volumeingrédient);";
                                    using (MySqlCommand commandeAjout = new MySqlCommand(ajoutIngrédient, connecUser))
                                    {
                                        commandeAjout.Parameters.AddWithValue("@idIngrédient", idIng);
                                        commandeAjout.Parameters.AddWithValue("@NomIngrédient", ingredient[i]);
                                        commandeAjout.Parameters.AddWithValue("@VolumeIngrédient", quantite[i]);
                                        commandeAjout.ExecuteNonQuery();
                                    }
                                }
                            }
                            #endregion
                            using (MySqlCommand commandeIngredient = new MySqlCommand(requeteIngredient, connecUser))
                            {
                                commandeIngredient.Parameters.AddWithValue("@idPlat", IdPlat);
                                commandeIngredient.Parameters.AddWithValue("@idIngrédient", idIng);
                                commandeIngredient.Parameters.AddWithValue("@QuantiteIngredient", quantite[i]);
                                commandeIngredient.ExecuteNonQuery();
                            }
                        }
                        Console.WriteLine(
                         "                                      -------------------------------------------\n" +
                         "                                      |  Les ingrédients ont été sauvegardés !  |\n" +
                         "                                      -------------------------------------------\n");
                    }
                    #endregion
                    else
                    {
                        Console.WriteLine("" +
                            "                            -------------------------------------------------------------------\n" +
                            "                            |  Pour le bien de nos consommateurs,                             |\n" +
                            "                            |  nous ne pouvons pas vous laisser ajouter ce que vous voulez !  |\n" +
                            "                            -------------------------------------------------------------------");
                    }
                    Console.WriteLine("" +
                            "                                      --------------------------------------------\n" +
                            "                                      |  Souhaitez vous ajouter un autre plat ?  |\n" +
                            "                                      --------------------------------------------\n");
                    string reponse = Console.ReadLine().ToLower().Trim();
                    if (reponse != "oui" && reponse != "yes" && reponse != "o")
                    {
                        continuer = false;
                    }
                }

            }


            ///Nous donnons la possibilité au cuisinier de mettre à jour les caractéristiques d'un plat déjà existant et présent dans le catalogue du cuisinier. 
            ///On affiche la liste des plats que le cuisinier propose, avec l'id du plat, l'utilisateur sélectionne le plat qu'il souhaite modifier. On lis les valeurs déjà présentent dans le plat puis on demande 
            ///les nouvelles informations au cuisinier. Si le champ est laissé vide, les informations sont conservées sinon elles seront rajoutés à la place des anciennes.
            void ModifierPlat(MySqlConnection connecUser)
            {
                Console.Clear();
                Console.WriteLine(
                    "                                      -------------------------------------------------------------\n" +
                    "                                      |             MODIFICATION D'UN PLAT EXISTANT               |\n" +
                    "                                      ------------------------------------------------------------------------");
                string listplat = "select p.idPlat, p.NomPlat, p.TypePlat, p.PrixPersonne, p.NbPersonnes from Plat p join ProposePlat pp on p.idPlat = pp.idPlat where pp.idCuisinier = @idCuisinier;";
                using (var commandelist = new MySqlCommand(listplat, connecUser))
                {
                    commandelist.Parameters.AddWithValue("@idCuisinier", id);
                    using (var lecture = commandelist.ExecuteReader())
                    {
                        if (!lecture.HasRows)
                        {
                            Console.WriteLine(
                    "                                      -------------------------------------------------------------\n" +
                    "                                      |                 Pas de plat à modifier                    |\n" +
                    "                                      -------------------------------------------------------------");
                            return;
                        }
                        Console.WriteLine(
                    "                                      | ID du plat    |     Nom     |     Type     | Prix/pers   | Nb pers   |");
                        while (lecture.Read())
                        {
                            var listeid = lecture["idPlat"].ToString();
                            var listenom = lecture["NomPlat"].ToString();
                            var listetype = lecture["TypePlat"].ToString();
                            var listeprix = lecture["PrixPersonne"].ToString();
                            var listenbPers = lecture["NbPersonnes"].ToString();
                            Console.WriteLine("                                      | " +
                        listeid.PadRight(13) + " | " +
                        listenom.PadRight(11) + " | " +
                        listetype.PadRight(12) + " | " +
                        listeprix.PadLeft(11) + " | " +
                        listenbPers.PadLeft(9) + " |");
                        }
                    }
                }
                Console.WriteLine(
                    "                                      ------------------------------------------------------------------------\n" +
                    "                                      |                 Entrez l'ID du plat à modifier :          |\n" +
                    "                                      -------------------------------------------------------------");
                string platid = Console.ReadLine().Trim().ToUpper();
                Console.Clear();
                string chopeinfo = "select NomPlat, TypePlat, PrixPersonne, NbPersonnes from Plat where idPlat = @idPlat;";
                string nvnom = ""; string nvtype = ""; decimal nvprix; int nvnbPers;
                using (var commandechope = new MySqlCommand(chopeinfo, connecUser))
                {
                    commandechope.Parameters.AddWithValue("@idPlat", platid);
                    using (var re = commandechope.ExecuteReader())
                    {
                        if (!re.Read())
                        {
                            Console.WriteLine(
                    "                                      ------------------------------------------------\n" +
                    "                                      |                 Plat non trouvé              |\n" +
                    "                                      ------------------------------------------------");
                            return;
                        }
                        nvnom = re.GetString("NomPlat");
                        nvtype = re.GetString("TypePlat");
                        nvprix = re.GetDecimal("PrixPersonne");
                        nvnbPers = re.GetInt32("NbPersonnes");
                    }
                }
                Console.WriteLine("                                      |                 Nom : " + nvnom + "              |");
                var s = Console.ReadLine().Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    nvnom = s;
                }
                Console.WriteLine("                                      |                 Type du plat : " + nvtype + "              |");
                s = Console.ReadLine().Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    nvtype = s;
                }
                Console.WriteLine("                                      |                 Prix par personne : " + nvprix + "              |");
                s = Console.ReadLine().Trim();
                if (decimal.TryParse(s, out var dp))
                {
                    nvprix = dp;
                }
                Console.WriteLine("                                      |                 Nombre de personnes : " + nvnbPers + "              |");
                s = Console.ReadLine().Trim();
                if (int.TryParse(s, out var ip))
                {
                    nvnbPers = ip;
                }

                string maj = "update Plat set NomPlat = @nom, TypePlat = @type, PrixPersonne = @prix , NbPersonnes = @nb where idPlat = @id;";
                using (var commandemaj = new MySqlCommand(maj, connecUser))
                {
                    commandemaj.Parameters.AddWithValue("@nom", nvnom);
                    commandemaj.Parameters.AddWithValue("@type", nvtype);
                    commandemaj.Parameters.AddWithValue("@prix", nvprix);
                    commandemaj.Parameters.AddWithValue("@nb", nvnbPers);
                    commandemaj.Parameters.AddWithValue("@id", platid);
                    commandemaj.ExecuteNonQuery();
                }
                Console.Clear();
                Console.WriteLine(
                          "                                      -----------------------------------\n" +
                          "                                      |   Plat modifié avec succès !    |\n" +
                          "                                      -----------------------------------");
            }


            ///Le client autant Particulier ou Entreprise peut modifier la date de livraison et l'adresse d'arrivée d'une commande existante. 
            ///On récupère la liste des commandes du client et on les affiches avec l'id de la commande. 
            ///En fonction de l'id de la commande qui est sélectionné par l'utilisateur, on renseigne les informations actuelles de la commande et on propose la saisie de nouvelles informations au client. 
            ///De manière optionnelle, on propose de modifier la station de métro la plus proche du client si il n'est pas à la localisation initialement présente dans le compte.
            ///Ainsi, on met à jour les données. 
            void ModifierCommande(MySqlConnection connecUser)
            {
                int n1 = 12; int n2 = 12; int n3 = 12; int n4 = 20;
                Console.Clear();
                Console.WriteLine(
            "                                      -------------------------------------------------------------\n" +
            "                                      |             MODIFICATION D'UNE COMMANDE EXISTANTE         |\n" +
            "                                      -------------------------------------------------------------");
                Console.WriteLine(
                "                                      |ID commande".PadRight(n1) + " | Date cmd.".PadRight(n2) + " | Date livr.".PadRight(n3) + " | Adr. arrivée".PadRight(n4) + "   |");
                string reqlist = "select c.idCommande, c.DateCommande, l.DateLivraison, l.AdresseArrivée from CommandeTransactions c join LigneCommande l on c.idCommande = l.idCommande where c.idClient = ( select idClient from Client where idCompte = @idCompte)";
                using (var commandelist = new MySqlCommand(reqlist, connecUser))
                {
                    commandelist.Parameters.AddWithValue("@idCompte", id);
                    using (var fd = commandelist.ExecuteReader())
                    {
                        if (!fd.HasRows)
                        {
                            Console.WriteLine(
            "                                      -------------------------------------------------------------\n" +
            "                                      |                 AUCUNE COMMANDE À MODIFIER                |\n" +
            "                                      -------------------------------------------------------------");
                            return;
                        }
                        while (fd.Read())
                        {
                            string idCom = fd["idCommande"].ToString();
                            string dateCom = Convert.ToDateTime(fd["DateCommande"]).ToString("yyyy-MM-dd");
                            string dateliv = Convert.ToDateTime(fd["DateLivraison"]).ToString("yyyy-MM-dd");
                            string adrArr = fd["AdresseArrivée"].ToString();
                            Console.WriteLine(
                        "                                      | " + idCom.PadRight(n1 - 2) + " | " + dateCom.PadRight(n2 - 3) + " | " + dateliv.PadRight(n3 - 2) + " | " + adrArr.PadRight(n4 - 2) + " |");
                        }

                    }
                }
                Console.WriteLine(
            "                                      -------------------------------------------------------------\n" +
            "                                      |     Entrez l'ID de la commande à modifier :               |\n" +
            "                                      -------------------------------------------------------------");
                string choix = Console.ReadLine().Trim().ToUpper();
                Console.Clear();
                string vachercher = "select l.DateLivraison, l.AdresseArrivée from LigneCommande l  join CommandeTransactions c on l.idCommande = c.idCommande where l.idCommande = @idCommande  and c.idClient = (select idClient from Client where idCompte = @idCompte);";
                DateTime dateLiv;
                string adresseArr = "";
                using (var comvachercher = new MySqlCommand(vachercher, connecUser))
                {
                    comvachercher.Parameters.AddWithValue("@idCommande", choix);
                    comvachercher.Parameters.AddWithValue("@idCompte", id);
                    using (var fp = comvachercher.ExecuteReader())
                    {
                        if (!fp.Read())
                        {
                            Console.WriteLine(
            "                                      ------------------------------------------------\n" +
            "                                      |                 Commande non trouvée         |\n" +
            "                                      ------------------------------------------------");
                            return;
                        }
                        dateLiv = fp.GetDateTime("DateLivraison");
                        adresseArr = fp.GetString("AdresseArrivée");
                    }
                }
                Console.WriteLine(
            "                                      ------------------------------------------------\n" +
            "                                      |    Date de livraison actuelle : " + dateLiv.ToString("yyyy-MM-dd") + "   |\n" +
            "                                      ------------------------------------------------\n");
                var s = Console.ReadLine().Trim();
                Console.WriteLine();
                if (DateTime.TryParse(s, out var nvDate))
                {
                    dateLiv = nvDate;
                }
                Console.WriteLine(
            "                                      ------------------------------------------------\n" +
            "                                      |    Adresse d'arrivée actuelle : " + adresseArr.PadRight(n4 - 9) + "  |\n" +
            "                                      ------------------------------------------------\n");
                s = Console.ReadLine().Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    adresseArr = s;
                }

                string maj = "update LigneCommande set DateLivraison = @dateLiv, AdresseArrivée = @adrArr where idCommande = @idCommande;";
                using (var commaj = new MySqlCommand(maj, connecUser))
                {
                    commaj.Parameters.AddWithValue("@dateLiv", dateLiv);
                    commaj.Parameters.AddWithValue("@adrArr", adresseArr);
                    commaj.Parameters.AddWithValue("@idCommande", choix);
                    commaj.ExecuteNonQuery();
                }
                Console.Clear();
                Console.WriteLine(
                    "                                      ------------------------------------------------------------------------\n" +
                    "                                      |   Voulez-vous mettre à jour votre station de métro la plus proche ?  |\n" +
                    "                                      ------------------------------------------------------------------------");
                string repMetro = Console.ReadLine().Trim().ToLower();
                if (repMetro == "o" || repMetro == "oui" || repMetro == "yes" ||repMetro == "y")
                {
                    GrapheMetro.SommaireMetro(matrice_infoStation);
                    Console.WriteLine(
                    "                                      ---------------------------------------------------\n" +
                    "                                      |   Entrez le numéro de votre nouvelle station :  |\n" +
                    "                                      ---------------------------------------------------");
                    if (int.TryParse(Console.ReadLine(), out int newMetro))
                    {
                        const string majCompte = " update Compte set MetroLePlusProche = @metro where idCompte = @idCompte;";
                        using (var comMajCompte = new MySqlCommand(majCompte, connecUser))
                        {
                            comMajCompte.Parameters.AddWithValue("@metro", newMetro);
                            comMajCompte.Parameters.AddWithValue("@idCompte", id);
                            comMajCompte.ExecuteNonQuery();
                        }
                        Console.WriteLine(" Votre station de métro a été mise à jour.");
                    }
                    else
                    {
                        Console.WriteLine(" le métro n’a pas été modifié.");
                    }
                }
                Console.WriteLine(
                    "                                      --------------------------------------\n" +
                    "                                      |   Commande modifiée avec succès !  |\n" +
                    "                                      --------------------------------------");
            }


            ///Pour les cuisiniers, ils peuvent consulter le détail des livraisons liées à ses plats. Si ils le souhaitent, on peut leur fournir le plus court trajet en métro.
            ///On lui donne une image du trajet à faire, la liste des stations ainsi que les changements de lignes et le temps total du trajet.
            ///On affiche les plats qu'ils proposent, puis avec l'id du plat qui est renseigné, on lui fournit les informations liées à ce dernier. 
            ///Ensuite, si les cuisiniers le souhaitent, on récupére la station de métro présente dans le compte du cuisinier et celle renseigné par le client quand la commande a été faite. 
            ///Donc, grâce à ces stations, on lance l'algorithme qui détermine le chemin le plus court qui a été sélectionné par l'utilisateur. 
            void InfoLivraison(MySqlConnection connecUser)
            {
                bool continier = true;
                while (continier)
                {
                    Console.Clear();
                    int depmetro;
                    using (var commande = new MySqlCommand("select MetroLePlusProche from Compte where idCompte = @id;", connecUser))
                    {
                        commande.Parameters.AddWithValue("@id", id);
                        depmetro = Convert.ToInt32(commande.ExecuteScalar());
                    }
                    string requeteListePlat = "select pl.idPLat, pl.NomPlat from Plat pl join ProposePlat pp on pl.idPlat = pp.idPlat where pp.idCuisinier = @idCuisinier;";
                    using (MySqlCommand commandeListe = new MySqlCommand(requeteListePlat, connecUser))
                    {
                        commandeListe.Parameters.AddWithValue("@idCuisinier", id);
                        using (MySqlDataReader LectureListe = commandeListe.ExecuteReader())
                        {
                            if (LectureListe.HasRows)
                            {
                                Console.WriteLine("" +
                            "                                      -----------------------------------\n" +
                            "                                      |  Voici la liste de vos plats :  |\n" +
                            "                                      -----------------------------------\n");
 
                                while (LectureListe.Read())
                                {
                                    string IdPlat = LectureListe["idPlat"].ToString();
                                    string nomPlat = LectureListe["NomPlat"].ToString();
                                    Console.WriteLine("" +
                            "                                      -------------------------------------------------------------------\n" +
                            "                                      |  ID du plat : " + IdPlat + "  |  Nom du plat : " + nomPlat + "  |\n" +
                            "                                      -------------------------------------------------------------------\n");
                                }
                            }
                            else
                            {
                                Console.WriteLine("" +
                            "                                      ----------------------------------\n" +
                            "                                      |  Aucun plat lié à votre compte |\n" +
                            "                                      ----------------------------------\n");
                            }
                        }
                    }
                    Console.WriteLine("" +
                            "                                      ----------------------------------------------------\n" +
                            "                                      |  Entrez l'ID du plat pour afficher la livraison  |\n" +
                            "                                      ----------------------------------------------------\n");
                    string idPlat = Console.ReadLine().Trim().ToUpper();
                    string requeteLivraison = "select idLigneCommande, DateLivraison, AdresseDépart, AdresseArrivée, MetroDestination from LigneCommande where idPlat = @idPlat;";
                    using (MySqlCommand commandeRecupInfo = new MySqlCommand(requeteLivraison, connecUser))
                    {
                        commandeRecupInfo.Parameters.AddWithValue("@idPlat", idPlat);
                        using (MySqlDataReader reader = commandeRecupInfo.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                Console.Write("" +
                            "                                      ------------------------------------------------------------\n" +
                            "                                      |  Information de livraison pour le plat " + idPlat + " :  |\n" +
                            "                                      ------------------------------------------------------------\n");
                                while (reader.Read())
                                {
                                   string idLigne = reader["idLigneCommande"].ToString();
                                   DateTime dateLivraison = Convert.ToDateTime(reader["DateLivraison"]);
                                   string adresseDepart = reader["AdresseDépart"].ToString();
                                   string adresseArrivée = reader["AdresseArrivée"].ToString();
                                    int destMetro = reader.GetInt32("MetroDestination");

                                    Console.WriteLine("                                      |  ID Ligne       :           |" + idLigne);
                                    Console.WriteLine("                                      |  Date Livraison :           |" + dateLivraison.ToString("yyyy-MM-dd"));
                                    Console.WriteLine("                                      |  Adresse départ :           |" + adresseDepart);
                                    Console.WriteLine("                                      |  Adresse arrivée :          |" + adresseArrivée);
                                    Console.WriteLine("                                      ---------------------------------------------------------------");

                                    Console.WriteLine("" +
                           "                                      ---------------------------------------------------------------------\n" +
                           "                                      |  Souhaitez vous avoir le chemin le plus court pour la livraison ? |\n" +
                           "                                      ---------------------------------------------------------------------\n");
                                    string rep = Console.ReadLine().Trim().ToLower();
                                    if (rep == "yes" || rep == "y" || rep == "oui" || rep == "o")
                                    {
                                        Console.WriteLine("" +
                        "                                      -------------------------------------------------------------\n" +
                        "                                      | Cher cuisinier                                            |\n" +
                        "                                      | Tapez 1 pour l'algorithme de Dijkstra !                   |\n" +
                        "                                      | Tapez 2 pour l'algorithme de Floyd-Warshall !             |\n" +
                        "                                      | Tapez 3 pour l'algorithme de Bellman-Ford !               |\n" +
                        "                                      -------------------------------------------------------------");
                                        string rerep = Console.ReadLine().Trim();
                                        switch (rerep)
                                        {
                                            case "1":
                                                var PCC1 = GrapheMetro.AlgorithmeDijkstra(noeuds, MatriceAdjacence, ordre, destMetro, depmetro);
                                                Visuel visuel1 = new Visuel(noeudsVisuel, matrice_relation);    /// On crée un visuel à partir de la classe Visuel
                                                visuel1.GenererCarteAvecChemin("PlanMetro.png", PCC1);    /// On appelle la fonction GenererCarteAvecChemin qui affichera une image représentant le plan du métro avec le PCC entre deux stations
                                                break;
                                            case "2":
                                                var PCC2 = GrapheMetro.AlgorithmeFloydWarshall(MatriceAdjacence, ordre, noeuds, destMetro, depmetro);
                                                Visuel visuel2 = new Visuel(noeudsVisuel, matrice_relation);    /// On crée un visuel à partir de la classe Visuel
                                                visuel2.GenererCarteAvecChemin("PlanMetro.png", PCC2);    /// On appelle la fonction GenererCarteAvecChemin qui affichera une image représentant le plan du métro avec le PCC entre deux stations
                                                break;
                                            case "3":
                                                var PCC3 = GrapheMetro.AlgorithmeBellmanFord(noeuds, ordre, MatriceAdjacence, destMetro, depmetro);
                                                Visuel visuel3 = new Visuel(noeudsVisuel, matrice_relation);    /// On crée un visuel à partir de la classe Visuel
                                                visuel3.GenererCarteAvecChemin("PlanMetro.png", PCC3);    /// On appelle la fonction GenererCarteAvecChemin qui affichera une image représentant le plan du métro avec le PCC entre deux stations
                                                break;
                                            default:
                                                Console.WriteLine("" +
                                "                                      -----------------------------\n" +
                                "                                      | Algorithme non disponible |\n" +
                                "                                      -----------------------------\n");
                                                break;
                                        }
                                    }
                                }
                               
                            }
                        
                    
                            else
                            {
                                Console.WriteLine("" +
                            "                                      --------------------------------------------------------------------------\n" +
                            "                                      |  Aucune information de livraison trouvée pour le plat" + idPlat + ".  |\n" +
                            "                                      --------------------------------------------------------------------------\n");
                            }
                        }
                    }
                    Console.WriteLine("" +
                            "                                      ----------------------------------------------\n" +
                            "                                      |  Souhaitez vous consulter un autre plat ?  |\n" +
                            "                                      ----------------------------------------------\n");
                    string réponse = Console.ReadLine().ToLower().Trim();
                    if (réponse != "oui" && réponse != "yes" && réponse != "o")
                    {
                        continier = false;
                    }
                }

            }


            ///Un client ( particulier ou entreprise ) peut accéder à la carte des plats disponibles et voir leurs détails.
            ///Ains, il peut consulter les ingrédients, la recette etc... 
            ///Si il est satisfait par un plat, il peut passer commande en renseignant des informations.
            ///On affiche les plats disponibles, si le client le veux, il peut accéder à la composition de ces derniers. 
            ///Si le client comfirme une commande, on recherche l'id du cuisinier qui correspond au plat ( ProposePlat ) et on génère un nouvel id d'une commande et d'une ligne de commande.
            ///Ensuite, il faut saisir les dates et l'adresse d'arrivée. L'adresse de départ est récupérée depuis le compte du cuisinier. 
            ///On renseigne enfin les informations dans les tables CommandeTransactions et LigneCommande. 
            void ListePlat(MySqlConnection connecUser)
            {
                Console.Clear();
                string requeteListe = "select idPlat, NomPlat, TypePlat, PrixPersonne, NbPersonnes from Plat;";
                using (MySqlCommand commandeListe = new MySqlCommand(requeteListe, connecUser))
                {
                    using (MySqlDataReader lu = commandeListe.ExecuteReader())
                    {
                        if (lu.HasRows)
                        {
                            Console.WriteLine("" +
                        "                                ----------------------------------\n" +
                        "                                | Liste des plats disponibles :  |\n" +
                        "                                --------------------------------------------------------------------------------------\n" +
                        "                                | ID du plat | Nom du plat | type du plat | Prix du plat | Pour combien de personnes |\n" +
                        "                                --------------------------------------------------------------------------------------");
                            while (lu.Read())
                            {
                                string idPlat = lu["idPlat"].ToString();
                                string NomPlat = lu["NomPlat"].ToString();
                                string typePlat = lu["TypePlat"].ToString();
                                string prix = lu["PrixPersonne"].ToString();
                                string NbPersonne = lu["NbPersonnes"].ToString();
                                Console.WriteLine("" +
                                    "                                | " + idPlat + " | " + NomPlat + " | " + typePlat + " | " + prix + " | " + NbPersonne + " |");

                            }
                            Console.WriteLine("                                --------------------------------------------------------------------------------------");
                        }
                        else
                        {
                            Console.WriteLine("" +
                        "                                           ------------------------------\n " +
                        "                                           |    Aucun plat disponible   |\n" +
                        "                                           ------------------------------");
                        }
                    }
                }
                Console.WriteLine("" +
                        "                                           --------------------------------------------\n " +
                        "                                          |    Souhaitez vous passer une commande ?  |\n" +
                        "                                           --------------------------------------------");
                string réponse = Console.ReadLine().ToLower().Trim();
                if (réponse == "oui" || réponse == "yes" || réponse == "o")
                {
                    Console.WriteLine("" +
                        "                                           --------------------------------------------\n " +
                        "                                          |  Entrez l'ID du plat qui vous intéresse  |\n" +
                        "                                           --------------------------------------------");
                    string IdplatVoulu = Console.ReadLine().ToUpper().Trim();
                    Console.WriteLine("");
                    Console.Clear();
                    Console.WriteLine("" +
                        "                                           -----------------------------------------\n " +
                        "                                          |    Voulez vous voir les ingrédients   |\n" +
                        "                                           -----------------------------------------");
                    string reponse = Console.ReadLine().ToLower().Trim();
                    if (reponse == "oui" || reponse == "yes" || reponse == "o")
                    {
                        string requeteIngrédient = "select i.NomIngrédient, c.QuantiteIngredient from ComposePlat c join Ingrédient i on c.idIngrédient = i.idIngrédient where c.idPlat = @idPlat;";
                        using (MySqlCommand commandeIngrédient = new MySqlCommand(requeteIngrédient, connecUser))
                        {
                            commandeIngrédient.Parameters.AddWithValue("idPlat", IdplatVoulu);
                            using (MySqlDataReader luIngrédient = commandeIngrédient.ExecuteReader())
                            {
                                if (luIngrédient.HasRows)
                                {
                                    Console.WriteLine("" +
                        "                                           -------------------------------------------\n " +
                        "                                           |    Ingrédient pour le plat : " + IdplatVoulu + "  |\n" +
                        "                                           -------------------------------------------");

                                    while (luIngrédient.Read())
                                    {
                                        string nomIngrédient = luIngrédient["NomIngrédient"].ToString();
                                        string quantité = luIngrédient["QuantiteIngredient"].ToString();
                                        Console.WriteLine("" +
                            "                                           |    " + nomIngrédient + "  |  " + quantité + "  |\n" +
                            "                                           --------------------------------------------");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("" +
                        "                                           -------------------------------------------\n " +
                        "                                           |   Aucun ingrédient trouvé pour ce plat  |\n" +
                        "                                           -------------------------------------------");
                                }
                            }
                        }
                    }
                    Console.WriteLine("" +
                    "                                           ------------------------------------------------\n " +
                    "                                           |    Souhaitez vous valider votre commande ?   |\n" +
                    "                                           ------------------------------------------------");
                    string conf = Console.ReadLine().ToLower().Trim();
                    if (conf == "oui" || conf == "yes" || conf == "o")
                    {
                        string idCuisinier = "";
                        string requeteInfoCuisinier = "select idCuisinier from ProposePlat where idPlat = @idPlat";
                        using (MySqlCommand commandeInfoCuisinier = new MySqlCommand(requeteInfoCuisinier, connecUser))
                        {
                            commandeInfoCuisinier.Parameters.AddWithValue("@idPlat", IdplatVoulu);
                            object retour = commandeInfoCuisinier.ExecuteScalar();
                            if (retour != null)
                            {
                                idCuisinier = retour.ToString();
                            }
                            else
                            {
                                idCuisinier = "Mystère";
                            }
                        }
                        string idCommande = "";
                        string requeteIdCommande = "select max(cast(substring(idCommande, 4) as unsigned)) from CommandeTransactions;";
                        int IdMaxCommande = 0;
                        using (MySqlCommand commandeIdCommande = new MySqlCommand(requeteIdCommande, connecUser))
                        {
                            object retour = commandeIdCommande.ExecuteScalar();
                            IdMaxCommande = Convert.ToInt32(retour) + 1;
                        }
                        idCommande = "CMD" + IdMaxCommande.ToString("D3");
                        Console.WriteLine("");
                        Console.Clear();
                        Console.WriteLine("" +
                        "                                           ---------------------------------------------------------\n " +
                        "                                           |    Entrez la date de commande ( format YYYY-MM-DD ) : |\n" +
                        "                                           ---------------------------------------------------------");
                        DateTime dateCommande = DateTime.Parse(Console.ReadLine());
                        Console.WriteLine("" +
                        "                                           ----------------------------------------------------------\n " +
                        "                                           |    Entrez la date de livraison ( format YYYY-MM-DD ) : |\n" +
                        "                                           ----------------------------------------------------------");
                        DateTime dateLivraison = DateTime.Parse(Console.ReadLine());
                        Console.WriteLine("" +
                        "                                           -----------------------------------\n " +
                        "                                           |    Entrez l'adresse d'arrivée : |\n" +
                        "                                           -----------------------------------");
                        string AdresseArrivée = Console.ReadLine();
                        string AdresseDépart = "";
                        Console.Clear();
                        GrapheMetro.SommaireMetro(matrice_infoStation);
                        Console.WriteLine("" +
                        "                                           ----------------------------------------------------------------------\n " +
                        "                                           |  Sélectionnez le numéro de votre station de métro la plus proche : |\n" +
                        "                                           ----------------------------------------------------------------------");
                        int metrodest = Convert.ToInt32(Console.ReadLine().Trim());
                        string requeteDépart = "select Rue from Compte where idCompte = @idCuisinier";
                        using (MySqlCommand reqDep = new MySqlCommand(requeteDépart, connecUser))
                        {
                            reqDep.Parameters.AddWithValue("@idCuisinier", idCuisinier);
                            object retour = reqDep.ExecuteScalar();
                            if (retour != null)
                            {
                                AdresseDépart = retour.ToString();
                            }
                            else
                            {
                                AdresseDépart = "Inconnu";
                            }
                        }
                        string AvoirIdCLient = "select idClient from Client where idCompte = @idCompte;";
                        string idClient = "";
                        using (MySqlCommand comRecupIdClient = new MySqlCommand(AvoirIdCLient, connecUser))
                        {
                            comRecupIdClient.Parameters.AddWithValue("@idCompte", id);
                            object retour = comRecupIdClient.ExecuteScalar();
                            if (retour != null)
                            {
                                idClient = retour.ToString();
                            }
                            else
                            {
                                Console.WriteLine("Pas de clients enregistrés");
                                return;
                            }

                        }
                        string requeteMisDansCommande = "insert into CommandeTransactions(idCommande, DateCommande, idCuisinier, idClient ) values (@idCommande, @DateCommande, @idCuisinier, @idClient);";
                        using (MySqlCommand commandeMisDansCommande = new MySqlCommand(requeteMisDansCommande, connecUser))
                        {
                            commandeMisDansCommande.Parameters.AddWithValue("@idCommande", idCommande);
                            commandeMisDansCommande.Parameters.AddWithValue("@DateCommande", dateCommande);
                            commandeMisDansCommande.Parameters.AddWithValue("@idCuisinier", idCuisinier);
                            commandeMisDansCommande.Parameters.AddWithValue("@idClient", idClient);
                            commandeMisDansCommande.ExecuteNonQuery();
                        }
                        string idLigneCommande = "";
                        string reqIdLigneCom = "select max(cast(substring(idLigneCommande, 4) as unsigned)) from LigneCommande;";
                        int idmaxou = 0;
                        using (MySqlCommand comID = new MySqlCommand(reqIdLigneCom, connecUser))
                        {
                            object retour = comID.ExecuteScalar();
                            idmaxou = Convert.ToInt32(retour) + 1;
                        }
                        idLigneCommande = "LIG" + idmaxou.ToString("D3");
                        string reqAjoutTable = "insert into LigneCommande(idLigneCommande, DateLivraison, AdresseDépart, AdresseArrivée, idPlat, idCommande, MetroDestination ) values (@idLigneCommande, @DateLivraison, @AdresseDépart,@AdresseArrivée, @idPlat, @idCommande, @MetroDestination);";
                        using (MySqlCommand comPlaisir = new MySqlCommand(reqAjoutTable, connecUser))
                        {
                            comPlaisir.Parameters.AddWithValue("@idLigneCommande", idLigneCommande);
                            comPlaisir.Parameters.AddWithValue("@DateLivraison", dateLivraison);
                            comPlaisir.Parameters.AddWithValue("@AdresseDépart", AdresseDépart);
                            comPlaisir.Parameters.AddWithValue("@AdresseArrivée", AdresseArrivée);
                            comPlaisir.Parameters.AddWithValue("@idPlat", IdplatVoulu);
                            comPlaisir.Parameters.AddWithValue("@idCommande", idCommande);
                            comPlaisir.Parameters.AddWithValue("@MetroDestination", metrodest);
                            comPlaisir.ExecuteNonQuery();
                        }
                        Console.WriteLine("" +
                        "                                           ---------------------------\n " +
                        "                                           |    Commande validée !   |\n" +
                        "                                           ---------------------------");
                    }
                    else
                    {
                        Console.WriteLine("" +
                       "                                           ----------------------------------------------------------------------\n " +
                       "                                           |    On espère que vous trouverez votre bonheur la prochaine fois !  |\n" +
                       "                                           ----------------------------------------------------------------------");
                    }
                }
                else
                {
                    Console.WriteLine("" +
                        "                                           ----------------------------------------------------------------------\n " +
                        "                                           |    On espère que vous trouverez votre bonheur la prochaine fois !  |\n" +
                        "                                           ----------------------------------------------------------------------");
                }
            }


            ///L'utilisateur qui est un client peut laisser un avis sur une commande déjà passée. Si il a déjà mis un avis sur la commande, l'avis est mis à jour.
            ///On récupère l'id du client puis on lis et affiche l'ensemble des commandes passées par ce client. Ainsi, grâce à la liste, il peut choisir une commande à noter.
            ///Il faut saisir une date de retour puis un commentaire avec au maximum 100 caractères. 
            ///Dans l'ajout dans la base de données, on différencie la commande en fonction de l'existence ou non d'un retour déjà présent. 
            void AvisPlat(MySqlConnection connecUser)
            {
                Console.WriteLine("");
                Console.Clear();
                string idClient = "";
                string reqIdClient = " select idClient from Client where idCompte = @idCompte";
                using (MySqlCommand comIdClient = new MySqlCommand(reqIdClient, connecUser))
                {
                    comIdClient.Parameters.AddWithValue("@idCompte", id);
                    object retour = comIdClient.ExecuteScalar();
                    if (retour != null)
                    {
                        idClient = retour.ToString();
                    }
                    else
                    {
                        Console.WriteLine("" +
                        "                                           --------------------------------------------------------------\n " +
                        "                                           |    Le client n'est pas trouvé dans notre base de données   |\n" +
                        "                                           --------------------------------------------------------------");
                        return;
                    }
                }
                List<string> idCommande = new List<string>();
                List<string> idCuisinier = new List<string>();
                List<string> idPlat = new List<string>();
                List<string> NomPlat = new List<string>();
                List<DateTime> datesCommande = new List<DateTime>();
                string reqInfo = "select c.idCommande, c.idCuisinier, l.idPlat, p.NomPlat, c.DateCommande from CommandeTransactions c join LigneCommande l on c.idCommande = l.idCommande join Plat p on l.idPlat = p.idPlat where c.idClient = @idClient;";
                using (MySqlCommand comRecupInfo = new MySqlCommand(reqInfo, connecUser))
                {
                    comRecupInfo.Parameters.AddWithValue("@idClient", idClient);
                    using (MySqlDataReader lect = comRecupInfo.ExecuteReader())
                    {
                        while (lect.Read())
                        {
                            idCommande.Add(lect["idCommande"].ToString());
                            idCuisinier.Add(lect["idCuisinier"].ToString());
                            idPlat.Add(lect["idPlat"].ToString());
                            NomPlat.Add(lect["NomPlat"].ToString());
                            datesCommande.Add(Convert.ToDateTime(lect["DateCommande"]));
                        }
                    }
                }
                if (idCommande.Count == 0)
                {
                    Console.WriteLine("" +
                        "                                           --------------------------------------------------------\n " +
                        "                                           |    Vous n'avez pas fait de commande pour l'instant   |\n" +
                        "                                           --------------------------------------------------------");
                }
                else
                {
                    Console.WriteLine("" +
                        "                                           ------------------------------------------\n " +
                        "                                           |    Voici la liste de vos commandes :   |\n" +
                        "                                           ------------------------------------------");
                    for (int i = 0; i < idCommande.Count; i++)
                    {
                        Console.WriteLine("" +
                        "                                            |   Commande " + idCommande[i] + " |  Plat : " + NomPlat[i] + " | Date : " + datesCommande[i].ToString("yyyy-MM-DD") + " |");
                    }
                    Console.WriteLine("" +
                        "                                           ----------------------------------------------------------------------------\n " +
                        "                                           |    Quel commande souhaitez vous noter ? Renseignez l'id de la commande   |\n" +
                        "                                           ----------------------------------------------------------------------------");
                    string choix = Console.ReadLine().ToUpper().Trim();
                    int index = idCommande.IndexOf(choix);
                    if (index == -1)
                    {
                        Console.WriteLine("" +
                        "                                           ------------------------------------------------\n " +
                        "                                           |    L'id saisi ne correspond pas à un plat    |\n" +
                        "                                           ------------------------------------------------");
                    }
                    else
                    {
                        Console.WriteLine("" +
                        "                                           ----------------------------------------------------------------------------\n " +
                        "                                           |    Entrez la date de votre retour sur la commande (format YYYY-MM-DD )   |\n" +
                        "                                           ----------------------------------------------------------------------------");
                        DateTime dateRetour = Convert.ToDateTime(Console.ReadLine());
                        Console.WriteLine("" +
                        "                                           -------------------------------------------------\n " +
                        "                                           |    Entrez votre avis ( max 100 caractères )   |\n" +
                        "                                           -------------------------------------------------");
                        string avis = Console.ReadLine();
                        if (avis.Length > 100)
                        {
                            avis = avis.Substring(0, 100);
                        }
                        string reqVerifAvis = "select count(*) from FaitRetour where idClient = @idClient and idCommande = @idCommande;";
                        int a = 0;
                        using (MySqlCommand comVerifAvis = new MySqlCommand(reqVerifAvis, connecUser))
                        {
                            comVerifAvis.Parameters.AddWithValue("idClient", idClient);
                            comVerifAvis.Parameters.AddWithValue("idCommande", idCommande[index]);
                            a = Convert.ToInt32(comVerifAvis.ExecuteScalar());
                        }
                        if (a > 0)
                        {
                            string MAJAvis = "update FaitRetour set DateRetour = @DateRetour, Commentaire = @Commentaire where idClient = @idClient and idCommande = @idCommande;";
                            using (MySqlCommand maJAvis = new MySqlCommand(MAJAvis, connecUser))
                            {
                                maJAvis.Parameters.AddWithValue("@DateRetour", dateRetour);
                                maJAvis.Parameters.AddWithValue("@Commentaire", avis);
                                maJAvis.Parameters.AddWithValue("@idClient", idClient);
                                maJAvis.Parameters.AddWithValue("@idCommande", idCommande[index]);
                                maJAvis.ExecuteNonQuery();
                                Console.WriteLine("" +
                        "                                           --------------------------\n " +
                        "                                           |    Avis mis à jour !   |\n" +
                        "                                           --------------------------");
                            }
                        }
                        else
                        {
                            string AjoutAvis = "insert into FaitRetour (idClient,idCommande, idCuisinier, DateRetour, Commentaire) values (@idClient, @idCommande, @idCuisinier, @DateRetour, @Commentaire);";
                            using (MySqlCommand ReqAjoutAvis = new MySqlCommand(AjoutAvis, connecUser))
                            {
                                ReqAjoutAvis.Parameters.AddWithValue("@idClient", idClient);
                                ReqAjoutAvis.Parameters.AddWithValue("@idCommande", idCommande[index]);
                                ReqAjoutAvis.Parameters.AddWithValue("@idCuisinier", idCuisinier[index]);
                                ReqAjoutAvis.Parameters.AddWithValue("@DateRetour", dateRetour);
                                ReqAjoutAvis.Parameters.AddWithValue("@Commentaire", avis);
                                ReqAjoutAvis.ExecuteNonQuery();
                                Console.WriteLine("" +
                        "                                           ----------------------\n " +
                        "                                           |    Avis ajouté !   |\n" +
                        "                                           ----------------------");
                            }
                        }

                    }
                }


            }


            ///On affiche simplement l'historique des commandes du client qui utilise la méthode. 
            ///On récupère l'id du client puis en fonction de ce dernier, on récupère les informations des commandes.
            ///Enfin, on les affiche.
            void CommandesPassée(MySqlConnection connecUser)
            {
                Console.Clear();
                string idClient = "";
                string reqIdClient = "select idClient from Client where idCompte = @idCompte;";
                using (MySqlCommand comIdClient = new MySqlCommand(reqIdClient, connecUser))
                {
                    comIdClient.Parameters.AddWithValue("@idCompte", id);
                    object retour = comIdClient.ExecuteScalar();
                    if (retour != null)
                    {
                        idClient = retour.ToString();
                    }
                    else
                    {
                        Console.WriteLine(
                            "                                           ----------------------------------------------\n" +
                            "                                           |    Client non trouvé dans notre base !     |\n" +
                            "                                           ----------------------------------------------");
                        return;
                    }
                }
                string reqCommande = "select c.idCommande, c.DateCommande, p.NomPlat from CommandeTransactions c join LigneCommande l on c.idCommande = l.idCommande join Plat p on l.idPlat = p.idPlat where c.idClient = @idClient order by c.DateCommande;";
                using (MySqlCommand comCommande = new MySqlCommand(reqCommande, connecUser))
                {
                    comCommande.Parameters.AddWithValue("@idClient", idClient);
                    using (MySqlDataReader la = comCommande.ExecuteReader())
                    {
                        if (la.HasRows)
                        {
                            Console.WriteLine(
                       "                                           -------------------------------------------------\n" +
                       "                                           |   Voici la liste de vos commandes passées :   |\n" +
                       "                                           -------------------------------------------------------\n" +
                       "                                           |  ID Commande  |   Date de Commande  |  Nom du plat  |\n" +
                       "                                           -------------------------------------------------------");
                            while (la.Read())
                            {
                                string idCommande = la["idCommande"].ToString();
                                DateTime dateCommande = Convert.ToDateTime(la["DateCommande"]);
                                string nomPlat = la["NomPlat"].ToString();
                                Console.WriteLine("                                           |  " + idCommande + "        |   " + dateCommande.ToString("yyyy-MM-dd") + "  |  " + nomPlat + "  |");
                            }
                            Console.WriteLine("                                           -------------------------------------------------------");
                        }
                        else
                        {
                            Console.WriteLine(
                        "                                           -------------------------------------------------\n" +
                        "                                           |   Aucune commande passée trouvée.             |\n" +
                        "                                           -------------------------------------------------");
                        }
                    }

                }
            }


            ///Ici, un menu est affiché afin que le cuisinier qui souhaite se renseigner sur ses clients, puisse spécifier comme il souhaite trier les informations. 
            ///On présente le choix puis en fonction de ce qui est renseigné par l'utilisateur, on appel la méthode Afficher avec les informations correspondantes.
            void ListeClient(MySqlConnection connecUser)
            {
                Console.Clear();
                Console.WriteLine(""+
                "                                      ---------------------------------------------------\n"+
                "                                      |     Affichage des clients selon les critères :   |\n"+
                "                                      |     1. Trier par ordre alphabétique              |\n"+
                "                                      |     2. Trier par rue                             |\n" +
                "                                      |     3. Trier par montant total des achats        |\n" +
                "                                      ---------------------------------------------------");
                string choix = Console.ReadLine().Trim();
                string colonne;
                switch (choix)
                {
                    case "1":
                        colonne = "co.Nom";
                        break;
                    case "2":
                        colonne = "co.Rue";
                        break;
                    case "3":
                        colonne = "MontantTotal";
                        break;
                    default:
                        return;
                }
                Afficher(connecUser, colonne);
                Console.ReadKey();
            }


            ///On lance une requête qui permet d'extraire et trier la liste des clients selon ce qui est passé en paramètre.
            void Afficher(MySqlConnection connecUser, string a )
            {
                string req = " SELECT cl.idClient, co.Nom, co.Prénom, co.Rue,IFNULL(SUM(pl.PrixPersonne * pl.NbPersonnes), 0) AS MontantTotal FROM Client cl JOIN Compte co ON cl.idCompte = co.idCompte LEFT JOIN CommandeTransactions ct ON cl.idClient = ct.idClient LEFT JOIN LigneCommande lc ON ct.idCommande = lc.idCommande LEFT JOIN Plat pl ON lc.idPlat = pl.idPlat GROUP BY cl.idClient, co.Nom, co.Prénom, co.Rue ORDER BY "+a+";";
                using ( var commande = new MySqlCommand(req, connecUser))
                {
                    using ( var lecture = commande.ExecuteReader())
                    {
                        Console.WriteLine(
    "                                      -------------------------------------------------------------------------------\n" +
    "                                      | ID Client | Nom          | Prénom       | Rue             | Montant Total  |\n" +
    "                                      -------------------------------------------------------------------------------");
                        while( lecture.Read())
                        {
                            var id = lecture.GetString("idClient").PadRight(9);
                            var nom = lecture.GetString("Nom").PadRight(12);
                            var prenom = lecture.GetString("Prénom").PadRight(12);
                            var rue = lecture.GetString("Rue").PadRight(15);
                            var montant = lecture.GetDecimal("MontantTotal").ToString("C2").PadLeft(8);
                            Console.WriteLine("" +
                                "                                      | " +id + " | " +nom + " | " + prenom + " | " + rue + " | " +montant+ " |\n" +
                                "                                      -------------------------------------------------------------------------------");
                        }
                    }
                }
            }
            #endregion
        }


    }

}
