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

## Etapa 2: vagas próximas ao destino

Endpoint: `GET /api/vagas/proximas?latitude=-23.5568602&longitude=-46.6614121&raioKm=2`

Execute o servidor como explicado acima e abra:

http://localhost:5080/api/vagas/proximas?latitude=-23.5568602&longitude=-46.6614121&raioKm=2

A resposta inclui o destino, o raio, o total e as vagas com `distanciaKm`.
Esse exemplo retorna as vagas 1 e 2, nessa ordem. A vaga 1 fica no próprio ponto
pesquisado (0 km); a vaga 2 fica a aproximadamente 0,556 km. A vaga 3 está fora
do raio. Trocar `raioKm` por `0.1` retorna somente a vaga 1; por `10`, as três.
Coordenadas `0,0` retornam lista vazia com esse conjunto de demonstração.

Como usar com a primeira etapa: pesquise o endereço em `/api/localizacoes/buscar`,
escolha um dos resultados e copie a latitude e a longitude para `/api/vagas/proximas`.
Não escolhemos automaticamente o primeiro endereço, pois a busca pode ser ambígua.
Use ponto decimal na URL, como `-23.5568602`.

### Arquivos e lógica

- `Models/Vaga.cs` define uma vaga e o resultado de uma busca com distância.
- `Services/VagasService.cs` guarda três vagas fictícias em memória e calcula
  a distância de cada uma ao destino. Depois filtra pelo raio e ordena por distância.
- `Controllers/VagasController.cs` recebe as coordenadas e valida os parâmetros.
  Latitude e longitude são obrigatórias. Latitude vai de -90 a 90; longitude,
  de -180 a 180. O raio vai de 0,1 a 50 km e, se omitido, vale 2 km.
- `Program.cs` registra o novo serviço para que o controller possa recebê-lo.

`Select` transforma cada vaga em um resultado com distância. `Where` mantém
somente quem está dentro do raio. `OrderBy` coloca a menor distância primeiro.
O filtro usa a distância completa; apenas a resposta é arredondada a três casas.

A fórmula de Haversine usa latitude e longitude para aproximar a distância
na superfície da Terra, considerada uma esfera com raio de 6.371 km.
Ela representa distância em linha reta sobre essa superfície, e não trajeto
pelas ruas, tempo de viagem ou distância percorrida a pé.

As vagas e suas coordenadas são fictícias e independentes dos anúncios do
JavaScript. Ainda não há banco de dados, cadastro de vagas na API ou integração
com a interface. O filtro por disponibilidade é explicado na etapa 5. Não é feita uma nova
consulta ao Nominatim para calcular distâncias.

Teste de validação: omita `latitude` ou envie `latitude=100`. Deve retornar HTTP 400.
Busca válida sem vagas no raio retorna HTTP 200 com `total: 0` e `resultados: []`.

Para apresentar: “Após escolher as coordenadas do destino, a API calcula a distância
até cada vaga cadastrada, seleciona as que estão dentro do raio e ordena da mais
próxima à mais distante. Nesta versão usamos dados de demonstração em memória.”

## Etapa 3: criar reserva sem pagamento

`POST /api/reservas` cria uma reserva e `GET /api/reservas/{id}` consulta uma
reserva existente. POST envia dados no corpo da requisição, em JSON; não é possível
testar a criação apenas colando uma URL na barra do navegador.

Com o servidor rodando em um terminal, abra outro terminal PowerShell e execute:

```powershell
$amanha = [DateTimeOffset]::Now.ToOffset([TimeSpan]::FromHours(-3)).Date.AddDays(1)
$inicio = [DateTimeOffset]::new($amanha.AddHours(10), [TimeSpan]::FromHours(-3))
$fim = $inicio.AddHours(2)
$corpo = @{
    vagaId = 1
    motoristaId = 'motorista-demo'
    inicio = $inicio.ToString('o')
    fim = $fim.ToString('o')
} | ConvertTo-Json

$reserva = Invoke-RestMethod -Method Post -Uri 'http://localhost:5080/api/reservas' -ContentType 'application/json' -Body $corpo
$reserva | ConvertTo-Json
```

O script escolhe um início para amanhã e um fim duas horas depois. `ToString('o')`
formata a data com o fuso horário. Use datas ISO 8601 com fuso explícito nas chamadas,
por exemplo `2026-10-20T10:00:00-03:00`. O backend usa `DateTimeOffset` para comparar
instantes e armazena as datas em UTC, indicado por `+00:00` na resposta.

