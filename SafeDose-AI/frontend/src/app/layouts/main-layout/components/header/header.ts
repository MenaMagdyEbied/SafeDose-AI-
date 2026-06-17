import { ChangeDetectorRef, Component, inject } from '@angular/core';
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
import { UserProfile } from '../../../../core/auth/services/user-profile';

@Component({
  selector: 'app-header',
  imports: [LucideAngularModule, RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  private readonly router = inject(Router);
  protected readonly authService = inject(Auth);

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
    this.router.navigate(['/home']);
  }

  ngOnInit(): void {
    this.authService.user$.subscribe((user) => {
      this.userName = user?.name || user?.userName || '';
    });
  }
}
