import { Component, inject } from '@angular/core';
import { ChatMessage } from '../../../core/models';
import { Activity, LucideAngularModule, MessageSquare, Mic, Send, X } from 'lucide-angular';
import { Auth } from '../../../core/auth/services/auth';
import { Router } from '@angular/router';
import { ChatBotService } from '../../../core/services/chat-bot-service';
import { FormsModule } from '@angular/forms';

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

  isOpen = false;
  inputText = '';
  loading = false;
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

  getMsgClass(msg: ChatMessage): string {
    if (msg.sender === 'user')
      return 'p-3.5 rounded-2xl text-md leading-relaxed shadow-xs bg-primary text-white rounded-br-none text-right';
    const base = 'p-3.5 rounded-2xl text-md leading-relaxed shadow-xs rounded-bl-none text-right ';
    switch (msg.severityLevel) {
      case 'danger':
        return base + 'bg-danger-container text-on-danger-container border-r-4 border-danger';
      case 'caution':
        return base + 'bg-secondary-container text-on-secondary border-r-4 border-secondary';
      case 'safe':
        return base + 'bg-emerald-50 text-emerald-950 border-r-4 border-emerald-500';
      default:
        return base + 'bg-slate-50 text-on-background border-r-4 border-primary';
    }
  }

  async sendMessage(): Promise<void> {
    const text = this.inputText.trim();
    if (!text) return;
    this.messages = [
      ...this.messages,
      { id: 'user-' + Date.now(), sender: 'user', text, timestamp: new Date() },
    ];
    this.inputText = '';
    this.loading = true;

    const patientName = this.auth.user?.name || 'أحمد';
    const result = await this.chatbotService.sendMessage(text, this.messages, patientName);
    this.messages = [
      ...this.messages,
      {
        id: 'bot-' + Date.now(),
        sender: 'bot',
        text: result.reply,
        timestamp: new Date(),
        severityLevel: result.severityLevel,
        actions: result.actions,
      },
    ];
    this.loading = false;
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
