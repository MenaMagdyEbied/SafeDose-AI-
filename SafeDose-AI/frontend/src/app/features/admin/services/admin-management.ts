import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AdminManagement {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  registerAdmin(payload: Record<string, unknown>): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/Auth/registerAdmin`, payload);
  }

  // getDashboardStats(): Observable<any> {
  //   return this.http.get(`${this.apiUrl}/Admin/statistics`);
  // }

  // updatePrices(priceData: { itemId: string; newPrice: number }): Observable<any> {
  //   return this.http.put(`${this.apiUrl}/Admin/update-prices`, priceData);
  // }
}
