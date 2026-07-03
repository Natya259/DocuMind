# API

This file documents the main API endpoints (placeholders):

- POST /api/documents/upload
  - Upload a PDF for processing.

- GET /api/documents/{id}/status
  - Check processing status.

- POST /api/query
  - Body: { "documentId": "...", "query": "..." }
  - Returns: Answer with source citations and references to document chunks.

Authentication, rate-limiting, and usage logging should be added as needed.