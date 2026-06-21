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
  editMode = false;
  showDeleteConfirm = false;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) this.loadPrescription(id);
  }

  private async loadPrescription(id: number): Promise<void> {
    this.loading = true;
    try {
      // this.selectedPrescription = await this.prescriptionService.getById(id);
    } catch {
      this.selectedPrescription = null;
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
  }

  deletePrescription(): void {
    this.showDeleteConfirm = true;
  }

  async confirmDelete(): Promise<void> {
    if (!this.selectedPrescription) return;
    try {
      await this.prescriptionService.delete(this.selectedPrescription.id);
    } catch {
      // اختياري: تعرض رسالة خطأ
    }
    this.showDeleteConfirm = false;
    this.goBack();
  }

  goBack(): void {
    this.location.back();
  }
}
