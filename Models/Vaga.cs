namespace Loop.Models;

public record Vaga(int Id, string Titulo, string Endereco,
    double Latitude, double Longitude, decimal PrecoPorHora);

public record VagaProxima(Vaga Vaga, double DistanciaKm);
