import { Routes } from '@angular/router';
import { DocumentUploadComponent } from './features/documents/document-upload/document-upload.component';
import { RagChatComponent } from './features/rag-chat/rag-chat.component';

export const routes: Routes = [{ path: '', component: DocumentUploadComponent}, 
    {path: 'ragchat', component: RagChatComponent }];
