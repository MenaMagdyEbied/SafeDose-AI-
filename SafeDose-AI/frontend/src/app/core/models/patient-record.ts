export interface PatientRecord {
  id: string;
  name: string;
  avatar: string;
  status: 'stable' | 'urgent';
  statusText: string;
  adherence: number;
  lastDose: string;
  alertsCount: number;
}
