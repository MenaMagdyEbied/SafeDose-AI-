import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Patient } from '../../models';
import { environment } from '../../../../environments/environment';
import { RegisterResponse } from '../../models/register-response';
import { LoginResponse } from '../../models/login-response';
import { CookieService } from 'ngx-cookie-service';
import { SessionUser } from '../../models/session-user';
const TOKEN_KEY = 'safedose_jwt';
const USER_KEY = 'safedose_user';
const COOKIE_DAYS = 30;
@Injectable({
  providedIn: 'root',
})
export class Auth {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);
  private readonly cookieService = inject(CookieService);

  private userSubject = new BehaviorSubject<SessionUser | null>(this.readUserFromCookie());
  public user$ = this.userSubject.asObservable();
  showLoginModal = signal(false);

  get user(): SessionUser | null {
    return this.userSubject.value;
  }

  get token(): string | null {
    return this.cookieService.get(TOKEN_KEY) || null;
  }

  get isLoggedIn(): boolean {
    return !!this.token;
  }

  get isAdmin(): boolean {
    // عدّليها بالـ role الفعلي من الـ token لو محتاجة
    return false;
  }

  private readUserFromCookie(): SessionUser | null {
    const raw = this.cookieService.get(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  private getExpireDate(days: number): Date {
    const d = new Date();
    d.setDate(d.getDate() + days);
    return d;
  }

  private setSession(userName: string, email: string, token: string): void {
    const expire = this.getExpireDate(COOKIE_DAYS);
    this.cookieService.set(TOKEN_KEY, token, expire, '/', '', true, 'Strict');
    this.cookieService.set(
      USER_KEY,
      JSON.stringify({ userName, email }),
      expire,
      '/',
      '',
      true,
      'Strict',
    );
    this.userSubject.next({ userName, email });
  }

  logout(): void {
    this.cookieService.delete(TOKEN_KEY, '/');
    this.cookieService.delete(USER_KEY, '/');
    this.userSubject.next(null);
  }

  // ============ API calls ============
  login(payload: { email: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/Auth/login`, payload).pipe(
      tap((res) => {
        if (res.isAuthenticated) {
          this.setSession(res.userName, res.email, res.token);
        }
      }),
    );
  }

  register(payload: Record<string, unknown>): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/Auth/register`, payload);
  }

  confirmEmail(payload: Record<string, unknown>): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/Auth/emailConfirmation`, payload);
  }

  forgotPassword(payload: Record<string, unknown>): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/Auth/forgotPassword`, payload);
  }

  resetPassword(payload: Record<string, unknown>): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/Auth/resetPassword`, payload);
  }

  updateProfile(updates: Partial<SessionUser>): void {
    const current = this.userSubject.value;
    if (!current) return;
    const updated = { ...current, ...updates };
    const expire = this.getExpireDate(COOKIE_DAYS);
    this.cookieService.set(USER_KEY, JSON.stringify(updated), expire, '/', '', true, 'Strict');
    this.userSubject.next(updated);
  }
}
