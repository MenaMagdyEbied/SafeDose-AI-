import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AddFromPrescriptionPayload,
  AddMedicationPayload,
  MedicationHistoryResponse,
  MedicationResponse,
  UpdateMedicationPayload,
} from '../models';
import { PatientMedication } from '../models/patient-medication';
@Injectable({
  providedIn: 'root',
})
export class Medications {
  private readonly baseUrl = environment.apiUrl + '/medications';
  private readonly http = inject(HttpClient);

  readonly medications = signal<PatientMedication[]>([]);

  // Active medications for the currently loaded patient (used by interaction-checker's
  // loadFromProfile() and the manage page list).
  readonly currentPatientMeds = signal<MedicationResponse[]>([]);

  // POST /api/medications
  addManually(payload: AddMedicationPayload): Promise<MedicationResponse> {
    return firstValueFrom(this.http.post<MedicationResponse>(this.baseUrl, payload));
  }

  // POST /api/medications/from-prescription
  addFromPrescription(payload: AddFromPrescriptionPayload): Promise<{ insertedCount: number }> {
    return firstValueFrom(
      this.http.post<{ insertedCount: number }>(`${this.baseUrl}/from-prescription`, payload),
    );
  }

  // GET /api/medications/patient/{patientId}  (active list)
  getByPatient(patientId: number): Promise<MedicationResponse[]> {
    return firstValueFrom(
      this.http.get<MedicationResponse[]>(`${this.baseUrl}/patient/${patientId}`),
    );
  }

  // GET /api/medications/patient/{patientId}/history (grouped active/paused/stopped)
  getHistory(patientId: number): Promise<MedicationHistoryResponse> {
    return firstValueFrom(
      this.http.get<MedicationHistoryResponse>(`${this.baseUrl}/patient/${patientId}/history`),
    );
  }

  // GET /api/medications/{id}
  getById(id: number): Promise<MedicationResponse> {
    return firstValueFrom(this.http.get<MedicationResponse>(`${this.baseUrl}/${id}`));
  }

  // PUT /api/medications/{id}
  // Note (from backend): cannot update a medication whose status is Stopped (3).
  update(id: number, payload: UpdateMedicationPayload): Promise<MedicationResponse> {
    return firstValueFrom(this.http.put<MedicationResponse>(`${this.baseUrl}/${id}`, payload));
  }

  // POST /api/medications/{id}/pause   (Active -> Paused only)
  pause(id: number): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${this.baseUrl}/${id}/pause`, {}));
  }

  // POST /api/medications/{id}/resume  (Paused -> Active only)
  resume(id: number): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${this.baseUrl}/${id}/resume`, {}));
  }

  // POST /api/medications/{id}/stop    (Active or Paused -> Stopped, one-way / final)
  stop(id: number): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${this.baseUrl}/${id}/stop`, {}));
  }
}
