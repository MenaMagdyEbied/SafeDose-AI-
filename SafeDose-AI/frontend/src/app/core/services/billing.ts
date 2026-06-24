import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CheckoutPayload {
  tierCode: string;
  paymentMethod: string;
  fullName: string;
  email: string;
  phoneNumber: string;
}

export interface CheckoutResponse {
  paymentId?: number;
  paymobOrderId?: string;
  iframeUrl?: string;
  paymentUrl?: string;
  amount?: number;
  currency?: string;
}

export interface PaymentStatusResponse {
  success: boolean;
  subscriptionActive?: boolean;
  status?: string;
  statusArabic?: string;
}

@Injectable({
  providedIn: 'root',
})
export class Billing {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  checkout(payload: CheckoutPayload): Promise<CheckoutResponse> {
    return firstValueFrom(
      this.http.post<CheckoutResponse>(this.apiUrl + '/billing/checkout', payload),
    ).then((res) => ({
      ...res,
      paymentUrl: res.paymentUrl ?? res.iframeUrl ?? '',
    }));
  }

  getPaymentStatus(merchantOrderId: string): Promise<PaymentStatusResponse> {
    return firstValueFrom(
      this.http.get<PaymentStatusResponse & { subscriptionActive?: boolean; status?: string }>(
        this.apiUrl + '/billing/payment-status/' + encodeURIComponent(merchantOrderId),
      ),
    ).then((res) => ({
      ...res,
      success:
        res.success === true ||
        res.subscriptionActive === true ||
        String(res.status).toLowerCase() === 'success',
    }));
  }

  /** Poll until subscription activates or attempts exhausted. */
  async waitForPaymentConfirmation(
    merchantOrderId: string,
    maxAttempts = 15,
    intervalMs = 2000,
  ): Promise<PaymentStatusResponse> {
    let last: PaymentStatusResponse = { success: false };
    for (let i = 0; i < maxAttempts; i++) {
      last = await this.getPaymentStatus(merchantOrderId);
      if (last.success || last.subscriptionActive) return last;
      if (String(last.status).toLowerCase() === 'failed') return last;
      if (i < maxAttempts - 1) {
        await new Promise((r) => setTimeout(r, intervalMs));
      }
    }
    return last;
  }

  cancel(): Promise<void> {
    return firstValueFrom(this.http.post<void>(this.apiUrl + '/billing/cancel', {}));
  }
}
