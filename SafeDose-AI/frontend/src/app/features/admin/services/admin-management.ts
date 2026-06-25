import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

interface AdminListResponse {
  items?: unknown[];
  data?: unknown[];
  [key: string]: unknown;
}

@Injectable({
  providedIn: 'root',
})
export class AdminManagement {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  getAdmins(page = 1, pageSize = 20): Observable<AdminListResponse> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<AdminListResponse>(`${this.apiUrl}/admin/admins`, { params });
  }

  createAdmin(payload: Record<string, unknown>): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/admin/admins`, payload);
  }

  updateAdmin(id: string, payload: Record<string, unknown>): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/admin/admins/${id}`, payload);
  }

  deleteAdmin(id: string): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/admin/admins/${id}`);
  }

  toggleAdminStatus(id: string, isActive: boolean): Observable<unknown> {
    return this.http.patch(`${this.apiUrl}/admin/admins/${id}/status`, { isActive });
  }
}
