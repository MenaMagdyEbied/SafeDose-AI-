import { Location } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideAngularModule, Plus, Save, Trash2, TriangleAlert } from 'lucide-angular';
import { Prescription } from '../../core/services/prescription';

@Component({
  selector: 'app-prescription-detail',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './prescription-detail.html',
  styleUrl: './prescription-detail.css',
})
export class PrescriptionDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly location = inject(Location);
  private readonly prescriptionService = inject(Prescription);

  trashIcon = Trash2;
  alertTriangleIcon = TriangleAlert;
  plusIcon = Plus;
  saveIcon = Save;

  selectedPrescription: any = null;
  editMode = false;
  showDeleteConfirm = false;

  ngOnInit() {
    // const id = Number(this.route.snapshot.paramMap.get('id'));
    // const found = this.prescriptionService.getById(id);
    // if (found) {
    //   this.selectedPrescription = JSON.parse(JSON.stringify(found));
    // }
  }

  get warningsCount(): number {
    return this.selectedPrescription?.meds?.filter((m: any) => m.warning)?.length ?? 0;
  }

  addMed() {
    this.selectedPrescription.meds.push({
      name: '',
      dose: '',
      frequency: '',
      duration: '',
      warning: '',
      chemicalName: '',
      registryCode: '',
    });
  }

  removeMed(index: number) {
    this.selectedPrescription.meds.splice(index, 1);
  }

  save() {
    // this.prescriptionService.update(this.selectedPrescription);
    // this.editMode = false;
  }

  deletePrescription() {
    this.showDeleteConfirm = true;
  }

  confirmDelete() {
    // this.prescriptionService.delete(this.selectedPrescription.id);
    // this.showDeleteConfirm = false;
    // this.goBack();
  }

  goBack() {
    this.location.back();
  }
}
