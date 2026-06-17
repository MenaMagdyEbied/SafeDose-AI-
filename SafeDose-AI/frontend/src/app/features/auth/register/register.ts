import { Component, computed, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  ArrowLeft,
  ArrowRight,
  Bell,
  CircleCheck,
  Eye,
  EyeOff,
  FileText,
  LucideAngularModule,
  Mic,
  Share2,
  ShieldCheck,
  Stethoscope,
  TriangleAlert,
} from 'lucide-angular';
import { Auth } from '../../../core/auth/services/auth';
import { Permission } from '../../../core/models/permission';
import { passwordsMatchValidator } from '../../../shared/validators/passwords-match-validator';

@Component({
  selector: 'app-register',
  imports: [LucideAngularModule, RouterLink, ReactiveFormsModule],

  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(Auth);
  private readonly router = inject(Router);

  currentStep = 1;
  showPassword = false;
  showConfirm = false;
  loading = false;
  errorText = '';

  arrowLeftIcon = ArrowLeft;
  arrowRightIcon = ArrowRight;
  eyeIcon = Eye;
  eyeOffIcon = EyeOff;
  shieldCheckIcon = ShieldCheck;
  checkCircleIcon = CircleCheck;
  alertTriangleIcon = TriangleAlert;

  conditions = ['السكري', 'ارتفاع ضغط الدم', 'الربو', 'أمراض القلب', 'الحساسية', 'أخرى'];

  private readonly arabicScriptPattern = /[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF]/;

  private englishOnlyValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;

    if (!value || !this.arabicScriptPattern.test(value)) {
      return null;
    }

    return { englishOnly: true };
  }

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

  // ============ Form Groups (واحدة لكل step) ============
  step1Form: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    userName: [
      '',
      [Validators.required, Validators.minLength(3), this.englishOnlyValidator.bind(this)],
    ],
    phone: ['', [Validators.required, Validators.pattern(/^\+?[0-9]{10,14}$/)]],
    email: ['', [Validators.required, Validators.email]],
  });
  step2Form: FormGroup = this.fb.group({
    age: [null, [Validators.required, Validators.min(1), Validators.max(120)]],
    conditions: this.fb.array([]),
    emergency: ['', [Validators.pattern(/^\+?[0-9]{10,14}$/)]],
  });

  step3Form: FormGroup = this.fb.group({
    permissions: this.fb.array([]),
  });

  step4Form: FormGroup = this.fb.group(
    {
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(/^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$/),
        ],
      ],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatchValidator },
  );

  // ============ Getters للوصول السريع من الـ template ============
  get fullName() {
    return this.step1Form.get('fullName');
  }
  get phone() {
    return this.step1Form.get('phone');
  }
  get userName() {
    return this.step1Form.get('userName');
  }

  get email() {
    return this.step1Form.get('email');
  }
  get age() {
    return this.step2Form.get('age');
  }
  get emergency() {
    return this.step2Form.get('emergency');
  }
  get password() {
    return this.step4Form.get('password');
  }
  get confirmPassword() {
    return this.step4Form.get('confirmPassword');
  }

  get selectedConditions(): string[] {
    return (this.step2Form.get('conditions') as FormArray).value;
  }

  get selectedPermissions(): string[] {
    return (this.step3Form.get('permissions') as FormArray).value;
  }

  // ============ Conditions & Permissions toggles ============
  toggleCondition(cond: string): void {
    const arr = this.step2Form.get('conditions') as FormArray;
    const idx = arr.value.indexOf(cond);
    if (idx === -1) arr.push(this.fb.control(cond));
    else arr.removeAt(idx);
  }

  togglePermission(id: string): void {
    const arr = this.step3Form.get('permissions') as FormArray;
    const idx = arr.value.indexOf(id);
    if (idx === -1) arr.push(this.fb.control(id));
    else arr.removeAt(idx);
  }

  get requiredPermissionsAccepted(): boolean {
    return this.permissions
      .filter((p) => p.required)
      .every((p) => this.selectedPermissions.includes(p.id));
  }

  get canSubmit(): boolean {
    return this.step4Form.valid;
  }

  // ============ Navigation ============
  nextStep(): void {
    const currentForm = this.getCurrentForm();
    if (currentForm) {
      currentForm.markAllAsTouched();
      if (currentForm.invalid) return;
    }

    if (this.currentStep === 3 && !this.requiredPermissionsAccepted) {
      return;
    }

    if (this.currentStep < 4) this.currentStep++;
  }

  prevStep(): void {
    if (this.currentStep > 1) this.currentStep--;
  }

  private getCurrentForm(): FormGroup | null {
    switch (this.currentStep) {
      case 1:
        return this.step1Form;
      case 2:
        return this.step2Form;
      case 3:
        return this.step3Form;
      case 4:
        return this.step4Form;
      default:
        return null;
    }
  }

  // ============ Submit ============
  submit(): void {
    this.step4Form.markAllAsTouched();
    if (!this.canSubmit) return;

    this.loading = true;
    this.errorText = '';

    const payload = {
      fullName: this.step1Form.value.fullName,
      phoneNumber: this.step1Form.value.phone,
      userName: this.step1Form.value.userName,
      email: this.step1Form.value.email,
      age: this.step2Form.value.age,
      conditions: this.selectedConditions,
      emergencyContact: this.step2Form.value.emergency,
      permissions: this.selectedPermissions,
      password: this.step4Form.value.password,
      confirmPassword: this.step4Form.value.confirmPassword,
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.loading = false;
        // التوجيه لصفحة تأكيد الإيميل مع تمرير الإيميل في query params
        this.router.navigate(['/email-confirmation'], {
          queryParams: { email: this.step1Form.value.email },
        });
      },
      error: (err) => {
        this.loading = false;
        this.errorText = err?.error?.message || 'حدث خطأ أثناء إنشاء الحساب. حاول مرة أخرى.';
      },
    });
  }
}
