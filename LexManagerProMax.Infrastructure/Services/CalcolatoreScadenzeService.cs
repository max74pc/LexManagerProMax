using LexManagerProMax.Application.Interfaces;

namespace LexManagerProMax.Infrastructure.Services;

//public class CalcolatoreScadenzeService : ICalcolatoreScadenzeService
//{
//    private static readonly HashSet<DateTime> FestivitaItalia = ComputeFestivita();

//    public static DateTime CalcolaScadenza(
//        DateTime dataDecorrenza,
//        int giorniTermine,
//        bool escludiSabato = true,
//        bool escludiDomenica = true)
//    {
//        var data = dataDecorrenza;
//        var giorniContati = 0;

//        while (giorniContati < giorniTermine)
//        {
//            data = data.AddDays(1);

//            var isWeekend = (data.DayOfWeek == DayOfWeek.Saturday && escludiSabato)
//                         || (data.DayOfWeek == DayOfWeek.Sunday && escludiDomenica);

//            var isFestivo = FestivitaItalia.Contains(data.Date)
//                         || IsFestaRepubblica(data)
//                         || IsFestaLavoro(data);

//            if (!isWeekend && !isFestivo)
//                giorniContati++;
//        }

//        return data;
//    }

//    private static bool IsFestaRepubblica(DateTime d) => d.Month == 6 && d.Day == 2;
//    private static bool IsFestaLavoro(DateTime d) => d.Month == 5 && d.Day == 1;

//    private static HashSet<DateTime> ComputeFestivita()
//    {
//        var year = DateTime.Now.Year;
//        return
//        [
//            new DateTime(year, 1, 1),   // Capodanno
//            new DateTime(year, 1, 6),   // Epifania
//            new DateTime(year, 4, 25),  // Liberazione
//            new DateTime(year, 8, 15),  // Ferragosto
//            new DateTime(year, 11, 1),  // Tutti i santi
//            new DateTime(year, 12, 8),  // Immacolata
//            new DateTime(year, 12, 25), // Natale
//            new DateTime(year, 12, 26), // S. Stefano
//        ];
//    }
//}
