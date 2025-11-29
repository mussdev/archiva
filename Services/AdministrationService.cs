using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using anahged.Data;
using anahged.Models;
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
            return _gedContext.Users.Include(u => u.Groupe).ToList();
        }

        // Liste des rôles disponibles
        public IEnumerable<Groupe> GetAllRoles()
        {
            return _gedContext.Groupes.ToList();
        }

        // Methode pour récuperer un utilisateur par son ID
        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _gedContext.Users.Include(u => u.Groupe).FirstOrDefaultAsync(u => u.UserId == userId);
        }

        // Méthode pour mettre à jour un utilisateur
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var existingUser = await _gedContext.Users.FindAsync(user.UserId);
                if (existingUser == null)
                    return false;

                // Mettre à jour les propriétés
                existingUser.NomUser = user.NomUser;
                existingUser.PrenomUser = user.PrenomUser;
                existingUser.Email = user.Email;
                existingUser.Contact = user.Contact;
                existingUser.GroupeId = user.GroupeId;
                existingUser.Actif = user.Actif;

                _gedContext.Users.Update(existingUser);
                await _gedContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Méthode pour créer un nouvel utilisateur
        public async Task<(bool success, string message)> CreateUserAsync(User user, string password)
        {
            try
            {
                _logger.LogInformation("Tentative de création d'utilisateur: {Email}", user.Email);

                // Vérifier si l'email existe déjà
                if (await _gedContext.Users.AnyAsync(u => u.Email == user.Email))
                    return (false, "Un utilisateur avec cet email existe déjà.");

                // Vérifier que le mot de passe n'est pas vide
                if (string.IsNullOrEmpty(password))
                    return (false, "Le mot de pass est requis.");

                // Hacher le mot de passe
                user.Pwd = HasherMotDePasse(password);

                // Dans CreateUserAsync, avant de sauvegarder
                if (!await _gedContext.Groupes.AnyAsync(g => g.GroupeId == user.GroupeId))
                    return (false, "Le groupe spécifié n'existe pas.");

                    
                _gedContext.Users.Add(user);
                await _gedContext.SaveChangesAsync();
                return (true, "Utilisateur créé avec succès.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erreur de base de données lors de la création de l'utilisateur {Email}", user.Email);
                return (false, "Erreur de base de données lors de la création de l'utilisateur.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de la création de l'utilisateur {Email}", user.Email);
                return (false, "Erreur inconnue lors de la création de l'utilisateur: " + ex.Message);
            }
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

    }
}