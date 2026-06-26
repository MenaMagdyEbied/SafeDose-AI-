import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ParsePrescriptionResponse,
  SavePrescriptionPayload,
  SavePrescriptionResult,
} from '../models/prescription-api';
import { PrescriptionDetail, PrescriptionListItem } from '../models/prescription-list';

interface ApiEnvelope<T> {
  success: boolean;
  data: T;
}
@Injectable({
  providedIn: 'root',
})
export class Prescription {
  private readonly baseUrl = environment.apiUrl + '/Prescriptions';
  private readonly http = inject(HttpClient);

  /** POST /api/Prescriptions/parse — يحلل صورة الوصفة بالذكاء الاصطناعي */
  async parse(file: File): Promise<ParsePrescriptionResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return firstValueFrom(
      this.http.post<ParsePrescriptionResponse>(`${this.baseUrl}/parse`, formData),
    );
  }

  /** POST /api/Prescriptions/save */
  async save(payload: SavePrescriptionPayload): Promise<SavePrescriptionResult> {
    return firstValueFrom(this.http.post<SavePrescriptionResult>(`${this.baseUrl}/save`, payload));
  }

  /** GET /api/Prescriptions/Patient/{patientId}/Summary */
  async getByPatient(patientId: number): Promise<PrescriptionListItem[]> {
    const res = await firstValueFrom(
      this.http.get<ApiEnvelope<any[]>>(`${this.baseUrl}/Patient/${patientId}/Summary`, {
        headers: new HttpHeaders({ 'X-Skip-Loader': 'true' }),
      }),
    );
    return res.data.map((item) => ({
      id: item.prescriptionId,
      name: item.prescriptionName,
      date: item.date,
      drugCount: item.drugCount,
      drugNames: item.drugNames ?? [],
    }));
  }

  /** GET /api/Prescriptions/{prescriptionId}/Details */
  async getById(prescriptionId: number): Promise<PrescriptionDetail> {
    const res = await firstValueFrom(
      this.http.get<ApiEnvelope<any>>(`${this.baseUrl}/${prescriptionId}/Details`, {
        headers: new HttpHeaders({ 'X-Skip-Loader': 'true' }),
      }),
    );
    const d = res.data;
    return {
      id: d.prescriptionId,
      name: d.prescriptionName,
      date: d.date,
      drugCount: d.drugCount,
      meds: (d.medications ?? []).map((m: any) => ({
        name: m.drugName,
        dose: m.dose,
        frequency: m.frequency,
        duration: m.duration,
      })),
    };
  }

  /** DELETE /api/Prescriptions/{prescriptionId} */
  async delete(id: number): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.baseUrl}/${id}`, { responseType: 'text' }));
  }
}
