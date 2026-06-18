export interface PricingTier {
  pricingTierId: number;
  tierCode: string;
  tierName: string;
  price: number;
  currency: string;
  patientLimit: number;
  priceLabelArabic: string;
}
