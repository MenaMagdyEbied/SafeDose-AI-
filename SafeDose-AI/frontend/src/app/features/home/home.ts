import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  Activity,
  TriangleAlert,
  Heart,
  LucideAngularModule,
  Pill,
  Shield,
  Sparkles,
} from 'lucide-angular';
import { Plus } from 'lucide-angular';
import { Auth } from '../../core/auth/services/auth';

@Component({
  selector: 'app-home',
  imports: [LucideAngularModule, RouterLink, FormsModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected readonly router = inject(Router);
  protected readonly authService = inject(Auth);

  shieldIcon = Shield;
  pillIcon = Pill;
  sparklesIcon = Sparkles;
  activityIcon = Activity;
  alertIcon = TriangleAlert;
  heartIcon = Heart;
  plusIcon = Plus;

  goToPatient(): void {
    this.router.navigate(['/auth'], { queryParams: { role: 'patient' } });
  }

  goToCaregiver(): void {
    this.router.navigate(['/auth'], { queryParams: { role: 'caregiver' } });
  }

  // IDs MUST be unique — Angular @for track uses them to identify rows.
  reviews = [
    {
      id: 1,
      name: 'د. أحمد كمال',
      rating: 5,
      text: 'لقد غير هذا التطبيق تماماً أسلوب متابعتي لأدوية والديّ المسنين.',
    },
    {
      id: 2,
      name: 'سارة محمود',
      rating: 5,
      text: 'تطبيق ممتاز! ساعدني كتير في متابعة أدوية أمي. التنبيهات دايماً في وقتها.',
    },
    { id: 3, name: 'محمد علي', rating: 4, text: 'واجهة سهلة وبسيطة، وفحص التداخلات دقيق جداً.' },
    { id: 4, name: 'خلف علي', rating: 4, text: 'فحص التداخلات لقطة، بيوفر وقت كبير.' },
    { id: 5, name: 'فاطمة سعيد', rating: 5, text: 'بحب التذكيرات والـ UI واضح ومريح للعين.' },
  ];

  showReviewForm = signal(false);
  newReview = signal({ name: '', text: '', rating: 5 });

  getStars(rating: number): number[] {
    return Array(rating).fill(0);
  }

  getEmptyStars(rating: number): number[] {
    return Array(5 - rating).fill(0);
  }

  submitReview(): void {
    if (!this.newReview().name.trim() || !this.newReview().text.trim()) return;
    this.reviews.unshift({
      id: Date.now(),
      name: this.newReview().name,
      text: this.newReview().text,
      rating: this.newReview().rating,
    });
    this.newReview.set({ name: '', text: '', rating: 5 });
    this.showReviewForm.set(false);
  }
}
