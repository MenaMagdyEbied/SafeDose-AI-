export interface AnalyzedDrug {
  drugCatalogId?: number;
  name?: string;
  [key: string]: unknown;
}

export interface ConflictingPair {
  [key: string]: unknown;
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
