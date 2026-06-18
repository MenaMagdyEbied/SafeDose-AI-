export interface SubscriptionPlan {
  id: 'free' | 'family';
  name: string;
  price: number;
  currency: string;
  features: string[];
  maxFamilyMembers: number;
  maxInteractionChecks: number;
  maxMedications: number;
}
