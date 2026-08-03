# ProviderStuff para Jellyfin

ProviderStuff identifica em quais serviços de streaming cada filme ou série está
disponível, aplica tags como `provider:Netflix` e, opcionalmente, cria coleções
nativas chamadas **Netflix**, **Prime Video**, **Disney+**, **Max** e outros
provedores configurados.

A versão `1.3.1.0` foi desenvolvida para o Jellyfin Server `10.11.10`, usa
`.NET 9` e mantém o comportamento existente de criação de tags.

## O que o plugin faz

- Consulta os provedores de filmes, séries e episódios usando o TMDB.
- Aplica uma tag `provider:<nome>` para cada provedor correspondente.
- Cria uma coleção nativa do Jellyfin para cada provedor habilitado.
- Mantém nessas coleções apenas filmes e séries que possuam a tag do provedor.
- Adiciona novos itens e remove membros que já não correspondem à coleção.
- Usa a imagem configurada do provedor como capa, sem substituir uma capa
  existente.
- Expõe uma API autenticada para consultar provedores e seus itens.

O plugin não move, copia ou duplica mídias, não cria links para arquivos e não
altera diretamente o banco de dados do Jellyfin. As coleções armazenam apenas
referências aos itens que já existem na biblioteca.

## Como a navegação por provedores aparece

Cada provedor é representado por uma coleção padrão do Jellyfin:

```text
Coleções
├── Netflix
├── Prime Video
├── Disney+
└── Max
```

Ao abrir **Netflix**, por exemplo, são exibidos os filmes e séries que possuem
a tag `provider:Netflix`.

Esta implementação usa `BoxSet`, o modelo público e nativo de coleções do
Jellyfin. Por isso, ela pode ser navegada pelos clientes que já suportam
coleções, incluindo Jellyfin Web, Android TV, WebOS e Roku, sem endpoint ou
interface personalizada no cliente.

> Os provedores aparecem dentro da área **Coleções**. O plugin não transforma
> Netflix, Prime Video ou Disney+ em bibliotecas independentes na linha
> **Minha mídia**, porque plugins de servidor não podem adicionar com segurança
> novos atalhos de biblioteca a todos esses clientes. Fazer isso exigiria
> alterações específicas em cada cliente.

## Instalação pelo catálogo

No Jellyfin, abra **Painel → Plugins → Repositórios**, adicione:

```text
https://raw.githubusercontent.com/PedroHaoTavares/provider-stuff/main/manifest.json
```

Depois:

1. Abra o **Catálogo** de plugins.
2. Instale **ProviderStuff**.
3. Reinicie o servidor Jellyfin.

Requisitos:

- Jellyfin Server `10.11.10`;
- uma chave de API do TMDB;
- acesso do servidor Jellyfin à API e às imagens do TMDB.

## Configuração

Abra **Painel → ProviderStuff** no menu lateral. A página também permanece
disponível em **Painel → Plugins → ProviderStuff**.

### Configurações globais

- **TMDB API Key:** chave usada para consultar os provedores.
- **TMDB Country:** região da disponibilidade, como `BR`, `US` ou `DE`.
- **Create and synchronize provider collections:** ativa ou desativa a criação
  e sincronização de todas as coleções de provedores.

Desativar a opção global:

- não interrompe a criação das tags `provider:<nome>`;
- não apaga coleções já existentes;
- impede que o plugin crie ou sincronize coleções enquanto estiver desativada.

### Configuração de cada provedor

Para cada entrada, configure:

- **Name:** nome exibido e usado na tag, como `Netflix`;
- **TMDB Providers:** um ou mais IDs do TMDB associados à entrada;
- **Nome exibido da coleção:** título independente da tag, como `Minha Netflix`;
- **URL da imagem da coleção:** imagem opcional para a capa;
- **Create collection for this provider:** ativa a coleção somente para aquele
  provedor.

As opções global e individual precisam estar habilitadas para que a coleção
seja criada e sincronizada.

O plugin guarda o identificador interno da coleção. Depois da primeira
sincronização, renomear a coleção ou editar sua imagem diretamente no Jellyfin
não faz o plugin criar outra coleção. Alterações feitas nos campos de nome ou
imagem da página do ProviderStuff são aplicadas uma vez na próxima execução da
tarefa, sem sobrescrever continuamente personalizações posteriores.

### Exemplo

Para agrupar diferentes opções do TMDB em uma única entrada:

```text
Name: Prime Video
TMDB Providers: Amazon Prime Video, Amazon Video
Create collection for this provider: ativado
```

Os IDs selecionados são tratados como o mesmo provedor configurado e produzem a
tag `provider:Prime Video` e a coleção **Prime Video**.

## Executando a tarefa

Abra **Painel → Tarefas agendadas** e execute:

```text
ProviderStuff: Apply provider tags
```

Por padrão, a tarefa também é executada diariamente às 03:00. Em cada execução,
o plugin:

1. consulta filmes, séries e episódios existentes;
2. consulta os provedores disponíveis no país configurado;
3. adiciona as tags correspondentes;
4. cria ou localiza as coleções habilitadas;
5. sincroniza filmes e séries com cada coleção.

Episódios recebem tags, preservando o comportamento anterior, mas não são
adicionados individualmente às coleções. A série correspondente é o item
navegável.

## API

Todos os endpoints exigem autenticação normal do Jellyfin.

### Listar provedores

```http
GET /providerstuff/providers
```

Retorna o nome, IDs do TMDB, URL do logo, opção de coleção e, quando disponível,
o `collectionId`.

### Listar itens de um provedor

```http
GET /providerstuff/{providerName}/items
```

Parâmetros opcionais:

- `userId`: usuário usado para gerar os DTOs;
- `includeItemTypes`: `Movie`, `Series` ou `Episode`;
- `startIndex`: índice inicial;
- `limit`: quantidade máxima de resultados.

Exemplos:

```http
GET /providerstuff/Netflix/items?limit=50&startIndex=0
GET /providerstuff/Prime%20Video/items?includeItemTypes=Movie
```

O retorno é um `QueryResult<BaseItemDto>` paginado.

## Atualização a partir de uma instalação antiga

Este fork usa o GUID:

```text
2be7759b-4e1b-4965-94ad-37d80c84b506
```

Se houver uma cópia manual ou outra variante do ProviderStuff instalada,
remova somente a instalação antiga antes de instalar este fork pelo catálogo.
Não mantenha duas variantes do plugin carregadas ao mesmo tempo.

Após atualizar para `1.3.1.0`, revise a opção global de coleções e execute
a tarefa manualmente uma vez.

## Desenvolvimento

Pré-requisitos:

- .NET SDK 9;
- Python 3 para atualizar o manifesto de release.

Comandos principais:

```text
dotnet restore Jellyfin.Plugin.ProviderStuff.sln
dotnet test Jellyfin.Plugin.ProviderStuff.Tests/Jellyfin.Plugin.ProviderStuff.Tests.csproj --configuration Release
dotnet publish Jellyfin.Plugin.ProviderStuff/Jellyfin.Plugin.ProviderStuff.csproj --configuration Release
```

O artefato instalável é um arquivo
`providerstuff-<versão>.zip` contendo somente:

```text
Jellyfin.Plugin.ProviderStuff.dll
```

## Licença

Distribuído sob a licença GPL-3.0. Consulte [LICENSE](LICENSE).
