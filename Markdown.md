Atue como um **Product Manager Sênior, Scrum Master e Solution Architect**, com experiência em produtos digitais, arquitetura .NET/Azure, definição de MVP e organização de backlog para Azure DevOps.

Sua missão é criar o **backlog completo de um MVP simplificado para uma plataforma web colaborativa de compartilhamento de fotografias do meu casamento**.

O objetivo inicial é atender exclusivamente **um único casamento**, porém a arquitetura deve evitar decisões que impeçam uma futura evolução para uma plataforma comercial.

---

# 1. Contexto do Produto

O produto será uma aplicação web para que **todos os convidados do casamento possam compartilhar fotografias do evento em uma galeria colaborativa**.

O acesso acontecerá principalmente através de um **QR Code disponibilizado no local do evento**.

Ao acessar o evento, qualquer convidado deverá conseguir:

- visualizar os álbuns;
- navegar pelas fotos;
- abrir fotos em fullscreen;
- fazer upload de suas próprias fotos;
- escolher em qual álbum deseja publicar;
- marcar fotos como favoritas;
- baixar fotos individualmente;
- baixar suas fotos favoritas em lote.

A ideia central do produto é:

> **Todos tiram fotos → todos fazem upload → todos visualizam as fotos de todos.**

Não haverá necessidade de cadastro ou login para os convidados.

---

# 2. Premissas do MVP

Considere:

- 1 único evento;
- evento = casamento;
- aproximadamente 130 convidados;
- utilização concentrada no dia do casamento e nos dias seguintes;
- até 5.000 fotografias;
- até 4 álbuns;
- imagens de até 20 MB;
- todos os convidados podem realizar upload;
- todos os convidados podem visualizar as fotos;
- todos os convidados podem favoritar;
- todos os convidados podem realizar downloads;
- não haverá cadastro de convidados;
- não haverá autenticação para convidados;
- haverá somente uma área administrativa;
- não haverá múltiplos fotógrafos;
- não haverá múltiplos clientes;
- não haverá cobrança;
- não haverá multi-tenant no MVP.

---

# 3. Conceito de acesso

Existirão dois níveis de acesso.

## Público

Exemplo:

```text
/casamento-rodrigo-juliana

```

Qualquer pessoa que tenha acesso ao link poderá:

- visualizar;
- fazer upload;
- favoritar;
- baixar.

## Administrador

Exemplo:

```text
/admin

```

O administrador poderá:

- configurar o evento;
- configurar os 4 álbuns;
- definir capa;
- realizar uploads;
- excluir fotos;
- acompanhar processamento;
- visualizar estatísticas básicas.

---

# 4. Funcionalidades do MVP

O MVP deverá obrigatoriamente possuir:

1. Capa do evento
2. 4 álbuns
3. Upload colaborativo
4. Upload múltiplo
5. Geração de thumbnails
6. Geração de imagem otimizada para visualização
7. Galeria pública
8. Infinite Scroll
9. Lazy Loading
10. Visualização fullscreen
11. Download individual
12. Favoritos
13. Download de favoritos em lote
14. QR Code
15. URL pública
16. Área administrativa simples
17. Identificação da sessão do convidado
18. Exclusão de fotos pelo administrador

---

# 5. Upload colaborativo

Esta é uma funcionalidade central do produto.

Qualquer pessoa com acesso ao evento poderá selecionar fotografias e fazer upload.

Fluxo:

```text
Convidado
   ↓
Escolhe álbum
   ↓
Seleciona múltiplas fotos
   ↓
API autoriza upload
   ↓
Upload direto no Blob Storage
   ↓
Registro dos metadados
   ↓
Processamento
   ↓
Foto disponível

```

A aplicação não deverá exigir cadastro.

---

# 6. GuestSessionId

Cada visitante deverá receber um identificador persistente de sessão/dispositivo.

Exemplo:

```text
GuestSessionId = GUID

```

Armazenar no navegador utilizando uma estratégia apropriada.

Esse identificador deverá ser utilizado para:

- associar uploads;
- associar favoritos;
- permitir futuramente a funcionalidade "Minhas Fotos".

Não criar uma entidade de usuário/convidado no MVP.

---

# 7. Banco de Dados

Utilizar:

- Azure SQL;
- Entity Framework Core.

Entidades principais:

