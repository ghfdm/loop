# Loop — etapa 1: localização em C#

O backend usa ASP.NET Core e .NET 10. Nesta etapa, ele transforma um endereço
em possíveis localizações (latitude e longitude) usando o Nominatim.
O frontend existente ainda é um protótipo e não chama esta API.

## Como executar

Instale o SDK .NET 10. No terminal, dentro da pasta `loop`, execute:

```powershell
dotnet run --urls http://localhost:5080
```

Com o terminal aberto, visite no navegador:

http://localhost:5080/api/localizacoes/buscar?endereco=Avenida%20Paulista,%20Sao%20Paulo

A resposta é JSON (um formato de dados) com `atribuicao`, `fonte` e `resultados`.
Cada resultado contém `endereco`, `latitude` e `longitude`. Pode haver mais de
um local com o mesmo nome: a escolha do destino será feita pelo usuário.
Uma busca sem correspondências retorna `resultados: []` e HTTP 200.

## Entendendo os arquivos

- `loop.csproj`: declara um projeto web em C# para .NET 10.
- `Program.cs`: inicia o servidor e registra as dependências. O ASP.NET Core
  fornece essas dependências automaticamente às classes que precisam delas.
- `Controllers/LocalizacoesController.cs`: recebe a chamada HTTP GET no endereço
  `/api/localizacoes/buscar`. Valida o texto e devolve a resposta ao cliente.
- `Services/NominatimService.cs`: faz a chamada HTTP ao Nominatim, lê o JSON,
  transforma as coordenadas de texto em números e guarda resultados em cache.
- `Models/Localizacao.cs`: define os três campos de uma localização.
- `appsettings.json`: permite trocar a URL do provedor e a identificação da aplicação.

Fluxo: usuário envia endereço → controller valida → serviço consulta cache ou
Nominatim → serviço converte os dados → controller devolve JSON.

## Conceitos para apresentar

**Endpoint** é uma combinação de caminho e método HTTP. Aqui usamos GET porque
estamos consultando dados. `endereco` é um parâmetro enviado na URL.

**Geocodificação** é transformar uma descrição de endereço em coordenadas.
Latitude indica posição norte/sul; longitude indica posição leste/oeste.
Isso será a base para comparar o destino com as vagas cadastradas no Loop.
O Nominatim não conhece nossas reservas nem a disponibilidade das vagas.
Ele também não desenha o mapa nem calcula rotas.

**async/await** permite aguardar a rede sem ocupar uma thread durante toda a espera.
O `CancellationToken` permite cancelar a espera quando o cliente desconecta.

**Cache** guarda até 500 buscas, por até 24 horas, na memória do servidor.
Uma busca repetida pode reutilizar o resultado. O cache desaparece ao reiniciar.
O `SemaphoreSlim` deixa passar uma consulta externa por vez. O serviço aguarda
um segundo após cada chamada externa antes da próxima. Isso funciona para uma
única instância do Loop; várias instâncias precisariam compartilhar o controle.

**Tratamento de erros**: endereço ausente, muito curto ou acima de 200 caracteres
retorna HTTP 400. Falha do provedor retorna 502; tempo de resposta excedido,
504. Os detalhes técnicos são registrados no terminal do servidor.

## Uso do serviço público

A política exige identificação da aplicação, atribuição e no máximo uma chamada
por segundo. A identificação enviada é `LoopAcademico/1.0`; pode ser personalizada
em `appsettings.json` com o nome do grupo e um contato real. A resposta inclui a
atribuição: quando houver interface integrada, ela também deve exibir os créditos.
As buscas devem ser feitas ao enviar uma pesquisa, sem autocomplete a cada tecla.
Não envie informações pessoais ou confidenciais ao serviço público.

- Política: https://operations.osmfoundation.org/policies/nominatim/
- Busca: https://nominatim.org/release-docs/latest/api/Search/

## Próxima etapa

Busca das vagas cadastradas no Loop por proximidade ao destino. Depois virão
reserva sem pagamento, estados da reserva, conflitos de horário e cancelamento,
em etapas separadas.
