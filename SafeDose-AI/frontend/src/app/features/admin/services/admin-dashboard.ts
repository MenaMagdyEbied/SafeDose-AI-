import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface AdminGenderSplit {
  femalePercent: number;
  malePercent: number;
  totalUsersLabel: string;
}
export interface AdminTreatmentCards {
  issued: number;
  active: number;
  expired: number;
}
export interface AdminTeam {
  admins: number;
  caregivers: number;
  reviewers: number;
}
export interface AdminFreeVsPaid {
  free: number;
  paid: number;
  conversionRate: number;
  freePercent: number;
}
export interface AdminActivity {
  type: string;
  title: string;
  atUtc: string;
}
export interface AdminKpis {
  totalUsers: number;
  totalUsersTrendPercent: number;
  activeUsers: number;
  activeUsersTrendPercent: number;
  monthlyRevenue: number;
  monthlyRevenueTrendPercent: number;
  yearlyRevenue: number;
  yearlyRevenueTrendPercent: number;
  currency: string;
}

export interface AdminKpis {
  totalUsers: number;
  totalUsersTrendPercent: number;
  activeUsers: number;
  activeUsersTrendPercent: number;
  monthlyRevenue: number;
  monthlyRevenueTrendPercent: number;
  yearlyRevenue: number;
  yearlyRevenueTrendPercent: number;
  currency: string;
}

export interface AdminRevenueChart {
  period: 'monthly' | 'yearly';
  currentTotal: number;
  buckets: { label: string; value: number; percent: number; subPercent?: number }[];
}

@Injectable({
  providedIn: 'root',
})
export class AdminDashboard {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  gender(): Promise<AdminGenderSplit> {
    return firstValueFrom(
      this.http.get<AdminGenderSplit>(this.apiUrl + '/admin/dashboard/users/gender'),
    );
  }

  treatmentCards(): Promise<AdminTreatmentCards> {
    return firstValueFrom(
      this.http.get<AdminTreatmentCards>(this.apiUrl + '/admin/dashboard/treatment-cards'),
    );
  }

  team(): Promise<AdminTeam> {
    return firstValueFrom(this.http.get<AdminTeam>(this.apiUrl + '/admin/dashboard/team'));
  }

  freeVsPaid(): Promise<AdminFreeVsPaid> {
    return firstValueFrom(
      this.http.get<AdminFreeVsPaid>(this.apiUrl + '/admin/dashboard/users/free-vs-paid'),
    );
  }

  async kpis(): Promise<AdminKpis> {
    const raw: any = await firstValueFrom(this.http.get(this.apiUrl + '/admin/dashboard/kpis'));
    return {
      totalUsers: raw.totalUsers ?? 0,
      totalUsersTrendPercent: raw.totalUsersTrendPercent ?? 0,
      activeUsers: raw.activeUsers ?? 0,
      activeUsersTrendPercent: raw.activeUsersTrendPercent ?? 0,
      monthlyRevenue: raw.monthlyRevenue?.value ?? raw.monthlyRevenue ?? 0,
      monthlyRevenueTrendPercent:
        raw.monthlyRevenue?.trendPercent ?? raw.monthlyRevenueTrendPercent ?? 0,
      yearlyRevenue: raw.yearlyRevenue?.value ?? raw.yearlyRevenue ?? 0,
      yearlyRevenueTrendPercent:
        raw.yearlyRevenue?.trendPercent ?? raw.yearlyRevenueTrendPercent ?? 0,
      currency: raw.monthlyRevenue?.currency ?? raw.yearlyRevenue?.currency ?? 'ج.م',
    };
  }

  async revenue(period: 'monthly' | 'yearly' = 'monthly'): Promise<AdminRevenueChart> {
    const raw: any = await firstValueFrom(
      this.http.get(this.apiUrl + '/admin/dashboard/revenue?period=' + period),
    );
    const points: any[] = raw.points ?? raw.buckets ?? [];
    const maxVal = Math.max(...points.map((p: any) => p.total ?? 0), 1);
    return {
      period,
      currentTotal: raw.currentTotal ?? 0,
      buckets: points.map((p: any) => ({
        label: p.monthLabelArabic ?? p.label ?? '',
        value: p.total ?? p.value ?? 0,
        percent: Math.round(((p.total ?? p.value ?? 0) / maxVal) * 100),
        subPercent: p.subPercent ?? 0,
      })),
    };
  }

  async recentActivities(limit = 20): Promise<AdminActivity[]> {
    const raw: any[] = await firstValueFrom(
      this.http.get<any[]>(this.apiUrl + '/admin/dashboard/activities/recent?limit=' + limit),
    );
    return (raw ?? []).map((a) => ({
      type: a.type ?? '',
      title: a.titleArabic ?? a.title ?? '',
      atUtc: a.atUtc ?? '',
    }));
  }
}
