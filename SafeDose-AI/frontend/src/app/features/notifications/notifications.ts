import { Component } from '@angular/core';
import { Check, Clock, LucideAngularModule, Pill, TriangleAlert, Users, X } from 'lucide-angular';
import { MedNotification } from '../../core/models/med-notification';
import { FamilyNotification } from '../../core/models/family-notification';

@Component({
  selector: 'app-notifications',
  imports: [LucideAngularModule],
  templateUrl: './notifications.html',
  styleUrl: './notifications.css',
})
export class Notifications {
  pillIcon = Pill;
  usersIcon = Users;
  alertIcon = TriangleAlert;
  checkIcon = Check;
  clockIcon = Clock;
  xIcon = X;

  activeTab: 'meds' | 'family' = 'meds';

  medNotifications: MedNotification[] = [
    {
      id: 1,
      type: 'reminder',
      status: 'pending',
      title: 'حان وقت ميتفورمين',
      body: '٥٠٠ ملغ — بعد الأكل مباشرة',
      time: 'الآن',
      read: false,
    },
    {
      id: 2,
      type: 'warning',
      status: 'pending',
      title: 'تحذير: تداخل دوائي محتمل',
      body: 'أسبرين + وارفارين — راجع طبيبك قبل الأخذ',
      time: 'منذ ١٠ د',
      read: false,
    },
    {
      id: 3,
      type: 'reminder',
      status: 'taken',
      title: 'أملوديبين — الصباح',
      body: '٥ ملغ — مرة يومياً',
      time: 'منذ ٣ س',
      read: true,
    },
    {
      id: 4,
      type: 'reminder',
      status: 'skipped',
      title: 'أسبرين — الليل',
      body: '٨١ ملغ — قبل النوم',
      time: 'أمس',
      read: true,
    },
  ];

  familyNotifications: FamilyNotification[] = [
    {
      id: 1,
      memberName: 'أحمد علي',
      title: 'لم يأخذ دواءه!',
      body: 'ميتفورمين — فات موعده منذ ساعة',
      time: 'منذ ١ س',
      read: false,
    },
    {
      id: 2,
      memberName: 'سالي فؤاد',
      title: 'أخذت جرعة أملوديبين ✓',
      body: 'تم تسجيل الجرعة الصباحية بنجاح',
      time: 'منذ ٢ س',
      read: false,
    },
    {
      id: 3,
      memberName: 'أحمد علي',
      title: 'موعد كشف طبي غداً',
      body: 'د. محمد السيد — الساعة ١١ ص',
      time: 'منذ ٥ س',
      read: true,
    },
  ];

  get unreadMeds() {
    return this.medNotifications.filter((n) => !n.read).length;
  }

  get unreadFamily() {
    return this.familyNotifications.filter((n) => !n.read).length;
  }

  takeDose(notif: MedNotification) {
    notif.status = 'taken';
    notif.read = true;
  }

  snooze(notif: MedNotification) {
    notif.status = 'snoozed';
    notif.read = true;
  }

  skipDose(notif: MedNotification) {
    notif.status = 'skipped';
    notif.read = true;
  }

  markRead(notif: FamilyNotification) {
    notif.read = true;
  }

  markAllRead() {
    this.medNotifications.forEach((n) => (n.read = true));
    this.familyNotifications.forEach((n) => (n.read = true));
  }
}
