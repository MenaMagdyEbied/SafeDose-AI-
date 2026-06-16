import { Medication } from './medication';

export interface CardData {
  id: string;
  name: string;
  age: number;
  medications: Medication[];
  allergies: string[];
  doctorName: string;
  qrUrl?: string;
}
