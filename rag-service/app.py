from fastapi import FastAPI, HTTPException
from models import AskRequest, AskResponse
from rag_service import RETRIEVAL_MODE, responder_pregunta
import logging

logger = logging.getLogger("rag-service")

app = FastAPI(
    title="Python RAG Service",
    description="FastAPI service that retrieves context from a local knowledge base and generates answers with OpenAI.",
    version="1.1.0",
)


@app.get("/rag/health")
def health():
    return {
        "status": "ok",
        "retrieval_mode": RETRIEVAL_MODE,
    }


@app.post("/rag/ask", response_model=AskResponse)
def ask(request: AskRequest):
    try:
        return responder_pregunta(
            pregunta=request.pregunta,
            top_n=request.top_n,
        )
    except FileNotFoundError:
        logger.exception("Knowledge base file missing")
        raise HTTPException(status_code=404, detail="Knowledge base file was not found.")
    except Exception:
        logger.exception("Unhandled error while processing RAG ask request")
        raise HTTPException(status_code=500, detail="Unexpected error processing the request.")