Ao criar, a API retorna HTTP 201, a reserva e um cabeçalho `Location` com o endereço
para consultá-la. A resposta contém `id`, `vagaId`, `motoristaId`, `inicio`, `fim`,
`criadaEm` e `estado` (inicialmente `Confirmada`). Não há cobrança nesta etapa.

Para consultar a reserva criada:

```powershell
Invoke-RestMethod -Uri "http://localhost:5080/api/reservas/$($reserva.id)"
```

Para testar um conflito, repita a chamada POST usando o mesmo `$corpo`. Ela deve
retornar HTTP 409. O PowerShell mostrará um erro porque recebeu esse código HTTP.

### Arquivos e regras

- `Models/Reserva.cs`: define a reserva e os campos do pedido de criação.
- `Services/ReservasService.cs`: valida o período, verifica a vaga e os conflitos,
  cria um identificador `Guid` e guarda a reserva em um dicionário em memória.
- `Controllers/ReservasController.cs`: recebe JSON e traduz os resultados do serviço
  em respostas HTTP. O ASP.NET valida os campos obrigatórios antes de executar a ação.
- `Services/VagasService.cs`: ganhou uma consulta por ID para verificar se a vaga existe.
- `Program.cs`: registra uma instância compartilhada do serviço de reservas.

O início deve estar no futuro e o fim deve ser posterior ao início. Erros de campos
ou de período retornam HTTP 400. Vaga inexistente retorna 404. Conflito retorna 409.
Consultar um ID de reserva inexistente também retorna 404.

Dois períodos conflitam quando `novoInicio < fimExistente` e
`novoFim > inicioExistente`. Isso detecta sobreposição parcial, total e períodos
iguais. Uma reserva das 10h às 12h permite outra das 12h às 14h na mesma vaga.
Reservas de vagas diferentes não conflitam entre si.

`lock` protege o trecho que verifica e grava: sem ele, duas chamadas simultâneas
poderiam verificar a disponibilidade antes de qualquer uma gravar e ambas seriam
aceitas. O bloqueio funciona nesta única instância; um banco de dados exigirá uma
estratégia de concorrência própria, especialmente com múltiplas instâncias.

As reservas desaparecem ao reiniciar. `motoristaId` é apenas uma identificação
de demonstração: não há autenticação nem validação de usuário cadastrado. A consulta
por ID também não verifica o dono. O frontend ainda não utiliza esses endpoints.
Os horários de funcionamento e o filtro por período são explicados na etapa 5.
A máquina de estados é explicada na etapa 4 abaixo; o cancelamento, na etapa 6.

Para apresentar: “O cliente envia a vaga e o período. A API valida as datas,
verifica se a vaga existe e se está livre naquele intervalo. Se estiver livre,
cria uma reserva confirmada sem pagamento e devolve seu identificador.”

## Etapa 4: máquina de estados

O estado agora usa `enum EstadoReserva`, com valores definidos em C#, em vez de
texto livre. O JSON continua enviando os nomes como texto, como `Confirmada`.

| Estado atual | Operação | Próximo estado | Regra de horário |
| --- | --- | --- | --- |
| Confirmada | iniciar | EmAndamento | No início ou depois dele, antes do fim |
| EmAndamento | concluir | Concluida | No fim ou depois dele |
| Confirmada | concluir | Concluida | No fim ou depois dele, mesmo sem início registrado |
| Confirmada | cancelar | Cancelada | Antes do início |

Outras transições são rejeitadas. `Concluida` e `Cancelada` são finais: não permitem iniciar ou concluir
novamente. A conclusão direta de uma confirmada evita que uma reserva sem registro
de início fique impossível de encerrar. Isso não comprova que o motorista usou a vaga.

Endpoints (sem corpo JSON):

- `POST /api/reservas/{id}/iniciar`
- `POST /api/reservas/{id}/concluir`

Sucesso retorna HTTP 200 e a reserva atualizada. Reserva inexistente retorna 404;
estado ou horário incompatível retorna 409. Pedidos repetidos são rejeitados com
409. As mudanças são explícitas: o relógio não altera o estado automaticamente.
Depois de uma alteração, GET retorna o novo estado.

