import { Location } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideAngularModule, Plus, Save, Trash2, TriangleAlert } from 'lucide-angular';
import { Prescription } from '../../core/services/prescription';
import { PrescriptionDetail as PrescriptionDetailModel } from '../../core/models/prescription-list';

@Component({
  selector: 'app-prescription-detail',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './prescription-detail.html',
  styleUrl: './prescription-detail.css',
})
export class PrescriptionDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly location = inject(Location);
  private readonly prescriptionService = inject(Prescription);

  trashIcon = Trash2;
  alertTriangleIcon = TriangleAlert;
  plusIcon = Plus;
  saveIcon = Save;

  selectedPrescription: PrescriptionDetailModel | null = null;
  loading = false;
  errorText = '';
  editMode = false;
  showDeleteConfirm = false;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadPrescription(id);
    } else {
      this.errorText = 'رقم الوصفة غير صالح.';
    }
  }

  private async loadPrescription(id: number): Promise<void> {
    this.loading = true;
    this.errorText = '';
    try {
      this.selectedPrescription = await this.prescriptionService.getById(id);
    } catch {
      this.selectedPrescription = null;
      this.errorText = 'تعذر تحميل تفاصيل الوصفة. حاول مرة أخرى.';
    } finally {
      this.loading = false;
    }
  }

  get warningsCount(): number {
    return this.selectedPrescription?.meds?.filter((m) => m.warning)?.length ?? 0;
  }

  addMed(): void {
    this.selectedPrescription?.meds.push({ name: '', dose: '', frequency: '', duration: '' });
  }

  removeMed(index: number): void {
    this.selectedPrescription?.meds.splice(index, 1);
  }

  save(): void {
    this.editMode = false;
    // ملاحظة: مفيش endpoint تعديل (PUT/PATCH) ظاهر للـ Prescription لحد دلوقتي،
    // فالتعديل ده شكلي بس على الواجهة ومش بيترفع للباك إند. لو فيه endpoint تحديث
    // هاتيه عشان نوصله صح.
  }

  deletePrescription(): void {
    this.showDeleteConfirm = true;
  }

  async confirmDelete(): Promise<void> {
    if (!this.selectedPrescription) return;
    try {
      await this.prescriptionService.delete(this.selectedPrescription.id);
    } catch {
      this.errorText = 'تعذر حذف الوصفة.';
    }
    this.showDeleteConfirm = false;
    this.goBack();
  }

  goBack(): void {
    this.location.back();
  }
}
