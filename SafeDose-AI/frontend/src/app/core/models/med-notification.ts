export interface MedNotification {
  id: number;
  type: 'reminder' | 'warning';
  status: 'pending' | 'taken' | 'snoozed' | 'skipped';
  title: string;
  body: string;
  time: string;
  read: boolean;
}
