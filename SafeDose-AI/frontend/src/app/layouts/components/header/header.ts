import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ChevronDown, Heart, LogOut, LucideAngularModule, UserCheck } from 'lucide-angular';

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

  heartIcon = Heart;
  logOutIcon = LogOut;
  chevronDownIcon = ChevronDown;
  userCheckIcon = UserCheck;

  get currentRoute(): string {
    return this.router.url;
  }

  logout(): void {
    this.showLogoutConfirm = false;
    this.toastMessage = 'تم تسجيل الخروج بنجاح 🔒';

    setTimeout(() => {
      this.toastMessage = null;
      // 3. أجبري الواجهة على التحديث بعد تغيير القيمة
      this.cdr.detectChanges();
      console.log('تم مسح الرسالة!');
    }, 3000);

    this.router.navigate(['/home']);
  }
}
