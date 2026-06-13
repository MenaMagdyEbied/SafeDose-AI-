import { Injectable, signal } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Patient } from '../../models';

@Injectable({
  providedIn: 'root',
})
export class Auth {
  private userSubject = new BehaviorSubject<Patient | null>(null);
  public user$ = this.userSubject.asObservable();
  showLoginModal = signal(false);

  openLogin() {
    this.showLoginModal.set(true);
  }
  closeLogin() {
    this.showLoginModal.set(false);
  }
  constructor() {
    const stored = localStorage.getItem('safeDose_user');
    if (stored) {
      try {
        this.userSubject.next(JSON.parse(stored));
      } catch {}
    }
  }

  get user(): Patient | null {
    return this.userSubject.value;
  }

  get isLoggedIn(): boolean {
    return this.userSubject.value !== null;
  }

  get isAdmin(): boolean {
    return this.userSubject.value?.phone === '+201000000000';
  }

  login(patient: Patient, token: string): void {
    this.userSubject.next(patient);
    localStorage.setItem('safeDose_user', JSON.stringify(patient));
    document.cookie = `safedose_jwt=${token}; path=/; max-age=2592000; Secure; SameSite=Strict`;
  }

  logout(): void {
    this.userSubject.next(null);
    localStorage.removeItem('safeDose_user');
    document.cookie = 'safedose_jwt=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT;';
  }

  updateProfile(updates: Partial<Patient>): void {
    const current = this.userSubject.value;
    if (current) {
      const updated = { ...current, ...updates };
      this.userSubject.next(updated);
      localStorage.setItem('safeDose_user', JSON.stringify(updated));
    }
  }
}
