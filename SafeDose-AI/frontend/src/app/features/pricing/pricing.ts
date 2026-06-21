import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
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
import { EMPTY, from } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { ActivatedRoute, Router } from '@angular/router';
import { PricingTier } from '../../core/models/pricing-tier';
import { Subscription } from '../../core/services/subscription';

@Component({
  selector: 'app-pricing',
  imports: [LucideAngularModule],
  templateUrl: './pricing.html',
  styleUrl: './pricing.css',
})
export class Pricing implements OnInit {
  private readonly subscriptionService = inject(Subscription);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  checkIcon = Check;
  crownIcon = Crown;
  shieldIcon = Shield;
  sparklesIcon = Sparkles;
  usersIcon = Users;
  zapIcon = Zap;

  plans = signal<PricingTier[]>([]);
  loading = signal(false);
  upgradeRequired = this.route.snapshot.queryParams['reason'] === 'upgrade-required';

  ngOnInit(): void {
    this.loadTiers();
  }

  private loadTiers(): void {
    this.loading.set(true);

    from(this.subscriptionService.getTiers())
      .pipe(
        catchError(() => EMPTY),
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((result) => {
        const unique = Array.from(new Map(result.map((p) => [p.pricingTierId, p])).values());
        this.plans.set(unique);
      });
  }

  subscribe(tier: PricingTier): void {
    if (tier.tierCode === 'free') {
      this.router.navigate(['/register']);
    } else {
      this.router.navigate(['/payment'], { queryParams: { plan: tier.tierCode } });
    }
  }

  isFree(tier: PricingTier): boolean {
    return tier.tierCode === 'free';
  }

  isPopular(tier: PricingTier): boolean {
    return tier.tierCode === 'premium-monthly';
  }

  getPlanFeatures(plan: PricingTier): string[] {
    const featuresMap: Record<string, string[]> = {
      free: [
        'إنشاء حساب مريض واحد',
        'فحص ٣ تداخلات دوائية شهرياً',
        'مساعد ذكي أساسي',
        'تذكيرات الأدوية',
      ],
      'premium-monthly': [
        'فحص تداخلات غير محدود',
        'حتى ٥ مرضى',
        'مساعد ذكي متقدم',
        'مسح الوصفات الطبية',
        'تقارير للطبيب',
      ],
      'premium-annual': [
        'كل مميزات البريميوم الشهري',
        'توفير ٦٠ جنيه سنوياً',
        'أولوية في الدعم الفني',
        'تحديثات مجانية للسنة',
      ],
    };
    return featuresMap[plan.tierCode] ?? [];
  }
}