```text
Events
Albums
Photos
Favorites

```

A entidade `Photo` deverá possuir, no mínimo:

```text
Id
AlbumId
FileName
OriginalPath
DisplayPath
ThumbnailPath
FileSize
Width
Height
Status
UploadedBySessionId
CreatedAt

```

Definir:

- PK;
- FK;
- índices;
- constraints;
- estratégia de paginação;
- tratamento de concorrência;
- prevenção de registros duplicados.

Sugestão de índice:

```text
Photos
(
    AlbumId,
    CreatedAt,
    Id
)

```

E considerar índices apropriados para:

```text
Favorites
(
    EventId,
    PhotoId,
    GuestSessionId
)

```

---

# 8. Azure Blob Storage

Todas as imagens deverão ser armazenadas exclusivamente no Blob Storage.

Nunca armazenar os arquivos binários no SQL.

Utilizar:

```text
Original
Display
Thumbnail

```

Sugestão:

```text
/events/{eventId}/albums/{albumId}/original/
events/{eventId}/albums/{albumId}/display/
events/{eventId}/albums/{albumId}/thumbnail/

```

---

# 9. Segurança do Upload

Mesmo sem autenticação de convidados, o sistema deve controlar o acesso ao Storage.

Não expor:

- Storage Account Key;
- Connection String;
- credenciais permanentes.

A API deverá gerar autorização temporária, preferencialmente através de SAS.

O upload deverá ser limitado por:

- evento;
- álbum;
- tamanho;
- tipo de arquivo.

Permitir inicialmente:

```text
JPG
JPEG
PNG
WEBP

```

Tamanho máximo:

```text
20 MB por arquivo

```

---

# 10. Upload múltiplo

O frontend deverá suportar múltiplos arquivos.

Cada arquivo deverá possuir:

- nome;
- tamanho;
- progresso;
- estado;
- erro;
- retry.

Estados sugeridos:

```text
Pending
Uploading
Uploaded
Processing
Available
Failed

```

Uma falha individual não deverá interromper os demais uploads.

Controlar a quantidade de uploads simultâneos para evitar sobrecarga do navegador e do Storage.

---

# 11. Processamento das imagens

Após o upload deverão ser gerados:

```text
Original
Display
Thumbnail

```

Fluxo:

```text
Original
   ↓
Processamento
   ├── Display
   └── Thumbnail

```

O processamento não deverá bloquear desnecessariamente as requisições HTTP.

No MVP, avaliar processamento em background dentro da aplicação.

Não é obrigatório utilizar Azure Functions.

Também apresentar uma estratégia futura utilizando:

```text
Queue
+
Azure Function

```

---

# 12. Galeria colaborativa

Todos os convidados poderão visualizar todas as fotos disponíveis.

A galeria deverá apresentar os quatro álbuns.

As fotos deverão aparecer em ordem cronológica, salvo configuração diferente.

Utilizar:

- infinite scroll;
- lazy loading;
- paginação;
- thumbnails;
- carregamento sob demanda.

Não carregar 5.000 fotografias simultaneamente.

Utilizar cursor-based pagination.

---

# 13. Atualização das fotos

Não utilizar Azure SignalR no MVP.

A nova foto poderá:

1. aparecer após refresh;
2. ser descoberta por polling leve;
3. ou utilizar outra estratégia simples.

Avalie qual solução é melhor para o contexto de 130 convidados.

Considere também a alternativa:

> O convidado pode fazer upload e continuar navegando normalmente; o frontend verifica periodicamente se existem novas fotos.

A solução deve priorizar simplicidade.

---

# 14. Fullscreen

Permitir:

- abrir foto;
- próxima;
- anterior;
- fechar;
- swipe mobile;
- teclado desktop.

Utilizar `Display` e não `Original`.

---

# 15. Download individual

Permitir download do original.

Utilizar URL temporária/controlada.

O Blob Storage deve permanecer protegido.

Avaliar o uso de SAS.

---

# 16. Favoritos

Qualquer convidado poderá favoritar fotos.

Não haverá login.

Utilizar:

```text
GuestSessionId

```

O convidado deverá conseguir:

- favoritar;
- desfavoritar;
- visualizar favoritos;
- visualizar quantidade de favoritos.

Evitar duplicidade através de constraint no banco.

---

# 17. Download dos favoritos

Permitir download em lote.

