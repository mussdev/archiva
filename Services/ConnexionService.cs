using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using anahged.Data;
using anahged.Models;
using anahged.Pages;

namespace anahged.Services
{
    public class ConnexionService
    {
        private readonly GedContext _usersged;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public ConnexionService(GedContext usersged)
        {
            _usersged = usersged;
            // Clé et IV pour AES (à stocker de manière sécurisée dans une vraie application)
            _key = Encoding.UTF8.GetBytes("0123456789abcdef"); // 16 bytes pour AES-128
            _iv = Encoding.UTF8.GetBytes("abcdef9876543210");  // 16 bytes pour AES
        }

        public bool Authentifier(string nomUtilisateur, string motDePasse)
        {
            var utilisateurs = _usersged.Users.FirstOrDefault(u => u.Email == nomUtilisateur);
            
            try
            {
                if (utilisateurs != null)
                {
                    // Hasher le mot de passe entré par l'utilisateur
                    var motDePasseHash = HasherMotDePasse(motDePasse);
                    
                    if (motDePasseHash == utilisateurs.Pwd)
                    {
                        return true;
                    }
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Format de mot de passe invalide", nameof(motDePasse), ex);
            }
            return false;
        }

        // Méthode pour hasher le mot de passe
        private string HasherMotDePasse(string motDePasse)
        {
            // Utilisez le même algorithme de hachage que celui utilisé pour stocker les mots de passe dans la base de données
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(motDePasse));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public class ConnexionResult
        {
            public bool IsAuthenticated { get; set; }
            public required User Utilisateur { get; set; }
        }

        // Récupérer les informations de l'utilisateur connecté
        public string GetUserInitiales(User utilisateur)
        {
            if (utilisateur == null) return string.Empty;
            var nomInitiale = utilisateur.NomUser.Substring(0, 1).ToUpper() ?? "";
            var prenomInitiale = utilisateur.PrenomUser.Substring(0, 1).ToUpper() ?? "";
            return nomInitiale + prenomInitiale; 
        }
    }
}