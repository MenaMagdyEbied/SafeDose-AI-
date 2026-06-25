import { Component, OnInit, inject, signal } from '@angular/core';
import {
  Check,
  Clock,
  LucideAngularModule,
  Pill,
  Trash2,
  TriangleAlert,
  Users,
  X,
} from 'lucide-angular';
import { MedNotification } from '../../core/models/med-notification';
import { FamilyNotification } from '../../core/models/family-notification';
import { ConfirmDialog } from '../../shared/components/confirm-dialog/confirm-dialog';
import { Medications } from '../../core/services/medications';
import { PatientService } from '../../core/services/patient';

@Component({
  selector: 'app-notifications',
  imports: [LucideAngularModule, ConfirmDialog],
  templateUrl: './notifications.html',
  styleUrl: './notifications.css',
})
export class Notifications implements OnInit {
  private readonly medicationsService = inject(Medications);
  private readonly patientService = inject(PatientService);

  pillIcon = Pill;
  usersIcon = Users;
  alertIcon = TriangleAlert;
  checkIcon = Check;
  clockIcon = Clock;
  xIcon = X;
  trashIcon = Trash2;

  showDeleteDialog = false;
  pendingDeleteNotif: any = null;
  pendingDeleteType: 'med' | 'family' = 'med';
  activeTab: 'meds' | 'family' = 'meds';

  loading = signal(false);

  // Built from the patient's real active medications + their reminder times.
  // No hardcoded ميتفورمين/أملوديبين — every entry maps to a real Drug row.
  medNotifications: MedNotification[] = [];

  // Family notifications: real backend feed doesn't exist yet. Empty by default —
  // appears only when the family-plan grows. Kept here so the tab still renders.
  familyNotifications: FamilyNotification[] = [];

  async ngOnInit(): Promise<void> {
    await this.loadFromMedications();
  }

  private async loadFromMedications(): Promise<void> {
    this.loading.set(true);
    try {
      const patientId = await this.patientService.getPrimaryPatientId();
      if (!patientId) {
        this.medNotifications = [];
        return;
      }

      const meds = (await this.medicationsService.getByPatient(patientId)) || [];
      const out: MedNotification[] = [];
      const nowMin = this.minutesNow();
      let counter = 1;

      for (const m of meds as any[]) {
        const drugName: string = m.drugName || m.name || 'دواء';
        const dose: string = m.dose || m.drugDose || '';
        const mealTiming: string = m.mealTimingArabic || this.mealTimingLabel(m.mealTiming);
        const times: string[] =
          Array.isArray(m.times) && m.times.length > 0
            ? m.times
            : this.fallbackTimesForFrequency(m.frequency);

        for (const t of times) {
          const tMin = this.parseTimeMin(t);
          const status: MedNotification['status'] =
            tMin == null ? 'pending' : tMin <= nowMin ? 'taken' : 'pending';
          const time =
            tMin == null
              ? 'لاحقاً'
              : status === 'taken'
                ? `الساعة ${this.formatTime(tMin)}`
                : `الساعة ${this.formatTime(tMin)}`;

          out.push({
            id: counter++,
            type: 'reminder',
            status,
            title: `حان وقت ${drugName}`,
            body: `${dose}${mealTiming ? ' — ' + mealTiming : ''}`,
            time,
            read: status === 'taken',
          });
        }
      }

      // Soonest pending first, then today's taken doses.
      this.medNotifications = out.sort((a, b) => {
        if (a.status !== b.status) return a.status === 'pending' ? -1 : 1;
        return a.id - b.id;
      });
    } catch {
      this.medNotifications = [];
    } finally {
      this.loading.set(false);
    }
  }

  get unreadMeds() {
    return this.medNotifications.filter((n) => !n.read).length;
  }

  get unreadFamily() {
    return this.familyNotifications.filter((n) => !n.read).length;
  }

  takeDose(notif: MedNotification) {
    notif.status = 'taken';
    notif.read = true;
  }

  snooze(notif: MedNotification) {
    notif.status = 'snoozed';
    notif.read = true;
  }

  skipDose(notif: MedNotification) {
    notif.status = 'skipped';
    notif.read = true;
  }

  markAllRead() {
    this.medNotifications.forEach((n) => (n.read = true));
    this.familyNotifications.forEach((n) => (n.read = true));
  }

  markMedRead(notif: any): void {
    notif.read = true;
  }
  markRead(notif: any): void {
    notif.read = true;
  }

  deleteMedNotification(notif: any): void {
    this.medNotifications = this.medNotifications.filter((n) => n.id !== notif.id);
  }

  deleteFamilyNotification(notif: any): void {
    this.familyNotifications = this.familyNotifications.filter((n) => n.id !== notif.id);
  }

  confirmDelete(notif: any, type: 'med' | 'family', event: Event): void {
    event.stopPropagation();
    this.pendingDeleteNotif = notif;
    this.pendingDeleteType = type;
    this.showDeleteDialog = true;
  }

  executeDelete(): void {
    if (!this.pendingDeleteNotif) return;
    if (this.pendingDeleteType === 'med') {
      this.medNotifications = this.medNotifications.filter(
        (n) => n.id !== this.pendingDeleteNotif.id,
      );
    } else {
      this.familyNotifications = this.familyNotifications.filter(
        (n) => n.id !== this.pendingDeleteNotif.id,
      );
    }
    this.showDeleteDialog = false;
    this.pendingDeleteNotif = null;
  }

  cancelDelete(): void {
    this.showDeleteDialog = false;
    this.pendingDeleteNotif = null;
  }

  // ─── helpers ─────────────────────────────────────────────────────────────

  private minutesNow(): number {
    const d = new Date();
    return d.getHours() * 60 + d.getMinutes();
  }

  // Backend `times` can be "08:00", "08:00:00", or "HH:mm". Returns minutes since midnight.
  private parseTimeMin(t: string): number | null {
    if (!t) return null;
    const m = /^(\d{1,2}):(\d{2})/.exec(t);
    if (!m) return null;
    const h = parseInt(m[1], 10);
    const mm = parseInt(m[2], 10);
    if (!Number.isFinite(h) || !Number.isFinite(mm)) return null;
    return h * 60 + mm;
  }

  private formatTime(mins: number): string {
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    const ampm = h >= 12 ? 'م' : 'ص';
    const h12 = h === 0 ? 12 : h > 12 ? h - 12 : h;
    return `${h12}:${m.toString().padStart(2, '0')} ${ampm}`;
  }

  private mealTimingLabel(code: number | null | undefined): string {
    if (code === 1) return 'قبل الأكل';
    if (code === 2) return 'مع الأكل';
    if (code === 3) return 'بعد الأكل';
    if (code === 4) return 'قبل النوم';
    return '';
  }

  // If a med doesn't have explicit times saved, distribute frequency evenly across the day.
  private fallbackTimesForFrequency(freq: number | null | undefined): string[] {
    if (!freq || freq < 1) return [];
    if (freq === 1) return ['09:00'];
    if (freq === 2) return ['09:00', '21:00'];
    if (freq === 3) return ['09:00', '15:00', '21:00'];
    if (freq === 4) return ['08:00', '12:00', '16:00', '20:00'];
    return Array.from({ length: freq }, (_, i) => {
      const slot = Math.round((24 / freq) * i + 8) % 24;
      return `${slot.toString().padStart(2, '0')}:00`;
    });
  }
}
