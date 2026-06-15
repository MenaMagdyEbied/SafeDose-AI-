import { Routes } from '@angular/router';
import { CaregiverReview } from './features/caregiver-review/caregiver-review';
import { DigitalCard } from './features/digital-card/digital-card';
import { Home } from './features/home/home';
import { InteractionChecker } from './features/interaction-checker/interaction-checker';
import { InteractionResults } from './features/interaction-results/interaction-results';
import { PatientHome } from './features/patient-home/patient-home';
import { Pricing } from './features/pricing/pricing';
import { MainLayout } from './layouts/main-layout/main-layout';
import { NotFound } from './shared/components/not-found/not-found';
import { AdminLayout } from './layouts/admin-layout/admin-layout';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  {
    path: '',
    component: MainLayout,
    children: [
      { path: 'home', component: Home, title: 'الرئيسية | SafeDose AI' },
      { path: 'patient', component: PatientHome, title: 'بوابة المريض | SafeDose AI' },
      {
        path: 'interaction-checker',
        loadComponent: () =>
          import('./features/interaction-checker/interaction-checker').then(
            (c) => c.InteractionChecker,
          ),
        title: 'فحص التفاعلات | SafeDose AI',
      },
      {
        path: 'interaction-results',
        loadComponent: () =>
          import('./features/interaction-results/interaction-results').then(
            (c) => c.InteractionResults,
          ),

        title: 'نتائج الفحص | SafeDose AI',
      },
      { path: 'digital-card', component: DigitalCard, title: 'البطاقة الرقمية | SafeDose AI' },
      {
        path: 'caregiver-review',
        component: CaregiverReview,
        title: 'مراجعة الطاقم | SafeDose AI',
      },

      { path: 'pricing', component: Pricing, title: 'الأسعار | SafeDose AI' },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile').then((c) => c.Profile),
        title: 'الملف الشخصي | SafeDose AI',
      },
      {
        path: 'family-plan',
        loadComponent: () => import('./features/family-plan/family-plan').then((c) => c.FamilyPlan),
        title: 'خطة العيلة | SafeDose AI',
      },
      {
        path: 'payment',
        loadComponent: () => import('./features/payment/payment').then((c) => c.Payment),
        title: ' الدفع | SafeDose AI',
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/notifications/notifications').then((c) => c.Notifications),
        title: ' الأشعارات | SafeDose AI',
      },
      {
        path: 'prescription-detail/:id',
        loadComponent: () =>
          import('./features/prescription-detail/prescription-detail').then(
            (c) => c.PrescriptionDetail,
          ),
        title: ' تفاصيل الروشته | SafeDose AI',
      },
    ],
  },
  {
    path: 'admin',
    component: AdminLayout,
    // canActivate: [adminGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/admin/admin-dashboard/admin-dashboard').then((c) => c.AdminDashboard),
        title: 'لوحة التحكم | SafeDose AI',
      },
      {
        path: 'pricing',
        loadComponent: () =>
          import('./features/admin/admin-pricing/admin-pricing').then((c) => c.AdminPricing),
        title: 'تعديل الأسعار | SafeDose AI',
      },
    ],
  },

  { path: 'login', component: Login, title: 'تسجيل الدخول | SafeDose AI' },
  { path: 'register', component: Register, title: 'إنشاء حساب | SafeDose AI' },
  { path: '**', component: NotFound, title: 'الصفحة غير موجودة | SafeDose AI' },
];
