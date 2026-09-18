namespace Loop.Models;

public record Vaga(int Id, string Titulo, string Endereco,
    double Latitude, double Longitude, decimal PrecoPorHora,
    int HoraAbertura, int HoraFechamento);

public record VagaProxima(Vaga Vaga, double DistanciaKm);
