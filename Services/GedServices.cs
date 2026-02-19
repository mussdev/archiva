using System.Globalization;
using anahged.Data;
using anahged.Models;
using Microsoft.EntityFrameworkCore;

namespace anahged.Services
{
    public interface ICarteService
    {
        Task<byte[]> GetFichierPdfAsync(int id);
    }
    public class GedServices
    {
        private readonly GedContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly string _storagePathAdp = "wwwroot/ADP/NewADP/";
        private readonly string _storagePathVpl = "wwwroot/SICOGI/NewSICOGI/";
        private readonly string _storagePathCartes = "wwwroot/CARTES/NewCARTES/";

        public GedServices(GedContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Example method to get all ADP records
        public IEnumerable<Adp> GetAllAdps()
        {
            return _context.Adps.Take(10).ToList();
        }

        // Methode de recherche des fichiers ADP par divers critères
        public async Task<IList<Adp>> RechercherAdpAsync(string logement, string document, string client, string boite, string code, string annee)
        {
            if (string.IsNullOrEmpty(logement) &&
                string.IsNullOrEmpty(document) &&
                string.IsNullOrEmpty(client) &&
                string.IsNullOrEmpty(boite) &&
                string.IsNullOrEmpty(code) &&
                string.IsNullOrEmpty(annee))
            {
                return new List<Adp>();
            }

            var adpQuery = _context.Adps.AsQueryable();

            if (!string.IsNullOrEmpty(logement))
            {
                adpQuery = adpQuery.Where(a => a.Logement.Contains(logement));
            }

            if (!string.IsNullOrEmpty(document))
            {
                adpQuery = adpQuery.Where(a => a.Document.Contains(document));
            }

            if (!string.IsNullOrEmpty(client))
            {
                adpQuery = adpQuery.Where(a => a.Client.Contains(client));
            }

            if (!string.IsNullOrEmpty(boite))
            {
                adpQuery = adpQuery.Where(a => a.Boite.Contains(boite));
            }

            if (!string.IsNullOrEmpty(code))
            {
                adpQuery = adpQuery.Where(a => a.Code.Contains(code));
            }

            if (!string.IsNullOrEmpty(annee))
            {
                adpQuery = adpQuery.Where(a => a.Annee.Contains(annee));
            }

            if (!string.IsNullOrEmpty(boite) && !string.IsNullOrEmpty(code))
            {
                adpQuery = adpQuery.Where(a => a.Boite.Contains(boite) && a.Code.Contains(code));
            }

            if (!string.IsNullOrEmpty(boite) && !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(logement))
            {
                adpQuery = adpQuery.Where(a => a.Boite.Contains(boite) && a.Code.Contains(code) && a.Logement.Contains(logement));
            }

            return await adpQuery.ToListAsync();
        }

        // Methode de recherche des fichiers VPL par divers critères
        public async Task<IList<Vpl>> RechercherVlpAsync(string document, string annee, string client, string logement, string boite, string code)
        {
            if (string.IsNullOrEmpty(logement) &&
                string.IsNullOrEmpty(document) &&
                string.IsNullOrEmpty(client) &&
                string.IsNullOrEmpty(boite) &&
                string.IsNullOrEmpty(annee) &&
                string.IsNullOrEmpty(code))
            {
                return new List<Vpl>();
            }

            var vplQuery = _context.Vpls.AsQueryable();

            if (!string.IsNullOrEmpty(logement))
            {
                vplQuery = vplQuery.Where(a => a.Logement.Contains(logement));
            }

            if (!string.IsNullOrEmpty(document))
            {
                vplQuery = vplQuery.Where(a => a.Document.Contains(document));
            }

            if (!string.IsNullOrEmpty(client))
            {
                vplQuery = vplQuery.Where(a => a.Client.Contains(client));
            }

            if (!string.IsNullOrEmpty(boite))
            {
                vplQuery = vplQuery.Where(a => a.Boite.Contains(boite));
            }

            if (!string.IsNullOrEmpty(annee))
            {
                vplQuery = vplQuery.Where(a => a.Annee.Contains(annee));
            }

            if (!string.IsNullOrEmpty(code))
            {
                vplQuery = vplQuery.Where(a => a.Code.Contains(code));
            }

            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(boite))
            {
                vplQuery = vplQuery.Where(a => a.Code.Contains(code) && a.Boite.Contains(boite));
            }

            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(boite) && !string.IsNullOrEmpty(logement))
            {
                vplQuery = vplQuery.Where(a => a.Code.Contains(code) && a.Boite.Contains(boite) && a.Logement.Contains(logement));
            }

