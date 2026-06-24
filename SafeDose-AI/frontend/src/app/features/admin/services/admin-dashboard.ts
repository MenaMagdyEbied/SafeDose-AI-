import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';

// Mirror backend SafeDose.Application.DTOs.Admin shapes loosely (any-typed for now).
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
  buckets: { label: string; value: number; percent: number; subPercent?: number }[];
}

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

@Injectable({
  providedIn: 'root',
})
export class AdminDashboard {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);

  kpis(): Promise<AdminKpis> {
    return firstValueFrom(this.http.get<AdminKpis>(this.apiUrl + '/admin/dashboard/kpis'));
  }

  revenue(period: 'monthly' | 'yearly' = 'monthly'): Promise<AdminRevenueChart> {
    return firstValueFrom(
      this.http.get<AdminRevenueChart>(this.apiUrl + '/admin/dashboard/revenue?period=' + period),
    );
  }

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

  recentActivities(limit = 20): Promise<AdminActivity[]> {
    return firstValueFrom(
      this.http.get<AdminActivity[]>(
        this.apiUrl + '/admin/dashboard/activities/recent?limit=' + limit,
      ),
    );
  }
}
