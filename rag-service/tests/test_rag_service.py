from rag_service import cosine_similarity, puntuar_chunk_keywords


def test_puntuar_chunk_keywords_counts_overlap():
    score = puntuar_chunk_keywords("retrieval augmented generation", "retrieval generation pipeline")
    assert score == 2.0


def test_cosine_similarity_identical_vectors():
    vector = [1.0, 0.0, 0.0]
    assert cosine_similarity(vector, vector) == 1.0


def test_cosine_similarity_orthogonal_vectors():
    assert cosine_similarity([1.0, 0.0], [0.0, 1.0]) == 0.0