`ReservasService.AlterarEstado` concentra a máquina de estados: primeiro encontra
a reserva, depois verifica a transição, valida o horário e substitui o registro.
Tudo ocorre dentro do mesmo `lock` usado na criação. `with` cria uma cópia do
registro com o novo estado. O controller apenas transforma o resultado em HTTP.

### Teste rápido no PowerShell

Inicie o servidor em um terminal e execute este script em outro. Ele usa períodos
curtos para não precisar esperar horas. Execute o bloco inteiro de uma vez:

```powershell
$inicio = [DateTimeOffset]::Now.AddSeconds(5)
$corpo = @{
    vagaId = 3
    motoristaId = 'motorista-estados-demo'
    inicio = $inicio.ToString('o')
    fim = $inicio.AddSeconds(10).ToString('o')
} | ConvertTo-Json
$reserva = Invoke-RestMethod -Method Post -Uri 'http://localhost:5080/api/reservas' -ContentType 'application/json' -Body $corpo
$url = "http://localhost:5080/api/reservas/$($reserva.id)"

Start-Sleep -Seconds 6
Invoke-RestMethod -Method Post -Uri "$url/iniciar"

Start-Sleep -Seconds 10
Invoke-RestMethod -Method Post -Uri "$url/concluir"
Invoke-RestMethod -Uri $url
```

Você verá `EmAndamento` e depois `Concluida`. Tentar iniciar a reserva concluída
deve retornar 409. Uma reserva criada para amanhã também rejeita início e conclusão
hoje. Como a memória é compartilhada, conflitos de horário continuam valendo para
novos testes; reiniciar o servidor limpa os dados de demonstração.

Não há autenticação nem mudança automática por tarefa agendada. O cancelamento
é explicado na etapa 6.
Esses endpoints demonstram as regras; a identidade e a autorização do motorista
ainda precisarão ser verificadas antes de disponibilizar o sistema a usuários reais.

Para apresentar: “A máquina de estados define as mudanças permitidas no ciclo da
reserva. A API verifica o estado atual e o horário antes de aceitar a operação,
impedindo, por exemplo, iniciar uma reserva já concluída.”

## Etapa 5: conflitos e disponibilidade

As vagas agora têm `horaAbertura` e `horaFechamento` em horas inteiras:
vaga 1, 8h–22h; vaga 2, 8h–18h; vaga 3, 0h–24h. Funcionam todos os dias.
O fuso fixo da demonstração é UTC-03:00, independente do fuso da máquina ou do pedido.
Não há feriados, horários por dia da semana ou janelas noturnas nesta versão.

O período inteiro deve caber no funcionamento. É permitido começar na abertura e
terminar no fechamento. Nas vagas com fechamento diário, uma reserva não pode
atravessar a noite; a vaga 24 horas aceita períodos que atravessam dias.
Datas invertidas ou no passado retornam 400; fora do funcionamento retorna 409.

`VagasService.EstaNoHorarioDeFuncionamento` verifica o funcionamento.
`ReservasService.TemConflito` verifica sobreposição com reservas `Confirmada` ou
`EmAndamento`. `FiltrarDisponiveis` aplica essas regras à lista da busca, sob `lock`.
A criação também verifica tudo sob o mesmo bloqueio: uma busca não garante a vaga
até que a reserva seja criada. Reservas concluídas não bloqueiam horários.

### Testar a busca com datas

Com o servidor rodando e memória limpa, execute em outro terminal PowerShell:

```powershell
$amanha = [DateTimeOffset]::Now.ToOffset([TimeSpan]::FromHours(-3)).Date.AddDays(1)
$inicio = [DateTimeOffset]::new($amanha.AddHours(10), [TimeSpan]::FromHours(-3))
$fim = $inicio.AddHours(2)
$consulta = 'http://localhost:5080/api/vagas/proximas?latitude=-23.5568602&longitude=-46.6614121&raioKm=2'
$consulta += '&inicio=' + [Uri]::EscapeDataString($inicio.ToString('o'))
$consulta += '&fim=' + [Uri]::EscapeDataString($fim.ToString('o'))

# Antes da reserva: deve retornar vagas 1 e 2.
Invoke-RestMethod $consulta | ConvertTo-Json -Depth 5

$corpo = @{
    vagaId = 1
    motoristaId = 'motorista-disponibilidade-demo'
    inicio = $inicio.ToString('o')
    fim = $fim.ToString('o')
} | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'http://localhost:5080/api/reservas' -ContentType 'application/json' -Body $corpo

# Depois da reserva: deve retornar somente vaga 2 para esse período.
Invoke-RestMethod $consulta | ConvertTo-Json -Depth 5
```

