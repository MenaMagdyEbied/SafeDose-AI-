export interface PrescriptionListItem {
  id: number;
  name: string;
  date: string;
  source: 'scan' | 'manual';
  meds: { name: string }[];
}

export interface PrescriptionDetailMed {
  name: string;
  dose: string;
  frequency: string;
  duration: string;
  chemicalName?: string;
  registryCode?: string;
  warning?: string | null;
}

export interface PrescriptionDetail {
  id: number;
  name: string;
  date: string;
  source: 'scan' | 'manual';
  doctorName?: string | null;
  meds: PrescriptionDetailMed[];
}
