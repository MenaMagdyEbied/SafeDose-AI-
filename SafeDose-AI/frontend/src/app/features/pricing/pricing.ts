import { Component, inject } from '@angular/core';
import {
  Check,
  Crown,
  LucideAngularModule,
  Pill,
  Shield,
  Sparkles,
  Users,
  Zap,
} from 'lucide-angular';
import { Auth } from '../../core/auth/services/auth';
import { SubscriptionPlan } from '../../core/models';
import { Subscription } from '../../core/services/subscription';
import { Router } from '@angular/router';

@Component({
  selector: 'app-pricing',
  imports: [LucideAngularModule],
  templateUrl: './pricing.html',
  styleUrl: './pricing.css',
})
export class Pricing {
  isAnnual = false;

  private readonly subscriptionService = inject(Subscription);
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  checkIcon = Check;
  crownIcon = Crown;
  shieldIcon = Shield;
  sparklesIcon = Sparkles;
  usersIcon = Users;
  pillIcon = Pill;
  zapIcon = Zap;

  get plans(): SubscriptionPlan[] {
    return this.subscriptionService.getPlans();
  }

  subscribe(planId: 'free' | 'family'): void {
    if (!this.auth.isLoggedIn) {
      this.router.navigate(['/auth']);
      return;
    }
    this.auth.updateProfile({ subscriptionPlan: planId });
    this.router.navigate(['/patient']);
  }
}
