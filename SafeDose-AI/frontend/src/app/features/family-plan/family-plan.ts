import { Component, DestroyRef, computed, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule, Plus, SquarePen, Trash2, UserCheck, Users, X } from 'lucide-angular';
import { EMPTY, from } from 'rxjs';
import { catchError, finalize, switchMap } from 'rxjs/operators';
import { Patient } from '../../core/models/patient';
import { PatientService } from '../../core/services/patient';
import { ConfirmDialog } from '../../shared/components/confirm-dialog/confirm-dialog';
import { Info } from 'lucide-angular';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-family-plan',
  imports: [ReactiveFormsModule, LucideAngularModule, ConfirmDialog, RouterLink],
  templateUrl: './family-plan.html',
  styleUrl: './family-plan.css',
})
export class FamilyPlan implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly patientService = inject(PatientService);
  private readonly destroyRef = inject(DestroyRef);

  readonly isEditing = computed(() => this.editingId() !== null && this.editingId() !== undefined);
  readonly modalTitle = computed(() =>
    this.isEditing() ? 'تعديل بيانات المريض' : 'إضافة مريض جديد',
  );
  readonly saveButtonText = computed(() => (this.isEditing() ? 'حفظ التعديلات' : 'إضافة المريض'));
  plusIcon = Plus;
  usersIcon = Users;
  editIcon = SquarePen;
  trashIcon = Trash2;
  xIcon = X;
  infoIcon = Info;
  activateIcon = UserCheck;

  members = signal<Patient[]>([]);
  loading = signal(false);
  showModal = signal(false);
  showConfirmDelete = signal(false);
  editingId = signal<number | null>(null);

  pendingDeleteMemberId: number | null = null;
  pendingDeleteMemberName = '';
  error = '';

  readonly genderOptions = [
    { value: 0, label: 'اختر النوع' },
    { value: 2, label: 'ذكر' },
    { value: 1, label: 'أنثى' },
    { value: 3, label: 'أخرى' },
  ];

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    dateOfBirth: ['', Validators.required],
    gender: [0, Validators.required],
    bloodType: [''],
    chronicConditions: [''],
    allergies: [''],
  });

  ngOnInit(): void {
    this.loadPatients();
  }

  loadPatients(): void {
    this.loading.set(true);
    this.error = '';

    from(this.patientService.getMyPatients())
      .pipe(
        catchError(() => {
          this.members.set([]);
          this.error = 'تعذر تحميل بيانات أفراد العائلة.';
          return EMPTY;
        }),
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((response) => {
        this.members.set(Array.isArray(response) ? response : []);
      });
  }

  saveMember(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const payload = {
      fullName: this.form.value.fullName?.trim() ?? '',
      dateOfBirth: this.form.value.dateOfBirth ?? '',
      gender: Number(this.form.value.gender ?? 1),
      bloodType: this.form.value.bloodType?.trim() ?? '',
      chronicConditions: this.parseList(this.form.value.chronicConditions ?? ''),
      allergies: this.parseList(this.form.value.allergies ?? ''),
    };

    const currentEditingId = this.editingId();
    const save$ =
      currentEditingId !== null
        ? from(this.patientService.updatePatient(currentEditingId, payload))
        : from(this.patientService.createPatient(payload));

    save$
      .pipe(
        catchError(() => {
          this.error =
            currentEditingId !== null ? 'تعذر تحديث بيانات المريض.' : 'تعذر إضافة المريض.';
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.closeModal();
        this.loadPatients();
      });
  }

  activateMember(member: Patient): void {
    const memberId = this.patientService.resolvePatientId(member);
    if (memberId == null) {
      return;
    }

    this.error = '';

    from(this.patientService.setRunningPatient(memberId))
      .pipe(
        catchError(() => {
          this.error = 'تعذر تفعيل المريض.';
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  isActiveMember(member: Patient): boolean {
    const memberId = this.patientService.resolvePatientId(member);
    return memberId != null && memberId === this.patientService.currentPatientId;
  }

  deleteMember(id: number): void {
    this.error = '';

    from(this.patientService.deletePatient(id))
      .pipe(
        catchError(() => {
          this.error = 'تعذر حذف المريض.';
          return EMPTY;
        }),
        switchMap(() => from(this.patientService.getMyPatients())),
        catchError(() => {
          this.members.set([]);
          this.error = 'تعذر تحديث بيانات أفراد العائلة.';
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((response) => {
        this.members.set(Array.isArray(response) ? response : []);
      });
  }

  confirmDeleteMember(member: Patient): void {
    const memberId = this.patientService.resolvePatientId(member);
    this.pendingDeleteMemberId = memberId ?? null;
    this.pendingDeleteMemberName = member.fullName;
    this.showConfirmDelete.set(true);
  }

  executeDeleteMember(): void {
    const id =
      this.pendingDeleteMemberId ??
      this.members().find((m) => m.fullName === this.pendingDeleteMemberName)?.id ??
      null;
    if (id === null) {
      return;
    }

    this.cancelDelete();
    this.deleteMember(id);
  }

  cancelDelete(): void {
    this.showConfirmDelete.set(false);
    this.pendingDeleteMemberId = null;
    this.pendingDeleteMemberName = '';
  }

  openAddModal(): void {
    this.editingId.set(null); // ✅ signal
    this.error = '';
    this.form.reset({
      fullName: '',
      dateOfBirth: '',
      gender: 0,
      bloodType: '',
      chronicConditions: '',
      allergies: '',
    });
    this.showModal.set(true);
    document.body.style.overflow = 'hidden';
  }

  editMember(member: Patient): void {
    const memberId = this.patientService.resolvePatientId(member) ?? null;
    this.editingId.set(memberId);
    this.error = '';
    this.form.reset({
      fullName: member.fullName,
      dateOfBirth: member.dateOfBirth,
      gender: member.gender,
      bloodType: member.bloodType,
      chronicConditions: member.chronicConditions.join('\n'),
      allergies: member.allergies.join('\n'),
    });
    this.showModal.set(true);
    document.body.style.overflow = 'hidden';
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingId.set(null); // ✅ signal
    this.error = '';
    this.form.reset({
      fullName: '',
      dateOfBirth: '',
      gender: 0,
      bloodType: '',
      chronicConditions: '',
      allergies: '',
    });
    document.body.style.overflow = '';
  }

  private parseList(value: string): string[] {
    return value
      .split(/\n|,/)
      .map((i) => i.trim())
      .filter((i) => i.length > 0);
  }
}