Para simplificar o MVP:

- limite de 200 fotos por pacote;
- evitar processamento ilimitado;
- avaliar geração síncrona;
- controlar consumo de memória.

Caso a geração síncrona não seja segura, propor uma implementação simples em background.

Não adicionar Queue + Function automaticamente apenas por boas práticas.

---

# 18. Capa

O administrador poderá selecionar uma fotografia como capa.

A capa será utilizada na página principal do evento.

---

# 19. Álbuns

O evento possuirá quatro álbuns.

O administrador poderá:

- alterar nome;
- alterar ordem;
- ativar/desativar;
- excluir/conteúdo conforme regras definidas.

O convidado poderá:

- acessar;
- visualizar;
- realizar upload no álbum.

---

# 20. Administração

Criar:

```text
/admin

```

O administrador poderá:

- gerenciar evento;
- gerenciar álbuns;
- definir capa;
- visualizar uploads;
- visualizar processamento;
- excluir fotos;
- visualizar quantidade de fotos;
- identificar falhas.

Não criar:

- RBAC complexo;
- gerenciamento de múltiplos usuários;
- gestão de fotógrafos;
- multi-tenant.

---

# 21. Moderação

Como qualquer convidado pode fazer upload, definir uma estratégia mínima de moderação.

Para o MVP:

```text
Upload
 ↓
Processing
 ↓
Available

```

A publicação será automática.

O administrador deverá possuir capacidade de:

```text
Excluir foto

```

Não criar fluxo de aprovação manual inicialmente.

Também avaliar:

- limite de upload por sessão;
- validação de extensão;
- validação de MIME type;
- limite de tamanho;
- proteção contra arquivos maliciosos.

---

# 22. Identificação de origem

O sistema deverá registrar o `GuestSessionId` responsável pelo upload.

Não exibir necessariamente essa informação para os convidados no MVP.

Essa informação será útil para futura evolução para:

- "Minhas Fotos";
- histórico;
- moderação;
- identificação de abuso.

---

# 23. Segurança

Implementar no mínimo:

- HTTPS;
- CORS;
- validação de input;
- validação de arquivo;
- limite de tamanho;
- rate limiting básico;
- proteção da área administrativa;
- SAS temporário;
- Blob Storage privado;
- secrets fora do código;
- tratamento global de exceções.

Avaliar Azure Key Vault.

---

# 24. Arquitetura

Utilizar:

```text
React
TypeScript
Vite

.NET 10
ASP.NET Core
Entity Framework Core

Azure SQL
Azure Blob Storage

Application Insights

```

Estrutura:

```text
API
Application
Domain
Infrastructure

```

Evitar:

- Repository genérico;
- abstrações desnecessárias;
- microservices;
- mensageria desnecessária;
- cache distribuído;
- arquitetura distribuída.

---

# 25. Azure

Considerando que haverá apenas um casamento, avaliar a infraestrutura mínima.

Priorizar:

```text
Azure App Service
Azure SQL
Azure Blob Storage
Application Insights
Azure Key Vault

```

Não utilizar inicialmente:

```text
Azure Front Door
Azure SignalR
Azure Service Bus
Redis

```

Azure Functions somente se houver justificativa para o processamento das imagens.

Explique quando cada serviço deverá ser introduzido numa futura V2.

---

# 26. Performance

Considerar:

- 130 convidados;
- até 5.000 fotos;
- arquivos de até 20 MB;
- muitos uploads durante o evento;
- navegação simultânea;
- downloads.

Evitar que:

- a API trafegue os arquivos;
- imagens originais sejam utilizadas na galeria;
- milhares de imagens sejam carregadas simultaneamente;
- geração de ZIP consuma toda a memória.

---

# 27. Concorrência de Upload

O sistema deverá suportar diversos convidados realizando upload simultaneamente.

Considere um cenário em que várias pessoas estejam enviando fotos ao mesmo tempo.

Avaliar:

- concorrência de uploads;
- quantidade de uploads simultâneos por cliente;
- Blob Storage;
- banco de dados;
- criação de thumbnails;
- conflitos;
- retry;
- idempotência.

Não superdimensionar a solução.

---

# 28. DevOps

Utilizar:

- Git;
- Azure DevOps ou GitHub;
- CI/CD.

Pipeline mínima:

