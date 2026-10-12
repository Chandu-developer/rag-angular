import { inject, Injectable } from '@angular/core';
import { RagAnswerResponse } from '../models/RagAnswerResponse';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class RagService {

  constructor() { }
  private http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:44348/api/search';

  askQuestion(question: string) {
  return this.http.post<RagAnswerResponse>(
    this.apiUrl,
    {
      question,
      topK: 5
    }
  );
}
}
