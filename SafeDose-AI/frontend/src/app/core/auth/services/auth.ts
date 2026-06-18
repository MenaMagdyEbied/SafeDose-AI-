import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, catchError, Observable, of, switchMap, tap } from 'rxjs';
import { Patient } from '../../models';
import { environment } from '../../../../environments/environment';
import { RegisterResponse } from '../../models/register-response';
import { LoginResponse, MessageResponse } from '../../models/login-response';
import { CookieService } from 'ngx-cookie-service';
import { SessionUser, UserRole } from '../../models/session-user';
import { UserProfileData } from '../../models/user-profile';
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

  get role(): UserRole | null {
    return this.userSubject.value?.role ?? null;
  }

  get isUser(): boolean {
    return this.role === 'User';
  }

  get isAdmin(): boolean {
    return this.role === 'Admin' || this.role === 'SuperAdmin';
  }

  get isSuperAdmin(): boolean {
    return this.role === 'SuperAdmin';
  }

  private decodeToken(token: string): Record<string, any> | null {
    try {
      const payload = token.split('.')[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch {
      return null;
    }
  }

  private extractRole(token: string): UserRole {
    const claims = this.decodeToken(token);
    if (!claims) return 'User';

    const role =
      claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      claims['role'] ||
      'User';

    return role as UserRole;
  }

  private extractEmail(token: string): string {
    const claims = this.decodeToken(token);
    return claims?.['email'] || '';
  }

  private getExpireDate(days: number): Date {
    const d = new Date();
    d.setDate(d.getDate() + days);
    return d;
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

  private setSession(user: SessionUser, token: string): void {
    const expire = this.getExpireDate(COOKIE_DAYS);
    this.cookieService.set(TOKEN_KEY, token, expire, '/', '', true, 'Strict');
    this.cookieService.set(USER_KEY, JSON.stringify(user), expire, '/', '', true, 'Strict');
    this.userSubject.next(user);
  }

  logout(): void {
    this.cookieService.delete(TOKEN_KEY, '/');
    this.cookieService.delete(USER_KEY, '/');
    this.userSubject.next(null);
  }


  login(payload: { userName: string; password: string }): Observable<SessionUser> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/Auth/login`, payload).pipe(
      switchMap((res) => {
        if (!res.isAuthenticated) {
          throw new Error('Authentication failed');
        }

        const role = this.extractRole(res.token);
        const fallbackEmail = res.email || this.extractEmail(res.token);

        const fallbackUser: SessionUser = {
          userName: res.userName || payload.userName,
          email: fallbackEmail,
          role,
        };
        this.setSession(fallbackUser, res.token);

        // دلوقتي نجيب بيانات الـ profile الكاملة ونحدث الجلسة بيها
        return this.http.get<UserProfileData>(`${this.apiUrl}/UserProfile/userProfile`).pipe(
          tap((profile) => {
            const fullUser: SessionUser = {
              userName: profile.userName,
              email: profile.email,
              name: profile.name,
              phone: profile.phone,
              role,
              roles: profile.roles as UserRole[],
            };
            this.setSession(fullUser, res.token);
          }),
          switchMap(() => of(this.userSubject.value as SessionUser)),
          catchError(() => {
            // لو فشل جلب الـ profile، نفضل بالـ fallback اللي خزناه فوق
            return of(fallbackUser);
          }),
        );
      }),
    );
  }

  register(payload: Record<string, unknown>): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/Auth/register`, payload);
  }

  registerAdmin(payload: Record<string, unknown>): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/Auth/registerAdmin`, payload);
  }

  confirmEmail(payload: Record<string, unknown>): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/Auth/emailConfirmation`, payload);
  }

  forgotPassword(payload: Record<string, unknown>): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/Auth/forgotPassword`, payload);
  }

  resetPassword(payload: Record<string, unknown>): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/Auth/resetPassword`, payload);
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
