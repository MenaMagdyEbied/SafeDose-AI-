export interface SubscriptionPlan {
  id: 'free' | 'family';
  nameAr: string;
  nameEn: string;
  price: number;
  currency: string;
  features: string[];
  maxFamilyMembers: number;
  maxInteractionChecks: number;
  maxMedications: number;
}
