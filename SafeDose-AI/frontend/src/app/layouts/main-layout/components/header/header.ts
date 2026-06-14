import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import {
  ChevronDown,
  CircleUser,
  Heart,
  LogOut,
  LucideAngularModule,
  ShieldAlert,
  User,
  UserCheck,
  Users,
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
  circleUserIcon = CircleUser;
  userIcon = User;
  usersIcon = Users;
  userCircleIcon = CircleUser;
  shieldAlertIcon = ShieldAlert;

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
}
