import { Component } from '@angular/core';
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

@Component({
  selector: 'app-admin-dashboard',
  imports: [LucideAngularModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboard {
  calendarIcon = Calendar;
  trendUpIcon = TrendingUp;
  trendDownIcon = TrendingDown;
  creditCardIcon = CreditCard;

  revenuePeriod: 'monthly' | 'yearly' = 'monthly';

  kpis = [
    {
      label: 'إجمالي المستخدمين',
      value: '12,480',
      trend: 8.2,
      icon: Users,
      bg: 'bg-primary-container',
      iconColor: 'text-primary',
    },
    {
      label: 'مستخدمين نشطين',
      value: '9,104',
      trend: 4.1,
      icon: UserCheck,
      bg: 'bg-tertiary-container',
      iconColor: 'text-tertiary',
    },
    {
      label: 'إيرادات الشهر',
      value: '48,250 ج.م',
      trend: 12.4,
      icon: Wallet,
      bg: 'bg-secondary-container',
      iconColor: 'text-on-secondary-container',
    },
    {
      label: 'إيرادات السنة',
      value: '512,400 ج.م',
      trend: -2.3,
      icon: TrendingUp,
      bg: 'bg-danger-container',
      iconColor: 'text-danger',
    },
  ];

  revenueData = [
    { label: 'يناير', value: '32,100', percent: 60, subPercent: 38 },
    { label: 'فبراير', value: '35,400', percent: 65, subPercent: 40 },
    { label: 'مارس', value: '31,200', percent: 58, subPercent: 35 },
    { label: 'أبريل', value: '38,900', percent: 72, subPercent: 45 },
    { label: 'مايو', value: '41,000', percent: 76, subPercent: 48 },
    { label: 'يونيو', value: '37,500', percent: 70, subPercent: 42 },
    { label: 'يوليو', value: '43,200', percent: 80, subPercent: 50 },
    { label: 'أغسطس', value: '45,800', percent: 85, subPercent: 53 },
    { label: 'سبتمبر', value: '42,100', percent: 78, subPercent: 47 },
    { label: 'أكتوبر', value: '46,700', percent: 87, subPercent: 55 },
    { label: 'نوفمبر', value: '44,300', percent: 82, subPercent: 51 },
    { label: 'ديسمبر', value: '48,250', percent: 90, subPercent: 58 },
  ];

  genderSplit = {
    female: 58,
    male: 42,
  };

  totalUsers = '12.4k';

  usersStats = {
    free: 10620,
    paid: 1860,
  };

  get freePercent(): number {
    const total = this.usersStats.free + this.usersStats.paid;
    return Math.round((this.usersStats.free / total) * 100);
  }

  get conversionRate(): number {
    const total = this.usersStats.free + this.usersStats.paid;
    return Math.round((this.usersStats.paid / total) * 1000) / 10;
  }

  staffRoles = [
    {
      label: 'مدراء النظام (Admins)',
      count: 4,
      icon: UserCog,
      bg: 'bg-primary-container',
      color: 'text-primary',
    },
    {
      label: 'الطاقم الطبي (Caregivers)',
      count: 27,
      icon: Stethoscope,
      bg: 'bg-tertiary-container',
      color: 'text-tertiary',
    },
    {
      label: 'مراجعين',
      count: 9,
      icon: ShieldCheck,
      bg: 'bg-secondary-container',
      color: 'text-on-secondary-container',
    },
  ];

  cardsIssued = 8240;
  cardsActive = 7615;
  cardsExpired = 625;

  recentActivity = [
    {
      title: 'انضم مستخدم جديد: محمد أحمد',
      time: 'منذ ٥ دقائق',
      icon: UserPlus,
      bg: 'bg-primary-container',
      color: 'text-primary',
    },
    {
      title: 'ترقية باقة: سارة علي اشتركت في الباقة العائلية',
      time: 'منذ ١٢ دقيقة',
      icon: Wallet,
      bg: 'bg-secondary-container',
      color: 'text-on-secondary-container',
    },
    {
      title: 'تم إصدار كرت علاج جديد لـ: كريم محمود',
      time: 'منذ ٣٠ دقيقة',
      icon: CreditCard,
      bg: 'bg-tertiary-container',
      color: 'text-tertiary',
    },
    {
      title: 'تم تعديل أسعار الباقة العائلية',
      time: 'منذ ساعة',
      icon: FileText,
      bg: 'bg-danger-container',
      color: 'text-danger',
    },
  ];
}
