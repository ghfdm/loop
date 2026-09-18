using Loop.Models;

namespace Loop.Services;

public sealed class VagasService
{
    // Dados e coordenadas fictícios para demonstrar a busca, sem banco de dados.
    private readonly Vaga[] _vagas =
    [
        new(1, "Vaga demonstrativa Paulista", "Região da Avenida Paulista, São Paulo",
            -23.5568602, -46.6614121, 12m),
        new(2, "Garagem demonstrativa Bela Vista", "Região da Bela Vista, São Paulo",
            -23.5618602, -46.6614121, 10m),
        new(3, "Vaga demonstrativa Santana", "Região de Santana, São Paulo",
            -23.5000, -46.6250, 8m)
    ];

    public VagaProxima[] BuscarProximas(double latitude, double longitude, double raioKm)
    {
        return _vagas
            .Select(vaga => new VagaProxima(vaga,
                CalcularDistanciaKm(latitude, longitude, vaga.Latitude, vaga.Longitude)))
            // Filtramos antes de arredondar para não incluir vagas fora do raio.
            .Where(resultado => resultado.DistanciaKm <= raioKm)
            .OrderBy(resultado => resultado.DistanciaKm)
            .ThenBy(resultado => resultado.Vaga.Id)
            .Select(resultado => resultado with { DistanciaKm = Math.Round(resultado.DistanciaKm, 3) })
            .ToArray();
    }

    // Fórmula de Haversine: distância entre dois pontos na superfície de uma esfera.
    private static double CalcularDistanciaKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double raioTerraKm = 6371;
        static double Radianos(double graus) => graus * Math.PI / 180;

        var diferencaLat = Radianos(lat2 - lat1);
        var diferencaLon = Radianos(lon2 - lon1);
        var a = Math.Pow(Math.Sin(diferencaLat / 2), 2)
            + Math.Cos(Radianos(lat1)) * Math.Cos(Radianos(lat2))
            * Math.Pow(Math.Sin(diferencaLon / 2), 2);

        return 2 * raioTerraKm * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }
}
