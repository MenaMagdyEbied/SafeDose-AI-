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