import { HttpClient } from '@angular/common/http';
import { Injectable, effect, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Auth } from '../auth/services/auth';
import { ChatMessage } from '../models';
import { PatientService } from './patient';

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
  private readonly patientService = inject(PatientService);

  private selectedPatientId: number | null = null;

  constructor() {
    effect(() => {
      const activePatientId = this.patientService.currentPatientId;
      if (activePatientId != null) {
        this.selectedPatientId = activePatientId;
      }
    });
  }

  setPatient(patientId: number): void {
    this.selectedPatientId = patientId;
    void this.patientService.setRunningPatient(patientId).catch(() => {
      /* keep the local selection for the chat flow if the server update fails */
    });
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
        this.http.post<BackendChatResponse>(this.apiUrl + '/chatbot/chat-public', {
          message: text,
        }),
      );
      return this.toReply(res);
    }

    await this.syncPrimaryPatient();

    const payload: { message: string; patientId?: number } = { message: text };
    const activePatientId = this.resolvePatientId();
    if (activePatientId != null) payload.patientId = activePatientId;

    let res = await firstValueFrom(
      this.http.post<BackendChatResponse>(this.apiUrl + '/chatbot/chat', payload),
    );

    if (res.intent === 'needs_patient_registration') {
      await this.patientService.getPrimaryPatientId();
      await this.syncPrimaryPatient();
      res = await firstValueFrom(
        this.http.post<BackendChatResponse>(this.apiUrl + '/chatbot/chat', payload),
      );
    }

    if (res.intent === 'needs_patient_selection' && res.availablePatients?.length === 1) {
      this.selectedPatientId = res.availablePatients[0].patientId;
      payload.patientId = this.selectedPatientId;
      res = await firstValueFrom(
        this.http.post<BackendChatResponse>(this.apiUrl + '/chatbot/chat', payload),
      );
    }

    return this.toReply(res);
  }

  private async syncPrimaryPatient(): Promise<void> {
    const activePatientId = this.patientService.currentPatientId;
    if (activePatientId != null) {
      this.selectedPatientId = activePatientId;
      return;
    }

    if (this.selectedPatientId != null) return;
    const id = await this.patientService.getPrimaryPatientId();
    if (id != null) this.selectedPatientId = id;
  }

  private resolvePatientId(): number | null {
    return this.patientService.currentPatientId ?? this.selectedPatientId;
  }

  private toReply(res: BackendChatResponse): ChatBotReply {
    const intent = res.intent || 'drug_info';
    let reply = res.reply || '';

    // Symptom replies come back as a JSON blob carrying level/label/explanation/action.
    // Render them as a clean Arabic message instead of dumping raw JSON in the bubble.
    if (intent === 'symptom') {
      const parsed = this.tryParseSymptom(reply);
      if (parsed) reply = this.formatSymptomReply(parsed);
    }

    return {
      reply,
      intent,
      severityLevel: this.severityFor(intent, res.reply),
      availablePatients: res.availablePatients,
    };
  }

  private tryParseSymptom(raw: string): any | null {
    if (!raw) return null;
    const start = raw.indexOf('{');
    const end = raw.lastIndexOf('}');
    if (start < 0 || end < 0 || end <= start) return null;
    try {
      return JSON.parse(raw.slice(start, end + 1));
    } catch {
      return null;
    }
  }

  private formatSymptomReply(p: any): string {
    const labelMap: Record<number, string> = { 1: 'آمن', 2: 'احذر', 3: 'خطر' };
    const emojiMap: Record<number, string> = { 1: '✅', 2: '⚠️', 3: '🚨' };
    const lvl = Number(p?.level) || 0;
    const label = p?.label_ar || labelMap[lvl] || '';
    const emoji = emojiMap[lvl] || 'ℹ️';
    const explanation = (p?.explanation_ar || '').toString().trim();
    const action = (p?.recommended_action_ar || '').toString().trim();
    const patient = (p?.patient_name || '').toString().trim();

    const lines: string[] = [];
    lines.push(`${emoji} ${label}`);
    if (patient) lines.push(`المريض: ${patient}`);
    if (explanation) lines.push(`\n${explanation}`);
    if (action) lines.push(`\nالإجراء الموصى به: ${action}`);
    return lines.join('\n');
  }

  private severityFor(intent: string, reply: string): 'safe' | 'caution' | 'danger' | 'info' {
    if (intent === 'symptom') {
      try {
        const start = reply.indexOf('{');
        const end = reply.lastIndexOf('}');
        if (start >= 0 && end > start) {
          const parsed = JSON.parse(reply.slice(start, end + 1));
          if (parsed && parsed.level === 3) return 'danger';
          if (parsed && parsed.level === 2) return 'caution';
          if (parsed && parsed.level === 1) return 'safe';
        }
      } catch {
        /* fall through */
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
