import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  Crown,
  Shield,
  Trash2,
  Plus,
  Save,
  Info,
  CheckCircle,
  CircleCheck,
} from 'lucide-angular';
import { Plan } from '../../../core/models/plan';

@Component({
  selector: 'app-admin-pricing',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './admin-pricing.html',
  styleUrl: './admin-pricing.css',
})
export class AdminPricing {
  crownIcon = Crown;
  shieldIcon = Shield;
  trashIcon = Trash2;
  plusIcon = Plus;
  saveIcon = Save;
  infoIcon = Info;
  checkCircleIcon = CircleCheck;

  savedMessage = '';

  plans: Plan[] = [
    {
      id: 'free',
      nameAr: 'المجاني',
      nameEn: 'Free',
      price: 0,
      features: ['إنشاء حساب', 'فحص ٣ تداخلات دوائية شهرياً', 'تتبع دواء واحد', 'مساعد ذكي أساسي'],
    },
    {
      id: 'family',
      nameAr: 'العائلي',
      nameEn: 'Family',
      price: 99,
      features: [
        'كل ميزات الباقة المجانية',
        'فحص تداخلات دوائية غير محدود',
        'تتبع أدوية غير محدود',
        'حتى ٥ أفراد عائلة',
        'مساعد ذكي متقدم',
        'تقارير للطبيب',
        'أولوية الدعم',
      ],
    },
  ];

  addFeature(plan: Plan): void {
    plan.features.push('');
  }

  removeFeature(plan: Plan, index: number): void {
    plan.features.splice(index, 1);
  }

  saveAll(): void {
    // TODO: استدعاء API لحفظ البيانات
    this.savedMessage = 'تم حفظ التغييرات بنجاح ✅';
    setTimeout(() => (this.savedMessage = ''), 3000);
  }
}
