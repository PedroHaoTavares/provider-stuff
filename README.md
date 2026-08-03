# ProviderStuff para Jellyfin 10.11

Fork independente do ProviderStuff, preparado para Jellyfin 10.11.x.

- Versão inicial: `1.2.1.0`
- `targetAbi`: `10.11.0.0`
- Framework: `.NET 9` (`net9.0`)
- GUID do fork: `2be7759b-4e1b-4965-94ad-37d80c84b506`
- Repositório: `PedroHaoTavares/provider-stuff`

## URL do repositório no Jellyfin

Depois que a primeira Release terminar com sucesso, adicione esta URL em
**Painel → Plugins → Repositórios**:

```text
https://raw.githubusercontent.com/PedroHaoTavares/provider-stuff/main/manifest.json
```

Em seguida, abra o Catálogo, instale **ProviderStuff** e reinicie o Jellyfin.

## Publicar no GitHub

### 1. Colocar estes arquivos no fork

Copie todo o conteúdo deste pacote para a raiz do clone de:

```text
https://github.com/PedroHaoTavares/provider-stuff
```

Envie os arquivos para a branch `main`. Este pacote não altera o GitHub
automaticamente.

> A pasta local usada para criar este pacote ainda apontava para o repositório
> original `kamilkosek/jellyfin-plugin-provider-stuff`. Confirme o endereço do
> remoto antes de qualquer envio para evitar publicar no lugar errado.

### 2. Dar permissão de escrita à Action

No GitHub, abra:

**Settings → Actions → General → Workflow permissions**

Selecione:

```text
Read and write permissions
```

Salve. A Action precisa dessa permissão para atualizar o `manifest.json`, criar
a tag e publicar a Release.

### 3. Executar a Release

No GitHub:

1. Abra **Actions**.
2. Escolha **Release ProviderStuff**.
3. Clique em **Run workflow**.
4. Selecione a branch `main`.
5. Use:
   - `version`: `1.2.1.0`
   - `target_abi`: `10.11.0.0`
6. Confirme em **Run workflow**.

A Action:

1. restaura as dependências;
2. executa os testes;
3. compila o plugin em Release;
4. gera `providerstuff-1.2.1.0.zip` contendo somente
   `Jellyfin.Plugin.ProviderStuff.dll`;
5. calcula o MD5 do ZIP;
6. atualiza `version`, `targetAbi`, `sourceUrl`, `checksum` e `timestamp` no
   `manifest.json`;
7. envia o manifesto atualizado para `main`;
8. cria a tag `1.2.1.0`;
9. publica a GitHub Release com o ZIP.

Não crie antes uma tag ou Release chamada `1.2.1.0`; o workflow faz isso.

### 4. Conferir a publicação

Ao final, estas URLs devem funcionar:

```text
Manifest:
https://raw.githubusercontent.com/PedroHaoTavares/provider-stuff/main/manifest.json

Release:
https://github.com/PedroHaoTavares/provider-stuff/releases/tag/1.2.1.0

Download:
https://github.com/PedroHaoTavares/provider-stuff/releases/download/1.2.1.0/providerstuff-1.2.1.0.zip
```

O `checksum` do catálogo é o MD5 do arquivo ZIP, em hexadecimal minúsculo com
32 caracteres. Ele é recalculado em cada Release.

## Atualizações futuras

Execute novamente o mesmo workflow com uma versão nova, por exemplo
`1.2.1.1`. O script mantém versões anteriores no catálogo e coloca a nova no
topo. Tags já publicadas não são sobrescritas.

## Instalação limpa

Como este fork usa um GUID próprio, remova a instalação manual anterior antes
de instalar pelo catálogo. Pare o Jellyfin, remova apenas a pasta antiga do
ProviderStuff, inicie o servidor e faça a instalação pelo Catálogo.

## Desenvolvimento local

Pré-requisitos:

- .NET SDK 9
- Python 3, somente para atualizar o catálogo localmente

Comandos principais:

```text
dotnet restore Jellyfin.Plugin.ProviderStuff.sln
dotnet test Jellyfin.Plugin.ProviderStuff.Tests/Jellyfin.Plugin.ProviderStuff.Tests.csproj --configuration Release
dotnet publish Jellyfin.Plugin.ProviderStuff/Jellyfin.Plugin.ProviderStuff.csproj --configuration Release
```

O script `scripts/update_manifest.py` valida que o ZIP contém exclusivamente a
DLL esperada antes de calcular o MD5 e alterar o catálogo.

Este pacote também inclui em `release/` um ZIP local já validado. A Action
sempre recompila a DLL e gera um novo ZIP para a publicação oficial; portanto,
o MD5 pode mudar e será atualizado automaticamente.

## Estrutura de publicação

```text
.github/workflows/release.yml
    Compila, testa, empacota, atualiza o catálogo e publica a Release.

scripts/update_manifest.py
    Valida o ZIP, calcula MD5 e atualiza manifest.json.

manifest.json
    Catálogo adicionado ao Gerenciador de Repositórios do Jellyfin.

release/
    Diretório temporário usado pela Action para o ZIP final.
```

## Licença

GPL-3.0. Consulte `LICENSE`.
