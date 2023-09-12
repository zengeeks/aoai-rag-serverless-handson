# 🧪 5. ベクター検索の実装 (Python)



🚧🚧🚧🚧🚧🚧🚧  
🚧ただいま準備中🚧  
🚧🚧🚧🚧🚧🚧🚧


<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>


ここでは、アーキテクチャ図のベクター検索 (1～3) 部分の処理として、以下を実装します。

- Http trigger の Function App でリクエストを受け取る (①)
- Functio App で以下の処理を行ないます。
  - 検索クエリのベクター化を行なう (②)
  - Cognitive Search でベクター検索を行ない、結果を返す (③)。

![](./images/C5.png)

コンテンツは以下になります。

- xxxxx
- xxxxx




## xxx

## パッケージの構成

requirements.txt で、以下のように必要なパッケージを定義します。

```txt
azure-functions
openai
# openai の embeddings で必要
numpy
azure-search-documents==11.4.0b9
```

## local.settings.json の構成


```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "python",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsFeatureFlags": "EnableWorkerIndexing",
    "AZURE_OPENAI_ENDPOINT": "",
    "AZURE_OPENAI_API_KEY": "",
    "COGNITIVE_SEARCH_ENDPOINT": "",
    "COGNITIVE_SEARCH_QUERY_KEY": "",
    "COGNITIVE_SEARCH_INDEX_NAME": "azure"
  }
}
```


## Function App の実装


```python
import os
import azure.functions as func
import json
import logging
import openai

from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from azure.search.documents.models import Vector

openai.api_type = "azure"
openai.api_key = os.getenv("AZURE_OPENAI_API_KEY")
openai.api_base = os.getenv("AZURE_OPENAI_ENDPOINT")
openai.api_version = "2023-05-15"
DEPLOYMENT_ID = "text-embedding-ada-002"
search_client = SearchClient(os.getenv("COGNITIVE_SEARCH_ENDPOINT"),
                             os.getenv("COGNITIVE_SEARCH_INDEX_NAME"), 
                             AzureKeyCredential(os.getenv("COGNITIVE_SEARCH_QUERY_KEY")))

def vector_search(text: str):
    vector = Vector(value=get_embedding(text), k=3, fields="contentVector")
    results = search_client.search(
        search_text="",
        vectors=[vector],
        select=["title", "content", "category"],
    )

    items = []
    for result in results:
        items.append({
            "title": result["title"],
            "content": result["content"],
            "category": result["category"],
            "score": result["@search.score"]
        })
        
    return items

def get_embedding(text: str):
    embedding = openai.Embedding.create(input=text, deployment_id=DEPLOYMENT_ID)["data"][0]["embedding"]
    return embedding

app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

@app.route(route="search", methods=["GET"])
def search(req: func.HttpRequest) -> func.HttpResponse:

    query = req.params.get('query')
    if not query:
        return func.HttpResponse(
            "クエリ文字列 'query' に検索したい文字列を指定してください。",
             status_code=400
        )

    search_results = vector_search(query)
    logging.info(f"search_results:{search_results}")

    return func.HttpResponse(
            json.dumps(search_results),
            mimetype="application/json",
            status_code=200
    )
```


