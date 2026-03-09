using LexManagerProMax.Application.Interfaces;
using MediatR;

namespace LexManagerProMax.Application.UseCases.Dashboard;

public record DashboardKpiDto(
    int FascicoliAperti,
    int ScadenzeOggi,
    int Scadenze7Giorni,
    int TotaleClienti,
    int FascicoliChiusiMese,
    int SessioniAIUsate
);

public record GetDashboardKpiQuery : IRequest<DashboardKpiDto>;

// TODO
//public class GetDashboardKpiQueryHandler(
   // IFascicoloRepository fascicoliRepo,
//    ISoggettoRepository soggettiRepo,
//    ISessioneAIRepository aiRepo)
//    : IRequestHandler<GetDashboardKpiQuery, DashboardKpiDto>
//{
//    public async Task<DashboardKpiDto> Handle(
//        GetDashboardKpiQuery request, CancellationToken ct)
//    {
//        var oggi = DateTime.Today;
//        var tra7gg = oggi.AddDays(7);
//        var inizioMese = new DateTime(oggi.Year, oggi.Month, 1);

//        //var fascicoliAperti = await fascicoliRepo.CountAttiviAsync(ct);
//        //var scadenzeOggi = await fascicoliRepo.CountScadenzeAsync(oggi, oggi, ct);
//        //var scadenze7Giorni = await fascicoliRepo.CountScadenzeAsync(oggi, tra7gg, ct);
//        //var totaleClienti = await soggettiRepo.CountClientiAsync(ct);
//        //var fascicoliChiusiMese = await fascicoliRepo.CountChiusiAsync(inizioMese, oggi, ct);
//        //var sessioniAI = await aiRepo.CountAsync(ct);

//        return new DashboardKpiDto(
//            fascicoliAperti,
//            scadenzeOggi,
//            scadenze7Giorni,
//            totaleClienti,
//            fascicoliChiusiMese,
//            sessioniAI);
//    }
//}