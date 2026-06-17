import { Component, inject } from '@angular/core';
import { LucideAngularModule, CircleCheck, CircleX, X } from 'lucide-angular';
import { ToastService } from '../../../core/auth/services/toast-service';

@Component({
  selector: 'app-toast',
  imports: [LucideAngularModule],
  templateUrl: './toast.html',
  styleUrl: './toast.css',
})
export class Toast {
  toastService = inject(ToastService);
  checkIcon = CircleCheck;
  xCircleIcon = CircleX;
  xIcon = X;
}
