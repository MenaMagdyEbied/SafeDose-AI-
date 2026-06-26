export interface PricingTier {
  pricingTierId: number;
  tierCode: string;
  tierName: string;
  price: number;
  currency: string;
  patientLimit: number;
  priceLabelArabic: string;
  features?: string[];
  nameAr?: string;
  nameEn?: string;
  tierNameArabic?: string;
  monthlyPrice?: number;
  interactionCheckLimitPerDay?: number;
  medicationLimitPerPatient?: number;
  billingCycleDays?: number;
  isActive?: boolean;
}
