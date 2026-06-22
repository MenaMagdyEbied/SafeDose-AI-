import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Auth } from '../auth/services/auth';
import { ChatMessage } from '../models';

export interface ChatPatientOption {
  patientId: number;
  fullName: string;
  age: number | null;
  gender: string | null;
}

export interface ChatBotReply {
  reply: string;
  intent: string;
  severityLevel: 'safe' | 'caution' | 'danger' | 'info';
  availablePatients?: ChatPatientOption[];
}

interface BackendChatResponse {
  reply: string;
  intent: string;
  promptTokens?: number;
  completionTokens?: number;
  availablePatients?: ChatPatientOption[];
}

@Injectable({ providedIn: 'root' })
export class ChatBotService {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);
  private readonly auth = inject(Auth);

  // Active patient id for this chat session. Reused on follow-up messages.
  private selectedPatientId: number | null = null;

  setPatient(patientId: number): void {
    this.selectedPatientId = patientId;
  }

  resetPatient(): void {
    this.selectedPatientId = null;
  }

  async sendMessage(
    text: string,
    _history: ChatMessage[],
    _patientName: string,
  ): Promise<ChatBotReply> {
    if (!this.auth.isLoggedIn) {
      const res = await firstValueFrom(
        this.http.post<BackendChatResponse>(
          this.apiUrl + '/chatbot/chat-public',
          { message: text },
        ),
      );
      return this.toReply(res);
    }

    const payload: { message: string; patientId?: number } = { message: text };
    if (this.selectedPatientId != null) payload.patientId = this.selectedPatientId;

    const res = await firstValueFrom(
      this.http.post<BackendChatResponse>(this.apiUrl + '/chatbot/chat', payload),
    );

    if (res.intent === 'needs_patient_selection' && res.availablePatients && res.availablePatients.length === 1) {
      this.selectedPatientId = res.availablePatients[0].patientId;
    }

    return this.toReply(res);
  }

  private toReply(res: BackendChatResponse): ChatBotReply {
    return {
      reply: res.reply || '',
      intent: res.intent || 'drug_info',
      severityLevel: this.severityFor(res.intent, res.reply),
      availablePatients: res.availablePatients,
    };
  }

  private severityFor(intent: string, reply: string): 'safe' | 'caution' | 'danger' | 'info' {
    if (intent === 'symptom') {
      try {
        const parsed = JSON.parse(reply);
        if (parsed && parsed.level === 3) return 'danger';
        if (parsed && parsed.level === 2) return 'caution';
        if (parsed && parsed.level === 1) return 'safe';
      } catch {
        // not JSON
      }
      return 'caution';
    }
    if (intent === 'interaction_redirect') return 'info';
    if (intent === 'out_of_scope') return 'info';
    if (intent === 'needs_login') return 'info';
    if (intent === 'needs_patient_selection') return 'info';
    if (intent === 'needs_patient_registration') return 'info';
    return 'safe';
  }
}
