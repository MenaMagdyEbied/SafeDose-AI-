export interface ParsedDrug {
  drugName: string;
  dose: string | null;
  doctorName: string | null;
  route: number | null;
  frequency: number | null;
  startDate: string | null;
  endDate: string | null;
  mealTiming: number | null;
}

export interface ParsePrescriptionResponse {
  prescriptionId?: number;
  imageUrl?: string;
  drugs: ParsedDrug[];
}

export interface SaveDrugPayload {
  drugName: string;
  dose: string | null;
  doctorName: string | null;
  route: number | null;
  frequency: number | null;
  startDate: string | null;
  endDate: string | null;
  mealTiming: number | null;
}

export interface SavePrescriptionPayload {
  patientId: number;
  prescriptionName: string;
  imageUrl: string;
  drugs: SaveDrugPayload[];
}
