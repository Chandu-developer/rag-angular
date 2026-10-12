export interface RagAnswerResponse {
  answer: string;
  sources: {
    id: number;
    documentId: number;
    pageNumber: number;
    chunkIndex: number;
    chunkText: string;
    distance: number;
  }[];
}