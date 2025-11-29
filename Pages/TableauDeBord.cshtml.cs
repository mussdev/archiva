using anahged.Data;
using anahged.Models;
using anahged.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace anahged.Pages
{
    public class TableauDeBordModel : PageModel
    {
        private readonly GedContext _gedContext;
        public List<Adp> adps { get; set; } = default!;
        public int NbrBoiteAdp { get; set; }
        public int NbrLogementAdp { get; set; }
        public int NbrFichersAdp { get; set; }

        public int NbrBoiteVpl { get; set; }
        public int NbrLogementVpl { get; set; }
        public int NbrFichersVpl { get; set; }

        public int NbrTube { get; set; }
        public int NbrCartes { get; set; }

        public TableauDeBordModel(GedContext gedContext)
        {
            _gedContext = gedContext;
        }
        public void OnGet()
        {
            var InventairesAdps = InventaireAdp();
            NbrBoiteAdp = InventairesAdps.NbrBoite;
            NbrLogementAdp = InventairesAdps.NbrLogement;
            NbrFichersAdp = InventairesAdps.NbrFichers;

            // Inventaire VPL
            var InventairesVpl = InventaireVpl();
            NbrBoiteVpl = InventairesVpl.NbrBoiteVpl;
            NbrLogementVpl = InventairesVpl.NbrLogementVpl;
            NbrFichersVpl = InventairesVpl.NbrFichersVpl;

            // Inventaire Cartes
            var InventairesCartes = InventaireCarte();
            NbrTube = InventairesCartes.NbrTube;
            NbrCartes = InventairesCartes.NbrCartes;

        }

        // Methode inventaire des fichiers ADP
        public (int NbrBoite, int NbrLogement, int NbrFichers) InventaireAdp()
        {
            var nbrBoite = _gedContext.Adps.Select(a => a.Boite).Distinct().Count();
            var nbrLogement = _gedContext.Adps.Select(a => a.Logement).Distinct().Count();
            var nbrFichers = _gedContext.Adps.Select(a => a.IdAdp).Distinct().Count();

            return (nbrBoite, nbrLogement, nbrFichers);
        }

        // Methode inventaire des fichiers VPL
        public (int NbrBoiteVpl, int NbrLogementVpl, int NbrFichersVpl) InventaireVpl()
        {
            var NbrBoite = _gedContext.Vpls.Select(v => v.Boite).Distinct().Count();
            var NbrLogement = _gedContext.Vpls.Select(v => v.Logement).Distinct().Count();
            var NbrFichers = _gedContext.Vpls.Select(v => v.IdVpl).Distinct().Count();

            return (NbrBoite, NbrLogement, NbrFichers);
        }

        // Methode inventaire des Cartes numerisées
        public (int NbrTube, int NbrCartes) InventaireCarte()
        {
            var NbrTube = _gedContext.Cartes.Select(c => c.Tube).Distinct().Count();
            var NbrCartes = _gedContext.Cartes.Select(c => c.IdCarte).Distinct().Count();
            return (NbrTube, NbrCartes);
        }


    }
}
