using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using anahged.Data;
using anahged.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace anahged.Services
{
    public class AdministrationService
    {
        private readonly GedContext _gedContext;
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly ILogger<AdministrationService> _logger;

        public AdministrationService(GedContext gedContext, ILogger<AdministrationService> logger)
        {
            _gedContext = gedContext;
            // Clé et IV pour AES (à stocker de manière sécurisée dans une vraie application)
            _key = Encoding.UTF8.GetBytes("0123456789abcdef"); // 16 bytes pour AES-128
            _iv = Encoding.UTF8.GetBytes("abcdef9876543210");  // 16 bytes pour AES
            _logger = logger;
        }

        // Methode pour récupérer la liste des utilisateurs
        public IEnumerable<User> GetAllUsers()
        {
            return _gedContext.Users
                                .Include(u => u.Groupe)
                                .Include(u => u.Userstatuts).ToList();
        }

        // Liste des rôles disponibles
        public IEnumerable<Groupe> GetAllRoles()
        {
            return _gedContext.Groupes.ToList();
        }

        // Liste des opérations disponibles
        public IEnumerable<Operation> GetOperationList()
        {
            return _gedContext.Operations.ToList();
        }

        // Methode pour récuperer un utilisateur par son ID
        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _gedContext.Users.Include(u => u.Groupe).FirstOrDefaultAsync(u => u.UserId == userId);
        }

        // Méthode pour mettre à jour un utilisateur
        public async Task<bool> UpdateUserAsync(User user, List<int> statutIds ,string password = null)
        {
            try
            {
                var existingUser = await _gedContext.Users
                    .Include(u => u.Userstatuts)
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId); // Utiliser l'ID de l'utilisateur(user.UserId);
                
                if (existingUser == null)
                    return false;

                // Mettre à jour les propriétés
                existingUser.NomUser = user.NomUser;
                existingUser.PrenomUser = user.PrenomUser;
                existingUser.Email = user.Email;
                existingUser.Contact = user.Contact;
                existingUser.GroupeId = user.GroupeId;
                existingUser.Actif = user.Actif;

                // Mettre à jour le mot de passe si fourni
                if (!string.IsNullOrEmpty(password))
                {
                    // Hacher le mot de passe avant de le sauvegarder
                    existingUser.Pwd = HasherMotDePasse(password);
                }

                // Mettre à jour les statuts
                await UpdateUserStatuts(existingUser.UserId, statutIds);

                _gedContext.Users.Update(existingUser);
                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Méthode pour mettre à jour les statuts d'un utilisateur
        private async Task UpdateUserStatuts(int userId, List<int> newStatutIds)
        {
            // Récupérer les statuts existants de l'utilisateur
            var existingStatuts = await _gedContext.Userstatuts
                .Where(us => us.UserId == userId)
                .ToListAsync();

          //  Console.WriteLine($"Statuts existants pour user {userId}: {existingStatuts.Count}");
         //   Console.WriteLine($"Nouveaux statuts: {string.Join(",", newStatutIds)}");

            // Identifier les statuts à supprimer
            var statutsToRemove = existingStatuts
                .Where(es => !newStatutIds.Contains(es.IdStatut.Value))
                .ToList();

            Console.WriteLine($"Statuts à supprimer: {statutsToRemove.Count}");

            // Supprimer les statuts qui ne sont plus sélectionnés
            foreach (var statut in statutsToRemove)
            {
                _gedContext.Userstatuts.Remove(statut);
            }

            // Identifier les statuts à ajouter
            var existingStatutIds = existingStatuts.Select(es => es.IdStatut.Value).ToList();
            var statutsToAdd = newStatutIds
                .Where(id => !existingStatutIds.Contains(id))
                .ToList();

            Console.WriteLine($"Statuts à ajouter: {string.Join(",", statutsToAdd)}");

            // Ajouter les nouveaux statuts
            foreach (var statutId in statutsToAdd)
            {
                var userStatut = new Userstatut
                {
                    IdStatut = statutId,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
                _gedContext.Userstatuts.Add(userStatut);
                Console.WriteLine($"Ajout du statut {statutId} pour l'utilisateur {userId}");
            }
        }

        // Méthode pour créer un nouvel utilisateur
        public async Task<(bool success, string message)> CreateUserAsync(User user, string password, List<int> statutIds = null)
        {
            using var transaction = await _gedContext.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Tentative de création d'utilisateur: {Email}", user.Email);

                // Vérifier si l'email existe déjà
                if (await _gedContext.Users.AnyAsync(u => u.Email == user.Email))
                    return (false, "Un utilisateur avec cet email existe déjà.");

                // Vérifier que le mot de passe n'est pas vide
                if (string.IsNullOrEmpty(password))
                    return (false, "Le mot de passe est requis.");

                // Hacher le mot de passe
                user.Pwd = HasherMotDePasse(password);

                // Vérifier que le groupe existe
                if (!await _gedContext.Groupes.AnyAsync(g => g.GroupeId == user.GroupeId))
                    return (false, "Le groupe spécifié n'existe pas.");

                // Créer l'utilisateur
                _gedContext.Users.Add(user);
                await _gedContext.SaveChangesAsync();

                // Ajouter les statuts si fournis
                if (statutIds != null && statutIds.Any())
                {
                    await AddUserStatutsAsync(user.UserId, statutIds);
                }

                await transaction.CommitAsync();
                return (true, "Utilisateur créé avec succès.");
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erreur de base de données lors de la création de l'utilisateur {Email}", user.Email);
                return (false, "Erreur de base de données lors de la création de l'utilisateur.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erreur inattendue lors de la création de l'utilisateur {Email}", user.Email);
                return (false, "Erreur inconnue lors de la création de l'utilisateur: " + ex.Message);
            }
        }

        // Méthode pour ajouter les statuts à un utilisateur
        private async Task AddUserStatutsAsync(int userId, List<int> statutIds)
        {
            var userStatuts = statutIds.Select(statutId => new Userstatut
            {
                UserId = userId,
                IdStatut = statutId,
                CreatedAt = DateTime.Now
            }).ToList();

            _gedContext.Userstatuts.AddRange(userStatuts);
            await _gedContext.SaveChangesAsync();
        }

        // Méthode pour hasher le mot de passe
        private string HasherMotDePasse(string motDePasse)
        {
            if(string.IsNullOrEmpty(motDePasse))
                throw new ArgumentException("Le mot de passe ne peut pas être vide.", nameof(motDePasse));
            // Utilisez le même algorithme de hachage que celui utilisé pour stocker les mots de passe dans la base de données
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(motDePasse));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
        
        // Méthode pour supprimer un utilisateur
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _gedContext.Users.FindAsync(userId);
                if (user == null)
                    return false;

                _gedContext.Users.Remove(user);
                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Méthode pour créer un statut
        public async Task<bool> CreateStatutAsync(string codeStatut, string descriptionStatut, string noteStatut)
        {
            try
            {
                var statut = new Statut
                {
                    CodeStatut = codeStatut,
                    DescriptionStatut = descriptionStatut,
                    NoteStatut = noteStatut
                };

                _gedContext.Statuts.Add(statut);
                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Méthode pour récupérer la liste des statuts
        public IEnumerable<Statut> GetAllStatuts()
        {
            return [.. _gedContext.Statuts];
        }

        // Methode pour supprimer plusieurs statuts
        public async Task<bool> DeleteStatutsAsync(List<int> statutIds)
        {
            try
            {
                // Récupérer tous les statuts à supprimer en une seule requête
                var statuts = await _gedContext.Statuts
                    .Where(s => statutIds.Contains(s.IdStatut))
                    .ToListAsync();

                if (statuts == null || !statuts.Any())
                    return false;

                // Supprimer tous les statuts en une fois
                _gedContext.Statuts.RemoveRange(statuts);
                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log l'erreur si nécessaire
                Console.WriteLine($"Erreur lors de la suppression des statuts: {ex.Message}");
                return false;
            }
        }

        // Méthode pour modifier un statut
        public async Task<bool> UpdateStatutAsync(int id, string codeStatut, string descriptionStatut, string noteStatut)
        {
            try
            {
                var existingStatut = await _gedContext.Statuts.FindAsync(id);
                if (existingStatut == null)
                    return false;

                // Mettre à jour les propriétés
                existingStatut.CodeStatut = codeStatut;
                existingStatut.DescriptionStatut = descriptionStatut;
                existingStatut.NoteStatut = noteStatut;

                // Pas besoin de Update() car l'entité est déjà trackée par FindAsync()
                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Ajoutez un logging pour déboguer
                Console.WriteLine($"Erreur lors de la modification du statut: {ex.Message}");
                return false;
            }
        }

        /* Fin traitement des statuts */
        /* Traitement des opérations */
        // Création d'une opération
        public async Task<bool> CreateOperationAsync(string codeOperation, string descriptionOperation, int villeId)
        {
            try
            {
                // Vérifier si la ville existe
                var ville = await _gedContext.Villes.FindAsync(villeId);
                if (ville == null)
                    return false;

                var operation = new Operation
                {
                    CodeOpe = codeOperation,
                    DescriptionOpe = descriptionOperation,
                    IdVille = villeId // Assigner l'ID de la ville existante
                };

                _gedContext.Operations.Add(operation);
                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Méthode pour récupérer la liste des opérations
        public IEnumerable<Operation> GetAllOperations()
        {
            return [.. _gedContext.Operations];
        }

        // Méthode pour modifier une opération
        public async Task<bool> UpdateOperationAsync(int id, string codeOpe, string descriptionOpe, int? villeId)
        {
            try
            {
                var existingOperation = await _gedContext.Operations.FindAsync(id);
                if (existingOperation == null)
                    return false;

                // Mettre à jour les propriétés
                existingOperation.CodeOpe = codeOpe;
                existingOperation.DescriptionOpe = descriptionOpe;
                existingOperation.IdVille = villeId;

                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Ajoutez un logging pour déboguer
                Console.WriteLine($"Erreur lors de la modification de l'opération: {ex.Message}");
                return false;
            }
        }       

        // Méthode pour supprimer plusieurs opérations non liées
        public async Task<bool> DeleteOperationsAsync(List<int> operationIds)
        {
            try
            {
                // Vérification des entrées
                if (operationIds == null || !operationIds.Any())
                    return false;

                // Charger les opérations avec leurs relations
                var operations = await _gedContext.Operations
                    .Include(o => o.Adps)
                    .Include(o => o.Vpls)
                    .Include(o => o.Cartes)
                    .Where(o => operationIds.Contains(o.IdOpe))
                    .ToListAsync();

                if (operations == null || !operations.Any())
                    return false;

                // Filtrer les opérations qui n'ont aucune relation
                var operationsNonLiees = operations
                    .Where(o => !o.Adps.Any() && !o.Vpls.Any() && !o.Cartes.Any())
                    .ToList();

                if (!operationsNonLiees.Any())
                    return false;

                // Suppression
                _gedContext.Operations.RemoveRange(operationsNonLiees);
                await _gedContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression des opérations non liées: {ex.Message}");
                return false;
            }
        }

        // Code template pour le téléchargement des opérations
        public byte[] GenerateOperationTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Opérations");
            // En-têtes
            worksheet.Cell(1, 1).Value = "CodeOperation";
            worksheet.Cell(1, 2).Value = "DescriptionOperation";
            //worksheet.Cell(1, 3).Value = "IdVille";
            
            // Style
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
            // Données d'exemple
            worksheet.Cell(2, 1).Value = "OPE001";
            worksheet.Cell(2, 2).Value = "Test Concorde 1";
            worksheet.Cell(3, 1).Value = "OPE002";
            worksheet.Cell(3, 2).Value = "Test Concorde 2";
            worksheet.Cell(4, 1).Value = "OPE003";
            worksheet.Cell(4, 2).Value = "Test Concorde 3";
            
            // Ajuster la largeur
            worksheet.Column(1).Width = 15;
            worksheet.Column(2).Width = 30;
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
       
        /* Fin traitement des opérations */

        /* Traitement des villes */
        // Méthode pour créer une ville
        public async Task<bool> CreateVilleAsync(string codeVille, string descriptionVille)
        {
            try
            {
                var ville = new Ville
                {
                    CodeVille = codeVille,
                    DescriptionVille = descriptionVille
                };

                _gedContext.Villes.Add(ville);
                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false; ;
            }
        }

        // Méthode pour modifier une ville
        public async Task<bool> UpdateVillesAsync(int id, string codeVilles, string descriptionVilles)
        {
            try
            {
                var existingVilles = await _gedContext.Villes.FindAsync(id);
                if (existingVilles == null)
                    return false;

                // Mettre à jour les propriétés
                existingVilles.CodeVille = codeVilles;
                existingVilles.DescriptionVille = descriptionVilles;

                // Pas besoin de Update() car l'entité est déjà trackée par FindAsync()
                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Ajoutez un logging pour déboguer
                Console.WriteLine($"Erreur lors de la modification de la ville: {ex.Message}");
                return false;
            }
        }

        // Methode pour supprimer plusieurs villes
        public async Task<bool> DeleteVilleAsync(List<int> villeIds)
        {
            try
            {
                // Récupérer tous les villes à supprimer en une seule requête
                var villes = await _gedContext.Villes
                    .Where(v => villeIds.Contains(v.IdVille))
                    .ToListAsync();

                if (villes == null || !villes.Any())
                    return false;

                // Supprimer tous les villes en une fois
                _gedContext.Villes.RemoveRange(villes);
                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log l'erreur si nécessaire
                Console.WriteLine($"Erreur lors de la suppression des villes: {ex.Message}");
                return false;
            }
        }

        // Méthode pour récupérer la liste des villes
        public IEnumerable<Ville> GetAllVilles()
        {
            return [.. _gedContext.Villes];
        }

        // Methode pour télécharger un template ville
        public byte[] GenerateVilleTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Villes");

            // En-têtes
            worksheet.Cell(1, 1).Value = "CodeVille";
            worksheet.Cell(1, 2).Value = "DescriptionVille";
            
            // Style
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
            // Données d'exemple
            worksheet.Cell(2, 1).Value = "VIL001";
            worksheet.Cell(2, 2).Value = "Paris";
            worksheet.Cell(3, 1).Value = "VIL002";
            worksheet.Cell(3, 2).Value = "Lyon";
            worksheet.Cell(4, 1).Value = "VIL003";
            worksheet.Cell(4, 2).Value = "Marseille";
            
            // Ajuster la largeur
            worksheet.Column(1).Width = 15;
            worksheet.Column(2).Width = 30;
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    }

    // Classes pour le résultat de suppression
    /* public class DeleteResult
    {
        public bool IsSuccess { get; set; }
        public int SuccessCount { get; set; }
        public List<int> DeletedIds { get; set; } = new List<int>();
        public List<FailedOperation> FailedOperations { get; set; } = new List<FailedOperation>();
        public string ErrorMessage { get; set; }
    } */

    /* public class FailedOperation
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Reason { get; set; }
    } */
}