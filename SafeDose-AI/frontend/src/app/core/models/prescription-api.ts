
export interface ParsedMedication {
  drug_name_guess: string;
  dose_guess: string | null;
  frequency_guess: string | null;
  duration_guess: string | null;
  needsReview: boolean;
}

export interface ParsePrescriptionResponse {
  doctor_name: string | null;
  image_url: string | null;
  medications: ParsedMedication[];
}

export enum DrugRoute {
  Oral = 0,
}

export enum DrugFrequency {
  Daily = 1,
}

export enum MealTiming {
  None = 0,
  // ضبطيها حسب enum الباك إند الفعلي
}

export interface SavePrescriptionDrug {
  drugName: string;
  dose: string;
  doctorName: string;
  route: number;
  frequency: number;
  startDate: string;
  endDate: string;
  mealTiming: number;
}

export interface SavePrescriptionPayload {
  patientId: number;
  prescriptionName: string;
  imageUrl: string;
  drugs: SavePrescriptionDrug[];
}

export interface SavePrescriptionResult {
  prescriptionId: number;
}
