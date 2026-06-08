import { FamilyMember } from './family-member';

export interface Patient {
  phone: string;
  name: string;
  age: number;
  conditions: string[];
  allergies: string;
  doctorName?: string;
  subscriptionPlan: 'free' | 'family';
  subscriptionExpiry?: string;
  createdAt?: string;
  familyMembers?: FamilyMember[];
}
