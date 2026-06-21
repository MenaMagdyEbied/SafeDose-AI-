import { Component, input, output } from '@angular/core';
import { LucideAngularModule, Trash2 } from 'lucide-angular';

@Component({
  selector: 'app-confirm-dialog',
  imports: [LucideAngularModule],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialog {
  readonly isOpen = input(false);
  readonly title = input('هل أنت متأكد؟');
  readonly message = input('لا يمكن التراجع عن هذا الإجراء.');
  readonly confirmText = input('حذف');
  readonly cancelText = input('إلغاء');
  readonly variant = input<'danger' | 'warning' | 'primary'>('danger');

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  trashIcon = Trash2;

  get iconBg(): string {
    return {
      danger: 'bg-danger-container',
      warning: 'bg-secondary-container',
      primary: 'bg-primary/10',
    }[this.variant()];
  }

  get iconColor(): string {
    return { danger: 'text-danger', warning: 'text-secondary', primary: 'text-primary' }[
      this.variant()
    ];
  }

  get confirmBg(): string {
    return {
      danger: 'bg-danger hover:bg-red-700',
      warning: 'bg-secondary hover:bg-secondary-dark',
      primary: 'bg-primary hover:bg-primary-dark',
    }[this.variant()];
  }

  onConfirm(): void {
    this.confirmed.emit();
  }

  onCancel(): void {
    this.cancelled.emit();
  }
}
