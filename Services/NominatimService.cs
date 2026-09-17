using System.Globalization;
using System.Text.Json.Serialization;
using Loop.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Loop.Services;

public sealed class NominatimService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
{
    private readonly SemaphoreSlim _controle = new(1, 1);
    private DateTimeOffset _proximaConsulta = DateTimeOffset.MinValue;

    public async Task<Localizacao[]> BuscarAsync(string endereco, CancellationToken cancellationToken)
    {
        var chave = "endereco:" + endereco.Trim().ToUpperInvariant();
        if (cache.TryGetValue(chave, out Localizacao[]? armazenado))
            return armazenado!;

        // Apenas uma consulta externa por vez, com intervalo mínimo de um segundo.
        await _controle.WaitAsync(cancellationToken);
        try
        {
            // Outro usuário pode ter pesquisado o mesmo endereço enquanto aguardávamos.
            if (cache.TryGetValue(chave, out armazenado))
                return armazenado!;

            var espera = _proximaConsulta - DateTimeOffset.UtcNow;
            if (espera > TimeSpan.Zero)
                await Task.Delay(espera, cancellationToken);

            var client = httpClientFactory.CreateClient("Nominatim");
            var url = "search?format=jsonv2&limit=5&accept-language=pt-BR&q="
                + Uri.EscapeDataString(endereco.Trim());

            NominatimResultado[] dados;
            try
            {
                dados = await client.GetFromJsonAsync<NominatimResultado[]>(url, cancellationToken)
                    ?? throw new System.Text.Json.JsonException("Resposta vazia do Nominatim.");
            }
            finally
            {
                // Também espaçamos as chamadas quando o serviço externo falha.
                _proximaConsulta = DateTimeOffset.UtcNow.AddSeconds(1);
            }

            var locais = dados.Select(d => new Localizacao(
                d.Endereco,
                double.Parse(d.Latitude, CultureInfo.InvariantCulture),
                double.Parse(d.Longitude, CultureInfo.InvariantCulture))).ToArray();

            cache.Set(chave, locais, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                Size = 1
            });
            return locais;
        }
        finally
        {
            _controle.Release();
        }
    }

    // O Nominatim envia as coordenadas como texto, usando ponto decimal.
    private sealed class NominatimResultado
    {
        [JsonPropertyName("display_name")]
        public required string Endereco { get; init; }
        [JsonPropertyName("lat")]
        public required string Latitude { get; init; }
        [JsonPropertyName("lon")]
        public required string Longitude { get; init; }
    }
}
