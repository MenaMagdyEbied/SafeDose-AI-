// import { ChatAction } from './chat-action';

export interface ChatMessage {
  id: string;
  sender: 'user' | 'bot';
  text: string;
  timestamp: Date;
  severityLevel?: 'safe' | 'caution' | 'danger' | 'info';
  // actions?: ChatAction[];
  isPlayingSpeech?: boolean;
}
