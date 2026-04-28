from openai import OpenAI
from dotenv import load_dotenv
from file_loader import leer_archivo, dividir_en_chunks
import os
import re
import time

load_dotenv()

client = OpenAI()

MODELO_CHAT = "gpt-4.1-mini"
ARCHIVO_CONOCIMIENTO = "conocimiento.txt"


def normalizar_texto(texto: str) -> str:
    texto = texto.lower()
    texto = re.sub(r"[^\w\sáéíóúñ]", "", texto)
    return texto


def puntuar_chunk(pregunta: str, chunk: str) -> int:
    palabras_pregunta = normalizar_texto(pregunta).split()
    palabras_chunk = normalizar_texto(chunk)

    score = 0
    for palabra in palabras_pregunta:
        if palabra in palabras_chunk:
            score += 1

    return score


def obtener_mejores_chunks(pregunta: str, chunks: list[str], top_n: int = 2) -> list[tuple[str, int]]:
    resultados = []

    for chunk in chunks:
        puntaje = puntuar_chunk(pregunta, chunk)
        resultados.append((chunk, puntaje))

    resultados.sort(key=lambda x: x[1], reverse=True)
    return resultados[:top_n]


def estimar_costo(tokens_prompt: int | None, tokens_completion: int | None) -> float | None:
    if tokens_prompt is None or tokens_completion is None:
        return None

    # Estimación simple placeholder. Ajustar con precios reales.
    costo_prompt = tokens_prompt / 1_000_000 * 0.40
    costo_completion = tokens_completion / 1_000_000 * 1.60
    return round(costo_prompt + costo_completion, 6)


def responder_pregunta(pregunta: str, top_n: int = 2) -> dict:
    inicio = time.time()

    if not os.path.exists(ARCHIVO_CONOCIMIENTO):
        raise FileNotFoundError(f"No se encontró {ARCHIVO_CONOCIMIENTO}")

    texto = leer_archivo(ARCHIVO_CONOCIMIENTO)
    chunks = dividir_en_chunks(texto)

    mejores_chunks = obtener_mejores_chunks(pregunta, chunks, top_n=top_n)

    if not mejores_chunks or mejores_chunks[0][1] <= 0:
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
                )
            },
            {
                "role": "user",
                "content": f"Context:\n{contexto_recuperado}\n\nQuestion:\n{pregunta}"
            }
        ],
        temperature=0.2
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