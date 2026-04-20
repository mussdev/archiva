using System;
using System.Collections.Generic;
using anahged.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace anahged.Data;

public partial class GedContext : DbContext
{
    public GedContext()
    {
    }

    public GedContext(DbContextOptions<GedContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Adp> Adps { get; set; }

    public virtual DbSet<Carte> Cartes { get; set; }

    public virtual DbSet<Groupe> Groupes { get; set; }

    public virtual DbSet<HistoriqueAdp> HistoriqueAdps { get; set; }

    public virtual DbSet<HistoriqueCarte> HistoriqueCartes { get; set; }

    public virtual DbSet<HistoriqueVpl> HistoriqueVpls { get; set; }

    public virtual DbSet<Operation> Operations { get; set; }

    public virtual DbSet<Statut> Statuts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userconnexionlog> Userconnexionlogs { get; set; }

    public virtual DbSet<Usersession> Usersessions { get; set; }

    public virtual DbSet<Userstatut> Userstatuts { get; set; }

    public virtual DbSet<Validationsfile> Validationsfiles { get; set; }

    public virtual DbSet<Ville> Villes { get; set; }

    public virtual DbSet<Vpl> Vpls { get; set; }

 /*    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=127.0.0.1;database=bdsicogi;user=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.2.0-mysql"));
 */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Adp>(entity =>
        {
            entity.HasKey(e => e.IdAdp).HasName("PRIMARY");

            entity
                .ToTable("adp")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.DernierStatutAdpId, "idx_adp_dernierstatutadpId");

            entity.HasIndex(e => e.IdOpe, "idx_adp_idope");

            entity.HasIndex(e => e.Boite, "index_boite");

            entity.HasIndex(e => e.Client, "index_client");

            entity.HasIndex(e => e.Code, "index_code");

            entity.HasIndex(e => e.Document, "index_document");

            entity.HasIndex(e => e.Logement, "index_logement");

            entity.Property(e => e.IdAdp).HasColumnName("id_adp");
            entity.Property(e => e.Adresse)
                .HasMaxLength(100)
                .HasColumnName("adresse")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Annee)
                .HasMaxLength(4)
                .HasColumnName("annee")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Boite)
                .HasMaxLength(25)
                .HasColumnName("boite");
            entity.Property(e => e.Client)
                .HasColumnName("client")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Code)
                .HasMaxLength(25)
                .HasColumnName("code");
            entity.Property(e => e.CommuneQuartier)
                .HasMaxLength(50)
                .HasColumnName("commune_quartier")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Contact)
                .HasMaxLength(100)
                .HasColumnName("contact")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Cote)
                .HasMaxLength(20)
                .HasColumnName("cote")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.DateDocument)
                .HasMaxLength(50)
                .HasColumnName("date_document")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Document)
                .HasColumnName("document")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Fonctions)
                .HasMaxLength(255)
                .HasColumnName("fonctions")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Lien)
                .HasMaxLength(100)
                .HasColumnName("lien")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Logement)
                .HasMaxLength(25)
                .HasColumnName("logement");
            entity.Property(e => e.NumDossierAdp)
                .HasMaxLength(55)
                .HasColumnName("numDossierAdp");
            entity.Property(e => e.Ville)
                .HasMaxLength(50)
                .HasColumnName("ville")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.DernierStatutAdp).WithMany(p => p.Adps)
                .HasForeignKey(d => d.DernierStatutAdpId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_adp_statuts");

            entity.HasOne(d => d.IdOpeNavigation).WithMany(p => p.Adps)
                .HasForeignKey(d => d.IdOpe)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_adp_operations");
        });

        modelBuilder.Entity<Carte>(entity =>
        {
            entity.HasKey(e => e.IdCarte).HasName("PRIMARY");

            entity
                .ToTable("carte")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.DernierStatutCarteId, "idx_carte_dernierstatutcarteId");

            entity.HasIndex(e => e.IdOpe, "idx_carte_idope");

            entity.Property(e => e.IdCarte).HasColumnName("id_carte");
            entity.Property(e => e.Cote)
                .HasMaxLength(100)
                .HasColumnName("cote")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.DateCarte)
                .HasMaxLength(100)
                .HasColumnName("date_carte");
            entity.Property(e => e.Echelle)
                .HasMaxLength(50)
                .HasColumnName("echelle")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Legende)
                .HasMaxLength(255)
                .HasColumnName("legende")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Lien)
                .HasMaxLength(100)
                .HasColumnName("lien")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.NumDossierCarte)
                .HasMaxLength(55)
                .HasColumnName("numDossierCarte");
            entity.Property(e => e.Operation)
                .HasMaxLength(255)
                .HasColumnName("operation")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Originalite)
                .HasMaxLength(20)
                .HasColumnName("originalite")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Quartier)
                .HasMaxLength(50)
                .HasColumnName("quartier")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Tube)
                .HasMaxLength(25)
                .HasColumnName("tube")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Ville)
                .HasMaxLength(50)
                .HasColumnName("ville")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.DernierStatutCarte).WithMany(p => p.Cartes)
                .HasForeignKey(d => d.DernierStatutCarteId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_carte_statuts");

            entity.HasOne(d => d.IdOpeNavigation).WithMany(p => p.Cartes)
                .HasForeignKey(d => d.IdOpe)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_carte_operations");
        });

        modelBuilder.Entity<Groupe>(entity =>
        {
            entity.HasKey(e => e.GroupeId).HasName("PRIMARY");

            entity
                .ToTable("groupe")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.Property(e => e.GroupeId).HasColumnName("groupe_id");
            entity.Property(e => e.Administrateur).HasColumnName("administrateur");
            entity.Property(e => e.Carte).HasColumnName("carte");
            entity.Property(e => e.NomGroupe)
                .HasMaxLength(100)
                .HasColumnName("nom_groupe")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Superviseur).HasColumnName("superviseur");
            entity.Property(e => e.VplAdp).HasColumnName("vpl_adp");
            entity.Property(e => e.VpladpCarte).HasColumnName("vpladp_carte");
        });

        modelBuilder.Entity<HistoriqueAdp>(entity =>
        {
            entity.HasKey(e => e.IdHistoriqueAdp).HasName("PRIMARY");

            entity
                .ToTable("historique_adp")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.IdAdp, "id_adp");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.IdHistoriqueAdp).HasColumnName("id_historique_adp");
            entity.Property(e => e.Commentaire).HasMaxLength(255);
            entity.Property(e => e.DateHisto)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("date_histo");
            entity.Property(e => e.DateVu).HasColumnName("date_vu");
            entity.Property(e => e.IdAdp).HasColumnName("id_adp");
            entity.Property(e => e.TypeAction).HasMaxLength(55);
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.IdAdpNavigation).WithMany(p => p.HistoriqueAdps)
                .HasForeignKey(d => d.IdAdp)
                .HasConstraintName("historique_adp_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.HistoriqueAdps)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("historique_adp_ibfk_2");
        });

        modelBuilder.Entity<HistoriqueCarte>(entity =>
        {
            entity.HasKey(e => e.IdHistoriqueCarte).HasName("PRIMARY");

            entity
                .ToTable("historique_carte")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.IdCarte, "id_carte");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.IdHistoriqueCarte).HasColumnName("id_historique_carte");
            entity.Property(e => e.Commentaire).HasMaxLength(255);
            entity.Property(e => e.DateHisto)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("date_histo");
            entity.Property(e => e.DateVu).HasColumnName("date_vu");
            entity.Property(e => e.IdCarte).HasColumnName("id_carte");
            entity.Property(e => e.TypeAction).HasMaxLength(55);
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.IdCarteNavigation).WithMany(p => p.HistoriqueCartes)
                .HasForeignKey(d => d.IdCarte)
                .HasConstraintName("historique_carte_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.HistoriqueCartes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("historique_carte_ibfk_2");
        });

        modelBuilder.Entity<HistoriqueVpl>(entity =>
        {
            entity.HasKey(e => e.IdHistoriqueVpl).HasName("PRIMARY");

            entity
                .ToTable("historique_vpl")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.IdVpl, "id_vpl");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.IdHistoriqueVpl).HasColumnName("id_historique_vpl");
            entity.Property(e => e.Commentaire).HasMaxLength(255);
            entity.Property(e => e.DateHisto)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("date_histo");
            entity.Property(e => e.DateVu).HasColumnName("date_vu");
            entity.Property(e => e.IdVpl).HasColumnName("id_vpl");
            entity.Property(e => e.TypeAction).HasMaxLength(55);
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.IdVplNavigation).WithMany(p => p.HistoriqueVpls)
                .HasForeignKey(d => d.IdVpl)
                .HasConstraintName("historique_vpl_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.HistoriqueVpls)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("historique_vpl_ibfk_2");
        });

        modelBuilder.Entity<Operation>(entity =>
        {
            entity.HasKey(e => e.IdOpe).HasName("PRIMARY");

            entity.ToTable("operations");

            entity.HasIndex(e => e.CodeOpe, "CodeOpe").IsUnique();

            entity.HasIndex(e => e.IdVille, "idx_operations_idville");

            entity.Property(e => e.CodeOpe).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.DescriptionOpe).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("timestamp");

            entity.HasOne(d => d.IdVilleNavigation).WithMany(p => p.Operations)
                .HasForeignKey(d => d.IdVille)
                .HasConstraintName("fk_operations_villes");
        });

        modelBuilder.Entity<Statut>(entity =>
        {
            entity.HasKey(e => e.IdStatut).HasName("PRIMARY");

            entity.ToTable("statuts");

            entity.HasIndex(e => e.CodeStatut, "CodeStatut").IsUnique();

            entity.Property(e => e.CodeStatut).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.DescriptionStatut).HasMaxLength(100);
            entity.Property(e => e.NoteStatut).HasColumnType("text");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity
                .ToTable("users")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Actif).HasColumnName("actif");
            entity.Property(e => e.Contact)
                .HasMaxLength(100)
                .HasColumnName("contact");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.GroupeId).HasColumnName("groupe_id");
            entity.Property(e => e.NomUser)
                .HasMaxLength(100)
                .HasColumnName("nom_user");
            entity.Property(e => e.PrenomUser)
                .HasMaxLength(100)
                .HasColumnName("prenom_user");
            entity.Property(e => e.Pwd)
                .HasMaxLength(100)
                .HasColumnName("pwd");
        });

        modelBuilder.Entity<Userconnexionlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("userconnexionlogs")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => new { e.UserId, e.DateEvenement }, "idx_user_log_date");

            entity.Property(e => e.AdresseIp)
                .HasMaxLength(45)
                .HasColumnName("AdresseIP");
            entity.Property(e => e.DateEvenement).HasColumnType("datetime");
            entity.Property(e => e.TypeEvenement).HasColumnType("enum('LOGIN','LOGOUT','FAIL','TIMEOUT')");

            entity.HasOne(d => d.User).WithMany(p => p.Userconnexionlogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_userlogs_user");
        });

        modelBuilder.Entity<Usersession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("usersessions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => new { e.UserId, e.IsActive }, "idx_user_session_active");

            entity.Property(e => e.AdresseIp)
                .HasMaxLength(45)
                .HasColumnName("AdresseIP");
            entity.Property(e => e.DateConnexion).HasColumnType("datetime");
            entity.Property(e => e.DateDeconnexion).HasColumnType("datetime");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");

            entity.HasOne(d => d.User).WithMany(p => p.Usersessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_usersessions_user");
        });

        modelBuilder.Entity<Userstatut>(entity =>
        {
            entity.HasKey(e => e.IdUserStatut).HasName("PRIMARY");

            entity.ToTable("userstatuts");

            entity.HasIndex(e => e.IdStatut, "FK_UserStatuts_Statut");

            entity.HasIndex(e => new { e.UserId, e.IdStatut }, "UQ_UserStatut").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<Validationsfile>(entity =>
        {
            entity.HasKey(e => e.IdValidation).HasName("PRIMARY");

            entity.ToTable("validationsfiles");

            entity.Property(e => e.Commentaire).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.DateValidation).HasColumnType("datetime");
            entity.Property(e => e.IdStatut).HasDefaultValueSql("'1'");
            entity.Property(e => e.MotifRejet).HasColumnType("text");
            entity.Property(e => e.TypeAction).HasMaxLength(55);
        });

        modelBuilder.Entity<Ville>(entity =>
        {
            entity.HasKey(e => e.IdVille).HasName("PRIMARY");

            entity.ToTable("villes");

            entity.HasIndex(e => e.CodeVille, "CodeVille").IsUnique();

            entity.Property(e => e.CodeVille).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.DescriptionVille).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<Vpl>(entity =>
        {
            entity.HasKey(e => e.IdVpl).HasName("PRIMARY");

            entity
                .ToTable("vpl")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.DernierStatutVplId, "idx_vpl_dernierstatutvplId");

            entity.HasIndex(e => e.IdOpe, "idx_vpl_idope");

            entity.HasIndex(e => e.Boite, "index_boite");

            entity.HasIndex(e => e.Client, "index_client");

            entity.HasIndex(e => e.Code, "index_code");

            entity.HasIndex(e => e.Document, "index_document");

            entity.HasIndex(e => e.Logement, "index_logement");

            entity.Property(e => e.IdVpl).HasColumnName("id_vpl");
            entity.Property(e => e.Adresse)
                .HasMaxLength(100)
                .HasColumnName("adresse")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Annee)
                .HasMaxLength(4)
                .HasColumnName("annee")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Boite)
                .HasMaxLength(25)
                .HasColumnName("boite");
            entity.Property(e => e.Client)
                .HasColumnName("client")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Code)
                .HasMaxLength(25)
                .HasColumnName("code");
            entity.Property(e => e.CommuneQuartier)
                .HasMaxLength(50)
                .HasColumnName("commune_quartier")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Contact)
                .HasMaxLength(100)
                .HasColumnName("contact")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Cote)
                .HasMaxLength(20)
                .HasColumnName("cote")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.DateDocument)
                .HasMaxLength(50)
                .HasColumnName("date_document")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Document)
                .HasColumnName("document")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Fonctions)
                .HasMaxLength(255)
                .HasColumnName("fonctions")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Lien)
                .HasMaxLength(100)
                .HasColumnName("lien")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Logement)
                .HasMaxLength(25)
                .HasColumnName("logement");
            entity.Property(e => e.NumDossierVpl)
                .HasMaxLength(55)
                .HasColumnName("numDossierVpl");
            entity.Property(e => e.Ville)
                .HasMaxLength(50)
                .HasColumnName("ville")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.DernierStatutVpl).WithMany(p => p.Vpls)
                .HasForeignKey(d => d.DernierStatutVplId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_vpl_statuts");

            entity.HasOne(d => d.IdOpeNavigation).WithMany(p => p.Vpls)
                .HasForeignKey(d => d.IdOpe)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_vpl_operations");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
