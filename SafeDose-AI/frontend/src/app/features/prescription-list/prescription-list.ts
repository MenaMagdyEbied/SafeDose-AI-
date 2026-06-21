import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CircleCheck, Eye, LucideAngularModule, Trash2, TriangleAlert } from 'lucide-angular';
import { Prescription } from '../../core/services/prescription';
import { PatientService } from '../../core/services/patient';
import { PrescriptionListItem } from '../../core/models/prescription-list';

@Component({
  selector: 'app-prescription-list',
  imports: [LucideAngularModule, RouterLink],
  templateUrl: './prescription-list.html',
  styleUrl: './prescription-list.css',
})
export class PrescriptionList implements OnInit {
  private readonly router = inject(Router);
  private readonly prescriptionService = inject(Prescription);
  private readonly patientService = inject(PatientService);

  checkCircleIcon = CircleCheck;
  trashIcon = Trash2;
  eyeIcon = Eye;
  alertTriangleIcon = TriangleAlert;

  prescriptions = signal<PrescriptionListItem[]>([]);
  loading = signal(false);
  errorText = signal('');

  showDeleteConfirm = signal(false);
  prescriptionToDelete = signal<PrescriptionListItem | null>(null);

  private currentPatientId: number | null = null;

  ngOnInit(): void {
    this.loadPrescriptions();
  }

  private async loadPrescriptions(): Promise<void> {
    this.loading.set(true);
    this.errorText.set('');

    try {
      const patients = await this.patientService.getMyPatients();
      this.currentPatientId = patients[0]?.patientId ?? patients[0]?.id ?? null;

      if (!this.currentPatientId) {
        this.errorText.set('تعذر تحديد المريض الحالي.');
        return;
      }

      const list = await this.prescriptionService.getByPatient(this.currentPatientId);
      this.prescriptions.set(list);
    } catch {
      this.errorText.set('تعذر تحميل الوصفات. حاول مرة أخرى.');
    } finally {
      this.loading.set(false);
    }
  }

  viewPrescription(prescription: PrescriptionListItem): void {
    this.router.navigate(['/prescription-detail', prescription.id]);
  }

  confirmDeletePrescription(prescription: PrescriptionListItem): void {
    this.prescriptionToDelete.set(prescription);
    this.showDeleteConfirm.set(true);
  }

  async deleteConfirmed(): Promise<void> {
    const prescription = this.prescriptionToDelete();
    if (!prescription) return;

    try {
      await this.prescriptionService.delete(prescription.id);
      this.prescriptions.update((list) => list.filter((p) => p.id !== prescription.id));
    } catch {
      this.errorText.set('تعذر حذف الوصفة.');
    } finally {
      this.showDeleteConfirm.set(false);
      this.prescriptionToDelete.set(null);
    }
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.prescriptionToDelete.set(null);
  }
}
