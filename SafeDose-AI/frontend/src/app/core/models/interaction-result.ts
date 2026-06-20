export interface AnalyzedDrug {
  drugId: number;
  arabicName: string | null;
  englishName: string | null;
  dosageNote: string | null;
  role: string | null;
}

export interface ConflictingPair {
  drugA: string;
  drugB: string;
  reasonArabic: string;
  severity: 'low' | 'moderate' | 'high' | string;
}

export interface InteractionResult {
  interactionCheckId: number;
  level: number;
  labelArabic: string;
  color: string;
  titleArabic: string;
  explanationArabic: string;
  recommendedActionArabic: string;
  analyzedDrugs: AnalyzedDrug[];
  conflictingPairs: ConflictingPair[];
  sources: string[];
  safetyDisclaimerArabic: string;
  checkedAt: string;
}

export interface CheckInteractionsPayload {
  drugCatalogIds: number[];
  patientId: number;
}
