import { Injectable, signal } from '@angular/core';
export interface Toast {
  id: number;
  type: 'success' | 'error';
  message: string;
}
@Injectable({
  providedIn: 'root',
})
export class ToastService {
  toasts = signal<Toast[]>([]);
  private counter = 0;

  show(type: 'success' | 'error', message: string, duration = 4000): void {
    const id = ++this.counter;
    this.toasts.update((list) => [...list, { id, type, message }]);
    setTimeout(() => this.dismiss(id), duration);
  }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }
}
