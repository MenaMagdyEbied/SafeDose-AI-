import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class UserProfile {
  private readonly apiUrl = environment.apiUrl;

  private readonly httpClient = inject(HttpClient);

  getUserProfile(): Observable<unknown> {
    return this.httpClient.get(`${this.apiUrl}/UserProfile/userProfile`);
  }

  updateName(payload: { [key: string]: unknown }): Observable<unknown> {
    return this.httpClient.put(`${this.apiUrl}/UserProfile/updateName`, payload);
  }

  updateEmail(payload: { [key: string]: unknown }): Observable<unknown> {
    return this.httpClient.put(`${this.apiUrl}/UserProfile/updateEmail`, payload);
  }

  updatePhone(payload: { [key: string]: unknown }): Observable<unknown> {
    return this.httpClient.put(`${this.apiUrl}/UserProfile/updatePhone`, payload);
  }
}