```text
Commit
 ↓
Build
 ↓
Tests
 ↓
Deploy

```

Ambientes:

```text
DEV
PROD

```

---

# 29. Observabilidade

Utilizar Application Insights.

Registrar:

- requests;
- erros;
- uploads;
- processamento;
- downloads;
- exceptions.

Adicionar:

- correlation ID;
- logs estruturados;
- health check.

---

# 30. Backlog

Organizar:

```text
Epic
 └── Feature
      └── User Story
           └── Task

```

Para cada Epic:

- objetivo de negócio;
- Features;
- dependências;
- prioridade.

Para cada Feature:

- objetivo;
- User Stories;
- critérios de aceite;
- complexidade.

Para cada User Story:

- descrição;
- valor de negócio;
- prioridade;
- critérios de aceite;
- Tasks Backend;
- Tasks Frontend;
- Tasks Banco;
- Tasks Azure;
- Tasks DevOps;
- dependências.

---

# 31. Priorização

Utilizar:

```text
P0 = obrigatório
P1 = importante
P2 = pós-MVP
P3 = futuro

```

Criar tabela consolidada:

```text
ID
Epic
Feature
User Story
Prioridade
Complexidade
Sprint
Dependências

```

---

# 32. Roadmap

Criar roadmap em Sprints de 2 semanas.

Preferencialmente:

```text
Sprint 1
Fundação + Banco + Evento + Storage

Sprint 2
Upload colaborativo + Processamento + Galeria

Sprint 3
Fullscreen + Download + Favoritos + QR Code + Hardening

```

Pode alterar essa divisão caso exista uma organização melhor.

---

# 33. Riscos técnicos

Identificar riscos relacionados a:

- múltiplos convidados fazendo upload simultaneamente;
- uploads de arquivos grandes;
- consumo de memória;
- geração de ZIP;
- processamento de imagens;
- milhares de fotografias;
- banco;
- Blob;
- segurança;
- spam/abuso;
- fotos duplicadas;
- performance mobile;
- custo de Storage;
- custo de transferência.

Para cada risco apresentar:

- probabilidade;
- impacto;
- mitigação.

---

# 34. MVP mínimo

Definir claramente qual é a menor versão que ainda entrega a proposta de valor.

O fluxo mínimo deverá ser:

```text
QR Code
   ↓
Evento
   ↓
Álbum
   ↓
Ver fotos
   ↓
Enviar foto
   ↓
Foto processada
   ↓
Todos visualizam
   ↓
Favoritar
   ↓
Baixar

```

---

# 35. V2

Apresentar possibilidades futuras:

- múltiplos eventos;
- múltiplos fotógrafos;
- contas de usuários;
- autenticação;
- "Minhas Fotos";
- moderação;
- aprovação de fotos;
- SignalR;
- Azure Functions;
- Queue;
- CDN;
- Front Door;
- Redis;
- analytics;
- IA;
- reconhecimento facial;
- comentários;
- compartilhamento social;
- planos;
- pagamentos;
- dashboard.

Explicar como o MVP pode evoluir sem precisar ser reescrito.

---

# 36. Resultado esperado

Entregar um backlog completo, pronto para ser convertido em Azure DevOps.

A resposta deve conter:

1. Visão geral
2. Objetivo do produto
3. Fluxos principais
4. Arquitetura
5. Modelo de dados
6. Épicos
7. Features
8. User Stories
9. Tasks técnicas
10. Critérios de aceite
11. Dependências
12. Priorização
13. Roadmap
14. Definition of Done
15. Testes
16. Riscos
17. Estratégia de custo
18. MVP mínimo
19. Evolução V2
20. Tabela final consolidada para Azure DevOps

## Regra principal

Não superdimensionar a solução.

O sistema inicialmente atenderá:

```text
1 casamento
~130 convidados
até 5.000 fotos
1 dia de uso intenso

```

Porém, **todos os convidados são potenciais produtores e consumidores de fotos**.

Priorize:

- simplicidade;
- baixo custo;
- segurança;
- boa experiência mobile;
- upload resiliente;
- performance;
- arquitetura limpa.

Não adicione serviços Azure apenas por “boas práticas”.

Sempre prefira a solução mais simples que resolva corretamente o problema.

Ao mesmo tempo, evite decisões que inviabilizem uma futura evolução para uma plataforma comercial de compartilhamento de fotos.