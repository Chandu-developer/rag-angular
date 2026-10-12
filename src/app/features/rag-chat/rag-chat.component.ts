import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

import { RagAnswerResponse } from '../../models/RagAnswerResponse';
import { RagService } from '../../services/rag.service';

@Component({
  selector: 'app-rag-chat',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './rag-chat.component.html',
  styleUrl: './rag-chat.component.css'
})
export class RagChatComponent {
  question = '';
  answer = '';
  sources: RagAnswerResponse['sources'] = [];
  loading = false;
  errorMessage = '';

  constructor(private ragService: RagService) {}

  sendQuestion(): void {
    const question = this.question.trim();

    if (!question || this.loading) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.answer = '';
    this.sources = [];

    this.ragService.askQuestion(question).subscribe({
      next: (response: RagAnswerResponse) => {
        this.answer = response.answer;
        this.sources = response.sources;
        this.loading = false;
      },
      error: () => {
        this.errorMessage =
          'Unable to get an answer. Please try again.';
        this.loading = false;
      }
    });
  }
}