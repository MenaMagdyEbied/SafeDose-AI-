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
}

@Injectable({ providedIn: 'root' })
export class PatientService {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  readonly patients = signal<Patient[]>([]);


  readonly primaryPatient = signal<Patient | null>(null);

  async getMyPatients(): Promise<Patient[]> {
    const list = await firstValueFrom(this.http.get<Patient[]>(`${this.apiUrl}/patients/my`));
    this.patients.set(list);
    this.primaryPatient.set(list[0] ?? null);
    return list;
  }

  getPatientById(id: number): Promise<Patient> {
    return firstValueFrom(this.http.get<Patient>(`${this.apiUrl}/patients/${id}`));
  }

  createPatient(payload: PatientPayload): Promise<Patient> {
    return firstValueFrom(this.http.post<Patient>(`${this.apiUrl}/patients`, payload));
  }

  updatePatient(id: number, payload: PatientPayload): Promise<Patient> {
    return firstValueFrom(this.http.put<Patient>(`${this.apiUrl}/patients/${id}`, payload));
  }

  deletePatient(id: number): Promise<void> {
    return firstValueFrom(
      this.http.delete(`${this.apiUrl}/patients/${id}`, { responseType: 'text' }),
    ).then(() => void 0);
  }
}
