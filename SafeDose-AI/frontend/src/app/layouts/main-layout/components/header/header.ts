import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import {
  Bell,
  ChevronDown,
  ChevronLeft,
  CircleUser,
  CreditCard,
  Heart,
  LogIn,
  LogOut,
  LucideAngularModule,
  Pill,
  ShieldAlert,
  TriangleAlert,
  User,
  UserCheck,
  UserPlus,
  Users,
} from 'lucide-angular';
import { Auth } from '../../../../core/auth/services/auth';
import { Subscription } from '../../../../core/services/subscription';
import { Medications } from '../../../../core/services/medications';
import { PatientService } from '../../../../core/services/patient';

@Component({
  selector: 'app-header',
  imports: [LucideAngularModule, RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  private readonly router = inject(Router);
  protected readonly authService = inject(Auth);
  protected readonly subscriptionService = inject(Subscription);
  private readonly medicationsService = inject(Medications);
  private readonly patientService = inject(PatientService);

  showLogoutConfirm = false;
  accountMenu = false;
  bellMenu = false;
  bellTab: 'meds' | 'family' = 'meds';
  userName = '';

  // Icons
  heartIcon = Heart;
  logOutIcon = LogOut;
  logInIcon = LogIn;
  userPlusIcon = UserPlus;
  chevronDownIcon = ChevronDown;
  chevronLeftIcon = ChevronLeft;
  userCheckIcon = UserCheck;
  bellIcon = Bell;
  circleUserIcon = CircleUser;
  userIcon = User;
  usersIcon = Users;
  shieldAlertIcon = ShieldAlert;
  pillIcon = Pill;
  digitalCardIcon = CreditCard;
  alertIcon = TriangleAlert;

  // Bell dropdown content. Built from the user's actual active medications +
  // their reminder times — same source the /notifications page uses. No mock.
  medNotifications: any[] = [];

  // Family-side notifications: backend has no endpoint yet. Empty by default
  // so the count stays 0 instead of inventing fake entries.
  familyNotifications: any[] = [];

  get unreadMeds(): number {
    return this.medNotifications.filter((n: any) => !n.read).length;
  }
  get unreadFamily(): number {
    return this.familyNotifications.filter((n: any) => !n.read).length;
  }
  get unreadCount(): number {
    return this.unreadMeds + this.unreadFamily;
  }

  takeDose(notif: any): void {
    notif.status = 'taken';
    notif.read = true;
  }

  snooze(notif: any): void {
    notif.status = 'snoozed';
    notif.read = true;
  }

  markMedRead(notif: any): void {
    notif.read = true;
  }
  markFamilyRead(notif: any): void {
    notif.read = true;
  }

  logout(): void {
    this.showLogoutConfirm = false;
    this.authService.logout();
    this.subscriptionService.clear();
    this.medNotifications = [];
    this.familyNotifications = [];
    this.router.navigate(['/home']);
  }

  ngOnInit(): void {
    this.authService.user$.subscribe((user) => {
      this.userName = user?.name || user?.userName || '';
      if (user) {
        this.subscriptionService.refresh();
        this.loadBellNotifications();
      } else {
        this.subscriptionService.clear();
        this.medNotifications = [];
        this.familyNotifications = [];
      }
    });
  }

  private async loadBellNotifications(): Promise<void> {
    try {
      const patientId = await this.patientService.getPrimaryPatientId();
      if (!patientId) {
        this.medNotifications = [];
        return;
      }

      const meds = (await this.medicationsService.getByPatient(patientId)) || [];
      const nowMin = this.minutesNow();
      const out: any[] = [];
      let id = 1;

      // Only show today's upcoming + most recently passed (the bell is a peek view).
      for (const m of meds as any[]) {
        const drugName: string = m.drugName || m.name || 'دواء';
        const dose: string = m.dose || m.drugDose || '';
        const meal: string = m.mealTimingArabic || '';
        const times: string[] =
          Array.isArray(m.times) && m.times.length > 0 ? m.times : this.fallbackTimes(m.frequency);

        for (const t of times) {
          const tMin = this.parseMin(t);
          if (tMin == null) continue;
          const status = tMin <= nowMin ? 'taken' : 'pending';
          out.push({
            id: id++,
            type: 'reminder',
            status,
            title: `حان وقت ${drugName}`,
            body: `${dose}${meal ? ' — ' + meal : ''}`,
            time: status === 'pending' ? `الساعة ${this.fmt(tMin)}` : `الساعة ${this.fmt(tMin)}`,
            read: status === 'taken',
            sortKey: tMin,
          });
        }
      }

      // Most relevant first: next pending dose, then earlier same-day taken.
      out.sort((a, b) => {
        if (a.status !== b.status) return a.status === 'pending' ? -1 : 1;
        if (a.status === 'pending') return a.sortKey - b.sortKey;
        return b.sortKey - a.sortKey;
      });

      // Bell is a quick peek — cap at 5. The page shows the full list.
      this.medNotifications = out.slice(0, 5);
    } catch {
      this.medNotifications = [];
    }
  }

  private minutesNow(): number {
    const d = new Date();
    return d.getHours() * 60 + d.getMinutes();
  }

  private parseMin(t: string): number | null {
    const m = /^(\d{1,2}):(\d{2})/.exec(t || '');
    if (!m) return null;
    return parseInt(m[1], 10) * 60 + parseInt(m[2], 10);
  }

  private fmt(mins: number): string {
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    const ampm = h >= 12 ? 'م' : 'ص';
    const h12 = h === 0 ? 12 : h > 12 ? h - 12 : h;
    return `${h12}:${m.toString().padStart(2, '0')} ${ampm}`;
  }

  private fallbackTimes(freq: number | null | undefined): string[] {
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

  async changePatient(patientId: number): Promise<void> {
    await this.patientService.setRunningPatient(patientId);
  }
}
