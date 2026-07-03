# Architecture

This project uses a Retrieval-Augmented Generation (RAG) pattern:

1. Document ingestion: PDFs are uploaded and split into passages/chunks.
2. Embeddings: Each chunk gets a vector embedding stored in a vector store (Redis, Pinecone, or similar).
3. Retrieval: On user question, perform a semantic search to find relevant chunks.
4. Generation: The LLM composes an answer using retrieved chunks and returns a source-cited response.

Components:
- Frontend: React app for upload and chat UI.
- Backend: ASP.NET Core API for processing, embeddings, and orchestration.
- Vector store: Redis, Pinecone, or other vector DB.
- LLM: External API (OpenAI, Azure, etc.)