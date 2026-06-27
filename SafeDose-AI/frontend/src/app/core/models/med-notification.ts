export interface MedNotification {
  id: number;
  type: 'reminder' | 'warning';
  status: 'pending' | 'taken' | 'snoozed' | 'skipped' | 'missed';
  title: string;
  body: string;
  time: string;
  read: boolean;
  patientMedicationId?: number;
  drugName?: string;
}
