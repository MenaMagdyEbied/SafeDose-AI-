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

@Injectable({
  providedIn: 'root',
})
export class Subscription {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  // Cached live view of the current user's subscription. Components read this signal
  // instead of re-calling the API on every nav. Cleared on logout.
  readonly current = signal<SubscriptionInfo | null>(null);
  readonly hasActivePaidPlan = signal(false);

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
  }

  getSubscription(): Promise<SubscriptionInfo | null> {
    return this.refresh();
  }

  getTiers(): Promise<PricingTier[]> {
    return firstValueFrom(this.http.get<PricingTier[]>(`${this.apiUrl}/billing/tiers`)).catch(
      () => [],
    );
  }
}
