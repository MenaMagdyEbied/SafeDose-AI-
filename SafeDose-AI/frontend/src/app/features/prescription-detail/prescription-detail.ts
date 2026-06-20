import { Location } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideAngularModule, Plus, Save, Trash2, TriangleAlert } from 'lucide-angular';
import { Prescription } from '../../core/services/prescription';
interface PrescriptionMed {
  name: string;
  dose: string;
  frequency: string;
  duration: string;
  chemicalName?: string;
  registryCode?: string;
  warning?: string | null;
}

interface PrescriptionDetailData {
  id: number;
  name: string;
  date: string;
  source: 'scan' | 'manual';
  doctorName?: string | null;
  meds: PrescriptionMed[];
}
@Component({
  selector: 'app-prescription-detail',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './prescription-detail.html',
  styleUrl: './prescription-detail.css',
})
export class PrescriptionDetail implements OnInit {
 private readonly router = inject(Router);
  private readonly location = inject(Location);

  trashIcon = Trash2;
  alertTriangleIcon = TriangleAlert;
  plusIcon = Plus;
  saveIcon = Save;

  selectedPrescription: PrescriptionDetailData | null = null;
  editMode = false;
  showDeleteConfirm = false;

  ngOnInit(): void {
    const state = this.router.getCurrentNavigation()?.extras?.state ?? history.state;

    if (state?.prescription) {
      this.selectedPrescription = state.prescription;
    }
  }

  get warningsCount(): number {
    return this.selectedPrescription?.meds?.filter((m) => m.warning)?.length ?? 0;
  }

  addMed(): void {
    this.selectedPrescription?.meds.push({
      name: '',
      dose: '',
      frequency: '',
      duration: '',
    });
  }

  removeMed(index: number): void {
    this.selectedPrescription?.meds.splice(index, 1);
  }

  save(): void {
    // مفيش endpoint تحديث وصفة حاليًا، نقفل وضع التعديل محليًا فقط
    this.editMode = false;
  }

  deletePrescription(): void {
    this.showDeleteConfirm = true;
  }

  confirmDelete(): void {
    // مفيش endpoint حذف وصفة حاليًا
    this.showDeleteConfirm = false;
    this.goBack();
  }

  goBack(): void {
    this.location.back();
  }
}