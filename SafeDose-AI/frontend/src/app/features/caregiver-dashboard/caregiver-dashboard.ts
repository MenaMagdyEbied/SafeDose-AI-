import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  Bell,
  CircleCheck,
  Heart,
  LucideAngularModule,
  Search,
  Settings,
  Stethoscope,
  TriangleAlert
} from 'lucide-angular';

@Component({
  selector: 'app-caregiver-dashboard',
  imports: [LucideAngularModule, RouterLink, FormsModule],
  templateUrl: './caregiver-dashboard.html',
  styleUrl: './caregiver-dashboard.css',
})
export class CaregiverDashboard {
  heartIcon = Heart;
  stethoscopeIcon = Stethoscope;
  searchIcon = Search;
  bellIcon = Bell;
  settingsIcon = Settings;
  alertTriangleIcon = TriangleAlert;
  checkCircleIcon = CircleCheck;
  searchQuery = '';

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
  get filteredPatients() {
    if (!this.searchQuery.trim()) return this.patients;
    return this.patients.filter(
      (p) => p.name.includes(this.searchQuery) || p.statusText.includes(this.searchQuery),
    );
  }
}