Repita o POST para ver conflito (409). Para testar funcionamento, crie uma reserva
da vaga 1 das 7h às 9h ou das 21h às 23h: ambas devem retornar 409.
Das 12h às 14h é permitido após uma reserva das 10h às 12h.

`inicio` e `fim` são opcionais na busca, mas devem ser enviados juntos. Informar
apenas um retorna 400. Sem ambos, a busca continua geográfica e envia
`disponibilidadeVerificada: false`; com ambos, envia `true` e o período consultado.
URL usa datas ISO 8601; `EscapeDataString` protege caracteres como `+` do fuso.

Para apresentar: “Uma vaga próxima só aparece na busca por período se funcionar
durante todo o intervalo e não houver reserva ativa sobreposta. Na criação,
repetimos as verificações para evitar conflitos entre pedidos concorrentes.”

## Etapa 6: cancelamento de reserva

`POST /api/reservas/{id}/cancelar` cancela uma reserva sem corpo JSON.
Regra adotada nesta versão: somente reservas `Confirmada` podem ser canceladas,
e apenas antes do instante de início. `EmAndamento`, `Concluida` e `Cancelada`
rejeitam cancelamento. Uma confirmada cujo início já passou também rejeita.

Sucesso retorna HTTP 200 e o registro com `estado: "Cancelada"` e `canceladaEm`
em UTC. Reserva inexistente retorna 404; estado ou horário incompatível retorna
409. Repetir o cancelamento retorna 409, como nas demais operações de transição.

Não apagamos a reserva: o ID continua consultável via GET. O estado cancelado é
final e não pode voltar a confirmada. Para reservar novamente, cria-se outra
reserva, com um novo ID. Não há pagamento, estorno ou multa.

`EstadoReserva` ganhou `Cancelada`; `Reserva` ganhou `CanceladaEm` (nulo antes
do cancelamento). `ReservasService.Cancelar` usa a mesma máquina de estados,
incluindo o bloqueio compartilhado com criação e consulta de disponibilidade.
O controller ganhou a rota de cancelamento. `TemConflito` já considera apenas
reservas confirmadas e em andamento: por isso o cancelamento libera o período
automaticamente, sem precisar remover o registro.

### Teste completo no PowerShell

Inicie o servidor e use outro terminal. Com a memória limpa:

```powershell
$amanha = [DateTimeOffset]::Now.ToOffset([TimeSpan]::FromHours(-3)).Date.AddDays(1)
$inicio = [DateTimeOffset]::new($amanha.AddHours(10), [TimeSpan]::FromHours(-3))
$fim = $inicio.AddHours(2)
$corpo = @{
    vagaId = 1
    motoristaId = 'motorista-cancelamento-demo'
    inicio = $inicio.ToString('o')
    fim = $fim.ToString('o')
} | ConvertTo-Json

$reserva = Invoke-RestMethod -Method Post -Uri 'http://localhost:5080/api/reservas' -ContentType 'application/json' -Body $corpo
$url = "http://localhost:5080/api/reservas/$($reserva.id)"

# Cancela e consulta o registro preservado.
Invoke-RestMethod -Method Post -Uri "$url/cancelar"
Invoke-RestMethod -Uri $url

# O mesmo período está livre: cria outra reserva com novo ID.
Invoke-RestMethod -Method Post -Uri 'http://localhost:5080/api/reservas' -ContentType 'application/json' -Body $corpo
```

O cancelamento também faz a vaga voltar à busca por disponibilidade para aquele
período. Se criar outra reserva logo depois, ela volta a ocupar o intervalo.

Para apresentar: “Cancelar é uma mudança de estado, não uma exclusão. Preservamos
o registro e o momento do cancelamento, e o período deixa de bloquear a agenda.
A regra permite cancelamento de reserva confirmada somente antes do início.”

## Escopo atual

As seis etapas do backend foram implementadas com dados de demonstração em
memória. Ainda não há banco de dados, autenticação, autorização por motorista,
cadastro de vagas na API ou integração com a interface. As operações de reserva
continuam sem verificar a identidade de quem chama os endpoints.
