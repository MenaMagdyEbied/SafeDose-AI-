import { Component, inject } from '@angular/core';
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

@Component({
  selector: 'app-home',
  imports: [LucideAngularModule, RouterLink],
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
}
