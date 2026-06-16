import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import {
  Bell,
  ChevronDown,
  CircleUser,
  Heart,
  LogOut,
  LucideAngularModule,
  Pill,
  ShieldAlert,
  User,
  UserCheck,
  Users,
  TriangleAlert,
  ChevronLeft,
} from 'lucide-angular';

@Component({
  selector: 'app-header',
  imports: [LucideAngularModule, RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  showLogoutConfirm = false;
  toastMessage: string | null = null;
  accountMenu = false;
  heartIcon = Heart;
  logOutIcon = LogOut;
  chevronDownIcon = ChevronDown;
  userCheckIcon = UserCheck;
  bellIcon = Bell;
  circleUserIcon = CircleUser;
  userIcon = User;
  usersIcon = Users;
  userCircleIcon = CircleUser;
  shieldAlertIcon = ShieldAlert;
  pillIcon = Pill;
  bellMenu = false;
  alertIcon = TriangleAlert;
  chevronLeftIcon = ChevronLeft;

  bellTab: 'meds' | 'family' = 'meds';

  // Mock data - هتيجي من الـ service
  medNotifications = [
    {
      id: 1,
      type: 'reminder',
      status: 'pending',
      title: 'حان وقت ميتفورمين',
      body: '٥٠٠ ملغ — بعد الأكل',
      time: 'الآن',
      read: false,
    },

    {
      id: 3,
      type: 'reminder',
      status: 'taken',
      title: 'أملوديبين — الصباح',
      body: '٥ ملغ',
      time: 'منذ ٣ س',
      read: true,
    },
  ] as any[];

  familyNotifications = [
    {
      id: 1,
      memberName: 'أحمد علي',
      title: 'لم يأخذ دواءه!',
      body: 'ميتفورمين — فات موعده',
      time: 'منذ ١ س',
      read: false,
    },
    {
      id: 2,
      memberName: 'سالي فؤاد',
      title: 'أخذت جرعة أملوديبين ✓',
      body: 'تم تسجيل الجرعة',
      time: 'منذ ٢ س',
      read: false,
    },
    {
      id: 3,
      memberName: 'سالي فؤاد',
      title: 'أخذت جرعة أملوديبين ✓',
      body: 'تم تسجيل الجرعة',
      time: 'منذ ٢ س',
      read: false,
    },
    {
      id: 4,
      memberName: 'سالي فؤاد',
      title: 'أخذت جرعة أملوديبين ✓',
      body: 'تم تسجيل الجرعة',
      time: 'منذ ٢ س',
      read: false,
    },
    {
      id: 5,
      memberName: 'سالي فؤاد',
      title: 'أخذت جرعة أملوديبين ✓',
      body: 'تم تسجيل الجرعة',
      time: 'منذ ٢ س',
      read: false,
    },
    {
      id: 6,
      memberName: 'سالي فؤاد',
      title: 'أخذت جرعة أملوديبين ✓',
      body: 'تم تسجيل الجرعة',
      time: 'منذ ٢ س',
      read: false,
    },
    {
      id: 7,
      memberName: 'سالي فؤاد',
      title: 'أخذت جرعة أملوديبين ✓',
      body: 'تم تسجيل الجرعة',
      time: 'منذ ٢ س',
      read: false,
    },
    {
      id: 8,
      memberName: 'سالي فؤاد',
      title: 'أخذت جرعة أملوديبين ✓',
      body: 'تم تسجيل الجرعة',
      time: 'منذ ٢ س',
      read: false,
    },
    {
      id: 9,
      memberName: 'سالي فؤاد',
      title: 'أخذت جرعة أملوديبين ✓',
      body: 'تم تسجيل الجرعة',
      time: 'منذ ٢ س',
      read: false,
    },
  ] as any[];

  get unreadMeds(): number {
    return this.medNotifications.filter((n: any) => !n.read).length;
  }

  get unreadFamily(): number {
    return this.familyNotifications.filter((n: any) => !n.read).length;
  }

  get unreadCount(): number {
    return this.unreadMeds + this.unreadFamily;
  }

  takeDose(notif: any) {
    notif.status = 'taken';
    notif.read = true;
  }

  snooze(notif: any) {
    notif.status = 'snoozed';
    notif.read = true;
  }

  markFamilyRead(notif: any) {
    notif.read = true;
  }

  logout(): void {
    this.showLogoutConfirm = false;
    this.toastMessage = 'تم تسجيل الخروج بنجاح 🔒';

    setTimeout(() => {
      this.toastMessage = null;
      this.cdr.detectChanges();
      console.log('تم مسح الرسالة!');
    }, 3000);

    this.router.navigate(['/home']);
  }

  toggleDropdown() {
    this.accountMenu = !this.accountMenu;
  }
  markMedRead(notif: any) {
    notif.read = true;
  }
}
