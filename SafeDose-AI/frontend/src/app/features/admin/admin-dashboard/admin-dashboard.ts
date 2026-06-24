import { Component, OnInit, inject, signal } from '@angular/core';
import {
  LucideAngularModule,
  Users,
  UserCheck,
  Wallet,
  CreditCard,
  TrendingUp,
  TrendingDown,
  Calendar,
  ShieldCheck,
  UserCog,
  Stethoscope,
  UserPlus,
  FileText,
} from 'lucide-angular';
import {AdminDashboard as AdminDashboardService } from '../services/admin-dashboard';

@Component({
  selector: 'app-admin-dashboard',
  imports: [LucideAngularModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboard {
  private readonly api = inject(AdminDashboardService);

  calendarIcon = Calendar;
  trendUpIcon = TrendingUp;
  trendDownIcon = TrendingDown;
  creditCardIcon = CreditCard;

  revenuePeriod: 'monthly' | 'yearly' = 'monthly';
  loading = signal(true);
  loadError = signal('');

  // KPI cards. Defaults so the layout doesn't jump while loading.
  kpis = signal<any[]>([]);
  revenueData = signal<{ label: string; value: string; percent: number; subPercent: number }[]>([]);
  genderSplit = signal<{ female: number; male: number }>({ female: 0, male: 0 });
  totalUsers = signal('0');
  usersStats = signal<{ free: number; paid: number }>({ free: 0, paid: 0 });
  staffRoles = signal<any[]>([]);
  cardsIssued = signal(0);
  cardsActive = signal(0);
  cardsExpired = signal(0);
  recentActivity = signal<any[]>([]);

  async ngOnInit(): Promise<void> {
    await this.refresh();
  }

  async setPeriod(p: 'monthly' | 'yearly'): Promise<void> {
    this.revenuePeriod = p;
    try {
      const rev = await this.api.revenue(p);
      this.revenueData.set(
        (rev.buckets || []).map((b) => ({
          label: b.label,
          value: this.fmt(b.value),
          percent: b.percent,
          subPercent: b.subPercent ?? 0,
        })),
      );
    } catch {
      /* keep previous */
    }
  }

  async refresh(): Promise<void> {
    this.loading.set(true);
    this.loadError.set('');
    try {
      const [k, rev, g, c, t, fp, acts] = await Promise.all([
        this.api.kpis(),
        this.api.revenue(this.revenuePeriod),
        this.api.gender(),
        this.api.treatmentCards(),
        this.api.team(),
        this.api.freeVsPaid(),
        this.api.recentActivities(8),
      ]);

      this.kpis.set([
        {
          label: 'إجمالي المستخدمين',
          value: this.fmt(k.totalUsers),
          trend: k.totalUsersTrendPercent,
          icon: Users,
          bg: 'bg-primary-container',
          iconColor: 'text-primary',
        },
        {
          label: 'مستخدمين نشطين',
          value: this.fmt(k.activeUsers),
          trend: k.activeUsersTrendPercent,
          icon: UserCheck,
          bg: 'bg-tertiary-container',
          iconColor: 'text-tertiary',
        },
        {
          label: 'إيرادات الشهر',
          value: this.fmt(k.monthlyRevenue) + ' ' + (k.currency || 'ج.م'),
          trend: k.monthlyRevenueTrendPercent,
          icon: Wallet,
          bg: 'bg-secondary-container',
          iconColor: 'text-on-secondary-container',
        },
        {
          label: 'إيرادات السنة',
          value: this.fmt(k.yearlyRevenue) + ' ' + (k.currency || 'ج.م'),
          trend: k.yearlyRevenueTrendPercent,
          icon: TrendingUp,
          bg: 'bg-danger-container',
          iconColor: 'text-danger',
        },
      ]);

      this.revenueData.set(
        (rev.buckets || []).map((b) => ({
          label: b.label,
          value: this.fmt(b.value),
          percent: b.percent,
          subPercent: b.subPercent ?? 0,
        })),
      );

      this.genderSplit.set({ female: g.femalePercent, male: g.malePercent });
      this.totalUsers.set(g.totalUsersLabel || this.fmt(k.totalUsers));

      this.cardsIssued.set(c.issued);
      this.cardsActive.set(c.active);
      this.cardsExpired.set(c.expired);

      this.staffRoles.set([
        {
          label: 'مدراء النظام (Admins)',
          count: t.admins,
          icon: UserCog,
          bg: 'bg-primary-container',
          color: 'text-primary',
        },
        {
          label: 'الطاقم الطبي (Caregivers)',
          count: t.caregivers,
          icon: Stethoscope,
          bg: 'bg-tertiary-container',
          color: 'text-tertiary',
        },
        {
          label: 'مراجعين',
          count: t.reviewers,
          icon: ShieldCheck,
          bg: 'bg-secondary-container',
          color: 'text-on-secondary-container',
        },
      ]);

      this.usersStats.set({ free: fp.free, paid: fp.paid });

      // Map activity type -> icon + colour. Falls back gracefully on unknown types.
      const iconFor = (type: string) => {
        switch (type) {
          case 'signup':
            return { icon: UserPlus, bg: 'bg-primary-container', color: 'text-primary' };
          case 'subscription':
            return {
              icon: Wallet,
              bg: 'bg-secondary-container',
              color: 'text-on-secondary-container',
            };
          case 'treatment_card':
            return { icon: CreditCard, bg: 'bg-tertiary-container', color: 'text-tertiary' };
          case 'pricing_change':
            return { icon: FileText, bg: 'bg-danger-container', color: 'text-danger' };
          case 'payment':
            return {
              icon: Wallet,
              bg: 'bg-secondary-container',
              color: 'text-on-secondary-container',
            };
          default:
            return { icon: FileText, bg: 'bg-surface-container', color: 'text-outline' };
        }
      };
      this.recentActivity.set(
        (acts || []).map((a) => ({
          title: a.title,
          time: this.timeAgo(a.atUtc),
          ...iconFor(a.type),
        })),
      );
    } catch (err: any) {
      this.loadError.set(
        (err && err.error && err.error.message) || 'تعذر تحميل بيانات لوحة التحكم',
      );
    } finally {
      this.loading.set(false);
    }
  }

  // Existing template getters expect these names.
  get freePercent(): number {
    const s = this.usersStats();
    const total = s.free + s.paid;
    return total === 0 ? 0 : Math.round((s.free / total) * 100);
  }
  get conversionRate(): number {
    const s = this.usersStats();
    const total = s.free + s.paid;
    return total === 0 ? 0 : Math.round((s.paid / total) * 1000) / 10;
  }

  private fmt(n: number | null | undefined): string {
    if (n == null || !Number.isFinite(n)) return '0';
    if (n >= 1000) return new Intl.NumberFormat('ar-EG').format(n);
    return String(n);
  }

  private timeAgo(iso: string): string {
    if (!iso) return '';
    const then = new Date(iso).getTime();
    if (!Number.isFinite(then)) return '';
    const diffMin = Math.floor((Date.now() - then) / 60000);
    if (diffMin < 1) return 'الآن';
    if (diffMin < 60) return 'منذ ' + diffMin + ' دقيقة';
    const h = Math.floor(diffMin / 60);
    if (h < 24) return 'منذ ' + h + ' ساعة';
    const d = Math.floor(h / 24);
    return 'منذ ' + d + ' يوم';
  }
}
