import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DocumentUploadResponse {
  id: number;
  fileName: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentService {

  private http = inject(HttpClient);

  private readonly apiUrl = 'https://localhost:44348/api/documents';

  upload(file: File): Observable<DocumentUploadResponse> {

    const formData = new FormData();

    formData.append('file', file);

    return this.http.post<DocumentUploadResponse>(
      `${this.apiUrl}/upload`,
      formData
    );
  }
}