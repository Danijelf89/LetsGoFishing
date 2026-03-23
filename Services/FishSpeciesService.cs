using LetsGoFishing.Models;

namespace LetsGoFishing.Services;

/// <summary>
/// Servis koji sadrži podatke o vrstama riba, lovostaju i lovnim dužinama u Srbiji.
/// </summary>
public class FishSpeciesService
{
    private readonly List<FishSpecies> _species;

    public FishSpeciesService()
    {
        _species = InitializeSpecies();
    }

    private static List<FishSpecies> InitializeSpecies()
    {
        return
        [
            // Ribe sa sezonskim lovostajom
            new FishSpecies
            {
                Name = "Lipljen",
                LatinName = "Thymallus thymallus",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 3,
                ClosedSeasonEndDay = 31, ClosedSeasonEndMonth = 5,
                MinLengthCm = 30
            },
            new FishSpecies
            {
                Name = "Mladica",
                LatinName = "Hucho hucho",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 3,
                ClosedSeasonEndDay = 31, ClosedSeasonEndMonth = 8,
                MinLengthCm = 100,
                Note = "Strogo zaštićena vrsta"
            },
            new FishSpecies
            {
                Name = "Smuđ",
                LatinName = "Stizostedion lucioperca",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 3,
                ClosedSeasonEndDay = 30, ClosedSeasonEndMonth = 4,
                MinLengthCm = 40
            },
            new FishSpecies
            {
                Name = "Smuđ kamenjar",
                LatinName = "Stizostedion volgense",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 3,
                ClosedSeasonEndDay = 30, ClosedSeasonEndMonth = 4,
                MinLengthCm = 25
            },
            new FishSpecies
            {
                Name = "Štuka",
                LatinName = "Esox lucius",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 2,
                ClosedSeasonEndDay = 31, ClosedSeasonEndMonth = 3,
                MinLengthCm = 40
            },
            new FishSpecies
            {
                Name = "Bucov",
                LatinName = "Aspius aspius",
                ClosedSeasonStartDay = 15, ClosedSeasonStartMonth = 4,
                ClosedSeasonEndDay = 15, ClosedSeasonEndMonth = 6,
                MinLengthCm = 30
            },
            new FishSpecies
            {
                Name = "Deverika",
                LatinName = "Abramis brama",
                ClosedSeasonStartDay = 15, ClosedSeasonStartMonth = 4,
                ClosedSeasonEndDay = 31, ClosedSeasonEndMonth = 5,
                MinLengthCm = 20
            },
            new FishSpecies
            {
                Name = "Grgeč",
                LatinName = "Perca fluviatilis",
                MinLengthCm = 10,
                Note = "Poznat i kao Bandar"
            },
            new FishSpecies
            {
                Name = "Jezerska zlatovčica",
                LatinName = "Salvelinus alpinus",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 10,
                ClosedSeasonEndDay = 28, ClosedSeasonEndMonth = 2,
                MinLengthCm = 25
            },
            new FishSpecies
            {
                Name = "Klen",
                LatinName = "Leuciscus cephalus",
                ClosedSeasonStartDay = 15, ClosedSeasonStartMonth = 4,
                ClosedSeasonEndDay = 31, ClosedSeasonEndMonth = 5,
                MinLengthCm = 20
            },
            new FishSpecies
            {
                Name = "Krkuša",
                LatinName = "Gobio gobio",
                MinLengthCm = 10
            },
            new FishSpecies
            {
                Name = "Manić",
                LatinName = "Lota lota",
                MinLengthCm = 20
            },
            new FishSpecies
            {
                Name = "Mrena",
                LatinName = "Barbus barbus",
                ClosedSeasonStartDay = 15, ClosedSeasonStartMonth = 4,
                ClosedSeasonEndDay = 31, ClosedSeasonEndMonth = 5,
                MinLengthCm = 25
            },
            new FishSpecies
            {
                Name = "Nosara",
                LatinName = "Vimba vimba",
                MinLengthCm = 15,
                Note = "Poznata i kao Šljivar"
            },
            new FishSpecies
            {
                Name = "Ohridska pastrmka",
                LatinName = "Salmo letnica",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 10,
                ClosedSeasonEndDay = 28, ClosedSeasonEndMonth = 2,
                MinLengthCm = 40
            },
            new FishSpecies
            {
                Name = "Plotica",
                LatinName = "Rutilus pigus",
                ClosedSeasonStartDay = 15, ClosedSeasonStartMonth = 4,
                ClosedSeasonEndDay = 31, ClosedSeasonEndMonth = 5,
                MinLengthCm = 20
            },
            new FishSpecies
            {
                Name = "Potočna mrena",
                LatinName = "Barbus peloponnesius",
                MinLengthCm = 15
            },
            new FishSpecies
            {
                Name = "Potočna pastrmka",
                LatinName = "Salmo trutta",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 10,
                ClosedSeasonEndDay = 28, ClosedSeasonEndMonth = 2,
                MinLengthCm = 25
            },
            new FishSpecies
            {
                Name = "Potočna zlatovčica",
                LatinName = "Salvelinus fontinalis",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 10,
                ClosedSeasonEndDay = 28, ClosedSeasonEndMonth = 2,
                MinLengthCm = 20
            },
            new FishSpecies
            {
                Name = "Jaz",
                LatinName = "Leuciscus idus",
                ClosedSeasonStartDay = 15, ClosedSeasonStartMonth = 4,
                ClosedSeasonEndDay = 31, ClosedSeasonEndMonth = 5,
                MinLengthCm = 20,
                Note = "Poznata i kao Protiš"
            },
            new FishSpecies
            {
                Name = "Šaran",
                LatinName = "Cyprinus carpio",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 4,
                ClosedSeasonEndDay = 31, ClosedSeasonEndMonth = 5,
                MinLengthCm = 30
            },
            new FishSpecies
            {
                Name = "Skobalj",
                LatinName = "Chondrostoma nasus",
                ClosedSeasonStartDay = 15, ClosedSeasonStartMonth = 4,
                ClosedSeasonEndDay = 31, ClosedSeasonEndMonth = 5,
                MinLengthCm = 20
            },
            new FishSpecies
            {
                Name = "Som",
                LatinName = "Silurus glanis",
                ClosedSeasonStartDay = 1, ClosedSeasonStartMonth = 5,
                ClosedSeasonEndDay = 15, ClosedSeasonEndMonth = 6,
                MinLengthCm = 60
            },
            new FishSpecies
            {
                Name = "Amur",
                LatinName = "Ctenopharyngodon idella",
                MinLengthCm = 40,
                Note = "Beli amur"
            },
            new FishSpecies
            {
                Name = "Babuška",
                LatinName = "Carassius gibelio",
                MinLengthCm = 0,
                Note = "Srebrni karaš"
            },

            // Trajno zaštićene vrste
            new FishSpecies
            {
                Name = "Čikov",
                LatinName = "Misgurnus fossilis",
                IsProtected = true,
                MinLengthCm = 0
            },
            new FishSpecies
            {
                Name = "Dunavska jesetra",
                LatinName = "Acipenser gueldenstaedti",
                IsProtected = true,
                MinLengthCm = 0
            },
            new FishSpecies
            {
                Name = "Kečiga",
                LatinName = "Acipenser ruthenus",
                IsProtected = true,
                MinLengthCm = 0
            },
            new FishSpecies
            {
                Name = "Linjak",
                LatinName = "Tinca tinca",
                IsProtected = true,
                MinLengthCm = 0
            },
            new FishSpecies
            {
                Name = "Moruna",
                LatinName = "Huso huso",
                IsProtected = true,
                MinLengthCm = 0
            },
            new FishSpecies
            {
                Name = "Zlatni karaš",
                LatinName = "Carassius carassius",
                IsProtected = true,
                MinLengthCm = 0
            },

            // Invazivne vrste (bez ograničenja)
            new FishSpecies
            {
                Name = "Sunčica",
                LatinName = "Lepomis gibbosus",
                IsInvasive = true,
                MinLengthCm = 0,
                Note = "Invazivna vrsta"
            },
            new FishSpecies
            {
                Name = "Bezribica",
                LatinName = "Pseudorasbora parva",
                IsInvasive = true,
                MinLengthCm = 0,
                Note = "Invazivna vrsta"
            }
        ];
    }

    public IReadOnlyList<FishSpecies> GetAllSpecies() => _species.AsReadOnly();

    public FishSpecies? GetSpeciesByName(string name)
    {
        return _species.FirstOrDefault(s =>
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public FishingStatus GetFishingStatus(string fishName, DateTime date)
    {
        var species = GetSpeciesByName(fishName);
        if (species == null) return FishingStatus.Allowed;

        if (species.IsProtected) return FishingStatus.Protected;
        if (species.IsInvasive) return FishingStatus.Allowed;
        if (species.IsInClosedSeason(date)) return FishingStatus.ClosedSeason;

        return FishingStatus.Allowed;
    }

    public IReadOnlyList<FishSpecies> GetSpeciesInClosedSeason(DateTime date)
    {
        return _species
            .Where(s => !s.IsProtected && !s.IsInvasive && s.IsInClosedSeason(date))
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<FishSpecies> GetProtectedSpecies()
    {
        return _species.Where(s => s.IsProtected).ToList().AsReadOnly();
    }

    public IReadOnlyList<FishSpecies> GetInvasiveSpecies()
    {
        return _species.Where(s => s.IsInvasive).ToList().AsReadOnly();
    }
}
