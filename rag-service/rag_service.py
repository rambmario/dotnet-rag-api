from openai import OpenAI
from dotenv import load_dotenv
from file_loader import leer_archivo, dividir_en_chunks
import math
import os
import re
import time

load_dotenv()

client = OpenAI()

MODELO_CHAT = "gpt-4.1-mini"
MODELO_EMBEDDINGS = "text-embedding-3-small"
ARCHIVO_CONOCIMIENTO = "conocimiento.txt"
RETRIEVAL_MODE = os.getenv("RAG_RETRIEVAL_MODE", "embeddings").strip().lower()

_chunk_cache: list[str] | None = None
_embedding_cache: list[list[float]] | None = None


def normalizar_texto(texto: str) -> str:
    texto = texto.lower()
    texto = re.sub(r"[^\w\sáéíóúñ]", "", texto)
    return texto


def puntuar_chunk_keywords(pregunta: str, chunk: str) -> float:
    palabras_pregunta = normalizar_texto(pregunta).split()
    palabras_chunk = normalizar_texto(chunk)

    score = 0.0
    for palabra in palabras_pregunta:
        if palabra in palabras_chunk:
            score += 1.0

    return score


def cosine_similarity(a: list[float], b: list[float]) -> float:
    if not a or not b or len(a) != len(b):
        return 0.0

    dot = sum(x * y for x, y in zip(a, b))
    norm_a = math.sqrt(sum(x * x for x in a))
    norm_b = math.sqrt(sum(y * y for y in b))

    if norm_a == 0.0 or norm_b == 0.0:
        return 0.0

    return dot / (norm_a * norm_b)


def embed_texts(texts: list[str]) -> list[list[float]]:
    if not texts:
        return []

    response = client.embeddings.create(model=MODELO_EMBEDDINGS, input=texts)
    return [item.embedding for item in response.data]


def cargar_chunks() -> list[str]:
    global _chunk_cache

    if _chunk_cache is not None:
        return _chunk_cache

    if not os.path.exists(ARCHIVO_CONOCIMIENTO):
        raise FileNotFoundError(f"No se encontró {ARCHIVO_CONOCIMIENTO}")

    texto = leer_archivo(ARCHIVO_CONOCIMIENTO)
    _chunk_cache = dividir_en_chunks(texto)
    return _chunk_cache


def cargar_embeddings(chunks: list[str]) -> list[list[float]]:
    global _embedding_cache

    if _embedding_cache is not None:
        return _embedding_cache

    _embedding_cache = embed_texts(chunks)
    return _embedding_cache


def obtener_mejores_chunks_keywords(
    pregunta: str, chunks: list[str], top_n: int = 2
) -> list[tuple[str, float]]:
    resultados = [(chunk, puntuar_chunk_keywords(pregunta, chunk)) for chunk in chunks]
    resultados.sort(key=lambda x: x[1], reverse=True)
    return resultados[:top_n]


def obtener_mejores_chunks_embeddings(
    pregunta: str, chunks: list[str], top_n: int = 2
) -> list[tuple[str, float]]:
    chunk_embeddings = cargar_embeddings(chunks)
    pregunta_embedding = embed_texts([pregunta])[0]

    resultados = [
        (chunk, cosine_similarity(pregunta_embedding, embedding))
        for chunk, embedding in zip(chunks, chunk_embeddings)
    ]
    resultados.sort(key=lambda x: x[1], reverse=True)
    return resultados[:top_n]


def obtener_mejores_chunks(
    pregunta: str, chunks: list[str], top_n: int = 2
) -> list[tuple[str, float]]:
    if RETRIEVAL_MODE == "keywords":
        return obtener_mejores_chunks_keywords(pregunta, chunks, top_n=top_n)

    try:
        return obtener_mejores_chunks_embeddings(pregunta, chunks, top_n=top_n)
    except Exception:
        # Fallback for local demos without embedding quota/network issues.
        return obtener_mejores_chunks_keywords(pregunta, chunks, top_n=top_n)


def estimar_costo(tokens_prompt: int | None, tokens_completion: int | None) -> float | None:
    if tokens_prompt is None or tokens_completion is None:
        return None

    costo_prompt = tokens_prompt / 1_000_000 * 0.40
    costo_completion = tokens_completion / 1_000_000 * 1.60
    return round(costo_prompt + costo_completion, 6)


def responder_pregunta(pregunta: str, top_n: int = 2) -> dict:
    inicio = time.time()
    chunks = cargar_chunks()
    mejores_chunks = obtener_mejores_chunks(pregunta, chunks, top_n=top_n)

    # embeddings: low similarity; keywords: zero overlap
    min_score = 0.15 if RETRIEVAL_MODE != "keywords" else 0.0

    if not mejores_chunks or mejores_chunks[0][1] <= min_score:
        return {
            "respuesta": "No relevant context fragments found.",
            "chunks_usados": [],
            "modelo": MODELO_CHAT,
            "tokens_prompt": None,
            "tokens_completion": None,
            "costo_estimado": None,
            "tiempo_ms": int((time.time() - inicio) * 1000),
        }

    contexto_recuperado = "\n\n".join([chunk for chunk, _ in mejores_chunks])

    response = client.chat.completions.create(
        model=MODELO_CHAT,
        messages=[
            {
                "role": "system",
                "content": (
                    "Answer only based on the provided context. "
                    "If the answer can be clearly inferred from the context, do so. "
                    "If it is not in the context, state it explicitly."
                ),
            },
            {
                "role": "user",
                "content": f"Context:\n{contexto_recuperado}\n\nQuestion:\n{pregunta}",
            },
        ],
        temperature=0.2,
    )

    respuesta = response.choices[0].message.content

    usage = getattr(response, "usage", None)
    tokens_prompt = getattr(usage, "prompt_tokens", None) if usage else None
    tokens_completion = getattr(usage, "completion_tokens", None) if usage else None
    costo_estimado = estimar_costo(tokens_prompt, tokens_completion)

    return {
        "respuesta": respuesta,
        "chunks_usados": [chunk for chunk, _ in mejores_chunks],
        "modelo": MODELO_CHAT,
        "tokens_prompt": tokens_prompt,
        "tokens_completion": tokens_completion,
        "costo_estimado": costo_estimado,
        "tiempo_ms": int((time.time() - inicio) * 1000),
    }
