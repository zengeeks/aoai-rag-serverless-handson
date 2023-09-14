# Azure OpenAI Service x RAG パターン実践ハンズオン

## 💫 概要

このハンズオンでは、Cosmos DB と Cognitive Search をデータストアとした Retrieval Augmented Generation (RAG) パターンの実践として以下を実現します。

- 自社独自のデータを検索対象としたベクター検索を行なうため検索インデックスの作成とリアルタイムで継続的なデータ更新
- Web API でのベクター検索


開発言語として C# を利用します。

<br>

## 🗺️ アーキテクチャ

ハンズオンで実現するアーキテクチャのイメージは以下となります。

![](./docs/images/R1.png)

### 検索インデックスの更新 (水色の線)

- Web API (Function App - HTTP trigger) で受け取ったデータ更新を Cosmos DB に保存します (①)。
- Cosmos DB でデータ更新されると、Function App (Cosmos DB Trigger) をトリガーされます (②)。
- Function App (Cosmos DB Trigger) の処理で以下を行ないます。
  - ベクター検索の対象データを Azure OpenAI Service でベクター化します (③)。
  - Cognitive Search のインデックスを更新します (④)。

### ベクター検索 (紫色の線)

- Web API の Function App (Http trigger) で検索のクエリを受け取ります (①)。
- Function App (Http trigger) の処理で以下を行ないます。
  - 検索クエリを Azure OpenAI Service でベクター化します (②)。
  - Cognitive Search のインデックスに対してベクター検索を行ない (③) 、Web API の結果として返します。

<br>

## 🚧 Azure のリソース作成時の注意

**※ 今回のハンズオンでは、Auzre のリソースを作成することで料金が発生するリソースもあります。ご自身の状況に応じて、今回のハンズオンの最後にリソースグループごとすべて消すなどは自己責任で行なってください。**

<br>

## 🔖 ハンズオンの構成

0～3のチャプターで今回利用する Azure のリソースをセットアップし、以降で実装を進めていきます。

タイトル | 概要
--- | ---
[🧪 0. Azure OpenAI Service のセットアップ](./docs/setup-cognitive-search.md) | 今回のハンズオンで利用する Azure OpenAI Service のリソースをセットアップします。
[🧪 1. Cognitive Search のセットアップ](./docs/setup-cognitive-search.md) | 今回のハンズオンで利用する Cognitive Search のリソースをセットアップします。
[🧪 2. Cosmos DB のセットアップ](./docs/setup-cosmos-db.md) | 今回のハンズオンで利用する Cosmos DB のリソースをセットアップします。
[🧪 3. Function App のセットアップ](./docs/setup-function-app.md) | 今回のハンズオンで利用する Function App のリソースをセットアップします。
[🧪 4. インデックスの更新処理の実装](./docs/implement-change-feed-dotnet.md) | Function App で、Change Feed 機能を活用して Cognitive Search のインデックスのデータを更新する処理を実装します。
[🧪 5. ベクター検索の実装](./docs/implement-vector-search.md) | Function App で、ベクター検索の API を実装します。
[(オプション) <br>🧪 6. Cosmos DB のデータ更新](./docs/Implement-cosmos-db-operations.md) | Function App で、Cosmos DB のデータを更新する処理を実装します。
[🚮 Azure のリソース削除](./docs/remove-azure-resources.md) | Azure のリソースを削除する方法を説明します。

<br>

## 🛠️ 事前準備

このハンズオンは、開発の環境・ツールとして以下を利用します。

### Visual Studio 2022 (または Visual Studio Code)

Visual Studio 2022 のバージョンは不問です。  
Azure のワークロードを利用します。セットアップ方法は以下をご参照ください。

- [.NET を使用して Azure 開発用に Visual Studio を構成する](https://learn.microsoft.com/ja-jp/dotnet/azure/configure-visual-studio)

ハンズオン資料では Visual Studio 2022 の操作のみを説明していますが、Visual Studio Code でも可能です。  
ご自身で C# と Azure の開発環境をセットアップの上ご利用ください。

### Azure サブスクリプション

このハンズオンでは、Azure で以下のリソースを利用します。サブスクリプションでこれらのリソースの利用・作成ができることを確認してください。

- Azure OpenAI Service
- Cognitive Search
- Function App
- Cosmos DB

## 🧑‍💻 Let's Get Started

以下のリンクからハンズオンの旅に出発しょう🚀

- [🧪 0. Azure OpenAI Service のセットアップ](./docs/setup-azure-openai.md)