            return await vplQuery.ToListAsync();
        }

        // Methode de recherche des fichiers Cartes par divers critères
        public async Task<IList<Carte>> RechercherCarteAsync(string ville, string quartier, string operation, string legende)
        {
            if (string.IsNullOrEmpty(ville) &&
                string.IsNullOrEmpty(quartier) &&
                string.IsNullOrEmpty(operation) &&
                string.IsNullOrEmpty(legende))
            {
                return new List<Carte>();
            }

            var adpQuery = _context.Cartes.AsQueryable();

            if (!string.IsNullOrEmpty(ville))
            {
                adpQuery = adpQuery.Where(a => a.Ville.Contains(ville));
            }

            if (!string.IsNullOrEmpty(quartier))
            {
                adpQuery = adpQuery.Where(a => a.Quartier.Contains(quartier));
            }

            if (!string.IsNullOrEmpty(operation))
            {
                adpQuery = adpQuery.Where(a => a.Operation.Contains(operation));
            }

            if (!string.IsNullOrEmpty(legende))
            {
                adpQuery = adpQuery.Where(a => a.Legende.Contains(legende));
            }

            return await adpQuery.ToListAsync();
        }

        // methode de lecture des cartes en format PDF
        public async Task<byte[]> GetFichierPdfAsync(int id)
        {
            var carte = await _context.Cartes.FindAsync(id);
            if (carte != null)
            {
                var cheminFichier = Path.Combine(_env.WebRootPath, carte.Lien);
                if (System.IO.File.Exists(cheminFichier))
                {
                    // return new FileContentResult(System.IO.File.ReadAllBytes(cheminFichier), "application/pdf");
                    using (var fichierPdf = new FileStream(cheminFichier, FileMode.Open))
                    {
                        var bytes = new byte[fichierPdf.Length];
                        fichierPdf.Read(bytes, 0, bytes.Length);
                        fichierPdf.Close(); // Fermer explicitement le fichier
                        return bytes;
                        // return await System.IO.File.ReadAllBytesAsync(cheminFichier);
                    }
                }
                else
                {
                    throw new Exception("Fichier non trouvé");
                }
            }
            else
            {
                throw new Exception("Carte non trouvée");
            }
        }

        // methode de lecture des ADP en format PDF
        public async Task<byte[]> GetFichierPdfAdpAsync(int id)
        {
            var adp = await _context.Adps.FindAsync(id);
            if (adp != null)
            {
                var cheminFichier = Path.Combine(_env.WebRootPath, adp.Lien);
                if (System.IO.File.Exists(cheminFichier))
                {
                    // return new FileContentResult(System.IO.File.ReadAllBytes(cheminFichier), "application/pdf");
                    using (var fichierPdf = new FileStream(cheminFichier, FileMode.Open))
                    {
                        var bytes = new byte[fichierPdf.Length];
                        fichierPdf.Read(bytes, 0, bytes.Length);
                        fichierPdf.Close(); // Fermer explicitement le fichier
                        return bytes;
                        // return await System.IO.File.ReadAllBytesAsync(cheminFichier);
                    }
                }
                else
                {
                    throw new Exception("Fichier non trouvé");
                }
            }
            else
            {
                throw new Exception("Adp non trouvée");
            }
        }

