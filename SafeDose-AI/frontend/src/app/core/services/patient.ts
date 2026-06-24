import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Patient } from '../models/patient';
export interface PatientPayload {
  fullName: string;
  dateOfBirth: string;
  gender: number;
  bloodType: string;
  chronicConditions: string[];
  allergies: string[];

  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  doctorName?: string | null;
  relationship?: string | null;
}

@Injectable({ providedIn: 'root' })
export class PatientService {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  readonly patients = signal<Patient[]>([]);
  readonly primaryPatient = signal<Patient | null>(null);

  private ensureInFlight: Promise<Patient | null> | null = null;

  resolvePatientId(patient: Patient | null | undefined): number | null {
    if (!patient) return null;
    return patient.patientId ?? patient.id ?? null;
  }

  pickPrimaryPatient(list: Patient[]): Patient | null {
    if (!list?.length) return null;
    const self = list.find((p) => p.relationship === 'self');
    return self ?? list[0];
  }

  isPrimaryPatient(patient: Patient, list?: Patient[]): boolean {
    const source = list ?? this.patients();
    const primary = this.pickPrimaryPatient(source);
    if (!primary) return false;
    return this.resolvePatientId(primary) === this.resolvePatientId(patient);
  }

  isFamilyMember(patient: Patient): boolean {
    return (
      patient.relationship != null && patient.relationship !== '' && patient.relationship !== 'self'
    );
  }

  async getMyPatients(): Promise<Patient[]> {
    const list = await firstValueFrom(this.http.get<Patient[]>(this.apiUrl + '/patients/my'));
    const sorted = this.sortPatients(list);
    this.patients.set(sorted);
    this.primaryPatient.set(this.pickPrimaryPatient(sorted));
    return sorted;
  }

  async getPrimaryPatientId(): Promise<number | null> {
    const cached = this.resolvePatientId(this.primaryPatient());
    if (cached != null) return cached;

    const list = await this.getMyPatients();
    const fromList = this.resolvePatientId(this.pickPrimaryPatient(list));
    if (fromList != null) return fromList;

    const ensured = await this.ensurePrimaryPatient();
    return this.resolvePatientId(ensured);
  }

  // Called once after login — idempotent, mutex-protected, merges server-side duplicates.
  async ensurePrimaryPatient(fallbackName?: string): Promise<Patient | null> {
    if (this.ensureInFlight) return this.ensureInFlight;

    this.ensureInFlight = this.runEnsurePrimary(fallbackName).finally(() => {
      this.ensureInFlight = null;
    });
    return this.ensureInFlight;
  }

  getPatientById(id: number): Promise<Patient> {
    return firstValueFrom(this.http.get<Patient>(this.apiUrl + '/patients/' + id));
  }

  createPatient(payload: PatientPayload): Promise<Patient> {
    return firstValueFrom(this.http.post<Patient>(this.apiUrl + '/patients', payload));
  }

  updatePatient(id: number, payload: PatientPayload): Promise<Patient> {
    return firstValueFrom(this.http.put<Patient>(this.apiUrl + '/patients/' + id, payload));
  }

  deletePatient(id: number): Promise<void> {
    return firstValueFrom(
      this.http.delete(this.apiUrl + '/patients/' + id, { responseType: 'text' }),
    ).then(() => void 0);
  }

  private async runEnsurePrimary(fallbackName?: string): Promise<Patient | null> {
    const list = await this.getMyPatients();
    const current = this.pickPrimaryPatient(list);
    if (current) {
      try {
        localStorage.removeItem('safedose_pending_patient');
      } catch {
        /* ignore */
      }
      return current;
    }

    try {
      await firstValueFrom(this.http.post<Patient>(this.apiUrl + '/patients/ensure-primary', {}));
      await this.getMyPatients();
      try {
        localStorage.removeItem('safedose_pending_patient');
      } catch {
        /* ignore */
      }
      return this.primaryPatient();
    } catch {
      return this.createPrimaryFromPending(fallbackName);
    }
  }

  private sortPatients(list: Patient[]): Patient[] {
    return [...list].sort((a, b) => {
      const aSelf = a.relationship === 'self' ? 0 : 1;
      const bSelf = b.relationship === 'self' ? 0 : 1;
      if (aSelf !== bSelf) return aSelf - bSelf;
      return (a.createdAt ?? '').localeCompare(b.createdAt ?? '');
    });
  }

  private async createPrimaryFromPending(fallbackName?: string): Promise<Patient | null> {
    let pending: Record<string, unknown> | null = null;
    try {
      const raw = localStorage.getItem('safedose_pending_patient');
      if (raw) pending = JSON.parse(raw);
    } catch {
      /* ignore */
    }

    const age = Number(pending?.['age']);
    let dateOfBirth = '';
    if (Number.isFinite(age) && age > 0 && age < 130) {
      const birthYear = new Date().getFullYear() - Math.floor(age);
      dateOfBirth = birthYear + '-01-01';
    }

    const name =
      (typeof pending?.['fullName'] === 'string' && pending['fullName']) || fallbackName || 'مريض';

    try {
      const created = await this.createPatient({
        fullName: name,
        dateOfBirth,
        gender: 0,
        bloodType: '',
        chronicConditions: Array.isArray(pending?.['chronicConditions'])
          ? (pending['chronicConditions'] as string[])
          : [],
        allergies: [],
        relationship: 'self',
      });
      await this.getMyPatients();
      try {
        localStorage.removeItem('safedose_pending_patient');
      } catch {
        /* ignore */
      }
      return created;
    } catch {
      return null;
    }
  }
}
