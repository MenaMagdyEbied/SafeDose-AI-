export interface Patient {
  id?: number;
  patientId?: number;
  fullName: string;
  dateOfBirth: string;
  gender: number;
  bloodType: string;
  chronicConditions: string[];
  allergies: string[];
  isActive?: boolean;
  createdAt?: string;
}
