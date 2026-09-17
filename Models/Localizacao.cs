namespace Loop.Models;

// Dados que o Loop devolve para quem pesquisou um endereço.
public record Localizacao(string Endereco, double Latitude, double Longitude);
