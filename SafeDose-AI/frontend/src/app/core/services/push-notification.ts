import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SwPush } from '@angular/service-worker';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Router } from '@angular/router';

export interface ReminderResponseAddDTO {
  patientMedicationId: number;
  drugName: string | null;
  drugTime: string | null;
  responseType: number;
}

const PUBLIC_VAPID_KEY =
  'BHWefFkQzDDwqqIF3m_0A0YGuWVqHd9HeaUilmAwBaSW3g4Kbn6K6ZEz8f084xf3Vi1QWmf6avcvLCdhapBNyU8';

@Injectable({
  providedIn: 'root',
})
export class PushNotification {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);
  private readonly swPush = inject(SwPush);

  get isSupported(): boolean {
    return this.swPush.isEnabled;
  }

  get isPermissionGranted(): boolean {
    return Notification.permission === 'granted';
  }

  async subscribe(): Promise<boolean> {
    if (!this.isSupported) return false;

    try {
      const subscription = await this.swPush.requestSubscription({
        serverPublicKey: PUBLIC_VAPID_KEY,
      });

      const subJson = subscription.toJSON();

      await firstValueFrom(
        this.http.post(`${this.apiUrl}/PushSubscription/Subscripe`, {
          endpoint: subscription.endpoint,
          p256DH: subJson.keys?.['p256dh'],
          auth: subJson.keys?.['auth'],
        }),
      );

      return true;
    } catch (err) {
      console.error('Push subscription failed:', err);
      return false;
    }
  }

  addReminderResponse(payload: ReminderResponseAddDTO): Promise<unknown> {
    return firstValueFrom(
      this.http.post(`${this.apiUrl}/ReminderResponse/addReminderResponse`, payload),
    );
  }

  getReminderHistory(patientId: number): Promise<unknown[]> {
    return firstValueFrom(this.http.get<unknown[]>(`${this.apiUrl}/ReminderResponse/${patientId}`));
  }

  listenToNotificationClicks(): void {
    this.swPush.notificationClicks.subscribe(({ action, notification }) => {
      console.log('notification clicked', action, notification);
    });
  }
}
