import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DocumentService } from '../../../services/document.service.ts.service';
import { Router } from '@angular/router';



@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule
  ],
  templateUrl: './document-upload.component.html',
  styleUrl: './document-upload.component.css'
})
export class DocumentUploadComponent {

  private documentService = inject(DocumentService);
  private router = inject(Router);

  selectedFile: File | null = null;

  uploading = false;

  message = '';

  onFileSelected(event: Event): void {

    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];

    if (file.type !== 'application/pdf') {
      this.message = 'Only PDF files are allowed.';
      this.selectedFile = null;
      return;
    }

    this.selectedFile = file;
    this.message = '';
  }

  upload(): void {

    if (!this.selectedFile) {
      return;
    }

    this.uploading = true;
    this.message = '';

    this.documentService
      .upload(this.selectedFile)
      .subscribe({
        next: response => {

          this.message =
            `Uploaded successfully: ${response.fileName}`;

          this.selectedFile = null;
          this.uploading = false;
        },

        error: error => {

          console.error(error);

          this.message =
            'Upload failed. Please try again.';

          this.uploading = false;
        }
      });
  }

  openRagChat() {
    this.router.navigate(['/ragchat']);
  }
}