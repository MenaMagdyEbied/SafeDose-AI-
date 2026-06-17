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
import { Card } from './shared/components/card/card';
import { PublicCard } from './features/public-card/public-card';
import { authGuard } from './core/auth/guards/auth-guard';
import { adminGuard } from './core/auth/guards/admin-guard';
import { guestGuard } from './core/auth/guards/guest-guard';
import { superAdminGuard } from './core/auth/guards/super-admin-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },

  {
    path: '',
    component: MainLayout,
    children: [
      { path: 'home', component: Home, title: 'الرئيسية | SafeDose AI' },
      {
        path: 'patient',
        component: PatientHome,
        canActivate: [authGuard],
        title: 'بوابة المريض | SafeDose AI',
      },
      {
        path: 'interaction-checker',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/interaction-checker/interaction-checker').then(
            (c) => c.InteractionChecker,
          ),
        title: 'فحص التفاعلات | SafeDose AI',
      },
      {
        path: 'interaction-results',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/interaction-results/interaction-results').then(
            (c) => c.InteractionResults,
          ),
        title: 'نتائج الفحص | SafeDose AI',
      },
      {
        path: 'digital-card',
        component: DigitalCard,
        canActivate: [authGuard],
        title: 'البطاقة الرقمية | SafeDose AI',
      },
      {
        path: 'caregiver-review',
        component: CaregiverReview,
        canActivate: [authGuard],
        title: 'مراجعة الطاقم | SafeDose AI',
      },
      { path: 'pricing', component: Pricing, title: 'الأسعار | SafeDose AI' },
      {
        path: 'profile',
        canActivate: [authGuard],
        loadComponent: () => import('./features/profile/profile').then((c) => c.Profile),
        title: 'الملف الشخصي | SafeDose AI',
      },
      {
        path: 'family-plan',
        canActivate: [authGuard],
        loadComponent: () => import('./features/family-plan/family-plan').then((c) => c.FamilyPlan),
        title: 'خطة العيلة | SafeDose AI',
      },
      {
        path: 'payment',
        canActivate: [authGuard],
        loadComponent: () => import('./features/payment/payment').then((c) => c.Payment),
        title: 'الدفع | SafeDose AI',
      },
      {
        path: 'notifications',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/notifications/notifications').then((c) => c.Notifications),
        title: 'الإشعارات | SafeDose AI',
      },
      {
        path: 'prescription-detail/:id',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/prescription-detail/prescription-detail').then(
            (c) => c.PrescriptionDetail,
          ),
        title: 'تفاصيل الروشتة | SafeDose AI',
      },
    ],
  },

  {
    path: 'admin',
    component: AdminLayout,
    canActivate: [adminGuard],
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
      {
        path: 'managers',
        canActivate: [superAdminGuard],

        loadComponent: () =>
          import('./features/admin/admin-manager/admin-manager').then((c) => c.AdminManager),
        title: 'إدارة المشرفين | SafeDose AI',
      },
    ],
  },

  { path: 'card/:id', component: PublicCard, title: 'البطاقة الرقمية | SafeDose AI' },

  {
    path: 'login',
    component: Login,
    canActivate: [guestGuard],
    title: 'تسجيل الدخول | SafeDose AI',
  },
  {
    path: 'register',
    component: Register,
    canActivate: [guestGuard],
    title: 'إنشاء حساب | SafeDose AI',
  },
  {
    path: 'email-confirmation',
    loadComponent: () =>
      import('./features/auth/email-confirmation/email-confirmation').then(
        (c) => c.EmailConfirmation,
      ),
    title: 'تأكيد البريد | SafeDose AI',
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password').then((c) => c.ForgotPassword),
    title: 'نسيت كلمة المرور | SafeDose AI',
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/reset-password/reset-password').then((c) => c.ResetPassword),
    title: 'إعادة تعيين كلمة المرور | SafeDose AI',
  },

  { path: '**', component: NotFound, title: 'الصفحة غير موجودة | SafeDose AI' },
];
