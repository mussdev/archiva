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

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vpl> Vpls { get; set; }

    /*    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
           => optionsBuilder.UseMySql("server=localhost;port=3306;database=bdsicogi;user=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.2.0-mysql"));
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

            entity.HasIndex(e => e.Lien, "cle_lien").IsUnique();

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
            entity.Property(e => e.Ville)
                .HasMaxLength(50)
                .HasColumnName("ville")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Carte>(entity =>
        {
            entity.HasKey(e => e.IdCarte).HasName("PRIMARY");

            entity
                .ToTable("carte")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

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
            entity.Property(e => e.DateHisto)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("date_histo");
            entity.Property(e => e.DateVu).HasColumnName("date_vu");
            entity.Property(e => e.IdAdp).HasColumnName("id_adp");
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
            entity.Property(e => e.DateHisto)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("date_histo");
            entity.Property(e => e.DateVu).HasColumnName("date_vu");
            entity.Property(e => e.IdCarte).HasColumnName("id_carte");
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
            entity.Property(e => e.DateHisto)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("date_histo");
            entity.Property(e => e.DateVu).HasColumnName("date_vu");
            entity.Property(e => e.IdVpl).HasColumnName("id_vpl");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.IdVplNavigation).WithMany(p => p.HistoriqueVpls)
                .HasForeignKey(d => d.IdVpl)
                .HasConstraintName("historique_vpl_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.HistoriqueVpls)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("historique_vpl_ibfk_2");
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

        modelBuilder.Entity<Vpl>(entity =>
        {
            entity.HasKey(e => e.IdVpl).HasName("PRIMARY");

            entity
                .ToTable("vpl")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

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
            entity.Property(e => e.Ville)
                .HasMaxLength(50)
                .HasColumnName("ville")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        OnModelCreatingPartial(modelBuilder);
        
        // Relation User -> Groupe
        modelBuilder.Entity<User>()
        .HasOne(u => u.Groupe)
        .WithMany(g => g.Users)
        .HasForeignKey(u => u.GroupeId)
        .OnDelete(DeleteBehavior.Restrict);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
