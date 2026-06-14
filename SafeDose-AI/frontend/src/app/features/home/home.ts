import { Component, inject } from '@angular/core';
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

@Component({
  selector: 'app-home',
  imports: [LucideAngularModule, RouterLink, FormsModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected readonly router = inject(Router);

  shieldIcon = Shield;
  pillIcon = Pill;
  sparklesIcon = Sparkles;
  activityIcon = Activity;
  alertIcon = TriangleAlert;
  heartIcon = Heart;
  goToPatient(): void {
    this.router.navigate(['/auth'], {
      queryParams: { role: 'patient' },
    });
  }

  goToCaregiver(): void {
    this.router.navigate(['/auth'], {
      queryParams: { role: 'caregiver' },
    });
  }

  plusIcon = Plus;

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
  ];

  showReviewForm = false;
  newReview = { name: '', text: '', rating: 5 };

  getStars(rating: number): number[] {
    return Array(rating).fill(0);
  }

  getEmptyStars(rating: number): number[] {
    return Array(5 - rating).fill(0);
  }

  submitReview() {
    if (!this.newReview.name.trim() || !this.newReview.text.trim()) return;
    this.reviews.unshift({
      id: Date.now(),
      name: this.newReview.name,
      text: this.newReview.text,
      rating: this.newReview.rating,
    });
    this.newReview = { name: '', text: '', rating: 5 };
    this.showReviewForm = false;
  }
}
