import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CardData } from '../models/card-data';

export type MedicalCardResponse = CardData;
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

interface MedicalCardApiData {
  fullName: string;
  bloodType: string;
  chronicConditions: string;
  allergies: string;
  dateOfBirth: string;
  gender: number;
  medicalCardToken: string;
  currentMedications: any[];
}
@Injectable({
  providedIn: 'root',
})
export class MedicalCardService {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  getPublicCard(token: string): Promise<MedicalCardResponse> {
    return firstValueFrom(
      this.http.get<MedicalCardResponse>(
        `${this.apiUrl}/MedicalCard/Public/${encodeURIComponent(token)}`,
      ),
    );
  }

  getPrivateCard(patientId: string): Promise<CardData> {
    return firstValueFrom(
      this.http.get<ApiResponse<MedicalCardApiData>>(
        `${this.apiUrl}/MedicalCard/Private/${encodeURIComponent(patientId)}`,
      ),
    ).then((res) => this.mapToCardData(res.data, patientId));
  }

  private mapToCardData(data: MedicalCardApiData, id: string): CardData {
    const birth = new Date(data.dateOfBirth);
    const age = new Date().getFullYear() - birth.getFullYear();

    return {
      id,
      name: data.fullName,
      age,
      medications: data.currentMedications.map((m) => ({
        name: m.name ?? m.drugName ?? '',
        dose: m.dose ?? '',
        frequency: m.frequency ?? '',
        startDate: m.startDate ?? '',
      })),
      allergies: data.allergies ? [data.allergies] : [],
      doctorName: '',
      qrUrl: data.medicalCardToken,
    };
  }

  async getPrivateQrCode(patientId: string): Promise<string> {
    const blob = await firstValueFrom(
      this.http.get(`${this.apiUrl}/MedicalCard/Private/${encodeURIComponent(patientId)}/qrcode`, {
        responseType: 'blob',
      }),
    );

    return await new Promise<string>((resolve, reject) => {
      const reader = new FileReader();
      reader.onloadend = () => resolve(reader.result as string);
      reader.onerror = () => reject(new Error('Failed to read QR code response'));
      reader.readAsDataURL(blob);
    });
  }

  async downloadPrivatePdf(patientId: string): Promise<void> {
    const blob = await firstValueFrom(
      this.http.get(`${this.apiUrl}/MedicalCard/Private/${encodeURIComponent(patientId)}/pdf`, {
        headers: new HttpHeaders({ 'X-Skip-Loader': 'true' }),
        responseType: 'blob',
      }),
    );

    const url = window.URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `medical-card-${patientId}.pdf`;
    anchor.click();
    window.setTimeout(() => window.URL.revokeObjectURL(url), 0);
  }
}
