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

  // Extended profile fields. All optional.
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  doctorName?: string | null;
  // self | son | daughter | father | mother | brother | sister | husband | wife | other
  relationship?: string | null;
}
