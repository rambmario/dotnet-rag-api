# Python RAG Service

FastAPI service that implements a Retrieval-Augmented Generation (RAG) flow for the monorepo.

## What It Does

- Reads knowledge content from `conocimiento.txt`.
- Splits content into chunks.
- Retrieves relevant chunks with **OpenAI embeddings** (`text-embedding-3-small`) by default.
- Falls back to keyword overlap scoring when embeddings are unavailable or `RAG_RETRIEVAL_MODE=keywords`.
- Sends selected context to an OpenAI chat model.
- Returns answer, used chunks, timing, and token/cost metadata.

## Requirements

- Python 3.10+
- OpenAI API key

## Setup

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
copy .env.example .env
```

`.env` example:

```env
OPENAI_API_KEY=your_openai_api_key_here
RAG_RETRIEVAL_MODE=embeddings
```

## Run

```powershell
uvicorn app:app --host 127.0.0.1 --port 8000 --reload
```

Docker (from repo root):

```powershell
docker compose up --build rag-service
```

## Endpoints

- `GET /rag/health`
- `POST /rag/ask`

## Example Request

```json
{
  "pregunta": "What is retrieval augmented generation?",
  "usuario": "demo",
  "rol": "user",
  "top_n": 2
}
```

## Tests

```powershell
pytest -q
```

## Integration Contract

Consumed by the .NET API through:

- `POST http://127.0.0.1:8000/rag/ask`

If you change host/port/route, update `RagApi:BaseUrl` in the .NET API configuration.

## Notes

- Chunk embeddings are cached in memory after the first request.
- Request fields intentionally keep Spanish names to stay aligned with the .NET contract.
- Error responses avoid leaking internal exception details.
