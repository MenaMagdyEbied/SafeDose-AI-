import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ChatMessage } from '../../../core/models';
import { Activity, LucideAngularModule, MessageSquare, Mic, Send, X } from 'lucide-angular';
import { Auth } from '../../../core/auth/services/auth';
import { Router } from '@angular/router';
import { ChatBotService } from '../../../core/services/chat-bot-service';
import { FormsModule } from '@angular/forms';
import { EMPTY, from } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

@Component({
  selector: 'app-chat-bot',
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './chat-bot.html',
  styleUrl: './chat-bot.css',
})
export class ChatBot {
  private readonly chatbotService = inject(ChatBotService);
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  isOpen = false;
  inputText = '';
  loading = signal(false);

  messages: ChatMessage[] = [
    {
      id: 'initial',
      sender: 'bot',
      text: 'مرحباً، أنا مساعد SafeDose الذكي. اسألني عن أدويتك وتداخلاتها وأعراضها الجانبية.',
      timestamp: new Date(),
      severityLevel: 'info',
    },
  ];

  quickChips = [
    'تحقق من تداخل دوائين',
    'ما هو دواء جلوكوفاج؟',
    'هل يمكنني تغيير جرعتي؟',
    'أعراض كونكور الجانبية',
  ];

  messageSquareIcon = MessageSquare;
  xIcon = X;
  sendIcon = Send;
  micIcon = Mic;
  heartRateIcon = Activity;

  // Bot bubbles: clean white with a thin colored stripe so it feels like a real
  // assistant chatting — not an alarm popup. Only Level-3 danger keeps a stronger
  // red tint since it's an actual emergency.
  getMsgClass(msg: ChatMessage): string {
    if (msg.sender === 'user') {
      return 'p-3.5 rounded-2xl text-md leading-relaxed shadow-xs bg-primary text-white rounded-br-none text-right';
    }
    const base =
      'p-3.5 rounded-2xl text-md leading-relaxed shadow-xs rounded-bl-none text-right text-on-background bg-white border ';
    switch (msg.severityLevel) {
      case 'danger':
        return 'p-3.5 rounded-2xl text-md leading-relaxed shadow-xs rounded-bl-none text-right text-on-background bg-rose-50 border border-rose-200 border-r-4 border-r-rose-500';
      case 'caution':
        return base + 'border-outline-variant border-r-4 border-r-amber-400';
      case 'safe':
        return base + 'border-outline-variant border-r-4 border-r-emerald-500';
      default:
        return base + 'border-outline-variant border-r-4 border-r-primary';
    }
  }

  sendMessage(): void {
    const text = this.inputText.trim();
    if (!text) return;

    this.messages = [
      ...this.messages,
      { id: 'user-' + Date.now(), sender: 'user', text, timestamp: new Date() },
    ];
    this.inputText = '';
    this.loading.set(true);

    const patientName = this.auth.user?.userName || 'أحمد';

    from(this.chatbotService.sendMessage(text, this.messages, patientName))
      .pipe(
        catchError(() => {
          this.messages = [
            ...this.messages,
            {
              id: 'bot-' + Date.now(),
              sender: 'bot',
              text: 'حدث خطأ أثناء معالجة الرسالة. حاول مرة أخرى.',
              timestamp: new Date(),
              severityLevel: 'info',
            },
          ];
          return EMPTY;
        }),
        finalize(() => {
          this.loading.set(false);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((result) => {
        this.messages = [
          ...this.messages,
          {
            id: 'bot-' + Date.now(),
            sender: 'bot',
            text: result.reply,
            timestamp: new Date(),
            severityLevel: result.severityLevel,
            patientOptions:
              result.intent === 'needs_patient_selection' && result.availablePatients?.length
                ? result.availablePatients.map((p) => ({
                    patientId: p.patientId,
                    fullName: p.fullName,
                  }))
                : undefined,
          },
        ];
      });
  }

  selectPatient(patientId: number, fullName: string): void {
    this.chatbotService.setPatient(patientId);
    this.messages = [
      ...this.messages,
      {
        id: 'user-select-' + Date.now(),
        sender: 'user',
        text: `أريد المتابعة بخصوص: ${fullName}`,
        timestamp: new Date(),
      },
    ];
    this.loading.set(true);

    from(
      this.chatbotService.sendMessage(`أريد المتابعة بخصوص: ${fullName}`, this.messages, fullName),
    )
      .pipe(
        catchError(() => {
          this.messages = [
            ...this.messages,
            {
              id: 'bot-' + Date.now(),
              sender: 'bot',
              text: 'حدث خطأ أثناء معالجة الرسالة. حاول مرة أخرى.',
              timestamp: new Date(),
              severityLevel: 'info',
            },
          ];
          return EMPTY;
        }),
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((result) => {
        this.messages = [
          ...this.messages,
          {
            id: 'bot-' + Date.now(),
            sender: 'bot',
            text: result.reply,
            timestamp: new Date(),
            severityLevel: result.severityLevel,
          },
        ];
      });
  }

  sendChip(chip: string): void {
    this.inputText = chip;
    this.sendMessage();
  }

  triggerAction(action: string): void {
    if (action === 'digital-card') this.router.navigate(['/digital-card']);
    else if (action === 'call-doctor') alert('جاري الاتصال بالطبيب المناوب...');
    else if (action === 'report-symptoms') alert('تم فتح تقرير تسجيل الأعراض.');
    this.isOpen = false;
  }
}
