import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PatientMedication, CreateMedicationPayload } from '../models/patient-medication';
@Injectable({
  providedIn: 'root',
})
export class Medications {private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  readonly medications = signal<PatientMedication[]>([]);

  async getByPatient(patientId: number): Promise<PatientMedication[]> {
    const list = await firstValueFrom(
      this.http.get<PatientMedication[]>(`${this.apiUrl}/medications/patient/${patientId}`),
    );
    this.medications.set(list);
    return list;
  }

  getHistory(patientId: number): Promise<PatientMedication[]> {
    return firstValueFrom(
      this.http.get<PatientMedication[]>(
        `${this.apiUrl}/medications/patient/${patientId}/history`,
      ),
    );
  }

  getById(id: number): Promise<PatientMedication> {
    return firstValueFrom(this.http.get<PatientMedication>(`${this.apiUrl}/medications/${id}`));
  }

  create(payload: CreateMedicationPayload): Promise<PatientMedication> {
    return firstValueFrom(
      this.http.post<PatientMedication>(`${this.apiUrl}/medications`, payload),
    );
  }

  createFromPrescription(payload: Record<string, unknown>): Promise<PatientMedication> {
    return firstValueFrom(
      this.http.post<PatientMedication>(`${this.apiUrl}/medications/from-prescription`, payload),
    );
  }

  update(id: number, payload: Partial<CreateMedicationPayload>): Promise<PatientMedication> {
    return firstValueFrom(
      this.http.put<PatientMedication>(`${this.apiUrl}/medications/${id}`, payload),
    );
  }

  pause(id: number): Promise<unknown> {
    return firstValueFrom(this.http.post(`${this.apiUrl}/medications/${id}/pause`, {}));
  }

  resume(id: number): Promise<unknown> {
    return firstValueFrom(this.http.post(`${this.apiUrl}/medications/${id}/resume`, {}));
  }

  stop(id: number): Promise<unknown> {
    return firstValueFrom(this.http.post(`${this.apiUrl}/medications/${id}/stop`, {}));
  }
}
