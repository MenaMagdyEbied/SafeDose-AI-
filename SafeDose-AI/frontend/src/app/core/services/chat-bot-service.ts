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
  //   reply: string;
  //   intent: string;
  //   promptTokens?: number;
  //   completionTokens?: number;
  //   availablePatients?: ChatPatientOption[];
  // }
  // @Injectable({ providedIn: 'root' })
  // export class ChatBotService {
  //   private readonly apiUrl = environment.apiUrl;
  //   private readonly http = inject(HttpClient);
  //   private readonly auth = inject(Auth);
  //   // Active patient id for this chat session. Reused on follow-up messages.
  //   private selectedPatientId: number | null = null;
  //   setPatient(patientId: number): void {
  //     this.selectedPatientId = patientId;
  //   }
  //   resetPatient(): void {
  //     this.selectedPatientId = null;
  //   }
  //   async sendMessage(
  //     text: string,
  //     history: ChatMessage[],
  //     patientName: string,
  //   ): Promise<{
  //     reply: string;
  //     severityLevel: 'safe' | 'caution' | 'danger' | 'info';
  //     // actions: ChatAction[];
  //   }> {
  //     try {
  //       const resp = await fetch('/api/chat/send', {
  //         method: 'POST',
  //         headers: { 'Content-Type': 'application/json' },
  //         body: JSON.stringify({
  //           message: text,
  //           history: history.map((m) => ({ sender: m.sender, text: m.text })),
  //           patientContext: {
  //             name: patientName,
  //             medications: ['Glucophage 500mg', 'Concor 5mg', 'Warfarin 5mg'],
  //             allergies: 'أدوية السلفا',
  //           },
  //         }),
  //       });
  //       if (!resp.ok) throw new Error('Server error');
  //       const data = await resp.json();
  //       return {
  //         reply: data.reply,
  //         severityLevel: data.severityLevel || 'info',
  //         actions: data.actions || [],
  //       };
  //     } catch {
  //       return this.clientFallback(text, patientName);
  //     }
  //   }
  //   private clientFallback(
  //     text: string,
  //     patientName: string,
  //   ): {
  //     reply: string;
  //     severityLevel: 'safe' | 'caution' | 'danger' | 'info';
  //     // actions: ChatAction[];
  //   } {
  //     const msg = text.toLowerCase();
  //     let reply = '';
  //     let severityLevel: 'safe' | 'caution' | 'danger' | 'info' = 'info';
  //     // const actions: ChatAction[] = [];
  //     if (
  //       msg.includes('تداخل') ||
  //       msg.includes('تعارض') ||
  //       (msg.includes('وارفارين') && msg.includes('أسبرين'))
  //     ) {
  //       reply =
  //         'تحذير طبي حرج: تناول الأسبرين مع الوارفارين يؤثر بشكل حاد ومباشر على تميع الدم ونسب INR وقد يؤدي إلى نزف عفوي خطر.\n\nالمصدر: DrugBank / EDA\n\nاستشر طبيبك قبل أي تغيير في علاجك.';
  //       severityLevel = 'danger';
  //       actions.push(
  //         { label: 'شاهد بطاقة الدواء', action: 'digital-card' },
  //         { label: 'تواصل بالطبيب', action: 'call-doctor' },
  //       );
  //     } else if (msg.includes('جرعة') || msg.includes('أغير') || msg.includes('أوقف')) {
  //       reply =
  //         'خط أحمر: بموجب لوائح الأمان والسلامة لشبكة SafeDose، لا يجوز توجيهك لتعديل أو إيقاف الجرعات الطبية دون مراجعة الطبيب المعالج.\n\nاستشر طبيبك قبل أي تغيير في علاجك.';
  //       severityLevel = 'danger';
  //       actions.push({ label: 'تواصل بالطبيب المعالج', action: 'call-doctor' });
  //     } else if (msg.includes('أعراض') || msg.includes('جانب')) {
  //       reply =
  //         'الأعراض الجانبية لدواء جلوكوفاج تشمل الغثيان العابر في الأيام الأولى. بينما دواء كونكور قد يسبب انخفاض نبض القلب أو الخمول العضلي الطفيف.\n\nالمصدر: هيئة الدواء المصرية\n\nاستشر طبيبك قبل أي تغيير في علاجك.';
  //       severityLevel = 'caution';
  //     } else {
  //       reply = `أهلاً بك يا ${patientName}. أنا المساعد الذكي لـ SafeDose AI. يمكنني مساعدتك في فحص التداخلات الدوائية والإجابة عن استفساراتك الطبية.\n\nاستشر طبيبك قبل أي تغيير في علاجك.`;
  //       severityLevel = 'safe';
  //     }
  //     return { reply, severityLevel };
  //   }
}
