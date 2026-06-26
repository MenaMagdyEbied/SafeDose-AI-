export interface Medication {
  name: string;
  dose: string;
  frequency: string;
  startDate: string;
}

export interface HealthProfile {
  id: string;
  fullName: string;
  phone: string;
  email: string;
  age: number | null;
  gender: string;
  bloodType: string;
  weight: number | null;
  height: number | null;
  conditions: string[];
  allergies: string;
  emergency: string;
  emergencyName: string;
  doctor: string;
  subscriptionPlan: 'free' | 'pro' | 'family';
  joinDate: string;
  medications: Medication[];
  lastCheckup: string;
}

export type MedicationStatus = 1 | 2 | 3;

export const MEDICATION_STATUS = {
  Active: 1 as MedicationStatus,
  Paused: 2 as MedicationStatus,
  Stopped: 3 as MedicationStatus,
};

export type MealTiming = 1 | 2 | 3 | 4;

export const MEAL_TIMING_OPTIONS: { value: MealTiming; label: string }[] = [
  { value: 1, label: 'قبل الأكل' },
  { value: 2, label: 'مع الأكل' },
  { value: 3, label: 'بعد الأكل' },
  { value: 4, label: 'قبل النوم' },
];

export const MAX_FREQUENCY = 3;
export const MAX_REMINDER_TIMES = 3;

export interface AddMedicationPayload {
  patientId: number;
  drugName: string;
  drugCatalogId?: number | null;
  route?: number | null;
  dose?: string | null;
  doctorName?: string | null;
  frequency?: number | null;
  startDate?: string | null; // 'YYYY-MM-DD'
  endDate?: string | null; // 'YYYY-MM-DD'
  mealTiming?: MealTiming | null;
  prescriptionId?: number | null;
  times?: string[] | null; // 'HH:mm' — length MUST equal frequency
}

export interface UpdateMedicationPayload {
  drugName?: string | null;
  dose?: string | null;
  frequency?: number | null;
  startDate?: string | null;
  endDate?: string | null;
  mealTiming?: MealTiming | null;
  times?: string[] | null; // pass [] to clear, omit/null to leave untouched
}

export interface FromPrescriptionMedicationItem {
  drugId: number;
  dose?: string | null;
  frequency?: number | null;
  startDate?: string | null;
  endDate?: string | null;
  mealTiming?: MealTiming | null;
}

export interface AddFromPrescriptionPayload {
  patientId: number;
  prescriptionId: number;
  medications: FromPrescriptionMedicationItem[];
}

// ===== Response (mirrors MedicationResponseDto) =====

export interface MedicationResponse {
  patientMedicationId: number;
  patientId: number;
  drugId: number;
  drugName: string;
  drugDose: string | null;
  prescriptionId: number | null;
  dose: string | null;
  frequency: number | null;
  startDate: string | null;
  endDate: string | null;
  mealTiming: MealTiming | null;
  status: MedicationStatus;
  statusArabic: string;
  mealTimingArabic: string | null;
  isVerified: boolean;
  verificationLabelArabic: string | null;
  drugCatalogId: number | null;
  times: string[];
}

export interface MedicationHistoryResponse {
  active: MedicationResponse[];
  paused: MedicationResponse[];
  stopped: MedicationResponse[];
}
