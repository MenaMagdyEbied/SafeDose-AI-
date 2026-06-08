import { CheckedMed } from './checked-med';

export interface InteractionResult {
  status: 'high' | 'medium' | 'low';
  title: string;
  severityText: string;
  explanation: string;
  checkedMeds: CheckedMed[];
  source: string;
}
