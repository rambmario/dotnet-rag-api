def leer_archivo(ruta: str) -> str:
    with open(ruta, "r", encoding="utf-8") as f:
        return f.read()


def dividir_en_chunks(texto: str) -> list[str]:
    return [p.strip() for p in texto.split("\n") if p.strip()]