from pydantic import BaseModel
from typing import List


class AskRequest(BaseModel):
    pregunta: str
    usuario: str = "anonimo"
    rol: str = "user"
    top_n: int = 2


class AskResponse(BaseModel):
    respuesta: str
    chunks_usados: List[str]
    modelo: str
    tokens_prompt: int | None = None
    tokens_completion: int | None = None
    costo_estimado: float | None = None
    tiempo_ms: int