
export interface PrescriptionListItem {
  id: number;
  name: string;
  date: string;
  drugCount: number;
  drugNames: string[];
}

export interface PrescriptionDetailMed {
  name: string;
  dose: string;
  frequency: string;
  duration: string;
  chemicalName?: string;
  registryCode?: string;
  warning?: string;
}

export interface PrescriptionDetail {
  id: number;
  name: string;
  date: string;
  drugCount: number;
  doctorName?: string | null;
  source?: 'scan' | 'manual';
  meds: PrescriptionDetailMed[];
}
