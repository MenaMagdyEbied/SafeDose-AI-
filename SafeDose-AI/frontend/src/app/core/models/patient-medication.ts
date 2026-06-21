export interface PatientMedication {
  patientMedicationId: number;
  patientId: number;
  drugId: number;
  drugName: string;
  drugDose: string;
  prescriptionId: number | null;
  dose: string;
  frequency: number;
  startDate: string;
  endDate: string;
  mealTiming: number;
  status: number;
  statusArabic: string;
  mealTimingArabic: string;
  isVerified: boolean;
  verificationLabelArabic: string;
  drugCatalogId: number;
  times: string[];
}

export interface CreateMedicationPayload {
  patientId: number;
  drugName: string;
  drugCatalogId: number;
  route: number;
  dose: string;
  doctorName: string;
  frequency: number;
  startDate: string;
  endDate: string;
  mealTiming: number;
  prescriptionId: number | null;
  times: string[];
}
