import { Component, EventEmitter, Input, Output } from '@angular/core';
import { LucideAngularModule, Trash2, X } from 'lucide-angular';

@Component({
  selector: 'app-confirm-dialog',
  imports: [LucideAngularModule],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialog {
  @Input() isOpen = false;
  @Input() title = 'هل أنت متأكد؟';
  @Input() message = 'لا يمكن التراجع عن هذا الإجراء.';
  @Input() confirmText = 'حذف';
  @Input() cancelText = 'إلغاء';
  @Input() variant: 'danger' | 'warning' | 'primary' = 'danger';

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  trashIcon = Trash2;

  get iconBg(): string {
    return {
      danger: 'bg-danger-container',
      warning: 'bg-secondary-container',
      primary: 'bg-primary/10',
    }[this.variant];
  }

  get iconColor(): string {
    return { danger: 'text-danger', warning: 'text-secondary', primary: 'text-primary' }[
      this.variant
    ];
  }

  get confirmBg(): string {
    return {
      danger: 'bg-danger hover:bg-red-700',
      warning: 'bg-secondary hover:bg-secondary-dark',
      primary: 'bg-primary hover:bg-primary-dark',
    }[this.variant];
  }

  onConfirm(): void {
    this.confirmed.emit();
  }

  onCancel(): void {
    this.cancelled.emit();
  }
}
