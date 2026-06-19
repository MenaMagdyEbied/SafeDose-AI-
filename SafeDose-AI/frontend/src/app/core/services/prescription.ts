import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ParsePrescriptionResponse, SavePrescriptionPayload } from '../models/prescription-api';

@Injectable({
  providedIn: 'root',
})
export class Prescription {
  private readonly baseUrl = environment.apiUrl + '/Prescriptions';
  private readonly http = inject(HttpClient);

  // POST /api/Prescriptions/parse
  parse(file: File): Promise<ParsePrescriptionResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return firstValueFrom(
      this.http.post<ParsePrescriptionResponse>(`${this.baseUrl}/parse`, formData),
    );
  }

  // POST /api/Prescriptions/save
  save(payload: SavePrescriptionPayload): Promise<{ prescriptionId: number }> {
    return firstValueFrom(
      this.http.post<{ prescriptionId: number }>(`${this.baseUrl}/save`, payload),
    );
  }
}