        public async Task<byte[]> GetFichierPdfVplAsync(int id)
        {
            var vpl = await _context.Vpls.FindAsync(id);
            if (vpl == null)
            {
                throw new Exception("Vpl non trouvée");
            }

            Console.WriteLine($"=== Recherche du fichier VPL {id} ===");
            Console.WriteLine($"Lien en base: {vpl.Lien}");

            // Chemin 1 : WebRootPath + lien
            var cheminWebRoot = Path.Combine(_env.WebRootPath, vpl.Lien);
            Console.WriteLine($"Chemin 1 (WebRoot): {cheminWebRoot}");
            if (System.IO.File.Exists(cheminWebRoot))
            {
                Console.WriteLine("Fichier trouvé avec WebRootPath");
                return await System.IO.File.ReadAllBytesAsync(cheminWebRoot);
            }

            // Chemin 2 : ContentRootPath + lien
            var cheminContentRoot = Path.Combine(_env.ContentRootPath, vpl.Lien);
            Console.WriteLine($"Chemin 2 (ContentRoot): {cheminContentRoot}");
            if (System.IO.File.Exists(cheminContentRoot))
            {
                Console.WriteLine("Fichier trouvé avec ContentRootPath");
                return await System.IO.File.ReadAllBytesAsync(cheminContentRoot);
            }

            // Chemin 3 : WebRootPath + SICOGI/NewSICOGI + nom du fichier
            var nomFichier = Path.GetFileName(vpl.Lien);
            var cheminNewSicogi = Path.Combine(_env.WebRootPath, "SICOGI", "NewSICOGI", nomFichier);
            Console.WriteLine($"Chemin 3 (NewSICOGI): {cheminNewSicogi}");
            if (System.IO.File.Exists(cheminNewSicogi))
            {
                Console.WriteLine("Fichier trouvé dans NewSICOGI");
                return await System.IO.File.ReadAllBytesAsync(cheminNewSicogi);
            }

            // Chemin 4 : WebRootPath + SICOGI + nom du fichier (sans NewSICOGI)
            var cheminSicogi = Path.Combine(_env.WebRootPath, "SICOGI", nomFichier);
            Console.WriteLine($"Chemin 4 (SICOGI): {cheminSicogi}");
            if (System.IO.File.Exists(cheminSicogi))
            {
                Console.WriteLine("Fichier trouvé dans SICOGI");
                return await System.IO.File.ReadAllBytesAsync(cheminSicogi);
            }

            // Si aucun chemin ne fonctionne, on lève une exception
            throw new Exception($"Fichier VPL non trouvé. Chemins testés:\n- {cheminWebRoot}\n- {cheminContentRoot}\n- {cheminNewSicogi}\n- {cheminSicogi}");
        }

