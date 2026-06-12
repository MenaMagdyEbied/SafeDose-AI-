import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  ArrowLeft,
  ArrowRight,
  Bell,
  CheckCircle,
  Eye,
  EyeOff,
  FileText,
  LucideAngularModule,
  Mic,
  Share2,
  ShieldCheck,
  Stethoscope,
} from 'lucide-angular';

export interface Permission {
  id: string;
  title: string;
  description: string;
  required: boolean;
  icon: any;
  color: string;
}

@Component({
  selector: 'app-register',
  imports: [FormsModule, LucideAngularModule,RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  
  currentStep = 1;
  showPassword = false;
  showConfirm = false;

  arrowLeftIcon = ArrowLeft;
  arrowRightIcon = ArrowRight;
  eyeIcon = Eye;
  eyeOffIcon = EyeOff;
  shieldCheckIcon = ShieldCheck;
  checkCircleIcon = CheckCircle;

  form = {
    fullName: '',
    phone: '',
    email: '',
    age: null as number | null,
    conditions: [] as string[],
    emergency: '',
    permissions: [] as string[],
    password: '',
    confirmPassword: '',
  };

  conditions = ['السكري', 'ارتفاع ضغط الدم', 'الربو', 'أمراض القلب', 'الحساسية', 'أخرى'];

  permissions: Permission[] = [
    {
      id: 'medical_data',
      title: 'معالجة البيانات الطبية',
      description: 'تخزين أسماء الأدوية وجدول الجرعات لفحص التداخلات تلقائياً',
      required: true,
      icon: FileText,
      color: 'bg-primary/10 text-primary',
    },
    {
      id: 'notifications',
      title: 'إشعارات الأدوية',
      description: 'إرسال تنبيهات عند موعد تناول الدواء أو وجود تعارض دوائي',
      required: true,
      icon: Bell,
      color: 'bg-secondary-container text-secondary-dark',
    },
    {
      id: 'doctor_share',
      title: 'مشاركة الطبيب',
      description: 'السماح بمشاركة تقارير الأعراض مع طبيبك المعالج فقط',
      required: false,
      icon: Share2,
      color: 'bg-tertiary-container text-tertiary',
    },
    {
      id: 'prescription_scan',
      title: 'تخزين الوصفات الطبية',
      description: 'حفظ صور الوصفات الطبية المُمسوحة ضوئياً لمدة ٩٠ يوماً',
      required: false,
      icon: Stethoscope,
      color: 'bg-surface-container-high text-outline',
    },
    {
      id: 'voice_input',
      title: 'الإدخال الصوتي',
      description: 'استخدام الميكروفون لإدخال أسماء الأدوية بالصوت',
      required: false,
      icon: Mic,
      color: 'bg-surface-container-high text-outline',
    },
  ];

  features = ['تنبيهات الأدوية', 'مشاركة الطبيب', 'تنسيق عائلي', 'دعم HIPAA'];

  get requiredPermissionsAccepted(): boolean {
    return this.permissions
      .filter((p) => p.required)
      .every((p) => this.form.permissions.includes(p.id));
  }

  get canSubmit(): boolean {
    return (
      !!this.form.password &&
      this.form.password === this.form.confirmPassword &&
      this.form.password.length >= 8
    );
  }

  toggleCondition(cond: string): void {
    const idx = this.form.conditions.indexOf(cond);
    if (idx === -1) this.form.conditions.push(cond);
    else this.form.conditions.splice(idx, 1);
  }

  togglePermission(id: string): void {
    const idx = this.form.permissions.indexOf(id);
    if (idx === -1) this.form.permissions.push(id);
    else this.form.permissions.splice(idx, 1);
  }

  nextStep(): void {
    if (this.currentStep < 4) this.currentStep++;
  }

  prevStep(): void {
    if (this.currentStep > 1) this.currentStep--;
  }

  submit(): void {
    if (!this.canSubmit) return;
    console.log('Form submitted:', this.form);
    // TODO: call auth service
  }
}
