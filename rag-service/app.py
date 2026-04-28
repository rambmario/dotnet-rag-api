from fastapi import FastAPI, HTTPException
from models import AskRequest, AskResponse
from rag_service import responder_pregunta

app = FastAPI(
    title="Python RAG Service",
    description="FastAPI service that retrieves context from a local knowledge base and generates answers with OpenAI.",
    version="1.0.0"
)


@app.get("/rag/health")
def health():
    return {"status": "ok"}


@app.post("/rag/ask", response_model=AskResponse)
def ask(request: AskRequest):
    try:
        resultado = responder_pregunta(
            pregunta=request.pregunta,
            top_n=request.top_n
        )
        return resultado
    except FileNotFoundError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))