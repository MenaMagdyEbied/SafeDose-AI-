import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { Heart, LayoutDashboard, Menu, Settings, X, Users } from 'lucide-angular';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { Auth } from '../../../../core/auth/services/auth';
import { UserProfile } from '../../../../core/auth/services/user-profile';
import {
  Bell,
  ChevronDown,
  ChevronLeft,
  CircleUser,
  CreditCard,
  LogIn,
  LogOut,
  Pill,
  ShieldAlert,
  TriangleAlert,
  User,
  UserCheck,
  UserPlus,
} from 'lucide-angular';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  protected readonly authService = inject(Auth);
  private readonly userProfileService = inject(UserProfile);
  settingsIcon = Settings;
  layoutDashboardIcon = LayoutDashboard;
  heartIcon = Heart;
  menuIcon = Menu;
  xIcon = X;
  usersIcon = Users;
  mobileMenu = false;


  showLogoutConfirm = false;
  accountMenu = false;
  bellMenu = false;
  bellTab: 'meds' | 'family' = 'meds';
  userName = '';

  // Icons
  logOutIcon = LogOut;
  logInIcon = LogIn;
  userPlusIcon = UserPlus;
  chevronDownIcon = ChevronDown;
  chevronLeftIcon = ChevronLeft;
  userCheckIcon = UserCheck;
  bellIcon = Bell;
  circleUserIcon = CircleUser;
  userIcon = User;
  shieldAlertIcon = ShieldAlert;
  pillIcon = Pill;
  digitalCardIcon = CreditCard;
  alertIcon = TriangleAlert;

  logout(): void {
    this.showLogoutConfirm = false;
    this.authService.logout();
    this.router.navigate(['/home']);
  }

  ngOnInit(): void {
    if (this.authService.isLoggedIn) {
      this.loadProfile();
    }

    this.authService.user$.subscribe((user) => {
      if (user) {
        this.userName = user.userName;
      } else {
        this.userName = '';
      }
    });
  }

  private loadProfile(): void {
    this.userProfileService.getUserProfile().subscribe({
      next: (profile: any) => {
        this.userName =
          profile?.userName || profile?.fullName || this.authService.user?.userName || '';
        if (profile?.userName) {
          this.authService.updateProfile({ userName: profile.userName });
        }
      },
      error: () => {
        this.userName = this.authService.user?.userName || '';
      },
    });
  }
}
