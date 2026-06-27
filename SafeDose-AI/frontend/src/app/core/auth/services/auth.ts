import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';
import { BehaviorSubject, catchError, Observable, of, switchMap, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LoginResponse, MessageResponse } from '../../models/login-response';
import { SessionUser, UserRole } from '../../models/session-user';
import { UserProfileData } from '../../models/user-profile';

const TOKEN_KEY = 'safedose_jwt';
const USER_KEY = 'safedose_user';
const COOKIE_DAYS = 30;

@Injectable({ providedIn: 'root' })
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
    const currentRole = this.userSubject.value?.role;
    if (currentRole) {
      return this.normalizeRole(currentRole);
    }

    const roles = this.userSubject.value?.roles;
    if (Array.isArray(roles) && roles.length) {
      return roles.reduce<UserRole | null>((matched, role) => {
        if (matched) return matched;
        return this.normalizeRole(role);
      }, null);
    }

    return null;
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

  private normalizeRole(role: string | null | undefined): UserRole {
    const normalized = (role ?? '').toString().trim().toLowerCase();

    if (
      normalized === 'superadmin' ||
      normalized === 'super-admin' ||
      normalized === 'super admin'
    ) {
      return 'SuperAdmin';
    }

    if (normalized === 'admin' || normalized === 'administrator' || normalized === 'moderator') {
      return 'Admin';
    }

    return 'User';
  }

  private extractRole(token: string): UserRole {
    const claims = this.decodeToken(token);
    if (!claims) return 'User';
    const role =
      claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      claims['role'] ||
      'User';
    return this.normalizeRole(role);
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
    const isSecure = window.location.protocol === 'https:';
    this.cookieService.set(TOKEN_KEY, token, expire, '/', '', isSecure, 'Strict');
    this.cookieService.set(USER_KEY, JSON.stringify(user), expire, '/', '', isSecure, 'Strict');
    this.userSubject.next(user);
  }

  logout(): void {
    this.cookieService.delete(TOKEN_KEY, '/');
    this.cookieService.delete(USER_KEY, '/');
    try {
      localStorage.removeItem('safedose_pending_patient');
    } catch {}
    this.userSubject.next(null);
  }

  login(payload: { userName: string; password: string }): Observable<SessionUser> {
    return this.http.post<LoginResponse>(this.apiUrl + '/Auth/login', payload).pipe(
      switchMap((res) => {
        if (!res.isAuthenticated) throw new Error('Authentication failed');

        const role = this.extractRole(res.token);
        const fallbackEmail = res.email || this.extractEmail(res.token);

        const fallbackUser: SessionUser = {
          userName: res.userName || payload.userName,
          email: fallbackEmail,
          role,
        };
        this.setSession(fallbackUser, res.token);

        return this.http.get<UserProfileData>(this.apiUrl + '/UserProfile/userProfile').pipe(
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
          switchMap((profile) =>
            this.ensurePrimaryPatientAsync(profile.name || fallbackUser.userName).pipe(
              switchMap(() => of(this.userSubject.value as SessionUser)),
            ),
          ),
          catchError(() => {
            return this.ensurePrimaryPatientAsync(fallbackUser.userName).pipe(
              switchMap(() => of(fallbackUser)),
            );
          }),
        );
      }),
    );
  }

  private ensurePrimaryPatientAsync(fallbackName: string): Observable<void> {
    return this.http.get<any[]>(this.apiUrl + '/patients/my').pipe(
      switchMap((existing) => {
        if (existing && existing.length > 0) {
          try {
            localStorage.removeItem('safedose_pending_patient');
          } catch {}
          return of(void 0);
        }

        let pending: any = null;
        try {
          const raw = localStorage.getItem('safedose_pending_patient');
          if (raw) pending = JSON.parse(raw);
        } catch {
          /* ignore */
        }

        const age = Number(pending?.age);
        let dateOfBirth: string | null = null;
        if (Number.isFinite(age) && age > 0 && age < 130) {
          const birthYear = new Date().getFullYear() - Math.floor(age);
          dateOfBirth = birthYear + '-01-01';
        }

        const body = {
          fullName: pending?.fullName || fallbackName || 'مريض',
          dateOfBirth,
          gender: null,
          bloodType: null,
          chronicConditions: Array.isArray(pending?.chronicConditions)
            ? pending.chronicConditions
            : [],
          allergies: [],
        };

        return this.http.post(this.apiUrl + '/patients', body).pipe(
          tap(() => {
            try {
              localStorage.removeItem('safedose_pending_patient');
            } catch {}
          }),
          switchMap(() => of(void 0)),
          catchError(() => {
            // Patient creation failed, but don't break login flow
            return of(void 0);
          }),
        );
      }),
      catchError(() => {
        // No patients endpoint error, but don't break login flow
        return of(void 0);
      }),
    );
  }

  register(payload: Record<string, unknown>): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(this.apiUrl + '/Auth/register', payload);
  }

  registerAdmin(payload: Record<string, unknown>): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(this.apiUrl + '/Auth/registerAdmin', payload);
  }

  confirmEmail(payload: Record<string, unknown>): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(this.apiUrl + '/Auth/emailConfirmation', payload);
  }

  resendCode(email: string): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(
      this.apiUrl + `/Auth/ResendCode/${encodeURIComponent(email)}`,
      {},
    );
  }

  forgotPassword(payload: Record<string, unknown>): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(this.apiUrl + '/Auth/forgotPassword', payload);
  }

  resetPassword(payload: Record<string, unknown>): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(this.apiUrl + '/Auth/resetPassword', payload);
  }

  updateProfile(updates: Partial<SessionUser>): void {
    const current = this.userSubject.value;
    if (!current) return;
    const updated = { ...current, ...updates };
    const expire = this.getExpireDate(COOKIE_DAYS);
    const isSecure = window.location.protocol === 'https:';
    this.cookieService.set(USER_KEY, JSON.stringify(updated), expire, '/', '', isSecure, 'Strict');
    this.userSubject.next(updated);
  }
}
