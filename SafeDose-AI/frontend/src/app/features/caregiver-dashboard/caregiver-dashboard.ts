import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  Heart,
  Pill,
  Stethoscope,
  Search,
  Bell,
  Settings,
  AlertTriangle,
  CheckCircle,
  LucideAngularModule,
  LucideIconProvider,
  LUCIDE_ICONS,
} from 'lucide-angular';

@Component({
  selector: 'app-caregiver-dashboard',
  imports: [LucideAngularModule,RouterLink],
  templateUrl: './caregiver-dashboard.html',
  styleUrl: './caregiver-dashboard.css',
})
export class CaregiverDashboard {
  heartIcon = Heart;
  stethoscopeIcon = Stethoscope;
  searchIcon = Search;
  bellIcon = Bell;
  settingsIcon = Settings;
  alertTriangleIcon = AlertTriangle;
  checkCircleIcon = CheckCircle;

  patients = [
    {
      id: '1',
      name: 'أحمد المنسي',
      status: 'stable' as const,
      statusText: 'سكري - ضغط',
      adherence: 94,
      lastDose: '١٠:٠٠ ص',
    },
    {
      id: '2',
      name: 'فاطمة حسن',
      status: 'urgent' as const,
      statusText: 'تنبيه تداخل دوائي',
      adherence: 72,
      lastDose: '٨:٣٠ ص',
    },
    {
      id: '3',
      name: 'محمد علي',
      status: 'stable' as const,
      statusText: 'قلب - كوليسترول',
      adherence: 88,
      lastDose: '٩:٠٠ ص',
    },
    {
      id: '4',
      name: 'سعاد عبدالله',
      status: 'stable' as const,
      statusText: 'ضغط - ربو',
      adherence: 96,
      lastDose: '٧:٠٠ ص',
    },
  ];
}
