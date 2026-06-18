import { inject, Injectable } from '@angular/core';
import { AdminStats, SubscriptionPlan } from '../models';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { PricingTier } from '../models/pricing-tier';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class Subscription {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  getSubscription(): Promise<PricingTier | null> {
    return firstValueFrom(this.http.get<PricingTier>(`${this.apiUrl}/billing/subscription`)).catch(
      () => null,
    );
  }

  getTiers(): Promise<PricingTier[]> {
    return firstValueFrom(this.http.get<PricingTier[]>(`${this.apiUrl}/billing/tiers`)).catch(
      () => [],
    );
  }
}