        // Methode d'enregistrement des fichiers ADP
        public async Task<string> EnregistrerFichierAdpAsync(IFormFile fichier, string annee, string code, string boite, string logement, string document, string dateDocument, string client, string fonctions, string adresse, string contact, int operationId, int userId, int statutId)
        {
            // Console.WriteLine($"Début de l'enregistrement - Fichier: {fichier?.FileName}, UserId: {userId}");

            if (fichier == null || fichier.Length == 0)
            {
                throw new Exception("Fichier invalide");
            }

            // Vérifier l'extension du fichier
            var extension = Path.GetExtension(fichier.FileName).ToLower();
            if (extension != ".pdf")
            {
                throw new Exception("Seuls les fichiers PDF sont autorisés");
            }

            try
            {
                // Créer le répertoire de stockage si nécessaire
                var cheminDossier = Path.Combine(_env.ContentRootPath, _storagePathAdp);
                if (!Directory.Exists(cheminDossier))
                {
                    Directory.CreateDirectory(cheminDossier);
                    Console.WriteLine($"Dossier créé: {cheminDossier}");
                }

                // Générer un nom de fichier unique pour éviter les conflits
                var nomFichier = $"{Path.GetFileName(fichier.FileName)}";
                var cheminFichier = Path.Combine(cheminDossier, nomFichier);

                Console.WriteLine($"Enregistrement du fichier: {cheminFichier}");

                // Enregistrer le fichier sur le disque
                using (var stream = new FileStream(cheminFichier, FileMode.Create))
                {
                    await fichier.CopyToAsync(stream);
                }

                // === CONVERSION DE LA DATE ===
                string formattedDate = dateDocument; // Par défaut, on garde la valeur originale
                
                if (!string.IsNullOrEmpty(dateDocument))
                {
                    // Le formulaire envoie la date au format yyyy-MM-dd, on la convertit en dd/MM/yyyy
                    if (DateTime.TryParseExact(dateDocument, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        formattedDate = parsedDate.ToString("dd/MM/yyyy");
                        Console.WriteLine($"Date convertie: {dateDocument} -> {formattedDate}");
                    }
                    else
                    {
                        Console.WriteLine($"Impossible de parser la date: {dateDocument}");
                    }
                }

                // Enregistrer les informations du fichier dans la base de données
                var adp = new Adp
                {
                    Annee = annee,
                    Code = code,
                    Boite = boite,
                    Logement = logement,
                    Document = document,
                    DateDocument = formattedDate, // Utiliser la date convertie
                    Client = client,
                    Fonctions = fonctions,
                    Adresse = adresse,
                    Contact = contact,
                    IdOpe = operationId,
                    Lien = Path.Combine("ADP/NewADP/", nomFichier).Replace("\\", "/") // Stocker le chemin relatif
                };

                _context.Adps.Add(adp);
                await _context.SaveChangesAsync();

                // Gérer l'historique d'enregistrement du fichier ADP
                var historiqueAdp = new HistoriqueAdp
                {
                    IdAdp = adp.IdAdp,
                    UserId = userId,
                    DateHisto = DateTime.Now,
                    DateVu = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.HistoriqueAdps.Add(historiqueAdp);
                await _context.SaveChangesAsync();

                // Code pour la gestion des validations de fichiers ADP
                var validationfile = new Validationsfile
                {
                    IdAdp = adp.IdAdp,
                    UserId = userId,
                    DateValidation = DateTime.Now,
                    IdStatut = statutId,
                };

                _context.Validationsfiles.Add(validationfile);
                await _context.SaveChangesAsync();

                return "Fichier ADP enregistré avec succès";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur dans EnregistrerFichierAdpAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Méthode d'enregistrement des fichiers VPL
        public async Task<string> EnregistrerFichierVplAsync(
            IFormFile fichier,
            string annee,
            string code,
            string boite,
            string logement,
            string document,
            string dateDocument,
            string client,
            string fonctions,
            string adresse,
            string contact,
            string ville,
            string communeQuartier,
            int userId)
        {
            if (fichier == null || fichier.Length == 0)
            {
                throw new Exception("Fichier invalide");
            }

            // Vérifier l'extension du fichier
            var extension = Path.GetExtension(fichier.FileName).ToLower();
            if (extension != ".pdf")
            {
                throw new Exception("Seuls les fichiers PDF sont autorisés");
            }

            // Créer le répertoire de stockage si nécessaire
            var cheminDossier = Path.Combine(_env.ContentRootPath, _storagePathVpl);
            if (!Directory.Exists(cheminDossier))
            {
                Directory.CreateDirectory(cheminDossier);
            }

            // Nettoyer et générer un nom de fichier unique
            var nomOriginal = Path.GetFileNameWithoutExtension(fichier.FileName);
            var nomNettoye = string.Concat(nomOriginal.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
            nomNettoye = nomNettoye.Length > 55 ? nomNettoye.Substring(0, 55) : nomNettoye;
            var nomFichier = $"{nomNettoye}{extension}";

            var cheminFichier = Path.Combine(cheminDossier, nomFichier);

            // Vérifier que le chemin n'est pas trop long
            if (cheminFichier.Length >= 260)
            {
                throw new Exception("Le chemin du fichier est trop long. Veuillez renommer le fichier.");
            }

            // Enregistrer le fichier sur le disque
            using (var stream = new FileStream(cheminFichier, FileMode.Create))
            {
                await fichier.CopyToAsync(stream);
            }

            // === CONVERSION DE LA DATE ===
            string formattedDate = dateDocument; // Par défaut, on garde la valeur originale
            
            if (!string.IsNullOrEmpty(dateDocument))
            {
                // Le formulaire envoie la date au format yyyy-MM-dd, on la convertit en dd/MM/yyyy
                if (DateTime.TryParseExact(dateDocument, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    formattedDate = parsedDate.ToString("dd/MM/yyyy");
                    Console.WriteLine($"Date convertie: {dateDocument} -> {formattedDate}");
                }
                else
                {
                    Console.WriteLine($"Impossible de parser la date: {dateDocument}");
                }
            }

            // Enregistrer les informations du fichier dans la base de données
            var vpl = new Vpl
            {
                Annee = annee,
                Code = code,
                Boite = boite,
                Logement = logement,
                Document = document,
                DateDocument = dateDocument,
                Client = client,
                Fonctions = fonctions,
                Adresse = adresse,
                Contact = contact,
                Ville = ville,
                CommuneQuartier = communeQuartier,
                Lien = Path.Combine("SICOGI/NewSICOGI/", nomFichier).Replace("\\", "/") // Stocker le chemin relatif
            };

            _context.Vpls.Add(vpl);
            await _context.SaveChangesAsync();

            // Gérer l'historique d'enregistrement du fichier VPL
            var historiqueVpl = new HistoriqueVpl
            {
                IdVpl = vpl.IdVpl,
                UserId = userId,
                DateHisto = DateTime.Now,
                DateVu = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.HistoriqueVpls.Add(historiqueVpl);
            await _context.SaveChangesAsync();

            return "Fichier VPL enregistré avec succès";
        }

        // Méthode d'enregistrement des fichiers Cartes
        public async Task<string> EnregistrerFichierCarteAsync(
        IFormFile fichier,
        string tube,
        string ville,
        string quartier,
        string operation,
        string legende,
        string originalite,
        string echelle,
        string date_carte,
        string cote,
        int userId)
        {
            if (fichier == null || fichier.Length == 0)
            {
                throw new Exception("Fichier invalide");
            }

            // Vérifier l'extension du fichier
            var extension = Path.GetExtension(fichier.FileName).ToLower();
            if (extension != ".pdf")
            {
                throw new Exception("Seuls les fichiers PDF sont autorisés");
            }

            // Créer le répertoire de stockage si nécessaire
            var cheminDossier = Path.Combine(_env.ContentRootPath, _storagePathCartes);
            if (!Directory.Exists(cheminDossier))
            {
                Directory.CreateDirectory(cheminDossier);
            }

            // Nettoyer et générer un nom de fichier simple sans GUID
            var nomOriginal = Path.GetFileNameWithoutExtension(fichier.FileName);
            var nomNettoye = string.Concat(nomOriginal.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
            
            // Limiter la longueur et ajouter un timestamp pour éviter les conflits
            nomNettoye = nomNettoye.Length > 55 ? nomNettoye.Substring(0, 55) : nomNettoye;
            
            // Utiliser un timestamp pour l'unicité au lieu du GUID
            //var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var nomFichier = $"{nomNettoye}{extension}";

            var cheminFichier = Path.Combine(cheminDossier, nomFichier);

            // Vérifier que le chemin n'est pas trop long
            if (cheminFichier.Length >= 260)
            {
                throw new Exception("Le chemin du fichier est trop long. Veuillez renommer le fichier.");
            }

            // Enregistrer le fichier sur le disque
            using (var stream = new FileStream(cheminFichier, FileMode.Create))
            {
                await fichier.CopyToAsync(stream);
            }

            // === CONVERSION DE LA DATE ===
            string formattedDate = date_carte; // Par défaut, on garde la valeur originale
            
            if (!string.IsNullOrEmpty(date_carte))
            {
                // Le formulaire envoie la date au format yyyy-MM-dd, on la convertit en dd/MM/yyyy
                if (DateTime.TryParseExact(date_carte, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    formattedDate = parsedDate.ToString("dd/MM/yyyy");
                    Console.WriteLine($"Date convertie: {date_carte} -> {formattedDate}");
                }
                else
                {
                    Console.WriteLine($"Impossible de parser la date: {date_carte}");
                }
            }

            // Enregistrer les informations du fichier dans la base de données
            var cartes = new Carte
            {
                Tube = tube,
                Ville = ville,
                Quartier = quartier,
                Operation = operation,
                Legende = legende,
                Originalite = originalite,
                Echelle = echelle,
                Cote = cote,
                DateCarte = formattedDate,
                Lien = Path.Combine("CARTES/NewCARTES/", nomFichier).Replace("\\", "/")
            };

            _context.Cartes.Add(cartes);
            await _context.SaveChangesAsync();

            // Gérer l'historique d'enregistrement du fichier VPL
            var historiqueCarte = new HistoriqueCarte
            {
                IdCarte = cartes.IdCarte,
                UserId = userId,
                DateHisto = DateTime.Now,
                DateVu = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.HistoriqueCartes.Add(historiqueCarte);
            await _context.SaveChangesAsync();

            return "Carte enregistrée avec succès";
        }
    }
}