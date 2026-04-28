# Python RAG Service

FastAPI service that implements a lightweight Retrieval-Augmented Generation (RAG) flow for the monorepo.

## What It Does

- Reads knowledge content from `conocimiento.txt`.
- Splits content into chunks.
- Scores chunks by keyword overlap with the user question.
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
```

Create `.env` in this folder:

```env
OPENAI_API_KEY=your_openai_api_key_here
```

## Run

```powershell
uvicorn app:app --host 127.0.0.1 --port 8000 --reload
```

## Endpoints

- `GET /rag/health`
- `POST /rag/ask`

## Example Request

```json
{
  "pregunta": "What is retrieval augmented generation?",
  "usuario": "recruiter",
  "rol": "user",
  "top_n": 2
}
```

## Integration Contract

This service is consumed by the .NET API under `../DotnetRagApi` through:

- `POST http://127.0.0.1:8000/rag/ask`

If you change host, port, or route, update `RagApi:BaseUrl` in:

- `../DotnetRagApi/DotnetRagApi.Api/appsettings.json`

## Notes

- The sample knowledge base currently lives in `conocimiento.txt`.
- Request fields intentionally keep Spanish names to stay aligned with the .NET contract.
