import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PricingTier } from '../models/pricing-tier';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';

// Shape returned by GET /billing/subscription. Mirrors backend SubscriptionDto.
export interface SubscriptionInfo {
  subscriptionId: number | null;
  tierCode: string;
  tierName: string;
  startAt: string | null;
  endAt: string | null;
  isActive: boolean;
  statusArabic: string;
}

interface AdminPricingTierResponse {
  id: number;
  tierName: string;
  tierNameArabic?: string;
  monthlyPrice?: number;
  patientLimit?: number;
  interactionCheckLimitPerDay?: number;
  medicationLimitPerPatient?: number;
  billingCycleDays?: number;
  isActive?: boolean;
  features?: Array<{ id?: number; labelArabic?: string; labelEnglish?: string }>;
}

@Injectable({
  providedIn: 'root',
})
export class Subscription {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);
  private readonly storageKey = 'safedose-pricing-tiers';

  // Cached live view of the current user's subscription. Components read this signal
  // instead of re-calling the API on every nav. Cleared on logout.
  readonly current = signal<SubscriptionInfo | null>(null);
  readonly hasActivePaidPlan = signal(false);
  readonly tiers = signal<PricingTier[]>([]);

  constructor() {
    this.tiers.set(this.readStoredTiers());
  }

  async refresh(): Promise<SubscriptionInfo | null> {
    try {
      const sub = await firstValueFrom(
        this.http.get<SubscriptionInfo>(`${this.apiUrl}/billing/subscription`),
      );
      this.current.set(sub);
      this.hasActivePaidPlan.set(!!sub?.isActive && sub.tierCode !== 'free');
      return sub;
    } catch {
      this.current.set(null);
      this.hasActivePaidPlan.set(false);
      return null;
    }
  }

  clear(): void {
    this.current.set(null);
    this.hasActivePaidPlan.set(false);
    if (typeof window !== 'undefined') {
      window.localStorage.removeItem(this.storageKey);
    }
    this.tiers.set(this.getDefaultTiers());
  }

  getSubscription(): Promise<SubscriptionInfo | null> {
    return this.refresh();
  }

  async getTiers(): Promise<PricingTier[]> {
    const stored = this.readStoredTiers();
    this.tiers.set(stored);

    try {
      const remote = await firstValueFrom(
        this.http.get<PricingTier[]>(`${this.apiUrl}/billing/tiers`),
      );
      if (remote?.length) {
        const normalized = this.normalizeTiers(remote);
        this.persistTiers(normalized);
        return normalized;
      }
    } catch {
      // Fall back to the locally saved tiers if the billing endpoint is unavailable.
    }

    return stored;
  }

  async getAdminTiers(): Promise<PricingTier[]> {
    try {
      const remote = await firstValueFrom(
        this.http.get<AdminPricingTierResponse[]>(`${this.apiUrl}/admin/pricing-tiers`),
      );
      if (remote?.length) {
        const normalized = this.mapAdminTiers(remote);
        this.persistTiers(normalized);
        return normalized;
      }
    } catch {
      // Fall back to the existing cached tiers if the admin endpoint is unavailable.
    }

    return this.tiers();
  }

  async saveTiers(tiers: PricingTier[]): Promise<PricingTier[]> {
    const normalized = this.normalizeTiers(tiers);
    this.persistTiers(normalized);

    await Promise.all(
      normalized.map(async (tier, index) => {
        const tierId = tier.pricingTierId || index + 1;
        try {
          await firstValueFrom(
            this.http.put(`${this.apiUrl}/admin/pricing-tiers/${tierId}`, {
              tierName: tier.tierName || tier.nameAr || 'Plan',
              tierNameArabic: tier.tierNameArabic || tier.nameAr || tier.tierName || 'خطة',
              monthlyPrice: Number(tier.price ?? 0),
              patientLimit: Number(tier.patientLimit ?? 1),
              interactionCheckLimitPerDay: 3,
              medicationLimitPerPatient: 1,
              billingCycleDays: tier.tierCode === 'premium-annual' ? 365 : 30,
              isActive: true,
            }),
          );

          for (const feature of tier.features ?? []) {
            if (!feature?.trim()) continue;
            try {
              await firstValueFrom(
                this.http.post(`${this.apiUrl}/admin/pricing-tiers/${tierId}/features`, {
                  labelArabic: feature,
                }),
              );
            } catch {
              // Ignore per-feature sync failures and keep the local copy intact.
            }
          }
        } catch {
          // Keep the local UI state even if the API call fails.
        }
      }),
    );

    return normalized;
  }

  private normalizeTiers(tiers: PricingTier[]): PricingTier[] {
    const defaults = this.getDefaultTiers();
    const byCode = new Map(defaults.map((tier) => [tier.tierCode, tier]));

    return tiers.map((tier, index) => {
      const fallback = byCode.get(tier.tierCode) ?? defaults[index] ?? defaults[0];
      const price = Number(tier.price ?? fallback.price) || 0;
      const tierName = tier.tierName || tier.nameAr || fallback.tierName;
      const priceLabelArabic = tier.priceLabelArabic || this.getPriceLabel(tier.tierCode, price);
      const features = tier.features ?? fallback.features ?? [];

      return {
        ...fallback,
        ...tier,
        pricingTierId: tier.pricingTierId || index + 1,
        tierCode: tier.tierCode || fallback.tierCode,
        tierName,
        price,
        currency: tier.currency || 'EGP',
        patientLimit: tier.patientLimit || fallback.patientLimit,
        priceLabelArabic,
        features,
      };
    });
  }

  private mergeTiers(localTiers: PricingTier[], remoteTiers: PricingTier[]): PricingTier[] {
    const localByCode = new Map(localTiers.map((tier) => [tier.tierCode, tier]));
    const remoteByCode = new Map(remoteTiers.map((tier) => [tier.tierCode, tier]));
    const fallbackTiers = this.getDefaultTiers();

    return fallbackTiers.map((fallbackTier) => {
      const localTier = localByCode.get(fallbackTier.tierCode);
      const remoteTier = remoteByCode.get(fallbackTier.tierCode);
      const baseTier = this.normalizeTiers([remoteTier ?? fallbackTier])[0];

      return {
        ...baseTier,
        ...localTier,
        pricingTierId:
          localTier?.pricingTierId ?? remoteTier?.pricingTierId ?? baseTier.pricingTierId,
        tierCode: localTier?.tierCode ?? remoteTier?.tierCode ?? baseTier.tierCode,
        tierName:
          localTier?.tierName || localTier?.nameAr || remoteTier?.tierName || baseTier.tierName,
        price: localTier?.price ?? remoteTier?.price ?? baseTier.price,
        currency: localTier?.currency || remoteTier?.currency || baseTier.currency,
        patientLimit: localTier?.patientLimit ?? remoteTier?.patientLimit ?? baseTier.patientLimit,
        priceLabelArabic:
          localTier?.priceLabelArabic || remoteTier?.priceLabelArabic || baseTier.priceLabelArabic,
        features: localTier?.features?.length
          ? localTier.features
          : (remoteTier?.features ?? baseTier.features),
        nameAr: localTier?.nameAr || remoteTier?.nameAr || baseTier.nameAr,
        nameEn: localTier?.nameEn || remoteTier?.nameEn || baseTier.nameEn,
      };
    });
  }

  private mapAdminTiers(adminTiers: AdminPricingTierResponse[]): PricingTier[] {
    const defaultTiers = this.getDefaultTiers();
    const planCodes = ['free', 'premium-monthly', 'premium-annual'];

    return adminTiers.map((tier, index) => {
      const fallback = defaultTiers[index] ?? defaultTiers[0];
      const tierCode = planCodes[index] ?? `tier-${tier.id ?? index + 1}`;
      const price = Number(tier.monthlyPrice ?? fallback.price) || 0;
      const features = (tier.features ?? [])
        .map((feature) => feature.labelArabic || '')
        .filter(Boolean);

      return {
        ...fallback,
        pricingTierId: tier.id ?? index + 1,
        tierCode,
        tierName: tier.tierNameArabic || tier.tierName || fallback.tierName,
        price,
        currency: 'EGP',
        patientLimit: Number(tier.patientLimit ?? fallback.patientLimit) || fallback.patientLimit,
        priceLabelArabic:
          price === 0
            ? 'مجاني'
            : tierCode === 'premium-annual'
              ? `${price} ج.م / سنة`
              : `${price} ج.م / شهر`,
        features: features.length ? features : fallback.features,
        nameAr: tier.tierNameArabic || tier.tierName || fallback.nameAr,
        nameEn: tier.tierName || fallback.nameEn,
        tierNameArabic: tier.tierNameArabic || tier.tierName || fallback.nameAr,
        monthlyPrice: price,
        interactionCheckLimitPerDay: tier.interactionCheckLimitPerDay,
        medicationLimitPerPatient: tier.medicationLimitPerPatient,
        billingCycleDays: tier.billingCycleDays ?? (tierCode === 'premium-annual' ? 365 : 30),
        isActive: tier.isActive ?? true,
      };
    });
  }

  private persistTiers(tiers: PricingTier[]): void {
    this.tiers.set(tiers);
    if (typeof window !== 'undefined') {
      window.localStorage.setItem(this.storageKey, JSON.stringify(tiers));
    }
  }

  private readStoredTiers(): PricingTier[] {
    if (typeof window === 'undefined') {
      return this.getDefaultTiers();
    }

    const stored = window.localStorage.getItem(this.storageKey);
    if (!stored) {
      return this.getDefaultTiers();
    }

    try {
      const parsed = JSON.parse(stored) as PricingTier[];
      return this.normalizeTiers(parsed);
    } catch {
      return this.getDefaultTiers();
    }
  }

  private getDefaultTiers(): PricingTier[] {
    return [
      {
        pricingTierId: 1,
        tierCode: 'free',
        tierName: 'المجاني',
        price: 0,
        currency: 'EGP',
        patientLimit: 1,
        priceLabelArabic: 'مجاني',
        features: [
          'إنشاء حساب',
          'فحص ٣ تداخلات دوائية شهرياً',
          'تتبع دواء واحد',
          'مساعد ذكي أساسي',
        ],
        nameAr: 'المجاني',
        nameEn: 'Free',
      },
      {
        pricingTierId: 2,
        tierCode: 'premium-monthly',
        tierName: 'مدفوع شهرياً',
        price: 99,
        currency: 'EGP',
        patientLimit: 5,
        priceLabelArabic: '99 ج.م / شهر',
        features: [
          'كل ميزات الباقة المجانية',
          'فحص تداخلات دوائية غير محدود',
          'تتبع أدوية غير محدود',
          'حتى ٥ أفراد عائلة',
          'مساعد ذكي متقدم',
          'تقارير للطبيب',
          'أولوية الدعم',
        ],
        nameAr: 'مدفوع شهرياً',
        nameEn: 'Premium Monthly',
      },
      {
        pricingTierId: 3,
        tierCode: 'premium-annual',
        tierName: 'مدفوع سنوياً',
        price: 990,
        currency: 'EGP',
        patientLimit: 5,
        priceLabelArabic: '990 ج.م / سنة',
        features: [
          'كل ميزات الباقة الشهرية',
          'توفير ١١٨ ج.م عن الاشتراك الشهري',
          'أولوية في الدعم الفني',
          'تحديثات مجانية للسنة',
        ],
        nameAr: 'مدفوع سنوياً',
        nameEn: 'Premium Annual',
      },
    ];
  }

  private getPriceLabel(tierCode: string, price: number): string {
    if (price === 0) return 'مجاني';
    if (tierCode === 'premium-annual') return `${price} ج.م / سنة`;
    return `${price} ج.م / شهر`;
  }
}
