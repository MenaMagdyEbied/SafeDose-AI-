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
  paymentUrl?: string;
  iframeUrl?: string;
}

export interface PaymentStatusResponse {
  success: boolean;
  status: string;
  statusArabic: string;
  amount: number;
  currency: string;
  subscriptionActive: boolean;
  tierCode?: string;
  tierName?: string;
  paidAt?: string;
  subscriptionEndAt?: string;
}
@Injectable({
  providedIn: 'root',
})
export class Billing {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  checkout(payload: CheckoutPayload): Promise<CheckoutResponse> {
    return firstValueFrom(
      this.http.post<CheckoutResponse>(`${this.apiUrl}/billing/checkout`, payload),
    );
  }

  getPaymentStatus(
    merchantOrderId: string,
    paymobSuccess?: boolean,
  ): Promise<PaymentStatusResponse> {
    const successQuery = paymobSuccess === undefined ? '' : `?success=${paymobSuccess}`;

    return firstValueFrom(
      this.http.get<PaymentStatusResponse>(
        `${this.apiUrl}/billing/payment-status/${merchantOrderId}${successQuery}`,
      ),
    );
  }
}
