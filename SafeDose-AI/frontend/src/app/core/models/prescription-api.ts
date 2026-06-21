
export interface PineconeCandidate {
  score: number;
  name: string;
  scientific_name: string;
  drug_class: string;
}

export type VerificationStatus = 'EXACT_MATCH' | 'POSSIBLE_MATCH' | 'NO_MATCH';
export type VerificationNextAction =
  | 'AUTO_SAVE'
  | 'SHOW_CONFIRMATION_DIALOG'
  | 'REQUEST_MANUAL_ENTRY';

export interface MedVerification {
  status: VerificationStatus;
  patient_confirmation_required: boolean;
  verified_scientific_name?: string;
  match_reason?: string;
  candidates_to_show_patient?: string[];
  ui_message_arabic?: string;
  next_action: VerificationNextAction;
}

export interface ParsedMedication {
  raw_text?: string;
  drug_name_guess: string;
  dose_guess: string | null;
  concentration_guess?: string | null;
  dosage_form_guess?: string | null;
  frequency_guess: string | null;
  duration_guess: string | null;
  confidence?: 'high' | 'medium' | 'low';
  pinecone_candidates?: PineconeCandidate[];
  verification?: MedVerification; // اختياري لأن الشكل المبسط مفيهوش
  needsReview?: boolean;
}

export interface ParsePrescriptionResponse {
  doctor_name: string | null;
  doctor_specialty?: string | null;
  clinic_name?: string | null;
  patient_name?: string | null;
  issue_date?: string | null;
  image_url?: string | null; // ظهر في الشكل التاني
  ocr_text?: string;
  medications: ParsedMedication[];
  extraction_quality?: string;
  warnings?: string[];
  overall_next_action?: string;
}

export interface SaveDrugDto {
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
  drugs: SaveDrugDto[];
}

export interface SavePrescriptionResult {
  prescriptionId: number;
}
